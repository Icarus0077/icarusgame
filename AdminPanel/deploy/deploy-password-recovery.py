"""Prepare/activate the tested password-recovery update on this recorded baseline.

No mailbox password is accepted on the command line or stored in this bundle.
"""
from datetime import datetime, timezone
from pathlib import Path
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
EXPECTED = {
    'package.json': '3c9d54d38ac589f989acbcc613c17fb2debba3a31aa8c0cf566a2ac7da20b74d',
    'pnpm-lock.yaml': '8e35302179e51783a8d581a6ef8ca401791a7c62c3bef5b9a85f0fcd24bafaa8',
    'app/api/_shared/credentials.ts': '802494753cfb2b82d797b355fec1256e0566762609f9e365195d66ebc759f8fd',
    'db/schema.ts': '4183806a1fec9fc71ff7b09b868c951634aa1ff61c4b68d68f31d7127abb97aa',
    'cloudflare-env.d.ts': 'd3662fc00a8e0bd0bb86fb3a790019167791cf74443ce48442aa43171828a4de',
    'drizzle/meta/_journal.json': '3112b3601cbd62807408901f10ff1e2dbcb8210919c276b774c0175a59ab2793',
}


def run(*args):
    subprocess.run(args, check=True)


def capture(*args):
    return subprocess.check_output(args, text=True).strip()


def pin_image(compose, image):
    source = compose.read_text()
    if source.count('    container_name: icarus-control') != 1:
        raise RuntimeError('Unexpected Compose service; refusing a broad rewrite.')
    source, count = re.subn(r'^    image:.*$', '    image: ' + image, source, count=1, flags=re.M)
    if count != 1:
        raise RuntimeError('Expected one pinned image in Compose.')
    compose.write_text(source)


def http_status(url, payload=None):
    request = urllib.request.Request(url, data=None if payload is None else json.dumps(payload).encode(),
                                     headers={} if payload is None else {'Content-Type': 'application/json'})
    class NoRedirect(urllib.request.HTTPRedirectHandler):
        def redirect_request(self, request, fp, code, message, headers, new_url):
            return None
    try:
        with urllib.request.build_opener(NoRedirect()).open(request, timeout=5) as response:
            return response.status
    except urllib.error.HTTPError as response:
        return response.code


def wait_healthy(port):
    for attempt in range(60):
        try:
            if http_status(f'http://127.0.0.1:{port}/api/health') == 200:
                return
        except (OSError, urllib.error.URLError):
            pass
        time.sleep(1)
    raise RuntimeError('Container failed health check on port ' + str(port))


def credential_fingerprint(data):
    digests = {}
    for path in data.rglob('*.sqlite'):
        with sqlite3.connect(path.as_uri() + '?mode=ro', uri=True) as db:
            if db.execute("SELECT 1 FROM sqlite_master WHERE type='table' AND name='game_user_credentials'").fetchone():
                rows = db.execute('SELECT user_id, salt, hash, iterations FROM game_user_credentials ORDER BY user_id').fetchall()
                digests[str(path.relative_to(data))] = hashlib.sha256(json.dumps(rows).encode()).hexdigest()
    return digests


