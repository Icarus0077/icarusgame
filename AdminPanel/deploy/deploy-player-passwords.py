"""Deploy the narrow self-chosen-player-password update from a fresh backup."""
from pathlib import Path
from datetime import datetime, timezone
import argparse
import hashlib
import json
import os
import re
import shutil
import sqlite3
import subprocess
import tarfile
import time
import urllib.error
import urllib.request

BASE = Path('/opt/icarus-control')
BUNDLE = Path(__file__).resolve().parent
arguments = argparse.ArgumentParser()
arguments.add_argument('--backup', required=True)
args = arguments.parse_args()
BACKUP = Path(args.backup).resolve(strict=True)
if not BACKUP.is_relative_to(BASE / 'backups'):
    raise RuntimeError('Backup must be inside the private backup directory')
STAMP = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ')
RELEASE = BASE / ('releases/player-passwords-' + STAMP)
STAGE = BASE / ('player-password-check-' + STAMP)
ALLOWED = {
    'Dockerfile',
    'app/api/_shared/password-reset.ts', 'app/api/_shared/reset-mail.ts',
    'app/api/_shared/credentials.ts', 'app/api/admin/route.ts',
    'app/api/game/v1/access/check/route.ts', 'app/AdminConsole.tsx',
    'public/password-reset.html',
}

def run(*args):
    subprocess.run(args, check=True)

def fingerprint(data):
    records = []
    for path in data.rglob('*.sqlite'):
        with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
            if db.execute("SELECT 1 FROM sqlite_master WHERE name='game_user_credentials'").fetchone():
                records += db.execute('SELECT user_id, salt, hash, iterations FROM game_user_credentials ORDER BY user_id').fetchall()
    return hashlib.sha256(json.dumps(records).encode()).digest()

def request(port, path, payload=None):
    url = f'http://127.0.0.1:{port}{path}'
    req = urllib.request.Request(url, data=None if payload is None else json.dumps(payload).encode(), headers={'Content-Type': 'application/json'})
    try:
        with urllib.request.urlopen(req, timeout=10) as r:
            return r.status, r.read()
    except urllib.error.HTTPError as r:
        return r.code, r.read()

def healthy(port):
    for _ in range(30):
        try:
            if request(port, '/api/health')[0] == 200:
                return
        except Exception:
            pass
        time.sleep(1)
    raise RuntimeError('Container failed health check')

def main():
    os.umask(0o077)
    manifest = json.loads((BACKUP / 'manifest.json').read_text())
    live = (BASE / 'current').resolve(strict=True)
    if str(live) != manifest['previous_release'] or RELEASE.exists() or STAGE.exists():
        raise RuntimeError('Live baseline changed or candidate already exists')
    shutil.copytree(live, RELEASE)
    with tarfile.open(BUNDLE / 'player-passwords.tar.gz') as archive:
        for item in archive.getmembers():
            if not item.isfile() or item.name not in ALLOWED:
                raise RuntimeError('Unexpected source bundle member')
        archive.extractall(RELEASE, filter='data')
    image = 'icarus-control:player-passwords-' + STAMP.lower()
    compose = RELEASE / 'deploy/compose.yml'
    source, count = re.subn(r'^    image:.*$', '    image: ' + image, compose.read_text(), count=1, flags=re.M)
    if count != 1:
        raise RuntimeError('Expected one explicitly pinned image')
    compose.write_text(source)
    run('docker', 'build', '-t', image, str(RELEASE))
    STAGE.mkdir(mode=0o700)
    shutil.copytree(BACKUP / 'data', STAGE / 'data')
    before = fingerprint(STAGE / 'data')
    candidate = 'icarus-player-password-check'
    run('docker', 'run', '-d', '--name', candidate, '-p', '127.0.0.1:3102:3000',
        '-v', f'{STAGE}/data:/data', '-v', f'{BASE}/secrets/runtime.env:/run/secrets/icarus.env:ro', image)
    try:
        healthy(3102)
        code, body = request(3102, '/password-reset')
        assert code == 200 and b'Kies je eigen wachtwoord' in body
        assert request(3102, '/api/admin')[0] == 200
        assert request(3102, '/api/game/v1/password-reset/confirm', {'token': 'invalid', 'password': 'example-password', 'confirmation': 'example-password'})[0] == 400
        assert request(3102, '/api/game/v1/access/check', {'user': 'nobody@example.invalid', 'password': 'example-password'})[0] == 401
        assert fingerprint(STAGE / 'data') == before
        print('PASS candidate: reset form, admin data, invalid links, password authentication; credentials unchanged')
        if (BASE / 'current').resolve() != live:
            raise RuntimeError('Production changed during preparation')
        production_before = fingerprint(Path('/var/lib/icarus-control'))
        try:
            run('docker', 'compose', '-p', manifest['compose_project'], '-f', str(compose), 'up', '-d', '--no-build')
            healthy(3100)
            assert request(3100, '/password-reset')[0] == 200
            assert fingerprint(Path('/var/lib/icarus-control')) == production_before
            next_link = BASE / 'current-player-passwords'
            next_link.symlink_to(RELEASE)
            os.replace(next_link, BASE / 'current')
        except Exception:
            run('docker', 'compose', '-p', manifest['compose_project'], '-f', manifest['previous_compose'], 'up', '-d', '--no-build')
            raise
        (BUNDLE / 'player-passwords-deployment.json').write_text(json.dumps({'release': str(RELEASE), 'backup': str(BACKUP), 'status': 'live'}, indent=2))
        print('LIVE: self-chosen player passwords. No player passwords changed during deployment.')
    finally:
        run('docker', 'rm', '-f', candidate)

if __name__ == '__main__':
    main()
