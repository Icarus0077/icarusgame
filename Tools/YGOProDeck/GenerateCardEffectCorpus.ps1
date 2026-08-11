param(
    [Parameter(Mandatory = $true)]
    [string]$InputJson,

    [string]$CatalogTsv = "Assets/StreamingAssets/CardDatabase/master_duel_catalog.tsv",

    [string]$ExistingBatchDirectory = "Documentation/CardEffectBatches",

    [string]$OutputDirectory = "Documentation/CardEffectCorpus",

    [ValidateRange(10, 500)]
    [int]$CardsPerShard = 100,

    [string]$SnapshotDate = "2026-08-10",

    [ValidateRange(0, 20000)]
    [int]$MaxCards = 0
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

function Format-DatabaseId {
    param([object]$Value)

    if ($null -eq $Value) {
        return ""
    }

    $text = [string]$Value
    if ($text.Length -lt 8) {
        return $text.PadLeft(8, "0")
    }

    return $text
}

function Get-StringSha256 {
    param([string]$Text)

    $sha = [Security.Cryptography.SHA256]::Create()
    try {
        $bytes = [Text.Encoding]::UTF8.GetBytes($(if ($null -eq $Text) { "" } else { $Text }))
        $hash = $sha.ComputeHash($bytes)
        return ([BitConverter]::ToString($hash)).Replace("-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function New-StatSpec {
    param([object]$Value)

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) {
        return [ordered]@{ kind = "NotApplicable"; value = $null }
    }

    $numeric = [long]$Value
    if ($numeric -lt 0) {
        return [ordered]@{ kind = "Variable"; value = $null }
    }

    return [ordered]@{ kind = "Fixed"; value = $numeric }
}

function Format-DisplayStat {
    param([object]$Value)

    if ($null -eq $Value -or [string]::IsNullOrWhiteSpace([string]$Value)) {
        return "n/a"
    }

    if ([long]$Value -lt 0) {
        return "?"
    }

    return [string]$Value
}

function Escape-MarkdownCell {
    param([object]$Value)

    if ($null -eq $Value) {
        return "n/a"
    }

    $text = [string]$Value
    if ([string]::IsNullOrWhiteSpace($text)) {
        return "n/a"
    }

    return $text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("|", "\|").Replace("`r", " ").Replace("`n", " ")
}

function Escape-MarkdownHeading {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return "Unnamed card"
    }

    return $Value.Replace("`r", " ").Replace("`n", " ").Replace("#", "&#35;")
}

function Get-ManualCardIds {
    param([string]$Directory)

    $ids = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
    if (-not (Test-Path -LiteralPath $Directory -PathType Container)) {
        return $ids
    }

    foreach ($file in (Get-ChildItem -LiteralPath $Directory -File -Filter "BATCH_*.md" | Sort-Object Name)) {
        foreach ($match in (Select-String -LiteralPath $file.FullName -Pattern '^databaseId: "([0-9]+)"').Matches) {
            [void]$ids.Add((Format-DatabaseId $match.Groups[1].Value))
        }
    }

    return $ids
}

function Split-EffectClauses {
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return @()
    }

    $bullet = [string][char]0x25CF
    $normalized = $Text.Replace("`r", "").Replace($bullet, "`n" + $bullet)
    $clauses = New-Object System.Collections.Generic.List[string]
    foreach ($rawLine in ($normalized -split "`n")) {
        $line = $rawLine.Trim()
        if ([string]::IsNullOrWhiteSpace($line)) {
            continue
        }

        if ($line -match '^\[\s*([^\]]+)\s*\]$') {
            $clauses.Add($line)
            continue
        }

        $sentences = [Text.RegularExpressions.Regex]::Split($line, '(?<=[.!?])\s+(?=["(A-Z0-9\u25CF])')
        foreach ($sentence in $sentences) {
            $trimmed = $sentence.Trim()
            if (-not [string]::IsNullOrWhiteSpace($trimmed)) {
                $clauses.Add($trimmed)
            }
        }
    }

    return $clauses.ToArray()
}

function Get-ClauseKind {
    param(
        [string]$Clause,
        [string]$CardType,
        [int]$Index
    )

    if ($Clause -match '^\[') {
        return "Section"
    }

    if ($Index -eq 0 -and $CardType -match 'Fusion|Synchro|XYZ|Link' -and
        ($Clause -match '\s\+\s|^\d+\+?\s+.*monsters?|Tuner|Link Material')) {
        return "SummonMaterial"
    }

    if ($Clause.StartsWith([string][char]0x25CF, [StringComparison]::Ordinal)) {
        return "SelectableOption"
    }

    if ($Clause -match '(?i)only (use|activate)|once per turn|once per duel|once while face-up') {
        if ($Clause -match '^(?i)You can only|^Once per') {
            return "Limit"
        }
    }

    if ($Clause -match '(?i)Quick Effect') {
        return "Quick"
    }

    if ($Clause -match '^(?i)(If|When|During|After|At the start|At the end|Each time|The first time)\b') {
        return "Trigger"
    }

    if ($Clause -match '^(?i)(Cannot|Must|Neither player|Your opponent cannot|All |Other |This card is|This card cannot|Gains |Unaffected by)') {
        return "ContinuousOrCondition"
    }

    if ($CardType -match 'Spell|Trap') {
        return "SpellTrapRule"
    }

    if ($Clause -match '(?i)You can|Once per turn:') {
        return "Ignition"
    }

    return "Rule"
}

