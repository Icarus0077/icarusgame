# Icarus-gameserver: werkende backup, herstel en mailwachtwoord

Dit bestand is het vaste herstelplan voor `gameserver.rene007.nl`. Raadpleeg dit vóór een volgende serverupdate. Wachtwoorden en API-sleutels horen nooit in dit document.

## Actuele aanvulling: 19 oktober 2026

- Actieve release: `/opt/icarus-control/releases/player-passwords-20261019T122133Z`.
- Back-up vóór de volledige wijziging: `/opt/icarus-control/backups/before-password-reset-20261019T120821Z`.
- Laatste back-up vóór de definitieve mailcorrectie: `/opt/icarus-control/backups/before-password-reset-20261019T122121Z`.
- De privé ingestelde OVH-mailgegevens zijn aanwezig; SMTP-aanmelding met STARTTLS en certificaatcontrole is geslaagd. Het mailboxwachtwoord hoeft niet opnieuw te worden ingevoerd.
- Player/Tester-accounts kiezen zelf een blijvend wachtwoord via e-mail, ook als nog geen wachtwoordrecord bestaat. Een beheerder maakt geen eenmalige toegangscode meer aan.
- De container bevat nu de standaard CA-certificaten. De SMTP-socket behoudt de OVH-hostnaam zodat de geïnstalleerde Workers-runtime de STARTTLS-identiteit correct controleert. Er is geen TLS-controle uitgeschakeld.
- Precies één goedgekeurde instelmail voor het bestaande spelersaccount is door OVH geaccepteerd op **19 oktober 2026, 12:22:18 UTC**. Inboxontvangst en de eigen wachtwoordkeuze moeten door de ontvanger worden bevestigd; er is geen testwachtwoord voor dit account ingesteld.
- 30 wachtwoord-/mailtests, 2 rendertests, TypeScript en gerichte lintcontrole zijn geslaagd. De releasebouw, geïsoleerde servercontrole en live healthcheck slagen. Live: health/login/resetpagina HTTP 200, admin-API zonder sessie HTTP 401, container `healthy`.
- De afzonderlijke echte browsertest slaagt voor de herstelpaginalink, verkeerde-wachtwoordmelding, login, verversen met sessiebehoud, spelersformulieren zonder beheerdergekozen wachtwoord, intern moderatorveld en uitloggen. Daarbij zijn geen extra accounts of mails aangemaakt.
- De bestaande Apache-webaanmelding en spelersgegevens zijn behouden. Tijdelijke mailtestcontainer en eenmalige mailhulpmiddelen zijn verwijderd; releases, auditlog en back-ups blijven behouden.

De onderstaande secties beschrijven het oorspronkelijke herstelpunt van 16 oktober. Hun toenmalige status “mailwachtwoord nog niet ingevuld” en “geen echte resetmail verzonden” zijn door bovenstaande controle achterhaald. Gebruik voor nieuwe updates de actuele releaseverwijzing en maak steeds eerst een nieuwe back-up.

## Vastgelegde werkende versie

Op 16 oktober 2026 is de werkende versie **vóór de spelerswachtwoord-resetupdate** veiliggesteld.

| Onderdeel | Exacte locatie/waarde |
| --- | --- |
| Server | `gameserver.rene007.nl` / `51.75.68.184` (`vps-a5b11eed`) |
| Private backupmap | `/opt/icarus-control/backups/before-password-reset-20261016T134353Z` |
| Werkende release vóór de update | `/opt/icarus-control/releases/web-login-20261016` |
| Werkende image | `sha256:494a23e5240ebbcf3df0cf82f1e8faafc2aac146b34428f9f7e1c0dc290cdb01` |
| Vastgezette backuptag | `icarus-control:backup-before-password-reset-20261016t134353z` |
| Actieve releaseverwijzing | `/opt/icarus-control/current` |
| Docker-container en Compose-project | `icarus-control` |
| Actuele spelersdatabase/persistente gegevens | `/var/lib/icarus-control` |
| Actieve runtimegeheimen | `/opt/icarus-control/secrets/runtime.env` |

De backup bevat `image.tar` (het exact draaiende Docker-image), `release.tar.gz` (broncode en instellingen), `private-config.tar.gz` (onder meer Apache-configuratie, oorspronkelijke beheerdersaanmelding en runtimegeheimen), `data/` (consistente SQLite-snapshots inclusief vastgelegde WAL-transacties) en `manifest.json` (versies, tijdstip en SHA-256-controlesommen). De backupmap is alleen voor root toegankelijk. Publiceer deze map nooit en zet de inhoud niet in Git.

