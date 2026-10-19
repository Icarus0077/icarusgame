// Browser integration check. Pass a Playwright module path and browser binary
// as arguments; supply the password over stdin, never as a CLI argument.
const assert = require('node:assert/strict');
const { chromium } = require(process.argv[2] || 'playwright');
const origin = 'https://gameserver.rene007.nl';

async function main() {
  const recoveryOnly = process.argv.includes('--recovery-link-only');
  let password = '';
  if (!recoveryOnly) {
    for await (const chunk of process.stdin) password += chunk;
    password = password.replace(/[\r\n]+$/, '');
    assert(password.length >= 12, 'A password must be supplied on stdin');
  }
  const browser = await chromium.launch({ headless: true, ...(process.argv[3] ? { executablePath: process.argv[3] } : {}) });
  try {
    const context = await browser.newContext();
    const page = await context.newPage();
    await page.goto(origin + '/login', { waitUntil: 'networkidle' });
    const recoveryLink = page.getByRole('link', { name: 'Wachtwoord vergeten?', exact: true });
    assert.equal(await recoveryLink.getAttribute('href'), '/password-reset');
    await recoveryLink.click();
    await page.waitForURL(origin + '/password-reset');
    await page.getByRole('textbox', { name: 'E-mailadres', exact: true }).waitFor();
    await page.getByRole('button', { name: 'Resetlink aanvragen', exact: true }).waitFor();
    console.log('PASS login recovery link opens the existing public password-reset form');
    if (recoveryOnly) return;
    await page.goto(origin + '/login', { waitUntil: 'networkidle' });
    await page.locator('#username').fill('rene');
    await page.locator('#password').fill('deliberately-invalid-test-password');
    await page.locator('#login-submit').click();
    await page.getByRole('alert').filter({ hasText: 'Gebruikersnaam of wachtwoord is onjuist.' }).waitFor();
    console.log('PASS browser displays incorrect-password alert');
    await page.locator('#password').fill(password);
    password = null;
    await Promise.all([
      page.waitForURL(origin + '/', { timeout: 20000 }),
      page.locator('#login-submit').click(),
    ]);
    await page.getByRole('button', { name: 'Uitloggen', exact: true }).waitFor();
    const cookie = (await context.cookies(origin)).find(item => item.name === '__Host-icarus_admin');
    assert(cookie && cookie.secure && cookie.httpOnly && cookie.sameSite === 'Strict');
    assert.equal((await context.request.get(origin + '/api/admin')).status(), 200);
    console.log('PASS real browser accepts session cookie and opens authenticated dashboard');
    await page.reload({ waitUntil: 'networkidle' });
    assert.equal(page.url(), origin + '/');
    await page.getByRole('button', { name: 'Uitloggen', exact: true }).waitFor();
    console.log('PASS browser refresh retains authenticated session');
    await page.getByRole('navigation', { name: 'Hoofdnavigatie' }).getByRole('button', { name: /Gebruikers/ }).click();
    await page.getByRole('button', { name: 'Speler toevoegen en uitnodigen', exact: true }).waitFor();
    await page.getByRole('heading', { name: 'Wachtwoordlink versturen', exact: true }).waitFor();
    assert.equal(await page.locator('input[type="password"]').count(), 0);
    await page.locator('select[name="role"]').selectOption('Tester');
    assert.equal(await page.locator('input[type="password"]').count(), 0);
    await page.locator('select[name="role"]').selectOption('Moderator');
    await page.getByLabel('Intern moderatorwachtwoord', { exact: true }).waitFor();
    await page.locator('select[name="role"]').selectOption('Player');
    assert.equal(await page.locator('input[type="password"]').count(), 0);
    console.log('PASS player management sends password links and never asks admins to choose player passwords');
    await Promise.all([
      page.waitForURL(origin + '/login?logged_out=1', { timeout: 20000 }),
      page.getByRole('button', { name: 'Uitloggen', exact: true }).click(),
    ]);
    assert.equal((await context.request.get(origin + '/api/admin')).status(), 401);
    console.log('PASS browser logout ends authenticated session');
  } finally {
    await browser.close();
  }
}

main().catch(error => { console.error('Browser check failed:', error.message); process.exitCode = 1; });
