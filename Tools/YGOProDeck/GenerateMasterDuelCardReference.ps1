param(
    [Parameter(Mandatory = $true)]
    [string]$InputJson,

    [string]$OutputMarkdown = "Documentation/YU_GI_OH_MASTER_DUEL_CARD_REFERENCE.md",

    [string]$DatabaseVersion = "unknown",

    [string]$DatabaseLastUpdate = "unknown",

    [string]$SnapshotDate = (Get-Date -Format "yyyy-MM-dd")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-PropertyValue {
    param(
        [object]$Object,
        [string]$Name
    )

    if ($null -eq $Object) {
        return $null
    }

    $property = $Object.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Get-MiscInfo {
    param([object]$Card)

    $items = @(Get-PropertyValue -Object $Card -Name "misc_info")
    if ($items.Count -eq 0) {
        return $null
    }

    return $items[0]
}

function Escape-MarkdownCell {
    param([object]$Value)

    if ($null -eq $Value) {
        return "—"
    }

    $text = [string]$Value
    if ([string]::IsNullOrWhiteSpace($text)) {
        return "—"
    }

    return $text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

function Format-DatabaseId {
    param([object]$Value)

    if ($null -eq $Value) {
        return "—"
    }

    $text = [string]$Value
    if ($text.Length -lt 8) {
        return $text.PadLeft(8, "0")
    }

    return $text
}

function Format-Stat {
    param([object]$Value)

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) {
        return "—"
    }

    $number = [long]$Value
    if ($number -lt 0) {
        return "?"
    }

    return [string]$number
}

function Format-LevelRankLink {
    param([object]$Card)

    $type = [string](Get-PropertyValue -Object $Card -Name "type")
    if ($type -eq "Link Monster") {
        $linkValue = Get-PropertyValue -Object $Card -Name "linkval"
        if ($null -eq $linkValue) {
            return "Link ?"
        }

        return "Link " + [string]$linkValue
    }

    $level = Get-PropertyValue -Object $Card -Name "level"
    if ($null -eq $level) {
        return "—"
    }

    if ($type -like "XYZ*") {
        return "Rank " + [string]$level
    }

    return "Level " + [string]$level
}

function Format-LinkMarkers {
    param([object]$Card)

    $markers = @(
        @(Get-PropertyValue -Object $Card -Name "linkmarkers") |
            Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) }
    )
    if ($markers.Count -eq 0) {
        return "—"
    }

    $labels = @{
        "Top" = "N"
        "Top-Right" = "NE"
        "Right" = "E"
        "Bottom-Right" = "SE"
        "Bottom" = "S"
        "Bottom-Left" = "SW"
        "Left" = "W"
        "Top-Left" = "NW"
    }

    $formatted = foreach ($marker in $markers) {
        $key = [string]$marker
        if ($labels.ContainsKey($key)) {
            $labels[$key]
        }
        else {
            $key
        }
    }

    return $formatted -join ","
}

function Format-OfficialLink {
    param(
        [object]$Card,
        [object]$Misc
    )

    $konamiId = Get-PropertyValue -Object $Misc -Name "konami_id"
    if ($null -ne $konamiId -and [long]$konamiId -gt 0) {
        $tcgDate = Get-PropertyValue -Object $Misc -Name "tcg_date"
        $locale = if ([string]::IsNullOrWhiteSpace([string]$tcgDate)) { "ae" } else { "en" }
        return "[Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=$konamiId&request_locale=$locale)"
    }

    $name = [string](Get-PropertyValue -Object $Card -Name "name")
    $encodedName = [Uri]::EscapeDataString($name)
    return "[search](https://www.db.yugioh-card.com/yugiohdb/card_search.action?keyword=$encodedName&ope=1&request_locale=en)"
}

$inputPath = (Resolve-Path -LiteralPath $InputJson).Path
$rawJson = [IO.File]::ReadAllText($inputPath, [Text.Encoding]::UTF8)
$payload = $rawJson | ConvertFrom-Json
$cards = @(Get-PropertyValue -Object $payload -Name "data")
if ($cards.Count -eq 0) {
    throw "The input JSON does not contain a non-empty data array."
}

