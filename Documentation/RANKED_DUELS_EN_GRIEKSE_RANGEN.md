# Ranked-duels en Griekse rangen

## Doel en afbakening

Bron: gebruikersverzoeken van 19 oktober 2026: ranked-duels simuleren, uitslagen
per account opslaan, Griekse metaal-/edelsteenemblemen en vervolgens een
niveauladder met diamantbeloningen. De nieuwste niveauregels hieronder
vervangen de eerdere zichtbare Elo-drempels. De kaartreferentie bevat geen
competitieregels. Interpretaties zijn hieronder afzonderlijk benoemd.

De eerste verticale slice bestaat uit een servergestuurde ranking, een
beheersimulatie en een rangpaneel in de Unity-client. De simulatie berekent
statistische uitslagen; zij voert **geen volledige Yu-Gi-Oh-kaartduels** uit.
Lokale wedstrijden tegen de bestaande AI tellen niet mee voor echte ranked.
Online matchmaking en een server die volledige duels valideert zijn nog niet
gekoppeld. Er is bewust geen openbare API waarmee een client zelf een winnaar
of rating kan insturen.

## Spelregels van de zichtbare ladder

- Ieder nieuw account begint op **Bronze 4**, met nul diamanten uit ranked.
- Binnen iedere rang is niveau 1 het hoogste niveau. Er zijn in totaal 39
  posities: Bronze 4 t/m 1, Silver 5 t/m 1, Gold 6 t/m 1, Platinum 7 t/m 1,
  Diamond 8 t/m 1 en Master 9 t/m 1.
- Vier gewonnen duels binnen het huidige niveau geven één promotie. Voorbeeld:
  Bronze 4 → Bronze 3 → Bronze 2 → Bronze 1 → Silver 5.
- Drie verloren duels binnen het huidige niveau geven één degradatie.
  De overgang is ook omgekeerd mogelijk: Silver 5 → Bronze 1.
- Interpretatie: winst en verlies hoeven niet achter elkaar te zijn. Beide
  tellers lopen naast elkaar. Een niveauwisseling zet beide tellers terug op nul.
- Gelijkspel telt mee in de wedstrijdhistorie, maar verandert beide tellers niet.
- Bronze 4 is de ondergrens en Master 1 de bovengrens. Aan die grens worden
  de tellers na respectievelijk drie verliezen/vier overwinningen opnieuw op
  nul gezet, zonder onbestaand niveau of beloning.
- Het klassement sorteert op de zichtbare ladderpositie, niet op Elo.

| Rang | Griekse beschermer | Niveaus, laag naar hoog | Hoofdmateriaal van het embleem |
| --- | --- | ---: | --- |
| Bronze | Icarus | 4 → 1 | Koperbruin, gebronsd metaal met donkere patina |
| Silver | Hermes | 5 → 1 | Neutraal zilver, zonder gouden randen |
| Gold | Athena | 6 → 1 | Rijk geelgoud op kader, helm en harnas |
| Platinum | Apollo | 7 → 1 | Koel, helder wit platina; geen geelgoud |
| Diamond | Poseidon | 8 → 1 | Transparante geslepen diamantfacetten met ijzige schittering |
| Master | Zeus | 9 → 1 | Koninklijk goud met amethist en violette bliksem |

De materiaalnamen staan voorop; de Griekse figuur blijft als beschermer bij
dezelfde rang horen. De bestaande IDs (`icarus` t/m `zeus`) en afbeeldingen
blijven behouden. Champion van Olympus is alleen een voorgestelde toekomstige
seizoenstitel, nog geen geïmplementeerde zevende rang.

## Diamanten bij promotie

- Promotie binnen dezelfde rang: **100 diamanten** op het account.
- Promotie naar de volgende rang: **200 diamanten in totaal**, dus niet 300.
- Elke legitieme promotie wordt beloond, ook als hetzelfde niveau eerder is
  behaald. De gebruiker heeft dit expliciet bevestigd. Er is geen eenmalige
  beloningslimiet op de hoogste ooit bereikte stand.
