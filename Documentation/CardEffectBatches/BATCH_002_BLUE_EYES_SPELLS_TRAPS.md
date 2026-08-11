# Card-effectbatch 002 — Blue-Eyes Spells en Traps

Status: **afgebakende batch van 10 kaarten**  
Bron voor de werking: de door de gebruiker als toegestaan aangemerkte kaartinformatie en de reeds aanwezige lokale kaartdefinities en effecthandlers.  
Runtime-impact: geen; dit document specificeert gewenst gedrag voor latere implementatie en tests.

## 1. Bingo Machine, Go!!!

```yaml
databaseId: "93437091"
name: Bingo Machine, Go!!!
cardClass: Normal Spell
effectHandlerId: bingo_clean
displayText: >-
  Show your opponent three cards from your Deck. Every shown card must either
  be a Blue-Eyes monster or a Spell/Trap, other than this card, that refers to
  Blue-Eyes White Dragon or Blue-Eyes Ultimate Dragon. Your opponent randomly
  selects one for you to keep; add it to your hand and shuffle the other two
  back into the Deck. Only one copy of this card may be activated per turn.
effectSpec:
  activation:
    kind: normalSpell
    activationZones: [hand, setSpellTrapZone]
    conditions:
      - controllerDeckContainsAtLeast:
          amount: 3
          matchAny:
            - archetype: Blue-Eyes
              cardKind: monster
            - cardKindAny: [spell, trap]
              mentionsAnyCardId: ["89631139", "23995346"]
              excludeCardId: "93437091"
  effects:
    - id: revealThreeAndRandomlyKeepOne
      selections:
        - zone: deck
          owner: controller
          amount: 3
          matchAny:
            - archetype: Blue-Eyes
              cardKind: monster
            - cardKindAny: [spell, trap]
              mentionsAnyCardId: ["89631139", "23995346"]
              excludeCardId: "93437091"
      operations:
        - revealSelectedTo: bothPlayers
        - opponentRandomlyChooses:
            amount: 1
            from: selectedCards
        - moveOpponentChoiceTo: controllerHand
        - shuffleRemainingSelectedInto: controllerDeck
      limits:
        key: bingoMachine.activation
        scope: perControllerPerTurn
```

Implementatienotitie: de tegenstander maakt geen strategische keuze; de engine gebruikt aantoonbaar willekeurige selectie en legt het gebruikte RNG-resultaat vast voor replays.

---

## 2. Monster Reborn

```yaml
databaseId: "83764719"
name: Monster Reborn
cardClass: Normal Spell
effectHandlerId: monster_reborn_clean
displayText: >-
  Choose a monster in either GY and Special Summon it to your field, provided
  that monster is legally allowed to return from the GY.
effectSpec:
  activation:
    kind: normalSpell
    activationZones: [hand, setSpellTrapZone]
    conditions:
      - controllerHasOpenMonsterZone: true
      - eitherGraveyardContainsLegalRevivalTarget: true
  effects:
    - id: reviveMonsterFromEitherGraveyard
      targets:
        - owner: eitherPlayer
          zone: graveyard
          cardKind: monster
          amount: 1
          summonLegality: standardRevivalRules
      operations:
        - ifTargetStillLegal:
            specialSummonTargetTo: controllerMonsterZone
            summonPosition: faceUpAttackOrDefenseByControllerChoice
            summonMethod: effect
```

Implementatienotitie: Extra Deck-, Ritual- en vergelijkbare monsters die niet eerst correct zijn opgeroepen, zijn geen legaal doel wanneer de algemene revivalregels dat verbieden.

---

## 3. Wishes for Eyes of Blue

