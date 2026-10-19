"""Exercise Apache login without logging credentials or private API data."""
import argparse
import getpass
import http.client
import http.cookiejar
from http.cookies import SimpleCookie
import json
from pathlib import Path
import socket
import ssl
import subprocess
import time
import urllib.parse
import urllib.request

DOMAIN = 'gameserver.rene007.nl'
ORIGIN = 'https://' + DOMAIN

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=3190)
    parser.add_argument('--password-file')
    parser.add_argument('--prompt-password', action='store_true')
    parser.add_argument('--check-reload', action='store_true', help='Explicitly reload Apache and verify the existing session survives.')
    args = parser.parse_args()
    def request(path, method='GET', body=None, headers=None, cookie=None):
        connection = http.client.HTTPSConnection(DOMAIN, context=ssl.create_default_context(), timeout=30)
        connection._create_connection = lambda address, timeout, source_address: socket.create_connection(('127.0.0.1', args.port), timeout, source_address)
        sent = dict(headers or {})
        if cookie:
            sent['Cookie'] = cookie
        if isinstance(body, dict):
            body = urllib.parse.urlencode(body)
            sent['Content-Type'] = 'application/x-www-form-urlencoded'
        connection.request(method, path, body=body, headers=sent)
        response = connection.getresponse()
        result = response.status, dict((k.lower(), v) for k, v in response.getheaders()), response.read()
        connection.close()
        return result

    def check(condition, label):
        if not condition:
            raise AssertionError(label)
        print('PASS ' + label)

    status, headers, body = request('/login')
    check(status == 200 and b'name="httpd_password"' in body and b'name="httpd_username"' in body, 'public login form')
    check('www-authenticate' not in headers, 'no browser authentication challenge')
    status, headers, _ = request('/')
    check(status in (302, 303, 307) and headers.get('location') == ORIGIN + '/login', 'dashboard requires web login')
    for forged in ({}, {'X-Icarus-Admin-User': 'rene', 'X-Icarus-Admin-Email': 'admin@' + DOMAIN}, {'Oai-Authenticated-User-Id': 'rene', 'Oai-Authenticated-User-Email': 'admin@' + DOMAIN}):
        status, headers, _ = request('/api/admin', headers=forged)
        check(status == 401 and 'www-authenticate' not in headers, 'admin API rejects missing session and forged identity')
    check(request('/api/health')[0] == 200, 'health remains public')
    status, headers, _ = request('/api/game/v1/access/check', 'POST', '{}', {'Content-Type': 'application/json'})
    check(status in (400, 401, 403) and 'application/json' in headers.get('content-type', '') and 'www-authenticate' not in headers, 'game API keeps independent JSON authentication')
    for path in ('/auth/login', '/auth/logout', '/'):
        status, _, _ = request(path, 'POST', {'httpd_username': 'invalid', 'httpd_password': 'invalid'}, {'Origin': 'https://other.example'})
        check(status == 403, 'cross-site POST rejected: ' + path)
    status, headers, _ = request('/auth/login', 'POST', {'httpd_username': 'not-an-account', 'httpd_password': 'invalid'}, {'Origin': ORIGIN})
    check(status in (302, 303, 307) and headers.get('location') == ORIGIN + '/login?error=1', 'incorrect password returns login error')
    if not args.password_file and not args.prompt_password:
        return
    password = getpass.getpass('Wachtwoord voor logincontrole: ') if args.prompt_password else Path(args.password_file).read_text().strip()
    status, headers, _ = request('/auth/login', 'POST', {'httpd_username': 'rene', 'httpd_password': password, 'httpd_location': 'https://other.example/'}, {'Origin': ORIGIN})
    print('Login response:', status, headers.get('location', '(none)'))
    check(status in (302, 303, 307) and headers.get('location') == ORIGIN + '/', 'valid login succeeds and external redirect is blocked')
    raw_cookie = headers.get('set-cookie', '')
    check(all(value in raw_cookie.lower() for value in ('httponly', 'secure', 'samesite=strict')), 'secure HttpOnly SameSite session cookie')
    check(password not in raw_cookie, 'password is not exposed in cookie')
    parsed = SimpleCookie(raw_cookie)
    session = parsed.get('__Host-icarus_admin')
    check(bool(session) and session['path'] == '/' and not session['domain'] and bool(session['secure']) and bool(session['httponly']) and session['samesite'].lower() == 'strict', 'browser-valid __Host cookie attributes, without literal quotes')
    if args.port == 443:
        # Unlike manual Cookie headers, CookieJar applies cookie path/secure rules
        # and follows the same redirect chain used by the login form's fetch.
        jar = http.cookiejar.CookieJar()
        opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(jar))
        login_request = urllib.request.Request(ORIGIN + '/auth/login', data=urllib.parse.urlencode({'httpd_username': 'rene', 'httpd_password': password}).encode(), headers={'Origin': ORIGIN})
        with opener.open(login_request, timeout=20) as browser_response:
            print('Redirect diagnostic:', browser_response.status, browser_response.url, 'stored cookie attributes:', [(item.name, item.domain, item.path, item.secure) for item in jar])
            check(browser_response.status == 200 and browser_response.url == ORIGIN + '/' and b'Uitloggen' in browser_response.read(), 'cookie jar follows login redirect to authenticated dashboard')
        with opener.open(ORIGIN + '/api/admin', timeout=20) as api_response:
            check(api_response.status == 200, 'stored cookie authenticates subsequent admin API request')
        # End the additional test session; HTTP 307 cannot replay POST here.
        saved = '; '.join(f'{item.name}={item.value}' for item in jar)
        request('/auth/logout', 'POST', '', {'Origin': ORIGIN}, saved)
    cookie = '; '.join(f'{key}={value.value}' for key, value in parsed.items())
    check(bool(cookie), 'session issued')
    status, _, body = request('/', cookie=cookie)
    check(status == 200 and b'Uitloggen' in body, 'authenticated dashboard with logout button')
    status, _, body = request('/api/admin', cookie=cookie)
    check(status == 200 and bool(json.loads(body).get('actor')), 'authenticated admin API')
    if args.check_reload:
        subprocess.run(['apache2ctl', 'configtest'], check=True)
        subprocess.run(['systemctl', 'reload', 'apache2'], check=True)
        time.sleep(2)
        check(request('/api/admin', cookie=cookie)[0] == 200, 'existing session survives Apache reload')
    for _ in range(10):
        check(request('/', cookie=cookie)[0] == 200 and request('/api/admin', cookie=cookie)[0] == 200, 'session remains valid across repeated independent connections')
    actor = json.loads(body)['actor']
    status, _, body = request('/api/admin', cookie=cookie, headers={'Oai-Authenticated-User-Id': 'forged', 'Oai-Authenticated-User-Email': 'forged@example.com'})
    check(status == 200 and json.loads(body)['actor'] == actor, 'proxy strips forged legacy identity')
    status, _, _ = request('/api/admin', 'POST', '{}', {'Content-Type': 'application/json', 'Origin': 'https://other.example'}, cookie)
    check(status == 403, 'authenticated cross-site mutation rejected')
    status, _, _ = request('/auth/logout', cookie=cookie)
    check(status == 403, 'GET cannot log out a session')
    status, headers, _ = request('/auth/logout', 'POST', '', {'Origin': ORIGIN}, cookie)
    check(status in (302, 303, 307) and headers.get('location') == ORIGIN + '/login?logged_out=1', 'logout redirects correctly')
    logout_cookie = SimpleCookie(headers.get('set-cookie', ''))
    cleared = '; '.join(f'{key}={value.value}' for key, value in logout_cookie.items())
    check(bool(logout_cookie), 'logout clears browser session cookie')
    check(request('/api/admin', cookie=cleared)[0] == 401, 'logged-out browser cannot access admin API')

if __name__ == '__main__':
    main()
