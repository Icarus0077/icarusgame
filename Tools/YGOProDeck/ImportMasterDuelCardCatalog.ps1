param(
    [string]$InputMarkdown = "Documentation/YU_GI_OH_MASTER_DUEL_CARD_REFERENCE.md",
    [string]$OutputTsv = "Assets/StreamingAssets/CardDatabase/master_duel_catalog.tsv",
    [string]$OutputManifest = "Assets/StreamingAssets/CardDatabase/master_duel_catalog.manifest.json"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Split-MarkdownRow {
    param([string]$Line)

    $cells = New-Object System.Collections.Generic.List[string]
    $current = New-Object System.Text.StringBuilder
    $escaped = $false
    for ($index = 1; $index -lt ($Line.Length - 1); $index++) {
        $character = $Line[$index]
        if ($escaped) {
            [void]$current.Append($character)
            $escaped = $false
        }
        elseif ($character -eq '\') {
            $escaped = $true
        }
        elseif ($character -eq '|') {
            $cells.Add($current.ToString().Trim())
            [void]$current.Clear()
        }
        else {
            [void]$current.Append($character)
        }
    }

    $cells.Add($current.ToString().Trim())
    return $cells.ToArray()
}

function Normalize-Cell {
    param([object]$Value)

    if ($null -eq $Value) {
        return ""
    }

    $text = [Net.WebUtility]::HtmlDecode([string]$Value).Trim()
    if ($text -eq [string][char]0x2014) {
        return ""
    }

    return $text
}

function Escape-TsvCell {
    param([object]$Value)

    $text = Normalize-Cell $Value
    return $text.Replace("\", "\\").Replace("`t", "\t").Replace("`r", "\r").Replace("`n", "\n")
}

function Find-HeaderIndex {
    param(
        [string[]]$Headers,
        [string[]]$Names
    )

    foreach ($name in $Names) {
        for ($index = 0; $index -lt $Headers.Count; $index++) {
            if ([string]::Equals($Headers[$index], $name, [StringComparison]::OrdinalIgnoreCase)) {
                return $index
            }
        }
    }

    return -1
}

function Get-OptionalCell {
    param(
        [string[]]$Cells,
        [int]$Index
    )

    if ($Index -lt 0 -or $Index -ge $Cells.Count) {
        return ""
    }

    return Normalize-Cell $Cells[$Index]
}

function Parse-Stat {
    param([string]$Value)

    $normalized = Normalize-Cell $Value
    if ([string]::IsNullOrEmpty($normalized)) {
        return @("NotApplicable", "")
    }

    if ($normalized -eq "?") {
        return @("Variable", "")
    }

    $parsed = 0
    if (-not [int]::TryParse($normalized, [ref]$parsed)) {
        throw "Invalid numeric stat '$normalized'."
    }

    return @("Fixed", [string]$parsed)
}

$inputPath = (Resolve-Path -LiteralPath $InputMarkdown).Path
$utf8NoBom = New-Object Text.UTF8Encoding($false)
$lines = [IO.File]::ReadAllLines($inputPath, [Text.Encoding]::UTF8)
$headerLineIndex = -1
for ($lineIndex = 0; $lineIndex -lt $lines.Length; $lineIndex++) {
    if ($lines[$lineIndex].StartsWith("| ID | CID | Kaart |", [StringComparison]::Ordinal)) {
        $headerLineIndex = $lineIndex
        break
    }
}

if ($headerLineIndex -lt 0) {
    throw "The Master Duel facts table header was not found in $InputMarkdown."
}

$headers = @(Split-MarkdownRow $lines[$headerLineIndex])
$requiredColumns = @("ID", "CID", "Kaart", "Klasse", "Frame", "Attr.", "Type/Race", "Lv/Rk/Link", "ATK", "DEF", "P", "Pijlen", "Archetype", "MD-rarity", "Effect", "Officieel")
$columnIndexes = @{}
foreach ($column in $requiredColumns) {
    $columnIndex = Find-HeaderIndex $headers @($column)
    if ($columnIndex -lt 0) {
        throw "Required column '$column' is missing from the Master Duel facts table."
    }

    $columnIndexes[$column] = $columnIndex
}

# Optional forward-compatible columns. The importer does not infer executable rules
# from prose: an effect handler and implementation status must be supplied explicitly.
$textIndex = Find-HeaderIndex $headers @("Tekst", "Kaarttekst", "Official text", "OfficialText")
$handlerIndex = Find-HeaderIndex $headers @("Effect-handler", "Effect-ID", "EffectHandlerId")
$statusIndex = Find-HeaderIndex $headers @("Implementatiestatus", "Status", "ImplementationStatus")
$aliasIndex = Find-HeaderIndex $headers @("Alias ID", "Alias", "AliasDatabaseId")
$setCodesIndex = Find-HeaderIndex $headers @("Setcodes", "Setcode", "SetCodes")
$treatedAsIndex = Find-HeaderIndex $headers @("Behandeld als", "Treated as", "TreatedAs")
$localizationKeyIndex = Find-HeaderIndex $headers @("Localisatiesleutel", "Localization key", "LocalizationKey")
$textLocaleIndex = Find-HeaderIndex $headers @("Teksttaal", "Text locale", "TextLocale", "Locale")

$outputLines = New-Object System.Collections.Generic.List[string]
$outputLines.Add("databaseId`tcid`tname`tcardClass`tframe`tattribute`traceOrType`taliasDatabaseId`tsetCodes`ttreatedAs`tlocalizationKey`ttextLocale`tlevel`trank`tlinkRating`tattackKind`tattackValue`tdefenseKind`tdefenseValue`tpendulumScale`tlinkMarkers`tarchetype`trarity`thasEffect`tofficialUrl`tofficialText`teffectHandlerId`timplementationStatus")
$seenIds = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)

for ($lineIndex = $headerLineIndex + 2; $lineIndex -lt $lines.Length; $lineIndex++) {
    $line = $lines[$lineIndex]
    if (-not $line.StartsWith("|", [StringComparison]::Ordinal)) {
        break
    }

    $cells = @(Split-MarkdownRow $line)
    if ($cells.Count -lt $requiredColumns.Count) {
        throw "Catalog row $($lineIndex + 1) has $($cells.Count) cells; expected at least $($requiredColumns.Count)."
    }

    $databaseId = Normalize-Cell $cells[$columnIndexes["ID"]]
    $numericId = 0L
    if (-not [long]::TryParse($databaseId, [ref]$numericId)) {
        throw "Catalog row $($lineIndex + 1) has invalid database ID '$databaseId'."
    }

    if (-not $seenIds.Add($databaseId)) {
        throw "Duplicate database ID '$databaseId'."
    }

    $level = ""
    $rank = ""
    $linkRating = ""
    $rating = Normalize-Cell $cells[$columnIndexes["Lv/Rk/Link"]]
    if ($rating -match '^Level\s+(\d+)$') {
        $level = $Matches[1]
    }
    elseif ($rating -match '^Rank\s+(\d+)$') {
        $rank = $Matches[1]
    }
    elseif ($rating -match '^Link\s+(\d+)$') {
        $linkRating = $Matches[1]
    }
    elseif (-not [string]::IsNullOrEmpty($rating)) {
        throw "Catalog row $($lineIndex + 1) has unsupported Level/Rank/Link value '$rating'."
    }

    $attack = Parse-Stat $cells[$columnIndexes["ATK"]]
    $defense = Parse-Stat $cells[$columnIndexes["DEF"]]
    $officialCell = Normalize-Cell $cells[$columnIndexes["Officieel"]]
    $officialUrl = if ($officialCell -match '\((https?://[^)]+)\)') { $Matches[1] } else { "" }
    $officialText = Get-OptionalCell $cells $textIndex
    $effectHandlerId = Get-OptionalCell $cells $handlerIndex
    $implementationStatus = Get-OptionalCell $cells $statusIndex
    if ([string]::IsNullOrEmpty($implementationStatus)) {
        if ([string]::IsNullOrEmpty($officialText)) {
            $implementationStatus = "StaticDataValidated"
        }
        elseif ([string]::IsNullOrEmpty($effectHandlerId)) {
            $implementationStatus = "TextLicensed"
        }
        else {
            $implementationStatus = "EffectsModeled"
        }
    }

    $hasEffect = (Normalize-Cell $cells[$columnIndexes["Effect"]]) -eq "ja"
    $values = @(
        $databaseId,
        (Normalize-Cell $cells[$columnIndexes["CID"]]),
        (Normalize-Cell $cells[$columnIndexes["Kaart"]]),
        (Normalize-Cell $cells[$columnIndexes["Klasse"]]),
        (Normalize-Cell $cells[$columnIndexes["Frame"]]),
        (Normalize-Cell $cells[$columnIndexes["Attr."]]),
        (Normalize-Cell $cells[$columnIndexes["Type/Race"]]),
        (Get-OptionalCell $cells $aliasIndex),
        (Get-OptionalCell $cells $setCodesIndex),
        (Get-OptionalCell $cells $treatedAsIndex),
        (Get-OptionalCell $cells $localizationKeyIndex),
        (Get-OptionalCell $cells $textLocaleIndex),
        $level,
        $rank,
        $linkRating,
        $attack[0],
        $attack[1],
        $defense[0],
        $defense[1],
        (Normalize-Cell $cells[$columnIndexes["P"]]),
        (Normalize-Cell $cells[$columnIndexes["Pijlen"]]),
        (Normalize-Cell $cells[$columnIndexes["Archetype"]]),
        (Normalize-Cell $cells[$columnIndexes["MD-rarity"]]),
        ([string]$hasEffect).ToLowerInvariant(),
        $officialUrl,
        $officialText,
        $effectHandlerId,
        $implementationStatus
    ) | ForEach-Object { Escape-TsvCell $_ }
    $outputLines.Add($values -join "`t")
}

if ($seenIds.Count -eq 0) {
    throw "No card rows were imported."
}

$outputPath = [IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputTsv))
$manifestPath = [IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputManifest))
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($outputPath)) | Out-Null
[IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($manifestPath)) | Out-Null
[IO.File]::WriteAllLines($outputPath, $outputLines, $utf8NoBom)