```yaml
databaseId: "80326401"
name: Wishes for Eyes of Blue
cardClass: Normal Spell
effectHandlerId: wishes_clean
displayText: >-
  Discard one card, then take both a Level 1 LIGHT Tuner and a different
  Spell/Trap that refers to Blue-Eyes White Dragon from your Deck into your
  hand. From the GY, you may banish this card and choose a Blue-Eyes White
  Dragon you control; attach a Blue-Eyes monster from your Extra Deck to it as
  an Equip Spell that grants 400 ATK. Each ability is usable once per turn.
effectSpec:
  activation:
    kind: normalSpell
    activationZones: [hand, setSpellTrapZone]
  effects:
    - id: searchTunerAndSupportCard
      kind: activated
      conditions:
        - controllerDeckContains:
            level: 1
            attribute: LIGHT
            tuner: true
            amount: 1
        - controllerDeckContains:
            cardKindAny: [spell, trap]
            mentionsCardId: "89631139"
            excludeCardId: "80326401"
            amount: 1
      costs:
        - discardFromHand:
            amount: 1
      selections:
        - id: tuner
          zone: deck
          owner: controller
          level: 1
          attribute: LIGHT
          tuner: true
          amount: 1
        - id: supportCard
          zone: deck
          owner: controller
          cardKindAny: [spell, trap]
          mentionsCardId: "89631139"
          excludeCardId: "80326401"
          amount: 1
      operations:
        - moveSelectionsToHand: [tuner, supportCard]
        - shuffleDeck: true
      limits:
        key: wishes.searchPair
        scope: perControllerPerTurn

    - id: equipExtraDeckBlueEyes
      kind: ignition
      activationZone: graveyard
      conditions:
        - controllerHasOpenSpellTrapZone: true
      costs:
        - banishSelfFrom: graveyard
      targets:
        - controller: self
          zone: monsterZone
          effectiveCardId: "89631139"
          amount: 1
      selections:
        - zone: extraDeck
          owner: controller
          archetype: Blue-Eyes
          cardKind: monster
          amount: 1
      operations:
        - moveSelectedToSpellTrapZone:
            faceUp: true
            treatedAs: equipSpell
            equippedTo: target
        - applyModifierToEquippedMonster:
            attack: 400
            duration: whileEquipRelationshipExists
      limits:
        key: wishes.equipBlueEyes
        scope: perControllerPerTurn
```

---

## 4. Roar of the Blue-Eyed Dragons

```yaml
databaseId: "17725109"
name: Roar of the Blue-Eyed Dragons
cardClass: Normal Spell
effectHandlerId: roar_clean
displayText: >-
  Choose one of two abilities. First, Special Summon a Blue-Eyes monster in
  Defense Position from your Deck, GY, or banished cards; Blue-Eyes White Dragon
  itself is available only when you do not already control one. Alternatively,
  banish this card from the GY to Fusion Summon using monsters from your hand or
  field, including at least one Blue-Eyes monster. After either use, your Extra
  Deck summons for the rest of the turn are restricted to Dragons. You may use
  only one of this card's abilities per turn.
effectSpec:
  effects:
    - id: specialSummonBlueEyes
      kind: activated
      activationZones: [hand, setSpellTrapZone]
      conditions:
        - controllerHasOpenMonsterZone: true
      selections:
        - zones: [deck, graveyard, banished]
          owner: controller
          archetype: Blue-Eyes
          amount: 1
          additionalLegality:
            ifEffectiveCardIdIs: "89631139"
            controllerMustNotControlEffectiveCardId: "89631139"
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
          position: faceUpDefense
        - applyExtraDeckSummonRestriction:
            allowedMonsterType: Dragon
            duration: restOfTurn
      limits:
        key: roar.sharedEffectUse
        scope: perControllerPerTurn
        sharedAcrossCardEffects: true

    - id: fusionSummonFromGraveyard
      kind: ignition
      activationZone: graveyard
      costs:
        - banishSelfFrom: graveyard
      selections:
        - zone: extraDeck
          owner: controller
          cardClass: FusionMonster
          materialSources: [hand, monsterZone]
          requiredMaterialPredicate:
            archetype: Blue-Eyes
            minimumAmount: 1
      operations:
        - performFusionSummonWithSelectedMaterials: true
        - applyExtraDeckSummonRestriction:
            allowedMonsterType: Dragon
            duration: restOfTurn
      limits:
        key: roar.sharedEffectUse
        scope: perControllerPerTurn
        sharedAcrossCardEffects: true
```

