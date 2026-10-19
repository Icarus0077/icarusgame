"""Promote the tested login candidate, retaining the previous release."""
from pathlib import Path
import hashlib
import json
import os
import secrets
import shutil
import subprocess
import time
import urllib.request

BASE = Path('/opt/icarus-control')
BUNDLE = Path(__file__).resolve().parent
STAGE = BASE / 'web-login-check'
RELEASE = BASE / 'releases/web-login-20261016'
SITES = Path('/etc/apache2/sites-available')

def run(*args):
    subprocess.run(args, check=True)

def healthy():
    for _ in range(30):
        try:
            with urllib.request.urlopen('http://127.0.0.1:3100/api/health', timeout=2) as response:
                if response.status == 200:
                    return
        except Exception:
            pass
        time.sleep(1)
    raise RuntimeError('Production container failed health check')

def main():
    previous = json.loads((STAGE / 'previous.json').read_text())
    assert str((BASE / 'current').resolve()) == previous['release'], 'Live release changed during test'
    assert hashlib.sha256(Path('/etc/apache2/icarus-control.htpasswd').read_bytes()).hexdigest() == previous['passwordFileSha256'], 'Account configuration changed during test'
    run('python3', str(BUNDLE / 'apache-auth-smoke.py'), '--password-file', str(STAGE / 'password'))
    identity = json.loads(subprocess.check_output(['docker', 'inspect', 'icarus-control'], text=True))[0]
    project = identity['Config']['Labels']['com.docker.compose.project']
    key = BASE / 'secrets/admin-session.key'
    if not key.exists():
        with key.open('x') as output:
            os.chmod(key, 0o600)
            output.write(secrets.token_hex(48) + '\n')
    (BASE / 'auth').mkdir(exist_ok=True)
    shutil.copy2(BUNDLE / 'admin-login.html', BASE / 'auth/admin-login.html')
    shutil.copy2(BUNDLE / 'apache-web-auth.conf', '/etc/apache2/icarus-control-web-auth.conf')
    shutil.copy2(BUNDLE / 'apache-web-auth.conf', RELEASE / 'deploy/apache-web-auth.conf')
    try:
        run('docker', 'compose', '-p', project, '-f', str(RELEASE / 'deploy/compose.yml'), 'up', '-d', '--no-build')
        healthy()
        shutil.copy2(BUNDLE / 'apache-gameserver.conf', SITES / 'gameserver.rene007.nl.conf')
        shutil.copy2(BUNDLE / 'apache-gameserver-ssl.conf', SITES / 'gameserver.rene007.nl-le-ssl.conf')
        run('apache2ctl', 'configtest')
        run('systemctl', 'reload', 'apache2')
        time.sleep(1)
        run('systemctl', 'is-active', '--quiet', 'apache2')
        run('python3', str(BUNDLE / 'apache-auth-smoke.py'), '--port', '443')
        next_link = BASE / 'current-web-login'
        next_link.symlink_to(RELEASE)
        os.replace(next_link, BASE / 'current')
    except Exception:
        for name in ('gameserver.rene007.nl.conf', 'gameserver.rene007.nl-le-ssl.conf'):
            shutil.copy2(STAGE / name, SITES / name)
        run('docker', 'compose', '-p', project, '-f', previous['release'] + '/deploy/compose.yml', 'up', '-d', '--no-build')
        run('apache2ctl', 'configtest')
        run('systemctl', 'restart', 'apache2')
        raise
    assert hashlib.sha256(Path('/etc/apache2/icarus-control.htpasswd').read_bytes()).hexdigest() == previous['passwordFileSha256']
    run('a2dissite', 'icarus-web-login-check')
    run('apache2ctl', 'configtest')
    run('systemctl', 'reload', 'apache2')
    run('docker', 'rm', '-f', 'icarus-control-web-login-check')
    for name in ('password', 'users.htpasswd', 'session.key'):
        (STAGE / name).unlink()
    (SITES / 'icarus-web-login-check.conf').unlink()
    STAGE.chmod(0o700)
    print('LIVE: https://gameserver.rene007.nl/login')
    print('Original account password unchanged. Previous release and database snapshot retained.')

if __name__ == '__main__':
    main()
