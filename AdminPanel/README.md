# Icarus Control

Beveiligde operations-console voor de Icarus-gameserver. De app beheert gamegebruikers, IP/CIDR-toegang, kaartbeschikbaarheid, releases en een controleerbaar auditlog.

## Ontwikkeling

Deze map wordt samen met de Unity-client opgeslagen in de GitHub-repository
`Icarus0077/icarusgame`. Voer commits en pushes uit vanuit de projecthoofdmap.
De oorspronkelijke losse Git-geschiedenis is lokaal behouden als
`AdminPanel/.git-standalone-backup-20261019/`; deze back-up, lokale databases,
omgevingsbestanden en wachtwoorden worden niet gepubliceerd of in Docker gekopieerd.
`AdminPanel/build/sites-vite-plugin.ts` is benodigde broncode en hoort wel bij
de repository, ondanks de mapnaam `build`.

Vereist Node.js 22 of nieuwer en pnpm.

```bash
pnpm install
pnpm dev
```

## Kwaliteitscontrole

```bash
pnpm lint
pnpm exec tsc --noEmit
pnpm test
```

De gestructureerde beheerdata staat in Cloudflare D1/SQLite. Authenticatie komt van de private Sites-omgeving of van de vertrouwde Apache-authenticatieproxy; autorisatie wordt voor iedere beheeractie opnieuw aan de serverzijde gecontroleerd.

## Beheerderslogin op de VPS

De productieomgeving gebruikt sinds 16 oktober 2026 de web-login op `https://gameserver.rene007.nl/login`. Het bestaande Apache-beheeraccount blijft geldig. Apache `mod_auth_form` controleert het bestaande wachtwoordbestand en bewaart de sessie in een versleutelde, Secure/HttpOnly/SameSite-cookie; na 30 minuten inactiviteit verloopt de sessie. De console bevat een uitlogknop. De game-API behoudt zijn eigen authenticatie.

De configuratie staat in `deploy/apache-web-auth.conf` en `deploy/apache-gameserver-ssl.conf`. Vereiste modules zijn onder meer `request`, `auth_form`, `session`, `session_cookie` en `session_crypto`. De sessiesleutel staat uitsluitend op de server in `/opt/icarus-control/secrets/admin-session.key`. De proxy verwijdert aangeleverde identiteitheaders en controleert de Origin bij beheerwijzigingen en aanmelden/uitloggen.

De uitrol gebruikte een afgeschermde testpoort met een aparte databasekopie en testwachtwoord. `tests/apache-auth-smoke.py` controleert aanmelden, uitloggen, cookiebeveiliging, vervalste headers, redirects en CSRF zonder beheerdata te wijzigen of geheimen te loggen. De tijdelijke testomgeving is verwijderd. De vorige release en databasekopie zijn behouden voor terugval. Alleen de loginwijzigingen zijn op de bestaande productiebron toegepast; andere lokale wijzigingen zijn niet mee gepubliceerd.

Op 19 oktober 2026 is een tweede sessieprobleem hersteld: `SessionCookieRemove On` liet de huidige Apache-module de sessiecache bij herhaalde uitlezingen leegmaken. De configuratie gebruikt nu `SessionCookieRemove Off` en verwijdert de versleutelde sessiecookie pas tijdens de late headerverwerking voordat het verzoek naar de applicatie gaat. `apache-auth-smoke.py --port 443 --prompt-password --check-reload` controleert ook twintig opeenvolgende beschermde verzoeken en behoud van een bestaande sessie na het herladen van Apache. `tests/admin-login.browser.cjs` test met een echt, apart browserprofiel de foutmelding, loginredirect, behouden sessie na verversen en uitloggen; het wachtwoord wordt uitsluitend via stdin aangeleverd.

## Game-API

Games controleren toegang met `POST /api/game/v1/access/check`. Een speler logt in met `user` (e-mailadres of gebruikers-ID) en een persoonlijk wachtwoord in `password`. Het bestaande API-veld `accessCode` blijft als alias werken voor huidige Unity-clients; dit is geen gedeelde of eenmalige toegangscode. Vertrouwde backendservices mogen in plaats daarvan de bestaande `Authorization: Bearer <token>` header gebruiken. De body ondersteunt daarnaast `gameId`, `environment`, `playerIp` en `clientVersion`. Als `playerIp` ontbreekt, gebruikt de productieproxy het echte clientadres. Het antwoord bevat een server-authoritatieve beslissing, spelersinstellingen, actieve releases en kaartpolicy-overrides. De livenesscheck staat op `GET /api/health`.

Gamewachtwoorden worden nooit leesbaar opgeslagen: alleen een unieke salt en PBKDF2-SHA256-hash staan in de beheerdatabase. Bij het aanmaken van een Player/Tester-account verstuurt de beheerder een instellink; de speler kiest zelf een blijvend wachtwoord van 12–128 tekens. Zonder ingesteld wachtwoord is aanmelden niet mogelijk. Onder **Gebruikers → Wachtwoordlink versturen** kan de beheerder opnieuw een instel- of herstellink aanbieden. De beheerder kiest of overschrijft het spelerswachtwoord niet. Interne moderatoraccounts houden hun afzonderlijke beheerprocedure.