Implementatienotitie: de gedeelde limiet staat slechts één van beide effecten toe. De Dragon-beperking wordt alleen toegepast nadat het gekozen effect daadwerkelijk is opgelost.

---

## 5. Raigeki

```yaml
databaseId: "12580477"
name: Raigeki
cardClass: Normal Spell
effectHandlerId: raigeki_clean
displayText: >-
  Destroy every monster currently controlled by your opponent.
effectSpec:
  activation:
    kind: normalSpell
    activationZones: [hand, setSpellTrapZone]
    conditions:
      - opponentControlsAtLeastOneMonster: true
  effects:
    - id: destroyOpponentMonsters
      targets: []
      affectedCards:
        controller: opponent
        zone: monsterZone
        cardKind: monster
      operations:
        - destroyAllAffected:
            cause: cardEffect
```

Implementatienotitie: dit effect target niet. Vernietigingsbescherming wordt per monster toegepast; een beschermd monster voorkomt niet dat de overige monsters worden vernietigd.

---

## 6. Polymerization

```yaml
databaseId: "24094653"
name: Polymerization
cardClass: Normal Spell
effectHandlerId: polymerization_clean
displayText: >-
  Choose a Fusion Monster you can legally summon, then use the materials named
  by that monster from your hand and field to perform its Fusion Summon.
effectSpec:
  activation:
    kind: normalSpell
    activationZones: [hand, setSpellTrapZone]
    conditions:
      - legalFusionSummonExists:
          extraDeckOwner: controller
          materialSources: [hand, monsterZone]
  effects:
    - id: fusionSummon
      selections:
        - id: fusionMonster
          zone: extraDeck
          owner: controller
          cardClass: FusionMonster
          legalSummonZoneRequired: true
        - id: materials
          zones: [hand, monsterZone]
          owner: controller
          mustSatisfyMaterialRecipeOf: fusionMonster
      operations:
        - consumeAsFusionMaterial:
            cards: materials
            defaultDestination: graveyard
        - specialSummonFromExtraDeck:
            card: fusionMonster
            summonMethod: fusion
            properlySummoned: true
```

Implementatienotitie: materiaalvervangingen en gewijzigde bestemmingen worden door algemene regels verwerkt, niet als uitzonderingen in deze kaart.

---

## 7. Chaos Form

```yaml
databaseId: "21082832"
name: Chaos Form
cardClass: Ritual Spell
effectHandlerId: chaos_form_clean
displayText: >-
  Ritual Summon a Chaos or Black Luster Soldier Ritual Monster from your hand.
  Supply Levels exactly equal to that monster's Level by Tributing monsters from
  your hand or field, by banishing Blue-Eyes White Dragon or Dark Magician from
  your GY, or by combining those options.
effectSpec:
  activation:
    kind: ritualSpell
    activationZones: [hand, setSpellTrapZone]
    conditions:
      - legalRitualSummonExists:
          ritualMonsterZone: hand
          ritualMonsterNameFamilyAny: [Chaos, BlackLusterSoldier]
          requiredLevelRelation: exactlyEqual
  effects:
    - id: ritualSummon
      selections:
        - id: ritualMonster
          zone: hand
          owner: controller
          cardClass: RitualMonster
          nameFamilyAny: [Chaos, BlackLusterSoldier]
        - id: tributeMaterials
          zones: [hand, monsterZone]
          owner: controller
          cardKind: monster
          amount: zeroOrMore
        - id: graveyardMaterials
          zone: graveyard
          owner: controller
          effectiveCardIdAny: ["89631139", "46986414"]
          amount: zeroOrMore
      validation:
        - sumLevelsOf: [tributeMaterials, graveyardMaterials]
          relation: exactlyEqual
          compareToLevelOf: ritualMonster
        - atLeastOneMaterialSelected: true
      operations:
        - tributeCards: tributeMaterials
        - banishCards: graveyardMaterials
        - specialSummonFromHand:
            card: ritualMonster
            summonMethod: ritual
            properlySummoned: true
```

