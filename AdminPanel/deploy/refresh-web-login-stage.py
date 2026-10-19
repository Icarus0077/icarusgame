from pathlib import Path
import subprocess

bundle = Path(__file__).resolve().parent
stage = Path('/opt/icarus-control/web-login-check')
config = (bundle / 'apache-web-auth.conf').read_text()
config = config.replace('127.0.0.1:3100', '127.0.0.1:3101')
config = config.replace('/etc/apache2/icarus-control.htpasswd', str(stage / 'users.htpasswd'))
config = config.replace('/opt/icarus-control/secrets/admin-session.key', str(stage / 'session.key'))
config = config.replace('/opt/icarus-control/auth', str(stage / 'auth'))
(stage / 'auth.conf').write_text(config)
subprocess.run(['apache2ctl', 'configtest'], check=True)
subprocess.run(['systemctl', 'reload', 'apache2'], check=True)