- Definitieve verduidelijking: opnieuw belonen voor een eerder behaald niveau
  vereist een echte degradatie en daarna een nieuwe promotie. Het nogmaals
  ontvangen van hetzelfde duel is geen nieuwe promotie of overwinning. Zowel
  een degradatie door verloren duels als door inactiviteit kan voorafgaan aan
  een opnieuw beloonde promotie.
- Voorbeeld: Bronze 4 → Bronze 3 geeft 100; terug naar Bronze 4 geeft niets;
  opnieuw Bronze 3 bereiken geeft weer 100.
- Ook een herhaalde overgang Bronze 1 → Silver 5 geeft opnieuw 200.
- Degraderen, een verval wegens inactiviteit, een nieuw account en winst op
  Master 1 geven geen promotiebeloning. Diamanten worden bij degradatie niet
  afgepakt.
- Een identiek opnieuw ontvangen duel of simulatieverzoek geeft nooit een
  tweede uitbetaling. Unieke beloningsregels koppelen uitbetaling aan het duel,
  niet aan het niveau. Rank, uitslag en diamantentegoed worden samen verwerkt.
- De echte diamantentegoeden zijn per account; de testcompetitie houdt aparte
  gesimuleerde diamanten bij en kan nooit het echte tegoed ophogen.

## Dertig dagen inactiviteit

Een volledige periode van 30 × 24 uur zonder voltooid ranked-duel verlaagt de
stand met één niveau. Interpretatie: dit herhaalt zich per volledige periode;
90 dagen inactiviteit betekent drie stappen omlaag, met Bronze 4 als ondergrens.
Het mag over een ranggrens gaan. Beide tellers worden bij een echte daling
teruggezet; er is geen beloning en het diamantentegoed blijft behouden.

De server bewaart het laatste ranked-speelmoment en hoeveel perioden al zijn
verwerkt. Opnieuw inloggen of het klassement verversen veroorzaakt daardoor
geen extra degradatie. Alleen inloggen geldt niet als ranked spelen. Het
eerstvolgende voltooide ranked-duel start een nieuwe inactiviteitsperiode.
De berekening gebeurt bij het ophalen/verwerken van de stand; er is geen
achtergrondtaak nodig en de uitkomst na een lange offlineperiode is dezelfde.

## Verborgen speelsterkte (MMR)

De eerdere Elo blijft als technische speelsterkte voor simulatiekansen en
toekomstige matchmaking bewaard: beginwaarde 1000, K=32 en schaal 400.
Deze waarde bepaalt **niet** de zichtbare rang of diamantbeloningen. Er zijn
geen plaatsingsduels nodig voordat een nieuw account Bronze 4 heeft.
Historische Elo-waarden en uitslagen worden niet gewist of omgezet in
terugwerkende promoties of uitbetalingen.

## Tegenstander die met je rang meegroeit

De AI-oefentegenstander gebruikt standaard automatische moeilijkheid op basis
van de serverstand bij inloggen, van Bronze 4 tot Master 1. Zonder geldige
niveaustand begint deze automatische stand bij Bronze 4. Elo en het
diamantentegoed worden niet als moeilijkheidscriterium gebruikt.

- Bronze en Silver gebruiken de Beginner-basis.
- Gold en Platinum gebruiken de Intermediate-basis.
- Diamond en Master gebruiken de Expert-basis.
- Binnen die basisgroepen wordt de keuze van acties per niveau nauwkeuriger:
  minder vroeg passen, minder evaluatiefouten en vaker een hoog gewaardeerde
  legale zet. Master 1 gebruikt de beste beschikbare heuristische keuze.
- Het duelmenu houdt handmatige Beginner/Intermediate/Expert-standen naast
  de automatische stand beschikbaar voor oefenen.

