"""Read-only account diagnosis; never prints password hashes or secrets."""
from pathlib import Path
import hashlib
import http.client
import json
import socket
import sqlite3
import ssl
import urllib.parse

base = Path('/opt/icarus-control')
users_file = Path('/etc/apache2/icarus-control.htpasswd')
names = [line.split(':', 1)[0] for line in users_file.read_text().splitlines()]
result = {'adminUsernameReneExists': 'rene' in names}
previous = json.loads((base / 'web-login-check/previous.json').read_text())
result['adminPasswordFileUnchangedSinceWebLoginMigration'] = hashlib.sha256(users_file.read_bytes()).hexdigest() == previous['passwordFileSha256']
for db_path in Path('/var/lib/icarus-control/v3/d1').rglob('*.sqlite'):
    if db_path.name == 'metadata.sqlite':
        continue
    connection = sqlite3.connect(f'file:{db_path}?mode=ro', uri=True)
    try:
        result['clientCanFindExactLoginRene'] = bool(connection.execute(
            'SELECT EXISTS(SELECT 1 FROM game_users WHERE lower(id)=? OR lower(email)=?)',
            ('rene', 'rene')
        ).fetchone()[0])
    finally:
        connection.close()

domain = 'gameserver.rene007.nl'
connection = http.client.HTTPSConnection(domain, context=ssl.create_default_context(), timeout=15)
connection._create_connection = lambda address, timeout, source_address: socket.create_connection(('127.0.0.1', 443), timeout, source_address)
connection.request('POST', '/auth/login', body=urllib.parse.urlencode({
    'httpd_username': 'rene', 'httpd_password': 'deliberately-incorrect-test-password'
}), headers={'Origin': f'https://{domain}', 'Content-Type': 'application/x-www-form-urlencoded'})
response = connection.getresponse()
result['invalidPasswordRedirectsToError'] = response.status == 302 and response.getheader('Location') == f'https://{domain}/login?error=1'
response.read()
connection.close()
print(json.dumps(result, indent=2))