De Unity-client toont een afgeschermd wachtwoordveld met tonen/verbergen en aanmelden met Enter. **Wachtwoord vergeten?** opent de openbare herstelpagina op de gameserver. Alleen de gebruikersnaam wordt onthouden; het wachtwoord wordt na verzenden uit het invoerveld gewist en nooit in PlayerPrefs opgeslagen. De client verstuurt wachtwoorden uitsluitend via HTTPS. Bestaande wachtwoorden en de serverkoppeling blijven behouden; een gewijzigde client moet opnieuw worden gebouwd voordat spelers deze aanmelding zien. Net als de bestaande server worden spaties voor en na het wachtwoord verwijderd.

## Openbaar wachtwoordherstel voor spelers

`/password-reset.html` laat spelers een link aanvragen en zelf een eerste of nieuw wachtwoord kiezen, ook wanneer het account nog geen wachtwoordrecord heeft. De API-routes zijn `POST /api/game/v1/password-reset/request` (body: `email`) en `POST /api/game/v1/password-reset/confirm` (body: `token`, `password`, `confirmation`). Alleen de link is eenmalig; het gekozen wachtwoord blijft bruikbaar bij iedere aanmelding.

- Alleen actieve rollen `Player` en `Tester` komen in aanmerking. Andere rollen en **alle** accounts waarvan gebruikers-ID of e-mailadres voorkomt in `admin_principals` zijn uitgesloten, ook als de beheerder daar is geblokkeerd. Dezelfde controle vindt plaats bij het gebruiken van de link; promoveren naar beheerder maakt eerdere links onbruikbaar.
- De link gaat uitsluitend naar het e-mailadres uit het bestaande account. Er worden geen bestaande wachtwoorden verstuurd. De link is 15 minuten geldig en eenmalig; alleen de SHA-256-hash van het willekeurige token staat in de database. Het token staat in het URL-fragment en wordt na openen uit de browsergeschiedenis verwijderd.
- Het wachtwoord blijft ongewijzigd totdat een geldige link wordt gebruikt. Een wijziging door een beheerder, een gewijzigd e-mailadres of een geblokkeerd account maakt oude links onbruikbaar. Eén succesvolle reset maakt alle eerder uitgegeven links voor dat account onbruikbaar.
- Aanvragen hebben dezelfde antwoordtekst voor bestaande, onbekende en beschermde accounts. Er geldt een limiet van 3 aanvragen per e-mailadres en 10 per client-IP per uur; bevestigen is beperkt tot 20 pogingen per IP per uur.
- Een aangemelde beheerder kan via `send-user-password-link` een bestaand actief spelersaccount opnieuw uitnodigen. Alleen deze beschermde route gebruikt een afzonderlijke limiet van 10 beheer-IP-aanvragen per uur, zodat eerdere openbare pogingen het oplossen van een mailstoring niet blokkeren. Dit is geen publiek configureerbare uitzondering.

### OVH-mailrelay en uitrol

Gebruik de bestaande OVH Email Pro-verbinding rechtstreeks op de gameserver: `pro2.mail.ovh.net`, poort `587`, verplichte STARTTLS met certificaatcontrole. Hiervoor hoeft niets in WordPress of FluentSMTP te worden gewijzigd. Een andere OVH-host is configureerbaar; poort `465` gebruikt directe TLS. De niet-geheime voorbeeldinstellingen staan in `deploy/password-reset.env.example`. Stel `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_FROM` en `SMTP_PASSWORD` in via de geheime serveromgeving (`/opt/icarus-control/secrets/runtime.env` voor de Docker-opstelling). Zet het SMTP-wachtwoord nooit in Git, de client of een URL.

De Docker-image installeert `ca-certificates` voor gecontroleerde uitgaande TLS. De Worker opent de SMTP-socket met de oorspronkelijke OVH-hostnaam, zodat STARTTLS die naam blijft verifiëren. Hiermee worden ook de niet-ondersteunde resolverstappen van de mailbibliotheek vermeden. De auditmelding **Wachtwoordlink aangeboden** betekent dat de mailserver de mail heeft geaccepteerd, niet dat inboxontvangst is bewezen. Mislukte verzending laat het wachtwoord ongewijzigd en verwijdert het ongebruikte token.

Publiceer de nieuwe servercode en migratie `0004_player_password_recovery.sql`, en neem de aangepaste Apache-configuratie over: `/password-reset`, `/password-reset.html` en de bestaande `/api/game/`-routes zijn openbaar; het adminpaneel en adminlogin blijven beschermd. Het publieke bestand wordt door de assets-server van `.html` naar de canonieke route doorgestuurd, dus beide adressen moeten toegestaan zijn. Hersteltabellen worden ook idempotent aangemaakt door de reset-API. Zonder volledige SMTP-instellingen geeft de resetaanvraag voor alle accounts HTTP 503; er worden dan geen mails verstuurd en geen wachtwoorden gewijzigd. Een latere afleverfout wordt zonder geheimen in het serverlog gemeld.

Het vastgelegde werkende herstelpunt, de terugzetprocedure en de exacte plaats voor het nieuwe mailboxwachtwoord staan in `Documentation/GAMESERVER_BACKUP_EN_HERSTEL.md` in het bovenliggende Unity-project. De scripts `deploy/backup-release.py` en `deploy/restore-release.py` bewaren en herstellen de software onafhankelijk van de live spelersdatabase.

Controleer voor livegang aflevering aan een expliciet gekozen testaccount en de bestaande SPF/DKIM/DMARC-instellingen. De geautomatiseerde tests gebruiken alleen een database in het geheugen en een vervangende mailzender; ze versturen geen echte e-mails. Voer ze uit met `pnpm run test:password-reset` (Node.js 24 of nieuwer voor deze tests).
