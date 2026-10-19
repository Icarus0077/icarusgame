"""Prepare an isolated VPS login candidate without changing the public vhost.

Run as root from the uploaded deployment bundle. Uses the current live sources,
so unrelated local changes cannot accidentally be published.
"""
from pathlib import Path
import hashlib
import json
import os
import secrets
import shutil
import sqlite3
import subprocess
import sys

BUNDLE = Path(__file__).resolve().parent
BASE = Path('/opt/icarus-control')
RELEASE = BASE / 'releases/web-login-20261016'
STAGE = BASE / 'web-login-check'
IMAGE = 'icarus-control:web-login-20261016'

def run(*args, **kwargs):
    return subprocess.run(args, check=True, **kwargs)

def replace_once(text, before, after):
    if text.count(before) != 1:
        raise RuntimeError('Live source differs; refusing an ambiguous patch: ' + before[:70])
    return text.replace(before, after, 1)

def main():
    if RELEASE.exists() or STAGE.exists():
        raise RuntimeError('Candidate already exists; inspect it before continuing.')
    live = (BASE / 'current').resolve(strict=True)
    shutil.copytree(live, RELEASE)
    STAGE.mkdir(mode=0o755)
    # An online SQLite backup includes WAL changes without stopping production.
    data = Path('/var/lib/icarus-control')
    shutil.copytree(data, STAGE / 'data')
    for original in data.rglob('*.sqlite'):
        target = STAGE / 'data' / original.relative_to(data)
        for suffix in ('-wal', '-shm'):
            target.with_name(target.name + suffix).unlink(missing_ok=True)
        with sqlite3.connect(original) as source, sqlite3.connect(target) as destination:
            source.backup(destination)
    (STAGE / 'previous.json').write_text(json.dumps({
        'release': str(live),
        'image': subprocess.check_output(['docker', 'inspect', 'icarus-control', '--format', '{{.Image}}'], text=True).strip(),
        'passwordFileSha256': hashlib.sha256(Path('/etc/apache2/icarus-control.htpasswd').read_bytes()).hexdigest(),
    }))
    for name in ('gameserver.rene007.nl.conf', 'gameserver.rene007.nl-le-ssl.conf'):
        shutil.copy2(Path('/etc/apache2/sites-available') / name, STAGE / name)

    source_path = RELEASE / 'app/AdminConsole.tsx'
    source = source_path.read_text()
    source = replace_once(source, 'export function AdminConsole', '''function redirectIfSessionExpired(response: Response) {
  if (response.status !== 401) return false;
  window.location.assign("/login?expired=1");
  return true;
}

export function AdminConsole''')
    source = replace_once(source, '      const response = await fetch("/api/admin", { cache: "no-store" });', '      const response = await fetch("/api/admin", { cache: "no-store" });\n      if (redirectIfSessionExpired(response)) return;')
    source = replace_once(source, '      .then(async (response) => {', '      .then(async (response) => {\n        if (redirectIfSessionExpired(response)) return;')
    source = replace_once(source, 'if (!cancelled) setData(payload);', 'if (!cancelled && payload) setData(payload);')
    source = replace_once(source, '      const payload = (await response.json()) as { error?: string };', '      if (redirectIfSessionExpired(response)) return;\n      const payload = (await response.json()) as { error?: string };')
    source = replace_once(source, '  async function loadData() {', '''  async function logout(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    try {
      const response = await fetch("/auth/logout", { method: "POST", redirect: "manual" });
      if (response.type !== "opaqueredirect" && !response.ok) throw new Error("Uitloggen is niet gelukt. Probeer het opnieuw.");
      window.location.replace("/login?logged_out=1");
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Uitloggen is niet gelukt.");
      setBusy(false);
    }
  }

  async function loadData() {''')
    source = replace_once(source, '          </div>\n        </header>', '''            <form action="/auth/logout" method="post" onSubmit={logout}>
              <button className="tableAction" style={{ minHeight: 44, padding: "0 12px", fontSize: 11 }} disabled={busy} type="submit">Uitloggen</button>
            </form>
          </div>
        </header>''')
    source_path.write_text(source)
    shutil.copy2(BUNDLE / 'admin-login.html', RELEASE / 'public/admin-login.html')
    for name in ('apache-gameserver.conf', 'apache-gameserver-ssl.conf', 'apache-web-auth.conf'):
        shutil.copy2(BUNDLE / name, RELEASE / 'deploy' / name)
    compose = RELEASE / 'deploy/compose.yml'
    compose.write_text(compose.read_text().replace('    container_name:', f'    image: {IMAGE}\n    container_name:', 1))
    run('docker', 'build', '-t', IMAGE, str(RELEASE))
    run('docker', 'run', '-d', '--name', 'icarus-control-web-login-check',
        '-p', '127.0.0.1:3101:3000', '-v', f'{STAGE}/data:/data',
        '-v', f'{BASE}/secrets/runtime.env:/run/secrets/icarus.env:ro', IMAGE)

    password = secrets.token_urlsafe(32)
    password_path = STAGE / 'password'
    password_path.write_text(password)
    password_path.chmod(0o600)
    run('htpasswd', '-ciB', str(STAGE / 'users.htpasswd'), 'rene', input=password + '\n', text=True, stdout=subprocess.DEVNULL)
    os.chown(STAGE / 'users.htpasswd', 0, 33)
    (STAGE / 'users.htpasswd').chmod(0o640)
    key = STAGE / 'session.key'
    key.write_text(secrets.token_hex(48) + '\n')
    key.chmod(0o600)
    config = (BUNDLE / 'apache-web-auth.conf').read_text()
    config = config.replace('127.0.0.1:3100', '127.0.0.1:3101')
    config = config.replace('/etc/apache2/icarus-control.htpasswd', str(STAGE / 'users.htpasswd'))
    config = config.replace('/opt/icarus-control/secrets/admin-session.key', str(key))
    config = config.replace('/opt/icarus-control/auth', str(STAGE / 'auth'))
    (STAGE / 'auth').mkdir()
    shutil.copy2(BUNDLE / 'admin-login.html', STAGE / 'auth/admin-login.html')
    (STAGE / 'auth.conf').write_text(config)
    vhost = (BUNDLE / 'apache-gameserver-ssl.conf').read_text()
    vhost = vhost.replace('*:443', '127.0.0.1:3190')
    vhost = vhost.replace('/etc/apache2/icarus-control-web-auth.conf', str(STAGE / 'auth.conf'))
    stage_vhost = Path('/etc/apache2/sites-available/icarus-web-login-check.conf')
    stage_vhost.write_text('Listen 127.0.0.1:3190\n' + vhost)
    run('a2enmod', 'request', 'auth_form', 'session', 'session_cookie', 'session_crypto')
    run('a2ensite', 'icarus-web-login-check')
    try:
        run('apache2ctl', 'configtest')
        run('systemctl', 'reload', 'apache2')
    except Exception:
        run('a2dissite', 'icarus-web-login-check')
        raise
    print('Candidate ready on loopback TLS port 3190; public login unchanged.')

if __name__ == '__main__':
    main()