function Get-TaggedValues {
    param(
        [string]$Text,
        [ValidateSet("Mechanic", "Zone", "Timing", "Cost", "Operation")]
        [string]$Category
    )

    $patterns = switch ($Category) {
        "Mechanic" {
            [ordered]@{
                Draw = '(?i)\bdraw\b'
                Search = '(?i)add .+ from (your|the) Deck .+ hand|\bsearch\b'
                Excavate = '(?i)\bexcavat'
                Reveal = '(?i)\breveal'
                SpecialSummon = '(?i)Special Summon'
                NormalSummon = '(?i)Normal Summon'
                RitualSummon = '(?i)Ritual Summon'
                FusionSummon = '(?i)Fusion Summon'
                SynchroSummon = '(?i)Synchro Summon'
                XyzSummon = '(?i)Xyz Summon'
                LinkSummon = '(?i)Link Summon'
                Destroy = '(?i)\bdestroy'
                Banish = '(?i)\bbanish'
                SendToGraveyard = '(?i)send .+ to the GY|send .+ to your GY|sent to the GY'
                Discard = '(?i)\bdiscard'
                Tribute = '(?i)\bTribute'
                DetachMaterial = '(?i)\bdetach'
                AttachMaterial = '(?i)attach .+ material|as material'
                ReturnToHand = '(?i)return .+ hand|return it to the hand'
                ReturnToDeck = '(?i)return .+ Deck|shuffle .+ into the Deck'
                Target = '(?i)\btarget'
                Negate = '(?i)\bnegate'
                Equip = '(?i)\bequip'
                Counter = '(?i)\bcounter'
                Token = '(?i)\bToken'
                ControlChange = '(?i)take control|switch control|control of it'
                AttackModifier = '(?i)(gain|lose|becomes?|double|halve).{0,35}\bATK\b|\bATK becomes|increase .+ ATK|decrease .+ ATK'
                DefenseModifier = '(?i)(gain|lose|becomes?|double|halve).{0,35}\bDEF\b|\bDEF becomes|increase .+ DEF|decrease .+ DEF'
                LevelRankModifier = '(?i)(increase|decrease|becomes?|change).{0,40}\b(Level|Rank)\b'
                BattlePosition = '(?i)battle position|Defense Position|Attack Position'
                Piercing = '(?i)\bpiercing\b'
                DirectAttack = '(?i)attack directly|direct attack'
                MultipleAttack = '(?i)attack .+ times|second attack|multiple attacks'
                BattleDamage = '(?i)battle damage'
                EffectDamage = '(?i)inflict .+ damage|effect damage'
                LifePoints = '(?i)\bLP\b|Life Points'
                Protection = '(?i)cannot be (destroyed|targeted|banished|Tributed)|unaffected by|is not affected'
                CopyEffect = '(?i)copy .+ effect|this effect becomes|apply that effect'
                Random = '(?i)random|coin toss|roll a six-sided die'
                OncePerTurn = '(?i)once per turn|only use .+ per turn|only activate .+ per turn'
                OncePerDuel = '(?i)once per Duel'
                QuickEffect = '(?i)Quick Effect'
                Pendulum = '(?i)Pendulum'
                Material = '(?i)Fusion Material|Synchro Material|Xyz Material|Link Material|as material'
            }
        }
        "Zone" {
            [ordered]@{
                Hand = '(?i)\bhand\b'
                Deck = '(?i)\bDeck\b'
                ExtraDeck = '(?i)Extra Deck'
                Graveyard = '(?i)\bGY\b|Graveyard'
                Banished = '(?i)banished|banishment'
                MonsterZone = '(?i)Monster Zone'
                SpellTrapZone = '(?i)Spell & Trap Zone|Spell/Trap Zone'
                FieldZone = '(?i)Field Zone'
                PendulumZone = '(?i)Pendulum Zone'
                Field = '(?i)\bfield\b'
            }
        }
        "Timing" {
            [ordered]@{
                DrawPhase = '(?i)Draw Phase'
                StandbyPhase = '(?i)Standby Phase'
                MainPhase = '(?i)Main Phase'
                BattlePhase = '(?i)Battle Phase'
                DamageStep = '(?i)Damage Step'
                DamageCalculation = '(?i)damage calculation'
                EndPhase = '(?i)End Phase'
                OpponentTurn = "(?i)opponent's turn"
                EitherTurn = '(?i)either player.s turn'
                OnSummon = '(?i)(Normal|Special|Fusion|Synchro|Xyz|Link|Ritual) Summoned'
                OnAttack = '(?i)declares an attack|attack is declared|at the start of the Damage Step'
                OnDestruction = '(?i)is destroyed|was destroyed'
                OnSentToGraveyard = '(?i)sent to the GY'
                OnLeaveField = '(?i)leaves the field|left the field'
                ChainResponse = '(?i)when .+ is activated|in response to'
            }
        }
        "Cost" {
            [ordered]@{
                Discard = '(?i)\bdiscard'
                Tribute = '(?i)\bTribute'
                Banish = '(?i)\bbanish'
                PayLifePoints = '(?i)pay \d+ LP|pay half your LP'
                DetachMaterial = '(?i)\bdetach'
                SendToGraveyard = '(?i)send .+ to the GY'
                Reveal = '(?i)\breveal'
                RemoveCounter = '(?i)remove .+ counter'
            }
        }
        "Operation" {
            [ordered]@{
                Draw = '(?i)\bdraw\b'
                AddToHand = '(?i)add .+ to your hand|add it to your hand'
                SpecialSummon = '(?i)Special Summon'
                NormalSummon = '(?i)Normal Summon'
                Destroy = '(?i)\bdestroy'
                Banish = '(?i)\bbanish'
                SendToGraveyard = '(?i)send .+ to the GY|sent to the GY'
                ReturnToHand = '(?i)return .+ hand|return it to the hand'
                ReturnToDeck = '(?i)return .+ Deck|shuffle .+ into the Deck'
                Negate = '(?i)\bnegate'
                ChangeControl = '(?i)take control|switch control'
                ModifyAttack = '(?i)\bATK\b'
                ModifyDefense = '(?i)\bDEF\b'
                ChangeBattlePosition = '(?i)change .+ battle position|change .+ Position'
                Equip = '(?i)\bequip'
                AttachMaterial = '(?i)attach .+ material|as material'
                SetCard = '(?i)\bSet \d|\bSet that|Set it'
                Shuffle = '(?i)\bshuffle'
                Reveal = '(?i)\breveal'
                InflictDamage = '(?i)inflict .+ damage'
                GainLifePoints = '(?i)gain .+ LP'
            }
        }
    }

    $values = New-Object System.Collections.Generic.List[string]
    foreach ($entry in $patterns.GetEnumerator()) {
        if ($Text -match $entry.Value) {
            $values.Add([string]$entry.Key)
        }
    }

    return @($values.ToArray() | Sort-Object -Unique)
}