Bij de eindcontrole zijn de opgeslagen SQLite-snapshots expliciet gesloten en gestabiliseerd zonder losse WAL-/SHM-bestanden. De volledige SQL-inhoud vóór en na die afronding was gelijk; alle integriteitscontroles zijn opnieuw geslaagd. De oorspronkelijke opgeslagen bestanden blijven daarnaast bewaard in `data-before-finalization.tar.gz`. Het manifest legt deze afronding vast. Alle negen uiteindelijke backupbestanden zijn daarna tegen hun SHA-256-controlesom gecontroleerd. Er is hierbij niets aan de live database veranderd.

**Verificatie:** de productie-healthcheck was HTTP 200; de drie databasebestanden doorstonden `PRAGMA integrity_check`. De afzonderlijke herstelproef is geslaagd: het opgeslagen image is uit `image.tar` geladen en met een kopie van de opgeslagen databases en oorspronkelijke runtimeconfiguratie gestart op een uitsluitend lokaal bereikbare testpoort. De healthcheck gaf daar HTTP 200. Ook het bronarchief is uitgepakt. Dit is een geïsoleerde herstelproef; er is geen database of werkende productieserver teruggedraaid.

De nieuwe versie is daarna geactiveerd als `/opt/icarus-control/releases/password-reset-20261016T135031Z`, image `icarus-control:password-reset-20261016t135031z`. De oude release en backup blijven bewaard.

## Exacte plaats voor het nieuwe wachtwoord van info@rene007.nl

Dit is een bestand **op de Linux-gameserver**, niet op de Windows-pc en niet in WordPress:

```text
/opt/icarus-control/secrets/runtime.env
```

Open het via je bestaande SSH-/serverbeheerverbinding als root. Vanaf deze Windows-werk-pc is de bestaande verbinding:

```powershell
ssh -i "C:\Users\rene\.ssh\icarus_ollama_ed25519" -o IdentitiesOnly=yes root@51.75.68.184
```

Open vervolgens op de server:

```bash
nano /opt/icarus-control/secrets/runtime.env
```

De lege `SMTP_PASSWORD=`-regel is alvast aangemaakt en staat bij oplevering op regel 7. De andere SMTP-waarden zijn eveneens ingesteld; het nieuwe wachtwoord moet nog privé worden ingevuld.

Laat bestaande regels zoals `GAME_API_TOKEN` en `SEED_DEMO_DATA` intact. Zorg dat onderstaande instellingen ieder precies eenmaal voorkomen. Vul alleen de voorbeeldtekst achter `SMTP_PASSWORD=` aan met het **nieuwe OVH-mailboxwachtwoord**:

```dotenv
SMTP_HOST=pro2.mail.ovh.net
SMTP_PORT=587
SMTP_USER=info@rene007.nl
SMTP_FROM=info@rene007.nl
SMTP_PASSWORD='VUL_HIER_HET_NIEUWE_MAILBOXWACHTWOORD_IN'
```

De laatste regel hierboven is een voorbeeld, geen werkend wachtwoord. Gebruik geen `<...>`-tekens om het echte wachtwoord. Bij een wachtwoord met een enkel aanhalingsteken is deze eenvoudige notatie niet geschikt; gebruik dan een secretbeheerder of laat de dotenv-notatie gecontroleerd invoeren. Zet het wachtwoord niet als los shellcommando in de terminal, want dan kan het in de opdrachtgeschiedenis belanden. Deel geen screenshot van de ingevulde waarde.

`runtime.env` is eigendom van root en hoort bestandsrechten `600` te hebben:

```bash
chown root:root /opt/icarus-control/secrets/runtime.env
chmod 600 /opt/icarus-control/secrets/runtime.env
```

De container krijgt dit bestand alleen-lezen als `/run/secrets/icarus.env`. Wrangler leest het bij het starten. Na opslaan moet de container daarom opnieuw worden aangemaakt; een nieuwe softwarebuild is niet nodig:

```bash
docker compose -p icarus-control -f /opt/icarus-control/current/deploy/compose.yml up -d --no-build --force-recreate
curl --fail http://127.0.0.1:3100/api/health
```

De nieuwe serverversie met de SMTP-koppeling is actief. Zonder mailwachtwoord blijft inloggen werken, maar geeft de openbare resetaanvraag een algemene HTTP 503-melding. Er worden dan geen wachtwoorden gewijzigd en geen mails verzonden. De openbare pagina staat op `https://gameserver.rene007.nl/password-reset`; de `.html`-link uit de client verwijst daar veilig naartoe. Beide adressen zijn uitgezonderd van de beheerdersaanmelding.

FluentSMTP en WordPress hoeven hiervoor niet aangepast te worden. Dit is dezelfde OVH-verbinding, rechtstreeks gebruikt door de gameserver. Het mailboxwachtwoord is niet het wachtwoord van een spelaccount of van de beheerder.

