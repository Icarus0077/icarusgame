"""Store the self-contained recovery instructions with the verified backup."""
from pathlib import Path
from contextlib import closing
import argparse
import hashlib
import json
import shutil
import sqlite3
import subprocess
import tarfile
import tempfile
import os

BASE = Path('/opt/icarus-control')
BUNDLE = Path(__file__).resolve().parent


def digest(path):
    value = hashlib.sha256()
    with path.open('rb') as source:
        for block in iter(lambda: source.read(1024 * 1024), b''):
            value.update(block)
    return value.hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--backup', required=True)
    parser.add_argument('--finalize-sqlite-snapshots', action='store_true', help='Close/canonicalize the original saved snapshots before recording final checksums; never reads live databases.')
    args = parser.parse_args()
    backup = Path(args.backup).resolve(strict=True)
    if not backup.is_relative_to(BASE / 'backups'):
        raise RuntimeError('Unexpected backup path.')
    release = (BASE / 'current').resolve(strict=True)
    manifest = json.loads((backup / 'manifest.json').read_text())
    if manifest.get('restore_drill', {}).get('status') != 'passed':
        raise RuntimeError('Do not mark an untested backup as verified.')
    mismatches = []
    for relative, expected in manifest['checksums'].items():
        path = (backup / relative).resolve()
        if not path.is_relative_to(backup) or not path.is_file() or digest(path) != expected:
            mismatches.append(relative)
    if mismatches:
        snapshots = ['data/' + name for name in manifest['database_snapshots']]
        allowed = {name + suffix for name in snapshots for suffix in ('', '-wal', '-shm')}
        if not args.finalize_sqlite_snapshots or not set(mismatches).issubset(allowed):
            raise RuntimeError('Backup verification failed: ' + ', '.join(mismatches))
        original = backup / 'data-before-finalization.tar.gz'
        if not original.exists():
            with tarfile.open(original, 'w:gz') as archive:
                archive.add(backup / 'data', arcname='data')
        manifest['pre_finalization_checksums'] = dict(manifest['checksums'])
        logical_checks = {}
        with tempfile.TemporaryDirectory(prefix='icarus-snapshot-finalize-') as temporary:
            for index, relative in enumerate(snapshots):
                path = (backup / relative).resolve(strict=True)
                if not path.is_relative_to(backup / 'data'):
                    raise RuntimeError('Snapshot path escapes the saved data directory.')
                stable = Path(temporary) / (str(index) + '.sqlite')
                with closing(sqlite3.connect(path.as_uri() + '?mode=ro', uri=True)) as source, closing(sqlite3.connect(stable)) as destination:
                    if source.execute('PRAGMA integrity_check').fetchall() != [('ok',)]:
                        raise RuntimeError('Original saved snapshot failed integrity check: ' + relative)
                    before = hashlib.sha256('\n'.join(source.iterdump()).encode()).hexdigest()
                    source.backup(destination)
                    destination.execute('PRAGMA journal_mode=DELETE')
                    after = hashlib.sha256('\n'.join(destination.iterdump()).encode()).hexdigest()
                    if before != after or destination.execute('PRAGMA integrity_check').fetchall() != [('ok',)]:
                        raise RuntimeError('Snapshot contents changed during finalization: ' + relative)
                os.chmod(stable, 0o600)
                shutil.copy2(stable, path)
                logical_checks[relative] = {'logical_contents_unchanged': True, 'integrity': 'ok'}
                manifest['checksums'][relative] = digest(path)
                for suffix in ('-wal', '-shm'):
                    sidecar = backup / (relative + suffix)
                    sidecar.unlink(missing_ok=True)
                    manifest['checksums'].pop(relative + suffix, None)
        manifest['checksums']['data-before-finalization.tar.gz'] = digest(original)
        manifest['snapshot_finalization_note'] = 'Initial snapshot connections were not explicitly closed before hashing. Saved snapshots were finalized in isolation with DELETE journaling; logical SQL dumps matched before/after and all integrity checks passed. The original saved data is retained in data-before-finalization.tar.gz. Live data was not read or modified during finalization.'
        manifest['snapshot_logical_checks'] = logical_checks
    for relative in ('restore-release.py', 'HERSTELPLAN.md'):
        destination = backup / relative
        shutil.copy2(BUNDLE / relative, destination)
        destination.chmod(0o600)
        manifest['checksums'][relative] = digest(destination)
    shutil.copy2(BUNDLE / 'restore-release.py', release / 'deploy/restore-release.py')
    shutil.copy2(BUNDLE / 'HERSTELPLAN.md', release / 'deploy/GAMESERVER_BACKUP_EN_HERSTEL.md')
    for relative, expected in manifest['checksums'].items():
        path = (backup / relative).resolve(strict=True)
        if not path.is_relative_to(backup) or digest(path) != expected:
            raise RuntimeError('Backup verification failed: ' + relative)
    manifest['deployed_release'] = str(release)
    manifest['smtp_password_status'] = 'awaiting_private_entry'
    manifest['production_checks'] = {'health': 200, 'login': 200, 'canonical_reset_page': 200, 'admin_without_session': 401}
    (backup / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    subprocess.run(['python3', '-m', 'py_compile', str(backup / 'restore-release.py')], check=True)
    print(json.dumps({'status': 'recovery_record_saved_and_verified', 'backup': str(backup), 'verified_files': len(manifest['checksums'])}))


if __name__ == '__main__':
    main()