Dit is een geleidelijke afstelling van de bestaande AI, geen garantie dat elk
duel op een hoger niveau gewonnen wordt en geen volledige zoek-AI. De
tegenstander krijgt geen extra levenspunten of geheime kaartinformatie en
blijft dezelfde legaliteitscontroles gebruiken. Ook in automatische stand
blijft dit een oefenduel: het kent geen echte rankingpunten of diamanten toe.

Controle: `CleanDuelRankedAiSmokeTest.RunSuite` is geslaagd in Unity
6000.6.6f1. Deze controle combineert login, alle 39 rangniveaus en de AI.
Gecontroleerd zijn oplopende profielinstellingen, reproduceerbare legale
keuzes over 256 seeds, behouden handmatige standen, afgeschermde face-down
gegevens en een volledig autonome Master-beurt. Een winpercentage per rang
is niet vastgesteld: hogere instellingen garanderen geen sterkere uitkomst
in ieder afzonderlijk duel.

Dit is een spelprogressie, geen bewering over een mythologische machtsrangorde.
De emblemen zijn nieuw gegenereerd; er zijn geen commerciële rankafbeeldingen
gekopieerd of uit Master Duel gehaald.

## Gebruiken in Icarus Control

1. Open de lokaal draaiende Icarus Control en kies **Ranked**.
2. Blijf in **Testcompetitie** (`simulation-v1`). Kies bestaande toegestane
   accounts, of maak de zes speciale demoaccounts aan.
3. Selecteer 2–32 accounts, een seed en 1–100 duels. Start de simulatie.
4. Bekijk de opgeslagen resultaten, het klassement en de geschiedenis van een
   geselecteerd account. De historie toont de laatste 100 uitslagen; oudere
   uitslagen blijven in de database bewaard.
5. **Echte ranked** (`ranked-v1`) is alleen-lezen in dit paneel. Een
   simulatieverzoek voor dit seizoen wordt ook op de server geweigerd.

Demoaccounts hebben IDs `ranked_demo_<naam>`, een niet-bezorgbaar adres onder
`ranked.icarus.invalid`, geen wachtwoord en geen gametoegang. Hun speciale
testtoegang geldt uitsluitend voor het simulatieseizoen. Bestaande accounts en
hun wachtwoorden worden niet veranderd.

Een seed maakt dezelfde resultaten reproduceerbaar bij dezelfde beginstand
en dezelfde accountselectie. Een herhaalde simulatie ná gewijzigde ratings
begint uiteraard vanaf die nieuwe stand. Vijf procent van de statistische
uitkomsten is gelijkspel; de overige uitslagen gebruiken de Elo-winstkans.

Bij een netwerkfout gebruikt opnieuw proberen hetzelfde simulatie-ID. Een
eerder opgeslagen batch wordt teruggegeven zonder hem opnieuw te verwerken.
Gewijzigde instellingen met hetzelfde ID leveren een conflict op.

## Unity-client

Na succesvolle login geeft de server uitsluitend de eigen echte ranked-stand
mee, wanneer ranked voor het account aanstaat. De rangknop linksboven opent
de emblemen, rang en niveau, voortgang naar vier winsten/drie verliezen,
diamantentegoed en inactiviteitsdatum. De stand is een momentopname bij login;
opnieuw inloggen haalt de actuele stand op. Een oudere server zonder de
nieuwe voortgangsversie blijft veilig ondersteund: de client meldt dat de
niveaustand ontbreekt en leidt nooit een niveau uit oude Elo af.

Het rangpaneel blokkeert klikken op het onderliggende duelbord. Het sluiten
of openen van dit paneel verandert geen duelregels of uitslagen.

## Opslag en grenzen

- `ranked_profiles`: statistieken, niveau, tellers, activiteit en verval per
  game, seizoen en account.
- `ranked_matches`: uniek duel-ID, beide accounts, uitslag, Elo voor/na,
  rang/niveau voor/na, diamantbeloning, simulatievlag en tijdstip. Oude
  uitslagen zonder die nieuwe gegevens blijven als historische uitslag bestaan.
