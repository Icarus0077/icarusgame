"""Back up the running Icarus release without stopping or changing it.

The private backup includes runtime secrets. Never copy it to public hosting
or source control. SQLite's backup API includes committed WAL changes.
"""
from datetime import datetime, timezone
from pathlib import Path
from contextlib import closing
import hashlib
import json
import os
import shutil
import sqlite3
import subprocess
import tarfile
import urllib.request

BASE = Path('/opt/icarus-control')
DATA = Path('/var/lib/icarus-control')


def run(*args):
    return subprocess.check_output(args, text=True).strip()


def digest(path):
    value = hashlib.sha256()
    with path.open('rb') as source:
        for block in iter(lambda: source.read(1024 * 1024), b''):
            value.update(block)
    return value.hexdigest()


def main():
    os.umask(0o077)
    stamp = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ')
    live = (BASE / 'current').resolve(strict=True)
    if not live.is_relative_to(BASE / 'releases'):
        raise RuntimeError('Current release is outside the expected releases directory.')
    with urllib.request.urlopen('http://127.0.0.1:3100/api/health', timeout=10) as health:
        if health.status != 200:
            raise RuntimeError('Current release is not healthy; do not label it working.')

    container = json.loads(run('docker', 'inspect', 'icarus-control'))[0]
    project = container['Config']['Labels']['com.docker.compose.project']
    image_id = container['Image']
    backups = BASE / 'backups'
    backups.mkdir(mode=0o700, exist_ok=True)
    if backups.is_symlink():
        raise RuntimeError('Backups directory must not be a symlink.')
    target = backups / ('before-password-reset-' + stamp)
    target.mkdir(mode=0o700)

    # Keep an immutable, explicitly named copy of the exact running image.
    image_tag = 'icarus-control:backup-before-password-reset-' + stamp.lower()
    subprocess.run(['docker', 'tag', image_id, image_tag], check=True)
    subprocess.run(['docker', 'image', 'save', '-o', str(target / 'image.tar'), image_tag], check=True)
    with tarfile.open(target / 'release.tar.gz', 'w:gz') as archive:
        archive.add(live, arcname='release', filter=lambda item: None if '/node_modules/' in item.name or '/.git/' in item.name else item)

    configs = [
        BASE / 'secrets/runtime.env', BASE / 'secrets/admin-session.key', BASE / 'auth',
        Path('/etc/apache2/icarus-control-web-auth.conf'), Path('/etc/apache2/icarus-control.htpasswd'),
        Path('/etc/apache2/sites-available/gameserver.rene007.nl.conf'),
        Path('/etc/apache2/sites-available/gameserver.rene007.nl-le-ssl.conf'),
    ]
    with tarfile.open(target / 'private-config.tar.gz', 'w:gz') as archive:
        for path in configs:
            if path.exists():
                archive.add(path, arcname=str(path).lstrip('/'))

    snapshot = target / 'data'
    shutil.copytree(DATA, snapshot)
    databases = []
    for original in DATA.rglob('*.sqlite'):
        destination = snapshot / original.relative_to(DATA)
        for suffix in ('-wal', '-shm', '-journal'):
            destination.with_name(destination.name + suffix).unlink(missing_ok=True)
        # sqlite3.Connection's context manager commits but does not close.
        # Close both connections before hashing files; WAL finalization can
        # otherwise change a snapshot header after manifest creation.
        with closing(sqlite3.connect(original.as_uri() + '?mode=ro', uri=True)) as source, closing(sqlite3.connect(destination)) as saved:
            source.backup(saved)
            saved.execute('PRAGMA journal_mode=DELETE')
            checks = [row[0] for row in saved.execute('PRAGMA integrity_check')]
        if checks != ['ok']:
            raise RuntimeError('Database snapshot failed integrity check: ' + str(original.relative_to(DATA)))
        databases.append(str(original.relative_to(DATA)))

    manifest = {
        'format': 1, 'created_utc': stamp, 'backup_directory': str(target),
        'previous_release': str(live), 'compose_project': project,
        'previous_compose': str(live / 'deploy/compose.yml'),
        'previous_image_id': image_id, 'backup_image_tag': image_tag,
        'runtime_env': str(BASE / 'secrets/runtime.env'),
        'database_snapshots': databases, 'database_integrity': 'ok',
        'checksums': {str(path.relative_to(target)): digest(path) for path in target.rglob('*') if path.is_file()},
        'restore_drill': 'not_run_yet',
    }
    if run('docker', 'inspect', 'icarus-control', '--format', '{{.Image}}') != image_id or (BASE / 'current').resolve() != live:
        raise RuntimeError('The live release changed during backup; inspect before deployment.')
    (target / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
    print(json.dumps({
        'backup': str(target), 'previous_release': str(live),
        'image': image_tag, 'database_integrity': 'ok', 'status': 'backup_created',
    }))


if __name__ == '__main__':
    main()