Implementatienotitie: materiaalkeuze en verplaatsing gebeuren tijdens de effectresolutie. `46986414` is de database-ID voor Dark Magician.

---

## 8. Dragon Shrine

```yaml
databaseId: "41620959"
name: Dragon Shrine
cardClass: Normal Spell
effectHandlerId: dragon_shrine_clean
displayText: >-
  Send a Dragon monster from your Deck to the GY. If that exact card remains in
  your GY and is a Normal Monster, you may send one additional Dragon from your
  Deck to the GY. Only one copy of this card may be activated per turn.
effectSpec:
  activation:
    kind: normalSpell
    activationZones: [hand, setSpellTrapZone]
    conditions:
      - controllerDeckContains:
          monsterType: Dragon
          amount: 1
  effects:
    - id: sendDragonsToGraveyard
      selections:
        - id: firstDragon
          zone: deck
          owner: controller
          cardKind: monster
          monsterType: Dragon
          amount: 1
      operations:
        - moveSelection:
            card: firstDragon
            to: graveyard
        - ifAll:
            - firstDragonIsCurrentlyIn: controllerGraveyard
            - firstDragonCardClass: NormalMonster
          optional: true
          then:
            select:
              id: secondDragon
              zone: deck
              owner: controller
              cardKind: monster
              monsterType: Dragon
              amount: 1
            moveSelection:
              card: secondDragon
              to: graveyard
      limits:
        key: dragonShrine.activation
        scope: perControllerPerTurn
```

---

## 9. Ultimate Fusion

```yaml
databaseId: "71143015"
name: Ultimate Fusion
cardClass: Quick-Play Spell
effectHandlerId: ultimate_fusion_clean
displayText: >-
  During a Main Phase, Fusion Summon a monster whose listed materials refer to
  Blue-Eyes White Dragon or Blue-Eyes Ultimate Dragon. Return the required
  materials from your hand, field, and GY to the Deck. After the summon, you may
  destroy face-up opposing cards, up to the number of those two named monsters
  returned from your field. Only one copy may be activated per turn.
effectSpec:
  activation:
    kind: quickPlaySpell
    activationZones: [handWhenLegal, setSpellTrapZone]
    timing:
      phase: main
      turnPlayer: either
    conditions:
      - legalFusionSummonExists:
          fusionRecipeMentionsAnyCardId: ["89631139", "23995346"]
          materialSources: [hand, monsterZone, graveyard]
  effects:
    - id: recycleMaterialsAndFusionSummon
      selections:
        - id: fusionMonster
          zone: extraDeck
          owner: controller
          cardClass: FusionMonster
          materialRecipeMentionsAnyCardId: ["89631139", "23995346"]
        - id: materials
          zones: [hand, monsterZone, graveyard]
          owner: controller
          mustSatisfyMaterialRecipeOf: fusionMonster
      derivedValues:
        - id: fieldNamedMaterialCount
          countFrom: materials
          previousZone: monsterZone
          effectiveCardIdAny: ["89631139", "23995346"]
      operations:
        - returnMaterialsToDeck: materials
        - shuffleDeck: true
        - specialSummonFromExtraDeck:
            card: fusionMonster
            summonMethod: fusion
            properlySummoned: true
        - ifPreviousSummonSucceeded:
            optional: true
            selectAtResolution:
              controller: opponent
              zone: field
              faceUp: true
              amountRange:
                minimum: 0
                maximumFrom: fieldNamedMaterialCount
            destroySelected:
              cause: cardEffect
      limits:
        key: ultimateFusion.activation
        scope: perControllerPerTurn
```