$outputPath = [IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputMarkdown))
$outputDirectory = [IO.Path]::GetDirectoryName($outputPath)
[IO.Directory]::CreateDirectory($outputDirectory) | Out-Null

$inputHash = (Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash
$duplicateDatabaseIds = @($cards | Group-Object { Get-PropertyValue -Object $_ -Name "id" } | Where-Object { $_.Count -gt 1 }).Count
$duplicateNames = @($cards | Group-Object { Get-PropertyValue -Object $_ -Name "name" } | Where-Object { $_.Count -gt 1 }).Count
$missingKonamiIds = @($cards | Where-Object {
    $value = Get-PropertyValue -Object (Get-MiscInfo -Card $_) -Name "konami_id"
    $null -eq $value -or [long]$value -le 0
}).Count
$variableAttackCount = @($cards | Where-Object {
    $value = Get-PropertyValue -Object $_ -Name "atk"
    $null -ne $value -and [long]$value -lt 0
}).Count
$variableDefenseCount = @($cards | Where-Object {
    $value = Get-PropertyValue -Object $_ -Name "def"
    $null -ne $value -and [long]$value -lt 0
}).Count
$pendulumCount = @($cards | Where-Object { $null -ne (Get-PropertyValue -Object $_ -Name "scale") }).Count
$linkCount = @($cards | Where-Object { $null -ne (Get-PropertyValue -Object $_ -Name "linkval") }).Count
$cardsWithArchetype = @($cards | Where-Object {
    -not [string]::IsNullOrWhiteSpace([string](Get-PropertyValue -Object $_ -Name "archetype"))
}).Count
$distinctArchetypeLabels = @($cards |
    ForEach-Object { Get-PropertyValue -Object $_ -Name "archetype" } |
    Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
    Sort-Object -Unique).Count
$hasEffectCount = @($cards | Where-Object {
    [long](Get-PropertyValue -Object (Get-MiscInfo -Card $_) -Name "has_effect") -eq 1
}).Count
$noEffectCount = $cards.Count - $hasEffectCount

$introTemplate = @'
# Yu-Gi-Oh! Master Duel-kaartreferentie en contentcontract

> Implementatiestatus: onderzoeks- en dataspecificatie; geen claim dat alle effecten al door de engine worden uitgevoerd.
>
> Snapshot: `{{SNAPSHOT_DATE}}` · YGOPRODeck database `{{DATABASE_VERSION}}` (bronupdate `{{DATABASE_LAST_UPDATE}}`) · `{{CARD_COUNT}}` unieke kaarten met het `master duel`-formatlabel.

## 1. Doel en afbakening

Dit document is de implementatiereferentie voor statische kaartdefinities en de route naar uitvoerbare kaarteffecten. De volledige feitenindex onderaan bevat voor iedere kaart in de snapshot de stabiele ID, naam, kaartsoort, frame, Attribute, Type, Level/Rank/Link Rating, ATK, DEF, Pendulum Scale, Link Arrows, archetypelabel, Master Duel-rarity en een link naar de officiële Konami-detailpagina.

De index neemt **geen volledige officiële kaartteksten en geen illustraties** over. Die materialen zijn auteursrechtelijk beschermd. Exacte gelokaliseerde kaarttekst moet uit een door het project gelicentieerde of door de gebruiker aangeleverde bron komen. Voor handmatige verificatie verwijst iedere rij naar Yu-Gi-Oh! Neuron. De YGOPRODeck-snapshot is een praktische, niet-officiële cache en is niet op zichzelf het laatste woord over Master Duel-beschikbaarheid, errata of rulings.

Met “aanval” wordt in de datalaag **ATK** bedoeld. Benoemde aanvallen uit anime, marketing of animaties zijn geen universeel officieel TCG-kaartveld. Modelleer een eventuele `attackCueId` uitsluitend als optionele, originele of gelicentieerde presentatie-inhoud; nooit als duelregel.

### Bronvolgorde

| Prioriteit | Bron | Gebruik |
| --- | --- | --- |
| 1 | Laatste expliciete projectbeslissing | Gewenste productscope en bewust gekozen afwijkingen |
| 2 | [Konami Yu-Gi-Oh! Neuron Card Database](https://www.db.yugioh-card.com/yugiohdb/cardsearch.action?request_locale=en) | Officiële kaartnaam, actuele TCG-tekst, basisstats, kaartsoort en detailrulings |
| 3 | [Official Rulebook](https://www.yugioh-card.com/en/rulebook/) en [Problem-Solving Card Text](https://www.yugioh-card.com/ygo_cms/ygo/all/uploads/New_Terminology_and_card_text_for_webpage.pdf) | Algemene regels, timing, targeting, voorwaarden en conjuncties |
| 4 | Master Duel-client of door de gebruiker aangeleverde, rechtmatig verkregen data | Master Duel-specifieke tekstversie, rarity, availability en regulation |
| 5 | [YGOPRODeck API v7](https://ygoprodeck.com/api-guide/) | Versieerbare importcache en snelle veldnormalisatie |
| 6 | [Project Ignis BabelCDB](https://github.com/ProjectIgnis/BabelCDB) | Optionele open-source compatibiliteitsreferentie voor CDB-structuur; nooit verwarren met officiële Konami-data |

Conflicten worden vastgelegd; er wordt niet stilzwijgend één bron gekozen. Master Duel gebruikt een eigen, veranderlijke card pool en regulation. De [TCG Forbidden & Limited List](https://www.db.yugioh-card.com/yugiohdb/forbidden_limited.action) mag daarom niet als Master Duel-banlist worden geïmporteerd.

## 2. Snapshot en reproduceerbaarheid

| Eigenschap | Waarde |
| --- | --- |
| API-query | `https://db.ygoprodeck.com/api/v7/cardinfo.php?format=master%20duel&misc=yes` |
| Snapshotdatum | `{{SNAPSHOT_DATE}}` |
| Databaseversie | `{{DATABASE_VERSION}}` |
| Bronupdate | `{{DATABASE_LAST_UPDATE}}` |
| Kaarten | `{{CARD_COUNT}}` |
| Unieke `databaseId` | `{{UNIQUE_DATABASE_IDS}}` |
| Dubbele namen | `{{DUPLICATE_NAMES}}` |
| Ontbrekende Konami `cid` | `{{MISSING_KONAMI_IDS}}` (allemaal tokens in deze snapshot) |
| Kaarten met archetypelabel | `{{CARDS_WITH_ARCHETYPE}}`; `{{ARCHETYPE_COUNT}}` verschillende labels |
| Pendulum-kaarten | `{{PENDULUM_COUNT}}` |
| Link-kaarten | `{{LINK_COUNT}}` |
| Variabele ATK (`?`) | `{{VARIABLE_ATK_COUNT}}` |
| Variabele DEF (`?`) | `{{VARIABLE_DEF_COUNT}}` |
| `has_effect = true` | `{{HAS_EFFECT_COUNT}}` |
| `has_effect = false` | `{{NO_EFFECT_COUNT}}` |
| SHA-256 van bron-JSON | `{{INPUT_HASH}}` |

YGOPRODeck vraagt gebruikers de volledige response lokaal te cachen en hanteert maximaal 20 requests per seconde. Afbeeldingen mogen niet voortdurend worden ge-hotlinkt; download een geautoriseerd beeld één keer en host het lokaal. Prijsvelden zijn vluchtig en horen niet in de dueldefinitie.

Deze referentie is opnieuw te genereren met:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/YGOProDeck/GenerateMasterDuelCardReference.ps1 `
  -InputJson C:\tmp\icarus-master-duel-cardinfo-2026-08-10.json `
  -OutputMarkdown Documentation/YU_GI_OH_MASTER_DUEL_CARD_REFERENCE.md `
  -DatabaseVersion {{DATABASE_VERSION}} `
  -DatabaseLastUpdate "{{DATABASE_LAST_UPDATE}}" `
  -SnapshotDate {{SNAPSHOT_DATE}}
```

Bij een update: haal één nieuwe API-snapshot op, lees eerst `checkDBVer.php`, genereer opnieuw, inspecteer de diff en voer catalogusvalidatie uit. Een gewijzigde kaarttekst of rarity moet een expliciete contentversie opleveren, zodat replays niet stilzwijgend uiteenlopen.

## 3. Verdeling in deze snapshot

### Kaartsoorten

{{TYPE_COUNTS}}

### Attributes

{{ATTRIBUTE_COUNTS}}

### Master Duel-rarities

{{RARITY_COUNTS}}

De rarity komt uit de derde-partijcache en is geen duelregel. Bewaar crafting rarity en tijdelijke shop-/packinformatie buiten de onveranderlijke kaartdefinitie.

## 4. Canoniek kaartmodel

Gebruik immutable definitions en aparte per-duel instances. Een definitie beschrijft wat een kaart **is**; een instance beschrijft waar die kaart zich bevindt en wat er tijdens dit duel mee is gebeurd.

```yaml
cardDefinition:
  schemaVersion: 1
  contentVersion: "source-date-or-hash"
  databaseId: "89631139"          # passcode/API-id; string bewaart voorloopnullen
  databaseIdNumeric: 89631139      # alleen voor bestaande int-API's
  konamiCardId: 4007               # Neuron cid; niet hetzelfde als passcode
  aliasOfDatabaseId: null
  classification:
    kind: Monster                  # Monster | Spell | Trap | Token | Skill
    frame: Normal                  # zie volledige enum hieronder
    subtypes: [Normal]
    attribute: LIGHT
    race: Dragon                   # officieel “Type” bij monsters
    spellTrapSubtype: null
  stats:
    level: 8
    rank: null
    linkRating: null
    attack: { kind: Fixed, value: 3000 }
    defense: { kind: Fixed, value: 2500 }
    pendulumScale: null
    linkMarkers: []
  membership:
    archetypeLabels: [Blue-Eyes]   # zoeklabel; regels gebruiken setcodes/name predicates
    setCodes: []
    treatedAsDatabaseId: null
  localization:
    nameKey: "card.89631139.name"
    rulesTextKey: "card.89631139.text"
    pendulumTextKey: null
  availability:
    masterDuelAvailable: true
    masterDuelRarity: Common
    availabilitySource: "ygoprodeck-{{DATABASE_VERSION}}"
  implementation:
    effectHandlerIds: []
    validationState: StaticDataOnly
  presentation:
    artworkAssetId: null
    summonCueId: null
    attackCueId: null
```

### ID-regels

- `databaseId`/passcode is de primaire content-ID. Bewaar hem canoniek als tekst, omdat officiële weergave voorloopnullen kan bevatten. De huidige code gebruikt `int`; dat kan als compatibiliteitsveld blijven zolang serialisatie expliciet is.
- `konamiCardId` is de `cid` voor de officiële Neuron-detailpagina. Verwar die niet met passcode, artwork-ID, runtime instance-ID of setcode.
- Een `artworkId` identificeert een beeldvariant, niet een andere kaartdefinitie. De snapshot bevat alternatieve beeldrecords; die veranderen regels en decklimieten niet.
- Een per-duel `instanceId` moet deterministisch en uniek zijn en mag nooit als content-ID worden gebruikt.
- `aliasOfDatabaseId`, “treated as” en archetype-/setcode-membership zijn verschillende concepten. Modelleer ze afzonderlijk.

### Stats

- Bewaar `ATK ?` en `DEF ?` als `Variable`, niet als `0`. Deze snapshot bevat `{{VARIABLE_ATK_COUNT}}` variabele ATK- en `{{VARIABLE_DEF_COUNT}}` variabele DEF-waarden.
- Spell/Trap-statistieken zijn `NotApplicable`, geen nulwaarden.
- Link Monsters hebben Link Rating en Link Markers, geen DEF en geen Defense Position.
- Het API-veld `level` betekent Level voor reguliere monsters en Rank voor Xyz. Gebruik afzonderlijke domeinvelden.
- Pendulum Scale staat los van Level/Rank; Pendulum-monsters hebben zowel een Monster- als Spell-context.
- Originele ATK/DEF, huidige ATK/DEF, modifiers en overrides zijn afzonderlijke waarden. Recalculatie moet deterministisch zijn.

### Kaartfamilies die het schema moet dragen

- Main Deck: Normal, Effect, Ritual, Flip, Tuner, Spirit, Union, Gemini, Toon, Special Summon en alle Pendulum-combinaties.
- Extra Deck: Fusion, Synchro, Xyz en Link, inclusief Tuner- en Pendulum-hybriden.
- Spells: Normal, Continuous, Equip, Field, Quick-Play en Ritual.
- Traps: Normal, Continuous en Counter.
- Overig: Tokens en eventueel Skill Cards voor andere formats. Tokens horen niet in Main/Extra/Side Deck en sommige hebben geen Neuron `cid`.
- Monster Types: Aqua, Beast, Beast-Warrior, Creator-God, Cyberse, Dinosaur, Divine-Beast, Dragon, Fairy, Fiend, Fish, Illusion, Insect, Machine, Plant, Psychic, Pyro, Reptile, Rock, Sea Serpent, Spellcaster, Thunder, Warrior, Winged Beast, Wyrm en Zombie.
- Attributes: DARK, DIVINE, EARTH, FIRE, LIGHT, WATER en WIND.

## 5. Van kaarttekst naar uitvoerbaar effect

Kaarttekst is geen veilig scriptformaat. Splits iedere kaart op in één of meer getypeerde `EffectUnit`s. Kosten worden betaald vóór chain-resolutie; targets worden bij activatie gekozen; niet-targetende keuzes gebeuren volgens de tekst en timing tijdens resolutie.

```yaml
effectUnit:
  effectId: "card.12345678.effect.1"
  kind: Activation                 # Activation | Ignition | Trigger | Quick | Continuous
  sourceLocations: [Hand]
  timing:
    event: CardDestroyed
    phase: Any
    optional: true
    spellSpeed: 2
  activationCondition: predicate-id
  cost: [typed-cost-step]
  targets: [typed-target-spec]
  resolution: [typed-operation]
  limits:
    kind: HardOncePerTurn          # none | per-copy | per-name | per-duel etc.
    sharedGroupId: null
  restrictions: []
  expiry: null
```

Minimaal te modelleren effectonderdelen:

| Onderdeel | Vereiste data |
| --- | --- |
| Activatie | eigenaar/controller, toegestane locatie, fase/step, open chain window, Spell Speed |
| Voorwaarde | huidige staat, vorige gebeurtenis, “if”/“when”, mandatory/optional, once-limit |
| Kosten | LP, discard, Tribute, detach, banish, send, reveal, counters; atomair en niet terugdraaien na negatie |
| Targets | aantal, zones, face-up/down, controller/eigenaar, type/stat/filter, nog steeds legaal bij resolutie |
| Resolutie | geordende operaties, simultaan versus opeenvolgend, gedeeltelijke resolutie, “then/also/and if you do” |
| Verplaatsing | bron, bestemming, positie, zichtbaarheid, reden, ownership/controller en previous location |
| Modifiers | stat/type/name/attribute/level, continuous versus lingering, dependency en expiry |
| Negatie | activation negation, effect negation en tijdelijk disabled zijn verschillende toestanden |
| Summon | procedure, materials, zones, proper summon, summon-negation window en restrictions |
| State checks | destruction, win/lose, zero LP, illegal attachments en trigger collection na resolutie |

Families die generieke engine-primitieven nodig hebben voordat duizenden kaarten schaalbaar worden: draw/search/excavate; send/discard/tribute/destroy/banish/return/shuffle; Special Summon en alle summonmethodes; target/select/choose; counters en Xyz materials; equip/continuous/field effects; control change; token generation; battle modifiers, piercing en multiple attacks; effect/activation/summon negation; replacement; name/type/attribute treatment; copy effects; once-per-turn/per-duel shared limits; delayed triggers en End Phase cleanup.

Problem-Solving Card Text-conjuncties zijn semantisch. Leg `and`, `and if you do`, `then`, `also` en `also, after that` niet vast als gewone tekstsplitsing. Gebruik officiële regels en kaart-specifieke rulings. Bij twijfel wordt de kaart `NeedsRulingReview`, niet “ongeveer werkend”.

## 6. Master Duel-specifieke lagen

Houd deze veranderlijke gegevens buiten de statische kaartdefinitie:

- card-pool availability met `effectiveFrom`/`effectiveTo`;
- regulation/banlist per regio, seizoen of event;
- crafting rarity, dismantle/craft-economie en shop-/packbronnen;
- errata- en localisatieversies;
- alternatieve art, finish, mates, cut-ins, summon- en attack-cues;
- event-only deckregels.

Voor iedere replay of netwerkmatch moeten `rulesVersion`, `catalogVersion`, `regulationVersion` en random seed worden opgeslagen. Een mismatch moet worden geweigerd of gemigreerd, nooit stil gecorrigeerd.

## 7. Projectanalyse en migratiepad

Huidige projectstatus op `{{SNAPSHOT_DATE}}`:

| Bestand | Wat bestaat | Bekende kloof |
| --- | --- | --- |
| `Assets/Scripts/MasterDuelCore/YgoCardDatabase.cs` | Eigen read-only parser voor `CardDatabase/cards.cdb`; leest ID, OT, alias, setcode, typeflags, ATK/DEF, packed level, race, attribute, category, naam en tekst | Verwachte `Assets/StreamingAssets/CardDatabase/cards.cdb` ontbreekt; scale, Link Rating/markers, regio, rarity en localisatie worden niet als domeinvelden aangeboden |
| `Assets/Scripts/MasterDuelCore/MasterDuelRuntime.cs` | `CardData`, basisframes, zones en eenvoudige duelstroom | Eén `Level`-veld voor Level/Rank/Link; geen Attribute/Race/setcode/linkmarkers/scale; effects gekoppeld aan losse strings |
| `Assets/Scripts/CleanDuel/CleanDuelModels.cs` | Werkende compacte definitie en per-duel state | Te smal voor volledige kaartpool; card-specific flags groeien snel |
| `Assets/Scripts/CleanDuel/CleanCardLibrary.cs` | Handmatige Blue-Eyes fallbacks en koppeling naar CDB | Volledige tekst/fallbacks en `effectKey` zijn handwerk; onbekende kaarten worden generieke monsters |
| `Assets/Scripts/Editor/YgoprodeckLuaImageDownloaderWindow.cs` | API-cache en gerate-limiteerde beelddownload | Importeert geen volledige statische kaartcatalogus of version manifest |

Kritieke correcties vóór brede kaartimport:

1. Verwijder de betekenisfout waarbij negatieve CDB-stats naar `0` worden genormaliseerd; gebruik `Variable`.
2. Splits Level, Rank en Link Rating; decodeer Pendulum Scale en Link Markers zonder DEF te misbruiken.
3. Voeg Attribute, Race, subtypes, setcode-/archetypepredicaten, alias/treatment en localisatiekeys toe.
4. Maak één immutable `CardDefinition` die zowel `MasterDuelRuntime` als `CleanDuel` consumeert; vermijd twee divergerende modellen.
5. Bouw een idempotente importer plus manifest met bronversie, snapshot-hash, tellingen en validatiefouten.
6. Houd kaarttekst/content en uitvoerbare effecten gescheiden. Een geïmporteerde kaart is pas `Playable` nadat haar effectfamilies en uitzonderingen getest zijn.
7. Vervang een centrale switch per kaart door geregistreerde, getypeerde handlers/primitives met expliciete uitzonderingshandlers.

Aanbevolen status per kaart:

`Discovered -> StaticDataValidated -> TextLicensed -> EffectsModeled -> RulesTested -> IntegrationTested -> Playable`

## 8. Acceptatiecriteria per kaart

Een kaart is pas compleet wanneer:

- identiteit, naam, classification, Attribute/Type, stats en speciale waarden tegen de officiële detailpagina zijn gecontroleerd;
- exacte tekstversie, taal en errata-herkomst zijn opgeslagen;
- alle afzonderlijke effecten conditions, costs, targets, resolution, limits, restrictions en expiry hebben;
- Main/Extra Deck-classificatie, summonprocedure en material constraints kloppen;
- legale én illegale acties, cancellation, negation, partial resolution en zone-capacity zijn getest;
- once-per-turn/per-name/per-duel scopes ook met meerdere copies, name changes en control changes getest zijn;
- hidden information, owner/controller, face-down state en previous location correct blijven;
- replay/determinismetest met vaste seed slaagt;
- relevante officiële rulings/conflicten zijn gelinkt;
- art/audio/VFX een aantoonbare licentie of originele placeholder hebben;
- Master Duel-availability en regulation afzonderlijk zijn gevalideerd.

Minimale regressietestnamen:

```text
<CardId>_CannotActivate_WhenConditionFalse
<CardId>_PaysCost_BeforeChainResolution
<CardId>_KeepsPaidCost_WhenActivationNegated
<CardId>_RevalidatesTargets_OnResolution
<CardId>_ResolvesPartially_WhenAllowed
<CardId>_HonorsSharedOnceLimit
<CardId>_ExpiresLingeringModifier_AtCorrectWindow
<CardId>_ProducesDeterministicEvents
```

## 9. Cataloguslegenda

- `ID`: passcode/API database ID; waarden korter dan acht cijfers worden voor weergave met nullen aangevuld. Een incidentele langere officiële database-ID blijft langer.
- `CID`: Konami Neuron card ID. De vijf tokens zonder CID linken naar zoeken op naam.
- `Klasse`: gedetailleerde kaartsoort zoals de importcache die levert.
- `Lv/Rk/Link`: expliciet gelabeld om Xyz Rank en Link Rating niet als Level te behandelen.
- `ATK`/`DEF`: `?` is variabel; `—` is niet van toepassing/ontbreekt.
- `P`: Pendulum Scale.
- `Pijlen`: N, NE, E, SE, S, SW, W, NW.
- `Effect`: metadata-indicatie dat de kaart feitelijke effecttekst heeft; dit zegt niets over implementatiestatus.
- `Officieel`: Konami Neuron-detailpagina; daar staat de actuele officiële TCG-tekst en kunnen rulings worden geraadpleegd.

## 10. Volledige Master Duel-feitenindex

> Gegenereerd uit de versieerbare snapshot. Master Duel-formatlabel en rarity zijn afkomstig uit YGOPRODeck; verifieer releasegevoelige informatie in de actuele client. Exacte effecttekst en artwork zijn bewust niet gedupliceerd.

| ID | CID | Kaart | Klasse | Frame | Attr. | Type/Race | Lv/Rk/Link | ATK | DEF | P | Pijlen | Archetype | MD-rarity | Effect | Officieel |
| --- | ---: | --- | --- | --- | --- | --- | --- | ---: | ---: | ---: | --- | --- | --- | --- | --- |
'@

function Format-CountTable {
    param(
        [object[]]$Items,
        [string]$ValueName
    )

    $lines = @("| $ValueName | Aantal |", "| --- | ---: |")
    foreach ($group in ($Items | Group-Object | Sort-Object Name)) {
        $label = Escape-MarkdownCell -Value $group.Name
        $lines += "| $label | $($group.Count) |"
    }

    return $lines -join "`r`n"
}

$typeCounts = Format-CountTable -Items @($cards | ForEach-Object { Get-PropertyValue -Object $_ -Name "type" }) -ValueName "Kaartsoort"
$attributeCounts = Format-CountTable -Items @($cards | ForEach-Object {
    $value = Get-PropertyValue -Object $_ -Name "attribute"
    if ($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)) { "Niet van toepassing" } else { $value }
}) -ValueName "Attribute"
$rarityCounts = Format-CountTable -Items @($cards | ForEach-Object {
    Get-PropertyValue -Object (Get-MiscInfo -Card $_) -Name "md_rarity"
}) -ValueName "Rarity"

$intro = $introTemplate
$intro = $intro.Replace("{{SNAPSHOT_DATE}}", $SnapshotDate)
$intro = $intro.Replace("{{DATABASE_VERSION}}", $DatabaseVersion)
$intro = $intro.Replace("{{DATABASE_LAST_UPDATE}}", $DatabaseLastUpdate)
$intro = $intro.Replace("{{CARD_COUNT}}", [string]$cards.Count)
$intro = $intro.Replace("{{UNIQUE_DATABASE_IDS}}", [string]($cards.Count - $duplicateDatabaseIds))
$intro = $intro.Replace("{{DUPLICATE_NAMES}}", [string]$duplicateNames)
$intro = $intro.Replace("{{MISSING_KONAMI_IDS}}", [string]$missingKonamiIds)
$intro = $intro.Replace("{{CARDS_WITH_ARCHETYPE}}", [string]$cardsWithArchetype)
$intro = $intro.Replace("{{ARCHETYPE_COUNT}}", [string]$distinctArchetypeLabels)
$intro = $intro.Replace("{{PENDULUM_COUNT}}", [string]$pendulumCount)
$intro = $intro.Replace("{{LINK_COUNT}}", [string]$linkCount)
$intro = $intro.Replace("{{VARIABLE_ATK_COUNT}}", [string]$variableAttackCount)
$intro = $intro.Replace("{{VARIABLE_DEF_COUNT}}", [string]$variableDefenseCount)
$intro = $intro.Replace("{{HAS_EFFECT_COUNT}}", [string]$hasEffectCount)
$intro = $intro.Replace("{{NO_EFFECT_COUNT}}", [string]$noEffectCount)
$intro = $intro.Replace("{{INPUT_HASH}}", $inputHash)
$intro = $intro.Replace("{{TYPE_COUNTS}}", $typeCounts)
$intro = $intro.Replace("{{ATTRIBUTE_COUNTS}}", $attributeCounts)
$intro = $intro.Replace("{{RARITY_COUNTS}}", $rarityCounts)

$utf8WithoutBom = New-Object Text.UTF8Encoding($false)
$writer = New-Object IO.StreamWriter($outputPath, $false, $utf8WithoutBom)
try {
    $writer.WriteLine($intro)

    $sortedCards = $cards | Sort-Object `
        @{ Expression = { [string](Get-PropertyValue -Object $_ -Name "name") }; Ascending = $true }, `
        @{ Expression = { [long](Get-PropertyValue -Object $_ -Name "id") }; Ascending = $true }

    foreach ($card in $sortedCards) {
        $misc = Get-MiscInfo -Card $card
        $databaseId = Format-DatabaseId -Value (Get-PropertyValue -Object $card -Name "id")
        $konamiId = Escape-MarkdownCell -Value (Get-PropertyValue -Object $misc -Name "konami_id")
        $name = Escape-MarkdownCell -Value (Get-PropertyValue -Object $card -Name "name")
        $type = Escape-MarkdownCell -Value (Get-PropertyValue -Object $card -Name "type")
        $frame = Escape-MarkdownCell -Value (Get-PropertyValue -Object $card -Name "frameType")
        $attribute = Escape-MarkdownCell -Value (Get-PropertyValue -Object $card -Name "attribute")
        $race = Escape-MarkdownCell -Value (Get-PropertyValue -Object $card -Name "race")
        $levelRankLink = Format-LevelRankLink -Card $card
        $attack = Format-Stat -Value (Get-PropertyValue -Object $card -Name "atk")
        $defense = Format-Stat -Value (Get-PropertyValue -Object $card -Name "def")
        $scale = Format-Stat -Value (Get-PropertyValue -Object $card -Name "scale")
        $markers = Format-LinkMarkers -Card $card
        $archetype = Escape-MarkdownCell -Value (Get-PropertyValue -Object $card -Name "archetype")
        $rarity = Escape-MarkdownCell -Value (Get-PropertyValue -Object $misc -Name "md_rarity")
        $hasEffectValue = Get-PropertyValue -Object $misc -Name "has_effect"
        $hasEffect = if ($null -ne $hasEffectValue -and [long]$hasEffectValue -eq 1) { "ja" } else { "nee" }
        $officialLink = Format-OfficialLink -Card $card -Misc $misc

        $writer.WriteLine("| $databaseId | $konamiId | $name | $type | $frame | $attribute | $race | $levelRankLink | $attack | $defense | $scale | $markers | $archetype | $rarity | $hasEffect | $officialLink |")
    }
}
finally {
    $writer.Dispose()
}

$outputItem = Get-Item -LiteralPath $outputPath
[pscustomobject]@{
    Output = $outputItem.FullName
    Bytes = $outputItem.Length
    Cards = $cards.Count
    InputSha256 = $inputHash
}