- `ranked_simulation_batches`: verzoek en rapport voor veilige herhaling.
- `ranked_wallets`: diamantentegoed per account en gescheiden echte/testscope.
- `ranked_rewards`: controleerbare diamantuitbetalingen per account en duel.
- Een transactie schrijft alle gewijzigde standen, tegoeden, uitslagen en auditregels
  samen. Een gelijktijdig gewijzigde beginstand wordt opnieuw gelezen.
- Blokkade, speeltoegang en ranked-toestemming worden bij het schrijven opnieuw
  gecontroleerd. Een fout geeft geen halve rangupdate.
- Beheerderauthenticatie is verplicht; alleen Super Admin, Admin en Moderator
  mogen de simulatie starten. Viewer kan gegevens alleen bekijken.
- `recordRankedMatch` is een interne serverfunctie voor toekomstige, door een
  autoritatieve duelserver bevestigde uitslagen. Deze functie is niet als
  openbare route gepubliceerd.

Basismigratie: `AdminPanel/drizzle/0005_ranked_simulation.sql`.
`AdminPanel/drizzle/0006_ranked_progression.sql` voegt niveaus, activiteit en
diamanttabellen toe. Schema's
worden daarnaast idempotent klaargezet bij ranked-gebruik. Bestaande MMR,
uitslagen, accounts en wachtwoorden blijven bewaard. Oudere profielen zonder
niveaus beginnen voor de nieuwe ladder op Bronze 4, zonder terugwerkende
diamantbeloning.

## Herstellen en uitrollen

Deze wijziging staat in de lokale projectcode. Zij is niet automatisch op de
publieke gameserver uitgerold. Een bestaande geïnstalleerde client moet opnieuw
worden gebouwd om het rangpaneel te bevatten.

Maak vóór een serverrelease een database- en softwareback-up volgens
`GAMESERVER_BACKUP_EN_HERSTEL.md`. Rol backend en client bewust uit. Oudere
software kan de nieuwe rangtabellen ongebruikt laten bestaan; verwijder die
tabellen of wedstrijdgeschiedenis niet om alleen software terug te draaien.
Een oude volledige database terugzetten kan nieuwere accounts, wachtwoorden
en uitslagen verliezen en is daarom geen standaard rollback.

## Acceptatie en controles

| Onderdeel | Bewijs / controle |
| --- | --- |
| Alle 39 niveaus, vier winsten/drie verliezen en rangovergangen | `tests/ranked.test.mjs` |
| Diamanten 100/200, herpromotie en dubbele aflevering | Echte SQLite-transacties in dezelfde tests |
| Inactiviteit na 30/60/90 dagen, ondergrens en herhaalde controle | Tijdgestuurde rankedtests |
| Verborgen MMR, grenzen en gelijkspel | Rankedtests |
| Opslag voor beide accounts, dubbel verzoek, conflict | Echte SQLite-transacties in dezelfde tests |
| Gelijktijdige uitslagen en rollback | Parallelle verzoeken, blokkade tijdens schrijven, foutinjectie |
| Test/echte ladder gescheiden | Simulatie + login-integratietests |
| Beheerrechten en ongeldige invoer | Ranked API-tests |
| Wachtwoordherstel blijft werken | `tests/password-reset.test.mjs` |
| Unity-import, DTO, galerij, klikblokkade | `CleanDuelRankedSmokeTest.Run` |
| Uiterlijk | `CleanDuelRankedSmokeTest.CapturePreview` en lokale browsercontrole |

Backendtests draaien met Node 24: `pnpm run test:ranked`. De volledige
backendcontrole omvat ook wachtwoordtests, TypeScript en `pnpm run build`.
Unity-tests gebruiken de projectversie uit `ProjectSettings/ProjectVersion.txt`.
Batchtests vereisen een gesloten interactieve editor; logbestanden en
previewbeelden staan buiten het Unity-project.

Nog apart te bouwen: echte online ranked-matchmaking, volledige
serverduelvalidatie, actuele rangverversing tijdens een sessie en seizoensbeheer.