function Get-LimitKind {
    param([string]$Clause)

    if ($Clause -match '(?i)once per Duel') {
        return "OncePerDuel"
    }

    if ($Clause -match '(?i)only use each effect .+ once per turn') {
        return "EachEffectHardOncePerTurn"
    }

    if ($Clause -match '(?i)only use (this|the following) effect .+ once per turn') {
        return "HardOncePerTurn"
    }

    if ($Clause -match '(?i)only activate 1 .+ per turn') {
        return "ActivationOncePerTurn"
    }

    if ($Clause -match '(?i)once per turn') {
        return "OncePerTurn"
    }

    if ($Clause -match '(?i)once while face-up') {
        return "OnceWhileFaceUp"
    }

    return "NoneDetected"
}

function Get-TargetingMode {
    param([string]$Clause)

    if ($Clause -match '(?i)cannot target|cannot be targeted') {
        return "TargetingReference"
    }

    if ($Clause -match '(?i)\btarget(?:s|ed|ing)?\b') {
        return "ExplicitTarget"
    }

    if ($Clause -match '(?i)\bchoose\b|\bselect\b|randomly picks') {
        return "NonTargetingSelectionOrChoice"
    }

    return "NoneDetected"
}

function Convert-TechnicalPerspective {
    param([string]$Text)

    $result = $Text.Trim()
    $result = $result -replace '(?i)\bIf you control no\b', 'If the controller controls no'
    $result = $result -replace '(?i)\bIf you control\b', 'If the controller controls'
    $result = $result -replace '(?i)\bWhile you control\b', 'While the controller controls'
    $result = $result -replace '(?i)\bfor each card you control\b', 'for each card controlled by the controller'
    $result = $result -replace '(?i)\band if you do\b', 'after that succeeds'
    $result = $result -replace '(?i)\band if you do not\b', 'if that does not succeed'
    $result = $result -replace '(?i)\bif you do\b', 'if the controller does'
    $result = $result -replace '(?i)\bif you do not\b', 'if the controller does not'
    $result = $result -replace '(?i)\bthen\b', 'afterward'
    $result = $result -replace '(?i)\balso,?\b', 'additionally'
    $result = $result -replace '(?i)\byour opponent\b', 'the opposing player'
    $result = $result -replace '(?i)\byou control\b', "controlled by this card's controller"
    $result = $result -replace '(?i)\byour\b', "the controller's"
    $result = $result -replace '(?i)\bYou can\b', 'The controller may'
    $result = $result -replace '(?i)\byou can\b', 'the controller may'
    $result = $result -replace '(?i)\bYou must\b', 'The controller must'
    $result = $result -replace '(?i)\byou must\b', 'the controller must'
    $result = $result -replace '(?i)\bYou cannot\b', 'The controller is not allowed to'
    $result = $result -replace '(?i)\byou cannot\b', 'the controller is not allowed to'
    $result = $result -replace '(?i)\byou\b', 'the controller'
    $result = $result -replace '(?i)\bthe controller activate\b', 'the controller activates'
    $result = $result -replace '(?i)\bthe controller control\b', 'the controller controls'
    $result = $result -replace '(?i)\bthe controller have\b', 'the controller has'
    $result = $result -replace '(?i)\bthe controller take\b', 'the controller takes'
    $result = $result -replace '(?i)\bthe controller gain\b', 'the controller gains'
    $result = $result -replace '(?i)\bthe controller inflict\b', 'the controller inflicts'
    $result = $result -replace '(?i)\bthe controller pay\b', 'the controller pays'
    $result = $result -replace '(?i)\bNeither player can\b', 'Both players are prohibited from'
    $result = $result -replace '(?i)\bThis card cannot\b', 'This card is prohibited from'
    $result = $result -replace '(?i)\bTarget (\d+|1|up to \d+)\b', 'Select $1 as a target'
    $result = $result -replace '(?i)\btarget (\d+|1|up to \d+)\b', 'select $1 as a target'
    $result = $result -replace '(?i)\badd it to the controller.s hand\b', "move it into the controller's hand"
    $result = $result -replace '(?i)\badd (.+?) to the controller.s hand\b', 'move $1 into the controller''s hand'
    return $result.Trim()
}

