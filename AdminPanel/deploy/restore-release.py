"""Restore software from a verified backup, preserving live player data/secrets.

Usage (root): python3 restore-release.py --backup /opt/icarus-control/backups/...
Database disaster recovery is deliberately a separate, documented operation.
"""
from datetime import datetime, timezone
from pathlib import Path
import argparse
import hashlib
import json
import os
import re
import shutil
import subprocess
import tarfile
import tempfile
import time
import urllib.request

BASE = Path('/opt/icarus-control')


def digest(path):
    value = hashlib.sha256()
    with path.open('rb') as source:
        for block in iter(lambda: source.read(1024 * 1024), b''):
            value.update(block)
    return value.hexdigest()


def run(*args):
    subprocess.run(args, check=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--backup', required=True)
    args = parser.parse_args()
    os.umask(0o077)
    backup = Path(args.backup).resolve(strict=True)
    if not backup.is_relative_to(BASE / 'backups'):
        raise RuntimeError('Backup must be inside the private Icarus backups directory.')
    manifest = json.loads((backup / 'manifest.json').read_text())
    for relative, expected in manifest['checksums'].items():
        path = (backup / relative).resolve(strict=True)
        if not path.is_relative_to(backup) or digest(path) != expected:
            raise RuntimeError('Backup checksum failed: ' + relative)
    if not (BASE / 'current').is_symlink():
        raise RuntimeError('The current release must be the expected symlink.')

    stamp = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ')
    destination = BASE / 'releases' / ('restored-' + stamp)
    destination.mkdir(mode=0o755)
    with tarfile.open(backup / 'release.tar.gz') as archive:
        archive.extractall(destination, filter='data')
    release = destination / 'release'
    compose = release / 'deploy/compose.yml'
    source = compose.read_text()
    image_line = '    image: ' + manifest['backup_image_tag']
    if re.search(r'^    image:.*$', source, re.M):
        source = re.sub(r'^    image:.*$', image_line, source, count=1, flags=re.M)
    else:
        source = source.replace('    container_name:', image_line + '\n    container_name:', 1)
    compose.write_text(source)
    run('docker', 'image', 'load', '-i', str(backup / 'image.tar'))
    restored_image = subprocess.check_output(['docker', 'image', 'inspect', manifest['backup_image_tag'], '--format', '{{.Id}}'], text=True).strip()
    if restored_image != manifest['previous_image_id']:
        raise RuntimeError('Restored image does not match the recorded working image.')

    previous_release = (BASE / 'current').resolve()
    work = Path(tempfile.mkdtemp(prefix='icarus-restore-config-'))
    with tarfile.open(backup / 'private-config.tar.gz') as archive:
        archive.extractall(work / 'snapshot', filter='data')
    config_paths = [
        Path('/etc/apache2/icarus-control-web-auth.conf'),
        Path('/etc/apache2/sites-available/gameserver.rene007.nl.conf'),
        Path('/etc/apache2/sites-available/gameserver.rene007.nl-le-ssl.conf'),
        BASE / 'auth/admin-login.html',
    ]
    # Do not restore runtime.env, htpasswd or the session key by default.
    # Preserve intentional password changes and keep player data intact.
    for index, path in enumerate(config_paths):
        if path.exists():
            shutil.copy2(path, work / ('previous-' + str(index)))
        saved = work / 'snapshot' / str(path).lstrip('/')
        if saved.exists():
            shutil.copy2(saved, path)
    try:
        run('apache2ctl', 'configtest')
        run('docker', 'compose', '-p', manifest['compose_project'], '-f', str(compose), 'up', '-d', '--no-build', '--force-recreate')
        for attempt in range(60):
            try:
                with urllib.request.urlopen('http://127.0.0.1:3100/api/health', timeout=2) as response:
                    if response.status == 200:
                        break
            except Exception:
                pass
            time.sleep(1)
        else:
            raise RuntimeError('Restored application did not become healthy.')
        run('systemctl', 'reload', 'apache2')
        link = BASE / ('current-restored-' + stamp)
        link.symlink_to(release)
        os.replace(link, BASE / 'current')
    except Exception:
        for index, path in enumerate(config_paths):
            saved = work / ('previous-' + str(index))
            if saved.exists():
                shutil.copy2(saved, path)
        run('docker', 'compose', '-p', manifest['compose_project'], '-f', str(previous_release / 'deploy/compose.yml'), 'up', '-d', '--no-build')
        raise
    print(json.dumps({'status': 'software_restored', 'release': str(release), 'database': 'preserved', 'password_files': 'preserved'}))


if __name__ == '__main__':
    main()