## Normaal terug naar de werkende software

Gebruik standaard alleen softwareherstel. Dat behoudt alle huidige spelers, ingestelde spelwachtwoorden, nieuwe SMTP-geheimen en recente gegevens. De resetupdate voegt alleen aparte tabellen toe; het verwijderen van die tabellen is niet nodig voor een terugkeer naar de oude software.

De zelfvoorzienende terugzetopdracht voor deze backup is:

```bash
python3 /opt/icarus-control/backups/before-password-reset-20261016T134353Z/restore-release.py --backup /opt/icarus-control/backups/before-password-reset-20261016T134353Z
```

Het script controleert alle opgeslagen SHA-256-controlesommen, laadt het bewaarde Docker-image, herstelt broncode in een **nieuwe** release-map, zet de bewaarde Apache-siteconfiguratie en beheerderinlogpagina terug, start die image met hetzelfde Compose-project en controleert de healthcheck. Pas daarna wijst `current` naar de herstelde release. De huidige beheerderswachtwoorden, sessiesleutel, runtimegeheimen en live database worden niet teruggedraaid.

Dit commando is bedoeld om door een beheerder uit te voeren wanneer herstel daadwerkelijk nodig is. Een terugzetactie is niet onderdeel van een gewone test.

Controleer daarna als uitgelogde bezoeker:

```bash
curl -s -o /dev/null -w '%{http_code}\n' https://gameserver.rene007.nl/api/health
curl -s -o /dev/null -w '%{http_code}\n' https://gameserver.rene007.nl/login
curl -s -o /dev/null -w '%{http_code}\n' https://gameserver.rene007.nl/api/admin
docker inspect icarus-control --format '{{.State.Health.Status}}'
```

Verwacht: health `200`, loginpagina `200`, admin-API zonder geldige sessie `401` en container `healthy`. Test daarnaast een echte beheerderaanmelding met het bestaande account; zet daarvoor geen wachtwoord in een commandoregel.

## Alleen bij echte databasebeschadiging: volledig gegevensherstel

**Niet nodig voor een gewone softwareterugzetactie.** Databaseherstel vervangt alle gegevens door de toestand van 16 oktober 2026 om 13:43:53 UTC. Nieuwere accounts, spelerswachtwoorden en beheerwijzigingen verdwijnen dan uit de actieve database. Voer dit uitsluitend na expliciete opdracht uit en maak eerst een extra actuele backup.

Stop de container vóór het vervangen van SQLite-bestanden. Een deskundige beheerder kan de huidige map eerst onder een nieuwe, nog niet bestaande naam bewaren en daarna de volledige `data/`-snapshot terugzetten. De toegestane bron en bestemming zijn uitsluitend:

```text
Bron: /opt/icarus-control/backups/before-password-reset-20261016T134353Z/data
Doel: /var/lib/icarus-control
```

Kopieer nooit alleen een losse SQLite-hoofddatabase over een draaiende database met WAL-bestanden. De snapshots in deze backup zijn al consistent gemaakt. Herstel ook geen oude `runtime.env`, Apache-`htpasswd` of sessiesleutel tenzij specifiek herstel van die bestanden nodig is; dat zou nieuwe wachtwoorden of geheimen terugdraaien.

## Vastgelegde status op 16 oktober en volgende updates

- Backup aangemaakt: **ja**, vóór wijzigingen aan de werkende versie.
- Database-integriteit: **geslaagd** voor alle drie snapshots.
- Definitieve SHA-256-controle: **geslaagd**, negen backupbestanden inclusief herstelhandleiding en terugzetscript.
- Afzonderlijke herstelproef: **geslaagd**, met het gearchiveerde image en kopieën van de opgeslagen databases.
- Spelerswachtwoord-resetupdate op productie: **geactiveerd en gezond**.
- Uitgelogde controle: health `200`, beheerderlogin `200`, canonieke resetpagina `200`, `.html`-link `307` naar de canonieke resetpagina, admin-API `401`.
- Beheerderswachtwoordbestand: **ongewijzigd**, gecontroleerd vóór en na de update.
- Nieuw mailboxwachtwoord ingevuld: **nee**, dit moet privé in `runtime.env` worden gezet.
- Verbinding met OVH vanaf de VPS: **geslaagd**, poort 587, STARTTLS en certificaatcontrole akkoord. Dit is geen aanmeld- of verzendtest.
- Echte resetmail verzonden/getest: **nee**.

Maak bij iedere volgende update opnieuw een gedateerde backup met `deploy/backup-release.py`. Noteer de nieuwe backupmap en actuele teststatus; overschrijf deze bestaande backup niet. Bewaar daarnaast een beveiligde kopie buiten deze VPS voor bescherming tegen volledig server- of schijfverlies.