function New-FallbackIntentSummary {
    param(
        [string]$Clause,
        [string]$Kind
    )

    $operations = @(Get-TaggedValues -Text $Clause -Category Operation)
    $zones = @(Get-TaggedValues -Text $Clause -Category Zone)
    $timings = @(Get-TaggedValues -Text $Clause -Category Timing)
    $quoted = @([Text.RegularExpressions.Regex]::Matches($Clause, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
    $numbers = @([Text.RegularExpressions.Regex]::Matches($Clause, '\b\d+\b') | ForEach-Object { $_.Value } | Sort-Object -Unique)

    $parts = New-Object System.Collections.Generic.List[string]
    $parts.Add("Mechanic group: $Kind")
    if ($operations.Count -gt 0) { $parts.Add("operations " + ($operations -join ", ")) }
    if ($timings.Count -gt 0) { $parts.Add("timing " + ($timings -join ", ")) }
    if ($zones.Count -gt 0) { $parts.Add("zones " + ($zones -join ", ")) }
    if ($quoted.Count -gt 0) { $parts.Add("referenced terms " + ($quoted -join ", ")) }
    if ($numbers.Count -gt 0) { $parts.Add("numeric parameters " + ($numbers -join ", ")) }
    $parts.Add("preserve the source-linked eligibility and ordering during manual validation")
    return ($parts -join "; ") + "."
}

function Convert-ClauseToIndependentIntent {
    param(
        [string]$Clause,
        [string]$Kind
    )

    $trimmed = $Clause.Trim()
    $sourceForContainmentCheck = $trimmed
    $isSelectableOption = $trimmed.StartsWith([string][char]0x25CF, [StringComparison]::Ordinal)
    if ($isSelectableOption) {
        $trimmed = $trimmed.Substring(1).Trim()
    }
    if ($trimmed -match '^\[\s*Pendulum Effect\s*\]$') {
        return "Section marker: the following rules apply while this card acts as a Pendulum Spell."
    }
    if ($trimmed -match '^\[\s*Monster Effect\s*\]$') {
        return "Section marker: the following rules apply while this card acts as a Monster."
    }
    if ($trimmed -match '^\[\s*(.+?)\s*\]$') {
        return "Section marker: $($Matches[1])."
    }

    if ($trimmed -match '^(?i)You can only use each effect of .+ once per turn\.?$') {
        return "Limit: every named ability of this card has one use per turn."
    }
    if ($trimmed -match '^(?i)You can only use (this|the following) effect of .+ once per turn\.?$') {
        return "Limit: this named ability has one use per turn."
    }
    if ($trimmed -match '^(?i)You can only activate 1 .+ per turn\.?$') {
        return "Limit: only one activation of this card is allowed during a turn."
    }
    if ($Kind -eq "SummonMaterial") {
        if ($trimmed -match '^(\d+)\s+Level\s+(\d+)\s+monsters?\.?$') {
            return "Summon recipe: exact material count $($Matches[1]); every material must have Level $($Matches[2])."
        }
        if ($trimmed.Contains(' + ')) {
            $recipe = $trimmed -replace '\s+\+\s+', ' together with '
            return "Summon recipe: combine $recipe."
        }
        if ($trimmed -match '^(\d+)\+\s+(.+?)\s+monsters?(.*)$') {
            return "Summon recipe: minimum material count $($Matches[1]); required material traits: $($Matches[2]); additional constraints:$($Matches[3])."
        }
        if ($trimmed -match '^(\d+)(\+?)\s+monsters?(.*)$') {
            $quantity = if ($Matches[2] -eq '+') { "at least" } else { "exactly" }
            return "Summon recipe: material count is $quantity $($Matches[1]); additional constraints:$($Matches[3])."
        }
        if ($trimmed -match '^(\d+)\s+(.+?)\s+monsters?(.*)$') {
            return "Summon recipe: exact material count $($Matches[1]); required material traits: $($Matches[2]); additional constraints:$($Matches[3])."
        }

        return New-FallbackIntentSummary -Clause $sourceForContainmentCheck -Kind $Kind
    }

    $rewritten = Convert-TechnicalPerspective $trimmed
    $semicolon = $rewritten.IndexOf(';')
    if ($semicolon -ge 0) {
        $setup = $rewritten.Substring(0, $semicolon).Trim().TrimEnd('.')
        $outcome = $rewritten.Substring($semicolon + 1).Trim()
        $rewritten = "Activation setup: $setup. Resolution: $outcome"
    }
    else {
        $colon = $rewritten.IndexOf(':')
        if ($colon -ge 0 -and $colon -lt 160) {
            $condition = $rewritten.Substring(0, $colon).Trim().TrimEnd('.')
            $outcome = $rewritten.Substring($colon + 1).Trim()
            $rewritten = "Availability or trigger: $condition. Resolution: $outcome"
        }
        elseif ($Kind -eq "ContinuousOrCondition") {
            $rewritten = "Persistent rule or summon condition: $rewritten"
        }
        elseif ($Kind -eq "Limit") {
            $rewritten = "Usage restriction: $rewritten"
        }
        else {
            $rewritten = "Rule intent: $rewritten"
        }
    }

    if ($isSelectableOption) {
        $rewritten = "Selectable outcome: $rewritten"
    }

    if ($sourceForContainmentCheck.Length -gt 25 -and $rewritten.IndexOf($sourceForContainmentCheck, [StringComparison]::Ordinal) -ge 0) {
        return New-FallbackIntentSummary -Clause $sourceForContainmentCheck -Kind $Kind
    }

    return $rewritten.Trim()
}

function Get-ReviewReasons {
    param(
        [string]$Text,
        [string]$CardType,
        [object[]]$Clauses
    )

    $reasons = New-Object System.Collections.Generic.List[string]
    if ($Text -match '(?i)and if you do|also, after that|\bthen\b|\balso\b') { $reasons.Add("ProblemSolvingConjunction") }
    if ($Text -match '(?i)instead|replace|would be') { $reasons.Add("ReplacementOrSubstitution") }
    if ($Text -match '(?i)copy .+ effect|this effect becomes|apply that effect') { $reasons.Add("CopiesOrTransformsEffect") }
    if ($Text -match '(?i)random|coin toss|six-sided die') { $reasons.Add("Randomness") }
    if ($Text -match '(?i)immediately after this effect resolves|during the Damage Step|damage calculation') { $reasons.Add("RulingSensitiveTiming") }
    if ($Text -match '(?i)unaffected by|cannot be negated|cannot activate cards or effects in response') { $reasons.Add("ProtectionOrResponseWindow") }
    if ($Text -match '(?i)Fusion Material|Synchro Material|Xyz Material|Link Material|\s\+\s' -or $CardType -match 'Fusion|Synchro|XYZ|Link') { $reasons.Add("SummonOrMaterialRules") }
    if ($Text -match '(?i)Pendulum') { $reasons.Add("PendulumDualContext") }
    if ($Clauses.Count -gt 5) { $reasons.Add("ManyClauses") }
    return @($reasons.ToArray() | Sort-Object -Unique)
}

function Get-Confidence {
    param(
        [bool]$HasEffect,
        [string]$CardType,
        [object[]]$ReviewReasons
    )

    if (-not $HasEffect -and $CardType -eq "Normal Monster") {
        return "High"
    }

    if ($ReviewReasons.Count -ge 2) {
        return "Low"
    }

    if ($ReviewReasons.Count -eq 1) {
        return "Medium"
    }

    return $(if ($HasEffect) { "Medium" } else { "High" })
}

function Get-ClauseParameters {
    param([string]$Clause)

    $quoted = @([Text.RegularExpressions.Regex]::Matches($Clause, '"([^"]+)"') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
    $numbers = @([Text.RegularExpressions.Regex]::Matches($Clause, '\b\d+\b') | ForEach-Object { [long]$_.Value } | Sort-Object -Unique)
    $conjunctions = New-Object System.Collections.Generic.List[string]
    foreach ($entry in ([ordered]@{
        AndIfYouDo = '(?i)\band if you do\b'
        Then = '(?i)\bthen\b'
        AlsoAfterThat = '(?i)\balso, after that\b'
        Also = '(?i)\balso\b'
        And = '(?i)\band\b'
    }).GetEnumerator()) {
        if ($Clause -match $entry.Value) {
            $conjunctions.Add([string]$entry.Key)
        }
    }

    return [ordered]@{
        quotedTerms = @($quoted)
        numbers = @($numbers)
        psctConjunctions = @($conjunctions.ToArray())
        hasColon = $Clause.Contains(':')
        hasSemicolon = $Clause.Contains(';')
    }
}

function New-NoEffectDisplayText {
    param([object]$Card)

    $attribute = [string](Get-PropertyValue $Card "attribute")
    $race = [string](Get-PropertyValue $Card "race")
    $level = Get-PropertyValue $Card "level"
    $attack = Format-DisplayStat (Get-PropertyValue $Card "atk")
    $defense = Format-DisplayStat (Get-PropertyValue $Card "def")
    $type = [string](Get-PropertyValue $Card "type")

    if ($type -eq "Normal Monster") {
        return "A $attribute $race Normal Monster at Level $level with $attack ATK and $defense DEF. It has no executable card effect; apply only standard monster, archetype, and name-treatment rules."
    }

    if ($type -eq "Token") {
        return "A Token definition used only when another effect creates it. Its placement, ownership, legal zones, and any temporary values come from the effect that generated the Token."
    }

    return "This card has no separately activated or continuous effect in the source metadata. Its summon recipe and standard rules still require validation where applicable."
}

function New-CardDraft {
    param(
        [object]$Card,
        [object]$CatalogRow
    )

    $databaseId = Format-DatabaseId (Get-PropertyValue $Card "id")
    $name = [string](Get-PropertyValue $Card "name")
    $type = [string](Get-PropertyValue $Card "type")
    $description = [string](Get-PropertyValue $Card "desc")
    $hasEffect = [string]$CatalogRow.hasEffect -eq "true"
    $clauses = @(Split-EffectClauses $description)
    $clauseDrafts = New-Object System.Collections.Generic.List[object]
    $displayClauses = New-Object System.Collections.Generic.List[string]
    $allMechanics = New-Object System.Collections.Generic.List[string]
    $allZones = New-Object System.Collections.Generic.List[string]
    $allTimings = New-Object System.Collections.Generic.List[string]

    for ($index = 0; $index -lt $clauses.Count; $index++) {
        $clause = [string]$clauses[$index]
        $kind = Get-ClauseKind -Clause $clause -CardType $type -Index $index
        $mechanics = @(Get-TaggedValues -Text $clause -Category Mechanic)
        $zones = @(Get-TaggedValues -Text $clause -Category Zone)
        $timings = @(Get-TaggedValues -Text $clause -Category Timing)
        $costSource = if ($clause.Contains(';')) { $clause.Substring(0, $clause.IndexOf(';')) } else { "" }
        $costs = @(Get-TaggedValues -Text $costSource -Category Cost)
        $operations = @(Get-TaggedValues -Text $clause -Category Operation)
        $intent = Convert-ClauseToIndependentIntent -Clause $clause -Kind $kind
        if ($clause.Length -gt 20 -and $intent.IndexOf($clause, [StringComparison]::Ordinal) -ge 0) {
            $intent = New-FallbackIntentSummary -Clause $clause -Kind $kind
        }
        $parameters = Get-ClauseParameters $clause
        $displayClauses.Add($intent)
        foreach ($value in $mechanics) { $allMechanics.Add($value) }
        foreach ($value in $zones) { $allZones.Add($value) }
        foreach ($value in $timings) { $allTimings.Add($value) }

        $clauseDrafts.Add([ordered]@{
            clauseId = "card.$databaseId.clause.$($index + 1)"
            sourceClauseSha256 = Get-StringSha256 $clause
            kind = $kind
            intent = $intent
            parameters = $parameters
            targeting = Get-TargetingMode $clause
            costs = @($costs)
            operations = @($operations)
            zones = @($zones)
            timing = @($timings)
            limit = Get-LimitKind $clause
        })
    }

    if (-not $hasEffect -and $type -eq "Normal Monster") {
        $displayText = New-NoEffectDisplayText $Card
        $clauseDrafts.Clear()
        $allMechanics.Clear()
        $allZones.Clear()
        $allTimings.Clear()
    }
    elseif ($displayClauses.Count -eq 0) {
        $displayText = New-NoEffectDisplayText $Card
    }
    else {
        $displayText = $displayClauses -join " "
    }

    if (-not [string]::IsNullOrEmpty($description) -and $description.Length -gt 25 -and
        $displayText.IndexOf($description, [StringComparison]::Ordinal) -ge 0) {
        throw "Generated display text for '$name' still contains the complete source description."
    }

    $reviewReasons = @(Get-ReviewReasons -Text $description -CardType $type -Clauses $clauses)
    $confidence = Get-Confidence -HasEffect $hasEffect -CardType $type -ReviewReasons $reviewReasons
    $misc = @((Get-PropertyValue $Card "misc_info"))
    $miscFirst = if ($misc.Count -gt 0) { $misc[0] } else { $null }
    $linkMarkerValue = Get-PropertyValue $Card "linkmarkers"
    $linkMarkers = if ($null -eq $linkMarkerValue) { @() } else { @($linkMarkerValue) }
    $rank = $null
    $level = Get-PropertyValue $Card "level"
    if ($type -match '^XYZ') {
        $rank = $level
        $level = $null
    }

    return [ordered]@{
        schemaVersion = 2
        databaseId = $databaseId
        cid = if ([string]::IsNullOrWhiteSpace([string]$CatalogRow.cid)) { $null } else { [long]$CatalogRow.cid }
        name = $name
        classification = [ordered]@{
            cardClass = $type
            frame = [string](Get-PropertyValue $Card "frameType")
            attribute = Get-PropertyValue $Card "attribute"
            raceOrType = Get-PropertyValue $Card "race"
            archetype = Get-PropertyValue $Card "archetype"
        }
        stats = [ordered]@{
            level = $level
            rank = $rank
            linkRating = Get-PropertyValue $Card "linkval"
            attack = New-StatSpec (Get-PropertyValue $Card "atk")
            defense = New-StatSpec (Get-PropertyValue $Card "def")
            pendulumScale = Get-PropertyValue $Card "scale"
            linkMarkers = @($linkMarkers)
        }
        masterDuel = [ordered]@{
            available = $true
            rarity = Get-PropertyValue $miscFirst "md_rarity"
        }
        source = [ordered]@{
            locale = "en"
            reference = [string]$CatalogRow.officialUrl
            sourceTextSha256 = Get-StringSha256 $description
            sourceTextIncluded = $false
        }
        displayText = $displayText
        effectSpecDraft = [ordered]@{
            status = $(if (-not $hasEffect -and $type -eq "Normal Monster") { "NoExecutableEffect" } else { "AutomatedIntentDraft" })
            reviewRequired = -not (-not $hasEffect -and $type -eq "Normal Monster")
            confidence = $confidence
            reviewReasons = @($reviewReasons)
            mechanics = @($allMechanics.ToArray() | Sort-Object -Unique)
            zones = @($allZones.ToArray() | Sort-Object -Unique)
            timing = @($allTimings.ToArray() | Sort-Object -Unique)
            clauses = @($clauseDrafts.ToArray())
        }
        implementation = [ordered]@{
            effectHandlerId = [string]$CatalogRow.effectHandlerId
            catalogStatus = [string]$CatalogRow.implementationStatus
            executableCoverageClaimed = $false
        }
    }
}

$inputPath = (Resolve-Path -LiteralPath $InputJson).Path
$catalogPath = (Resolve-Path -LiteralPath $CatalogTsv).Path
$batchPath = if ([IO.Path]::IsPathRooted($ExistingBatchDirectory)) {
    [IO.Path]::GetFullPath($ExistingBatchDirectory)
}
else {
    [IO.Path]::GetFullPath((Join-Path (Get-Location) $ExistingBatchDirectory))
}
$outputPath = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    [IO.Path]::GetFullPath($OutputDirectory)
}
else {
    [IO.Path]::GetFullPath((Join-Path (Get-Location) $OutputDirectory))
}
[IO.Directory]::CreateDirectory($outputPath) | Out-Null

$utf8NoBom = New-Object Text.UTF8Encoding($false)
$payload = [IO.File]::ReadAllText($inputPath, [Text.Encoding]::UTF8) | ConvertFrom-Json
$sourceCards = @((Get-PropertyValue $payload "data"))
if ($sourceCards.Count -eq 0) {
    throw "Input JSON does not contain a non-empty data array."
}

$sourceById = @{}
foreach ($card in $sourceCards) {
    $databaseId = Format-DatabaseId (Get-PropertyValue $card "id")
    if ($sourceById.ContainsKey($databaseId)) {
        throw "Duplicate source database ID '$databaseId'."
    }
    $sourceById[$databaseId] = $card
}

$catalogRows = @(Import-Csv -LiteralPath $catalogPath -Delimiter "`t")
if ($catalogRows.Count -eq 0) {
    throw "Catalog TSV does not contain cards."
}

$manualIds = Get-ManualCardIds $batchPath
$missingSourceIds = New-Object System.Collections.Generic.List[string]
$selectedRows = New-Object System.Collections.Generic.List[object]
foreach ($row in $catalogRows) {
    $databaseId = Format-DatabaseId $row.databaseId
    if (-not $sourceById.ContainsKey($databaseId)) {
        $missingSourceIds.Add($databaseId)
        continue
    }
    if (-not $manualIds.Contains($databaseId)) {
        $selectedRows.Add($row)
    }
}

if ($missingSourceIds.Count -gt 0) {
    throw "Catalog IDs missing from source JSON: $($missingSourceIds -join ', ')."
}

$sortedRows = @($selectedRows.ToArray() | Sort-Object @{ Expression = { [string]$_.name }; Ascending = $true }, @{ Expression = { [long]$_.databaseId }; Ascending = $true })
if ($MaxCards -gt 0 -and $sortedRows.Count -gt $MaxCards) {
    $sortedRows = @($sortedRows | Select-Object -First $MaxCards)
}

$drafts = New-Object System.Collections.Generic.List[object]
$seenDraftIds = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
$confidenceCounts = @{ High = 0; Medium = 0; Low = 0 }
$noEffectCount = 0
foreach ($row in $sortedRows) {
    $databaseId = Format-DatabaseId $row.databaseId
    $draft = New-CardDraft -Card $sourceById[$databaseId] -CatalogRow $row
    if (-not $seenDraftIds.Add($databaseId)) {
        throw "Duplicate generated database ID '$databaseId'."
    }
    $drafts.Add($draft)
    $confidenceCounts[[string]$draft.effectSpecDraft.confidence]++
    if ($draft.effectSpecDraft.status -eq "NoExecutableEffect") {
        $noEffectCount++
    }
}

$shardRecords = New-Object System.Collections.Generic.List[object]
$expectedShardNames = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::OrdinalIgnoreCase)
for ($offset = 0; $offset -lt $drafts.Count; $offset += $CardsPerShard) {
    $endExclusive = [Math]::Min($offset + $CardsPerShard, $drafts.Count)
    $startOrdinal = $offset + 1
    $endOrdinal = $endExclusive
    $fileName = "CARDS_{0:D5}_{1:D5}.md" -f $startOrdinal, $endOrdinal
    [void]$expectedShardNames.Add($fileName)
    $filePath = Join-Path $outputPath $fileName
    $firstCard = $drafts[$offset]
    $lastCard = $drafts[$endExclusive - 1]
    $writer = New-Object IO.StreamWriter($filePath, $false, $utf8NoBom)
    try {
        $writer.WriteLine("# Card-effectcorpus $startOrdinal-$endOrdinal")
        $writer.WriteLine("")
        $writer.WriteLine('> Automatisch concept uit de geautoriseerde bronintentie. De oorspronkelijke kaarttekst is niet opgenomen. `displayText` en `effectSpecDraft` moeten voor runtime-implementatie per effectfamilie en kaart worden gevalideerd.')
        $writer.WriteLine("")
        for ($index = $offset; $index -lt $endExclusive; $index++) {
            $draft = $drafts[$index]
            $writer.WriteLine("## $($index + 1). $(Escape-MarkdownHeading $draft.name)")
            $writer.WriteLine("")
            $writer.WriteLine('```json')
            $writer.WriteLine(($draft | ConvertTo-Json -Depth 12))
            $writer.WriteLine('```')
            $writer.WriteLine("")
        }
    }
    finally {
        $writer.Dispose()
    }

    $shardRecords.Add([ordered]@{
        file = $fileName
        start = $startOrdinal
        end = $endOrdinal
        count = $endOrdinal - $startOrdinal + 1
        firstCard = [string]$firstCard.name
        lastCard = [string]$lastCard.name
        sha256 = (Get-FileHash -LiteralPath $filePath -Algorithm SHA256).Hash.ToLowerInvariant()
    })
}

$indexPath = Join-Path $outputPath "ALL_CARDS_INDEX.md"
$indexWriter = New-Object IO.StreamWriter($indexPath, $false, $utf8NoBom)
try {
    $indexWriter.WriteLine("# Volledige index van automatisch gegenereerde kaartconcepten")
    $indexWriter.WriteLine("")
    $indexWriter.WriteLine("| ID | Kaart | Klasse | Confidence | Shard | Officiele verificatie |")
    $indexWriter.WriteLine("| --- | --- | --- | --- | --- | --- |")
    for ($index = 0; $index -lt $drafts.Count; $index++) {
        $draft = $drafts[$index]
        $shardIndex = [Math]::Floor($index / $CardsPerShard)
        $shard = $shardRecords[$shardIndex]
        $name = Escape-MarkdownCell $draft.name
        $cardClass = Escape-MarkdownCell $draft.classification.cardClass
        $official = if ([string]::IsNullOrWhiteSpace([string]$draft.source.reference)) { "n/a" } else { "[Neuron]($($draft.source.reference))" }
        $indexWriter.WriteLine("| $($draft.databaseId) | $name | $cardClass | $($draft.effectSpecDraft.confidence) | [$($shard.file)]($($shard.file)) | $official |")
    }
}
finally {
    $indexWriter.Dispose()
}

$readmePath = Join-Path $outputPath "README.md"
$readmeWriter = New-Object IO.StreamWriter($readmePath, $false, $utf8NoBom)
try {
    $readmeWriter.WriteLine("# Volledig Master Duel-kaarteffectcorpus")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine(('> Snapshot: `{0}` | cataloguskaarten: `{1}` | handmatig uitgewerkt: `{2}` | automatisch concept: `{3}`.' -f $SnapshotDate, $catalogRows.Count, $manualIds.Count, $drafts.Count))
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("## Wat deze run oplevert")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine('Alle cataloguskaarten die niet al in `Documentation/CardEffectBatches/` handmatig zijn uitgewerkt, staan in de shards hieronder. Iedere kaart bevat statische feiten, een nieuw geformuleerde spelbeschrijving, een hash van de gebruikte Engelstalige brontekst, een officiele verificatielink en een automatisch `effectSpecDraft` met clause-, timing-, zone-, cost-, target-, operation- en limitinformatie.')
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("De oorspronkelijke kaarttekst wordt niet opgenomen. Een automatisch concept is geen claim van uitvoerbare regelnauwkeurigheid: PSCT-conjuncties, chains, missing-the-timing, materiaalvervanging, zone-identiteit en kaart-specifieke rulings vereisen tests en waar nodig handmatige modellering. De 30 kaarten in de drie handmatige batches blijven de hogere kwaliteitslaag.")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("## Dekking")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("| Eigenschap | Aantal |")
    $readmeWriter.WriteLine("| --- | ---: |")
    $readmeWriter.WriteLine("| Catalogus totaal | $($catalogRows.Count) |")
    $readmeWriter.WriteLine("| Handmatige overrides | $($manualIds.Count) |")
    $readmeWriter.WriteLine("| Automatische kaartconcepten | $($drafts.Count) |")
    $readmeWriter.WriteLine("| Geen uitvoerbaar effect volgens metadata | $noEffectCount |")
    $readmeWriter.WriteLine("| Confidence High | $($confidenceCounts.High) |")
    $readmeWriter.WriteLine("| Confidence Medium | $($confidenceCounts.Medium) |")
    $readmeWriter.WriteLine("| Confidence Low | $($confidenceCounts.Low) |")
    $readmeWriter.WriteLine("| Markdown-shards | $($shardRecords.Count) |")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("Volledige zoekindex: [ALL_CARDS_INDEX.md](ALL_CARDS_INDEX.md). Handmatige specificaties: [../CardEffectBatches/README.md](../CardEffectBatches/README.md).")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("## Gebruik door een implementatie-agent")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine('1. Zoek de kaart op ID in `ALL_CARDS_INDEX.md`.')
    $readmeWriter.WriteLine('2. Open de genoemde shard en lees `displayText`, `effectSpecDraft.clauses` en `reviewReasons`.')
    $readmeWriter.WriteLine("3. Controleer de officiele bronlink en eventuele rulings.")
    $readmeWriter.WriteLine("4. Modelleer een herbruikbare effectfamilie; maak geen centrale switch per kaart-ID.")
    $readmeWriter.WriteLine("5. Voeg rules tests toe voor kosten, targets, chain-resolutie, zonebeweging, limieten en expiry.")
    $readmeWriter.WriteLine("6. Zet pas na bewezen tests een kaartstatus om van concept naar uitvoerbaar.")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("## Reproduceerbaarheid")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine(('- Bron-JSON SHA-256: `{0}`' -f (Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash.ToLowerInvariant()))
    $readmeWriter.WriteLine(('- Catalogus-TSV SHA-256: `{0}`' -f (Get-FileHash -LiteralPath $catalogPath -Algorithm SHA256).Hash.ToLowerInvariant()))
    $readmeWriter.WriteLine('- Generator: `Tools/YGOProDeck/GenerateCardEffectCorpus.ps1`')
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("## Shards")
    $readmeWriter.WriteLine("")
    $readmeWriter.WriteLine("| Bereik | Eerste kaart | Laatste kaart | Bestand | SHA-256 (kort) |")
    $readmeWriter.WriteLine("| ---: | --- | --- | --- | --- |")
    foreach ($shard in $shardRecords) {
        $shortHash = $shard.sha256.Substring(0, 12)
        $readmeWriter.WriteLine(('| {0}-{1} | {2} | {3} | [{4}]({4}) | `{5}` |' -f $shard.start, $shard.end, (Escape-MarkdownCell $shard.firstCard), (Escape-MarkdownCell $shard.lastCard), $shard.file, $shortHash))
    }
}
finally {
    $readmeWriter.Dispose()
}

$manifest = [ordered]@{
    schemaVersion = 1
    snapshotDate = $SnapshotDate
    sourceJson = [IO.Path]::GetFileName($inputPath)
    sourceJsonSha256 = (Get-FileHash -LiteralPath $inputPath -Algorithm SHA256).Hash.ToLowerInvariant()
    catalog = [IO.Path]::GetFileName($catalogPath)
    catalogSha256 = (Get-FileHash -LiteralPath $catalogPath -Algorithm SHA256).Hash.ToLowerInvariant()
    catalogCardCount = $catalogRows.Count
    manualOverrideCount = $manualIds.Count
    generatedDraftCount = $drafts.Count
    generatedUniqueDatabaseIds = $seenDraftIds.Count
    shardCount = $shardRecords.Count
    cardsPerShard = $CardsPerShard
    noExecutableEffectCount = $noEffectCount
    confidenceCounts = [ordered]@{
        high = $confidenceCounts.High
        medium = $confidenceCounts.Medium
        low = $confidenceCounts.Low
    }
    shards = @($shardRecords.ToArray())
} | ConvertTo-Json -Depth 6
[IO.File]::WriteAllText((Join-Path $outputPath "manifest.json"), $manifest + "`n", $utf8NoBom)

$actualShardFiles = @(Get-ChildItem -LiteralPath $outputPath -File -Filter "CARDS_*.md")
$unexpectedShardFiles = @($actualShardFiles | Where-Object { -not $expectedShardNames.Contains($_.Name) })
if ($unexpectedShardFiles.Count -gt 0) {
    throw "Unexpected stale shard files remain: $($unexpectedShardFiles.Name -join ', ')."
}

[pscustomobject]@{
    OutputDirectory = $outputPath
    CatalogCards = $catalogRows.Count
    ManualOverrides = $manualIds.Count
    GeneratedDrafts = $drafts.Count
    UniqueDraftIds = $seenDraftIds.Count
    Shards = $shardRecords.Count
    HighConfidence = $confidenceCounts.High
    MediumConfidence = $confidenceCounts.Medium
    LowConfidence = $confidenceCounts.Low
    NoExecutableEffect = $noEffectCount
}