### Definitieve beloningscontrole — 19 oktober 2026

Na de laatste verduidelijking zijn de bestaande regels opnieuw gecontroleerd
en zes aanvullende regressietests toegevoegd. Die bewijzen voor zowel 100 als
200 diamanten: een herpromotie na inactiviteitsdegradatie betaalt opnieuw,
een herhaald duel/simulatieverzoek betaalt niets extra en zelfs een gelijktijdig
dubbel ontvangen vierde overwinning telt en betaalt maar één keer.

- 50 rankedtests, 30 login-/wachtwoordtests en 2 rendercontroles geslaagd.
- TypeScriptcontrole en productiebuild van Icarus Control geslaagd.
- Unity 6000.6.6f1: `CleanDuelRankedSmokeTest.Run` geslaagd met exitcode 0;
  alle 39 niveaus, tellers, diamantentegoeden, inactiviteit en oude/uitgeschakelde
  accounts gecontroleerd. De inactiviteitsdatum heet in de UI nu
  "Volgende controle", omdat Bronze 4 niet verder kan dalen.
- Geen publicatie, live-accountmutaties of nieuwe Windows-clientbuild uitgevoerd.

## Artwork en reproduceerbare bronprompts

Gegenereerd en daarna op verzoek bewerkt met de ingebouwde imagegen-functie,
zes afzonderlijke opdrachten per pass, met echte transparantie. De actieve
PNG's zijn opgeslagen in:

- `Assets/Resources/Ranked/<id>.png` voor Unity (importlimiet 512px).
- `AdminPanel/public/ranked/<id>.png` voor Icarus Control.

Onderstaande bronprompts beschrijven de oorspronkelijke versie vóór de
materiaalpass. De materiaalpass onderaan legt de wijzigingen vast.

Gezamenlijke bronprompt voor alle zes:

> Use case: stylized-concept. Asset type: original fantasy card-game ranked
> badge for the Icarus game, ONE square 1:1 UI emblem only. Primary request:
> exquisitely illustrated sculpted Greek mythological character bust centered
> within a compact circular laurel medallion. Style: premium hand-painted game
> UI icon with dimensional metal relief, strong silhouette, readable at 64
> pixels, crisp edges, dark navy interior, restrained luminous highlights,
> consistent classical Greek ornament.

Per opdracht toegevoegd als `Subject`:

- Icarus: youthful Greek male aviator hero with short windswept hair and
  sweeping feathered wings rising behind his shoulders; bronze and teal.
- Hermes: young Greek male messenger god with winged helmet and a small
  caduceus motif; silver and aquamarine.
- Athena: Greek goddess of wisdom, dignified female face, elegant crested Greek
  helmet, tiny owl relief integrated in armor, silver and violet enamel.
- Apollo: Greek god of light and music, handsome calm male face crowned with
  laurel, small sun and lyre motifs, gold and warm amber enamel.
- Poseidon: Greek sea god, strong mature bearded face with flowing hair,
  prominent trident, wave-carved ornament, dark silver and ocean-blue enamel.
- Zeus: Greek sky god, powerful elderly bearded face with long silver hair,
  gold laurel crown, clear lightning-bolt motif, regal gold and electric
  violet enamel.

Afsluiting van alle opdrachten:

> Composition: close-up head and shoulders, medallion entirely inside frame,
> generous transparent margin, front three-quarter view. No letters, no numbers,
> no text, no watermark, no logos, no imitation of any existing game badge.
> Isolated on genuine transparent background.


## Materiaalpass — 19 oktober 2026

Bron: de gebruiker wil het metaal/de edelsteen als dominant beeldmateriaal,
in het bijzonder echt geelgoud voor Gold. Alle zes oorspronkelijke PNG's
zijn als lokale edit targets meegegeven, met behoud van transparantie,
portret, pose, Griekse motieven en medaillonsilhouet. Er wordt geen tintfilter
in de game over de plaatjes gezet: het materiaal zit in de afbeelding zelf.