$sourceHash = (Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash.ToLowerInvariant()
$catalogHash = (Get-FileHash -LiteralPath $outputPath -Algorithm SHA256).Hash.ToLowerInvariant()
$snapshotDate = "unknown"
$databaseVersion = "unknown"
$databaseLastUpdate = "unknown"
foreach ($line in $lines) {
    if ($line -match 'Snapshot:\s*`([^`]+)`.*database\s*`([^`]+)`\s*\(bronupdate\s*`([^`]+)`\)') {
        $snapshotDate = $Matches[1]
        $databaseVersion = $Matches[2]
        $databaseLastUpdate = $Matches[3]
        break
    }
}

$manifest = [ordered]@{
    schemaVersion = 2
    source = [IO.Path]::GetFileName($inputPath)
    snapshotDate = $snapshotDate
    databaseVersion = $databaseVersion
    databaseLastUpdate = $databaseLastUpdate
    sourceSha256 = $sourceHash
    catalogSha256 = $catalogHash
    cardCount = $seenIds.Count
    generatedFromStaticReference = $true
    defaultImplementationStatus = "StaticDataValidated"
    validationErrors = @()
} | ConvertTo-Json
[IO.File]::WriteAllText($manifestPath, $manifest + "`n", $utf8NoBom)

Write-Output "Imported $($seenIds.Count) Master Duel cards to $outputPath"
Write-Output "Catalog SHA-256: $catalogHash"
