# Zelfgekozen spelerswachtwoorden

Bron: gebruikersverzoek van 19 oktober 2026; bestaande game-API,
CleanDuelAccessClient en het OVH-e-mailherstel.

Uitkomst: een beheerder maakt een Player/Tester-account zonder gedeelde code
of vooraf gekozen wachtwoord. De speler krijgt een link op het geregistreerde
adres en kiest zelf een blijvend wachtwoord van 12–128 tekens.

Regels:

- Zonder wachtwoordrecord kan nog niet worden ingelogd; eerste instelling via
  e-mail is wel mogelijk. Dit herstelt ook bestaande accounts zonder record.
- Een instellink is eenmalig, verloopt na vijftien minuten en staat alleen
  gehasht in de database. Het gekozen wachtwoord blijft geldig bij herhaald
  aanmelden en staat alleen met salt/PBKDF2-hash opgeslagen.
- De huidige credentialversie, het e-mailadres, accountstatus en beheerrechten
  worden zowel bij uitgifte als gebruik van de link gecontroleerd.
- Geblokkeerde en beschermde beheeraccounts krijgen geen openbaar herstel.
- Een normale speler krijgt ook bij handmatige beheerreset een e-maillink;
  een beheerder overschrijft geen zelfgekozen spelerswachtwoord via game-settings.
- Bestaande Unity-clients blijven compatibel via het API-veld `accessCode`;
  nieuwe integraties mogen het duidelijkere veld `password` gebruiken.
- OVH gebruikt gecontroleerde STARTTLS op poort 587. Er worden geen SMTP-
  wachtwoorden, resetlinks of credentialhashes gelogd.

Acceptatie: eerste wachtwoord zonder eerder record, bescherming tegen
hergebruik/concurrerende wijzigingen, herhaald aanmelden via beide API-velden,
beheerdersuitsluiting, gerichte frontendcontrole en productiecontrole van
mailserveracceptatie. Werkelijke inboxontvangst moet de ontvanger bevestigen.

Geverifieerd op 19 oktober 2026: 30 wachtwoord-/mailtests en 2 rendertests
geslaagd; TypeScript, lintcontrole, serverbouw en geïsoleerde/live controles
geslaagd. OVH heeft precies één goedgekeurde instelmail geaccepteerd om
12:22:18 UTC. Het bestaande spelersaccount is niet door een beheerder van
een testwachtwoord voorzien. Beveiligde SMTP werkt met de CA-bundel en een
socket die de oorspronkelijke OVH-hostnaam behoudt.

Afzonderlijke echte browsertest geslaagd: herstelpagina vanaf login,
foutmelding bij verkeerd wachtwoord, beheerderlogin, sessiebehoud na
verversen, spelersformulieren zonder beheerdergekozen wachtwoord,
afzonderlijk moderatorveld en uitloggen. Geen extra accounts of mails
aangemaakt tijdens deze controle.