Implementatienotitie: de vernietiging target niet en gebruikt het aantal daadwerkelijk vanaf het veld teruggestuurde materialen met de vereiste effectieve namen.

---

## 10. True Light

```yaml
databaseId: "62089826"
name: True Light
cardClass: Continuous Trap
effectHandlerId: true_light_clean
displayText: >-
  Opposing effects cannot choose your Blue-Eyes White Dragons in the Monster
  Zone as targets. Once per turn, either Special Summon Blue-Eyes White Dragon
  from your hand or GY, or Set a Spell/Trap that refers to it directly from your
  Deck. If this face-up card travels from the Spell & Trap Zone to the GY,
  destroy every monster you control.
effectSpec:
  activation:
    kind: continuousTrap
    activationZones: [setSpellTrapZone]
    followsTrapActivationTiming: true
  effects:
    - id: protectBlueEyesFromTargeting
      kind: continuous
      activationZone: spellTrapZone
      affectedCards:
        controller: self
        zone: monsterZone
        effectiveCardId: "89631139"
      modifiers:
        - cannotBeTargetedBy:
            controller: opponent
            source: cardEffect

    - id: summonBlueEyes
      kind: activated
      activationZone: spellTrapZone
      selections:
        - zones: [hand, graveyard]
          owner: controller
          effectiveCardId: "89631139"
          amount: 1
          summonLegality: standardRevivalRules
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
      limits:
        key: trueLight.modeUse
        scope: perControllerPerTurn
        sharedWith: setSupportCard

    - id: setSupportCard
      kind: activated
      activationZone: spellTrapZone
      conditions:
        - controllerHasOpenSpellTrapZone: true
      selections:
        - zone: deck
          owner: controller
          cardKindAny: [spell, trap]
          mentionsCardId: "89631139"
          amount: 1
      operations:
        - setSelectedTo: controllerSpellTrapZone
      limits:
        key: trueLight.modeUse
        scope: perControllerPerTurn
        sharedWith: summonBlueEyes

    - id: destroyOwnMonstersWhenLost
      kind: trigger
      mandatory: true
      trigger:
        event: cardMoved
        card: self
        from: spellTrapZone
        previousFaceState: faceUp
        to: graveyard
      affectedCards:
        controller: self
        zone: monsterZone
        cardKind: monster
      operations:
        - destroyAllAffected:
            cause: cardEffect
```

Implementatienotitie: voor de laatste trigger moet het move-event de vorige zone en face-upstatus bewaren. Een verplaatsing naar verbanning of hand activeert deze trigger niet.

---

## Acceptatiecriteria voor batch 002

1. `displayText` wordt nooit geparseerd om gameplay te bepalen.
2. Activatievoorwaarden worden gecontroleerd vóór kosten worden betaald.
3. Kaarten die expliciet targeten, leggen doelen bij activatie vast; niet-targetende selecties gebeuren tijdens resolutie.
4. Fusion- en Ritual-materialen worden eerst volledig gevalideerd en daarna atomair verwerkt.
5. Willekeurige keuzes worden via de deterministische duel-RNG uitgevoerd en in replays vastgelegd.
6. Gedeelde once-per-turn-limieten blokkeren alle gekoppelde modi met dezelfde sleutel.
7. Vorige zone, controller en face-upstatus zijn onderdeel van zone-move-events.
8. De bestaande `effectHandlerId`-waarden blijven ongewijzigd; deze batch claimt geen aanvullende runtime-implementatie.

## Bewijsstatus

De statische gegevens en handlerkoppelingen zijn lokaal aangetroffen. De specificaties zijn niet als nieuwe runtimecode uitgevoerd en er is voor dit document geen Unity-compile, PlayMode-test of visuele controle nodig of uitgevoerd.