def prepare(backup, manifest):
    os.umask(0o077)
    live = (BASE / 'current').resolve(strict=True)
    if str(live) != manifest['previous_release']:
        raise RuntimeError('The live release changed since the backup.')
    for relative, expected in EXPECTED.items():
        if hashlib.sha256((live / relative).read_bytes()).hexdigest() != expected:
            raise RuntimeError('Live source changed; inspect before overlaying: ' + relative)

    stamp = datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%SZ')
    release = BASE / 'releases' / ('password-reset-' + stamp)
    stage = BASE / ('password-reset-check-' + stamp)
    if stage.exists() or release.exists():
        raise RuntimeError('Deployment candidate already exists.')
    stage.mkdir(mode=0o700)
    shutil.copytree(live, release, ignore=shutil.ignore_patterns('node_modules', '.git', 'dist', '.wrangler'))
    with tarfile.open(BUNDLE / 'password-release.tar.gz') as archive:
        for member in archive.getmembers():
            if not member.isfile() and not member.isdir():
                raise RuntimeError('Unexpected link/device in source bundle.')
            path = (release / member.name).resolve()
            if not path.is_relative_to(release):
                raise RuntimeError('Source bundle escapes the candidate release.')
            if member.isfile() and path.exists() and member.name not in EXPECTED:
                raise RuntimeError('Refusing to replace unreviewed live source: ' + member.name)
        archive.extractall(release, filter='data')
    shutil.copy2(BUNDLE / 'HERSTELPLAN.md', release / 'deploy/GAMESERVER_BACKUP_EN_HERSTEL.md')
    image = 'icarus-control:password-reset-' + stamp.lower()
    pin_image(release / 'deploy/compose.yml', image)

    # An actual isolated restore drill: archived image + copied saved databases
    # + original env. It does not stop or roll back the live container.
    shutil.copytree(backup / 'data', stage / 'restored-data')
    with tarfile.open(backup / 'release.tar.gz') as archive:
        archive.extractall(stage / 'restored-source', filter='data')
    with tarfile.open(backup / 'private-config.tar.gz') as archive:
        source = archive.extractfile('opt/icarus-control/secrets/runtime.env')
        if source is None:
            raise RuntimeError('Backup lacks runtime configuration.')
        (stage / 'runtime.env').write_bytes(source.read())
    run('docker', 'image', 'load', '-i', str(backup / 'image.tar'))
    restore_name = 'icarus-restore-check-' + stamp.lower()
    run('docker', 'run', '-d', '--name', restore_name, '-p', '127.0.0.1:3103:3000',
        '-v', f'{stage}/restored-data:/data', '-v', f'{stage}/runtime.env:/run/secrets/icarus.env:ro', manifest['backup_image_tag'])
    try:
        wait_healthy(3103)
        if http_status('http://127.0.0.1:3103/password-reset') == 200:
            raise RuntimeError('Backup unexpectedly contains the new reset page.')
    finally:
        run('docker', 'rm', '-f', restore_name)
    manifest['restore_drill'] = {'status': 'passed', 'utc': stamp, 'method': 'archived image, extracted source, copied SQLite snapshots, isolated loopback container health 200'}
    (backup / 'manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')

    run('docker', 'build', '-t', image, str(release))
    shutil.copytree(backup / 'data', stage / 'candidate-data')
    before = credential_fingerprint(stage / 'candidate-data')
    candidate_name = 'icarus-password-check-' + stamp.lower()
    run('docker', 'run', '-d', '--name', candidate_name, '-p', '127.0.0.1:3102:3000',
        '-v', f'{stage}/candidate-data:/data', '-v', f'{stage}/runtime.env:/run/secrets/icarus.env:ro', image)
    wait_healthy(3102)
    if http_status('http://127.0.0.1:3102/password-reset') != 200:
        raise RuntimeError('Candidate reset page is missing.')
    if http_status('http://127.0.0.1:3102/api/game/v1/password-reset/request', {'email': 'nobody@example.invalid'}) != 503:
        raise RuntimeError('Unconfigured SMTP must fail closed with HTTP 503.')
    if http_status('http://127.0.0.1:3102/api/game/v1/password-reset/confirm', {'token': 'invalid', 'password': 'example-player-password', 'confirmation': 'example-player-password'}) != 400:
        raise RuntimeError('Invalid reset tokens must be rejected.')
    if credential_fingerprint(stage / 'candidate-data') != before:
        raise RuntimeError('A stage check changed existing account credentials.')
    state = {'backup': str(backup), 'previous_release': str(live), 'release': str(release),
             'image': image, 'candidate_container': candidate_name, 'stage': str(stage),
             'admin_password_sha256': hashlib.sha256(Path('/etc/apache2/icarus-control.htpasswd').read_bytes()).hexdigest()}
    (BUNDLE / 'deployment-state.json').write_text(json.dumps(state, indent=2))
    print(json.dumps({'status': 'prepared_and_tested', 'release': str(release), 'restore_drill': 'passed', 'stage_checks': 'passed'}))