Gemeenschappelijke bewerkingsprompt (naam ingevuld per bestand):

> Use case: precise-object-edit. Asset type: transparent ranked UI emblem for
> Icarus. Input image: EDIT TARGET, the existing [name] badge. Primary request:
> adjust the emblem's dominant materials to its assigned rank. [Per-file
> instructions below.] Invariants: keep the exact Greek character, pose,
> recognizable facial features, character motifs, close-up framing, circular
> laurel medallion composition, square canvas, transparent outside silhouette
> and premium painted three-dimensional relief style. Do not add any text,
> letters, rank labels, watermarks or logos. Material edit only, not a redesign.
> Preserve true alpha transparency; no flat black or white background and no
> checkerboard baked in. Whole emblem fits within frame.

Specifieke instructies per bestand:

### icarus.png

BRONZE tier. Change ALL existing gold-colored metal to unmistakable bronze: warm copper-brown alloy, russet shadows, subdued burnished bronze highlights, fine hammered/patinated relief. Laurel frame, feather edging, headpiece, shoulder ornament and bottom crest all predominantly bronze, NOT bright yellow gold. Reduce teal enamel to tiny dark oxidized turquoise accents only, so bronze dominates the silhouette.

### hermes.png

SILVER tier. Make ALL hardware unmistakably SILVER: laurel frame, helmet, wings, caduceus and lower crest in neutral polished and lightly brushed silver with charcoal recesses and white highlights. Remove every yellow-gold/copper edge and inset. Turquoise gems become small clear smoky-white gems. Keep a little muted navy cloth. Silver is the overwhelming main material, not pale gold; no large icy-blue crystals.

### athena.png

GOLD tier. Make the main metal unmistakable rich YELLOW GOLD, not silver, white gold, bronze or platinum. All laurel leaves, complete outer medallion, helmet, engraved armor, owl relief and lower crest must be visibly solid polished gold with warm amber shadows and bright yellow-gold highlights. Gold dominates at least85percent of all hardware and ornament. Reduce purple to small deep-purple cloth/enamel recess accents. Preserve natural skin, facial identity and gaze; she is not a gold-skinned statue. The entire badge must read GOLD immediately even at thumbnail size.

### apollo.png

PLATINUM tier. Replace ALL existing yellow gold on laurel wreath, sun rays, lyre, garment brooch and complete outer medallion with brilliant PLATINUM. Premium cold white metal, mirror-polished crisp white highlights, cool blue-gray recesses, minimal tiny pale-ice-blue enamel accents. Absolutely no warm gold, copper or amber metal anywhere. Preserve natural skin and hair (hair may remain blond but reduce warm orange reflection). Platinum frame is brighter and whiter than brushed silver. Retain the sun and lyre character motifs.

### poseidon.png

DIAMOND tier. Replace the large laurel medallion frame and crest with visibly FACETED TRANSPARENT DIAMOND crystal rather than plain silver leaves. Keep the laurel silhouette but carve its leaves into chunky clean diamond facets, with icy cyan edges, prismatic white sparkles and clear crystalline refraction. Large diamond at lower crest and smaller diamond insets. Trident is polished white metal with diamond-cut points. The frame's main material is clear diamond, not blue opaque sapphire, not silver, not gold. Keep Poseidon's natural face, white beard, ocean-blue waves and recognizable trident; keep blues mostly in background and waves so sparkling diamonds dominate the frame.

### zeus.png

MASTER tier, luxurious gold-and-amethyst rather than a basic Gold rank. Keep gold as the main metal, with deeper polished royal-gold laurel frame and lightning, refined engraved relief and small faceted amethyst insets. Keep violet lightning and dark purple cloak as supporting accents. Preserve the existing silhouette, full beard, natural skin, face, crown and composition. Make the outer crest visibly premium, but no new objects or extra outer decorations. Ensure Master has gold plus violet while ordinary Gold is predominantly yellow gold.