def activate(manifest):
    state = json.loads((BUNDLE / 'deployment-state.json').read_text())
    if str((BASE / 'current').resolve()) != state['previous_release']:
        raise RuntimeError('Production changed while the candidate was being tested.')
    if capture('docker', 'inspect', 'icarus-control', '--format', '{{.Image}}') != manifest['previous_image_id']:
        raise RuntimeError('Production image changed; refresh the backup.')
    password_file = Path('/etc/apache2/icarus-control.htpasswd')
    if hashlib.sha256(password_file.read_bytes()).hexdigest() != state['admin_password_sha256']:
        raise RuntimeError('Admin authentication changed during deployment.')
    apache_source = Path('/etc/apache2/icarus-control-web-auth.conf').read_text()
    if not any(pattern in apache_source for pattern in (r'password-reset\.html', r'password-reset(?:\.html)')):
        raise RuntimeError('Expected public reset-page exception is missing from Apache. No broad config overwrite performed.')

    # Add only missing settings. The mailbox password is intentionally empty;
    # its owner will enter the rotated secret privately at this exact path.
    env_path = BASE / 'secrets/runtime.env'
    old_env = env_path.read_text()
    keys = {line.split('=', 1)[0].strip() for line in old_env.splitlines() if '=' in line and not line.lstrip().startswith('#')}
    defaults = {'SMTP_HOST': 'pro2.mail.ovh.net', 'SMTP_PORT': '587', 'SMTP_USER': 'info@rene007.nl', 'SMTP_FROM': 'info@rene007.nl', 'SMTP_PASSWORD': ''}
    with env_path.open('a') as output:
        if not old_env.endswith('\n'):
            output.write('\n')
        for key, value in defaults.items():
            if key not in keys:
                output.write(key + '=' + value + '\n')
    env_path.chmod(0o600)
    release = Path(state['release'])
    compose = release / 'deploy/compose.yml'
    try:
        run('apache2ctl', 'configtest')
        run('docker', 'compose', '-p', manifest['compose_project'], '-f', str(compose), 'up', '-d', '--no-build')
        wait_healthy(3100)
        for path, expected in [('/api/health', 200), ('/login', 200), ('/password-reset', 200), ('/api/admin', 401)]:
            if http_status('https://gameserver.rene007.nl' + path) != expected:
                raise RuntimeError('Production smoke check failed: ' + path)
        if http_status('https://gameserver.rene007.nl/api/game/v1/password-reset/request', {'email': 'nobody@example.invalid'}) != 503:
            raise RuntimeError('Production must not claim mail delivery before a secret is configured.')
        if hashlib.sha256(password_file.read_bytes()).hexdigest() != state['admin_password_sha256']:
            raise RuntimeError('Admin password file changed unexpectedly.')
        link = BASE / ('current-password-' + datetime.now(timezone.utc).strftime('%H%M%S'))
        link.symlink_to(release)
        os.replace(link, BASE / 'current')
    except Exception:
        run('docker', 'compose', '-p', manifest['compose_project'], '-f', manifest['previous_compose'], 'up', '-d', '--no-build')
        raise
    run('docker', 'rm', '-f', state['candidate_container'])
    print(json.dumps({'status': 'live', 'release': str(release), 'backup': state['backup'], 'mailbox_secret': 'awaiting_private_entry', 'admin_authentication': 'unchanged'}))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--backup', required=True)
    parser.add_argument('--activate', action='store_true')
    args = parser.parse_args()
    backup = Path(args.backup).resolve(strict=True)
    if not backup.is_relative_to(BASE / 'backups'):
        raise RuntimeError('Unexpected backup location.')
    manifest = json.loads((backup / 'manifest.json').read_text())
    if args.activate:
        activate(manifest)
    else:
        prepare(backup, manifest)
