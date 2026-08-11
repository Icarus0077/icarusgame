# Card-effectbatch 001 — Blue-Eyes-kern

Status: **afgebakende lokale batch van 10 kaarten**  
Bron voor de werking: bestaande kaartdefinities en effecthandlers in dit Unity-project.  
Niet gebruikt: geautomatiseerde extractie van Konami's website of overname van officiële kaarttekst.

## Doel en interpretatie

Deze batch bevat per kaart:

- bestaande identificatie en statistieken;
- een nieuw geschreven Engelstalige spelerstekst (`displayText`);
- een technische, taalneutrale `EffectSpec` voor implementatie en tests;
- de bestaande `effectHandlerId` wanneer die al in het project is geregistreerd.

De spelerstekst is uitsluitend een leesbare samenvatting. Bij een verschil is `effectSpec` leidend. Gestandaardiseerde speltermen zoals *Special Summon*, *GY*, *target*, *Ritual Summon* en *once per turn* zijn bewust behouden, omdat ze een technische betekenis hebben.

---

## 1. Blue-Eyes White Dragon

```yaml
databaseId: "89631139"
name: Blue-Eyes White Dragon
cardClass: Normal Monster
attribute: LIGHT
monsterType: Dragon
level: 8
attack: 3000
defense: 2500
effectHandlerId: ""
displayText: >-
  A legendary azure dragon whose overwhelming strength has made it an icon
  among duelists. Few monsters can meet its raw power in direct combat.
effectSpec:
  summonProcedure: normalMonsterRules
  effects: []
```

Implementatienotitie: dit is een Normal Monster. De beschrijving mag nooit als uitvoerbaar effect worden geïnterpreteerd.

---

## 2. Neo Kaiser Sea Horse

```yaml
databaseId: "54332792"
name: Neo Kaiser Sea Horse
cardClass: Tuner Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 4
attack: 1700
defense: 1650
effectHandlerId: neo_kaiser_clean
displayText: >-
  While Blue-Eyes White Dragon is on your field, you may Special Summon this
  monster from your hand. Once per turn, choose a LIGHT Tuner you control and
  raise or lower its Level by 1. After this card moves from your field to the
  GY, you may send one eligible Blue-Eyes-related monster from your Deck to the
  GY, other than another Neo Kaiser Sea Horse. Each ability is usable once per
  turn.
effectSpec:
  effects:
    - id: specialSummonSelf
      kind: ignition
      activationZone: hand
      conditions:
        - controllerControlsCardId: "89631139"
        - controllerHasOpenMonsterZone: true
      costs: []
      targets: []
      operations:
        - specialSummonSelfFrom: hand
      limits:
        key: neoKaiser.specialSummonSelf
        scope: perControllerPerTurn

    - id: adjustLightTunerLevel
      kind: ignition
      activationZone: monsterZone
      conditions: []
      costs: []
      targets:
        - controller: self
          zone: monsterZone
          faceUp: true
          attribute: LIGHT
          tuner: true
          amount: 1
      choices:
        - levelDelta: 1
        - levelDelta: -1
      operations:
        - modifyTargetLevelByChosenDelta: true
      limits:
        key: neoKaiser.adjustLevel
        scope: perControllerPerTurn

    - id: sendRelatedMonsterToGraveyard
      kind: trigger
      trigger:
        event: cardMoved
        from: monsterZone
        to: graveyard
        card: self
      conditions: []
      costs: []
      targets: []
      selections:
        - zone: deck
          owner: controller
          amount: 1
          excludeCardId: "54332792"
          matchAny:
            - archetype: Blue-Eyes
            - mentionsCardId: "89631139"
      operations:
        - moveSelectedCardTo: graveyard
      limits:
        key: neoKaiser.sendRelatedMonster
        scope: perControllerPerTurn
```

---

## 3. Sage with Eyes of Blue

```yaml
databaseId: "08240199"
name: Sage with Eyes of Blue
cardClass: Tuner Effect Monster
attribute: LIGHT
monsterType: Spellcaster
level: 1
attack: 0
defense: 1500
effectHandlerId: sage_clean
displayText: >-
  When this monster is Normal Summoned, you may take a different Level 1 LIGHT
  Tuner from your Deck into your hand. Alternatively, discard this card and
  choose an Effect Monster you control; send that monster to the GY, then
  Special Summon a Blue-Eyes monster from your Deck. The second ability is
  available once per turn.
effectSpec:
  effects:
    - id: searchLevelOneLightTuner
      kind: trigger
      trigger:
        event: normalSummoned
        card: self
      optional: true
      selections:
        - zone: deck
          owner: controller
          amount: 1
          level: 1
          attribute: LIGHT
          tuner: true
          excludeCardId: "08240199"
      operations:
        - moveSelectedCardTo: hand

    - id: transformControlledMonsterIntoBlueEyes
      kind: ignition
      activationZone: hand
      costs:
        - moveSelf:
            from: hand
            to: graveyard
      targets:
        - controller: self
          zone: monsterZone
          faceUp: true
          effectMonster: true
          amount: 1
      operations:
        - moveTargetTo: graveyard
        - ifPreviousOperationSucceeded:
            select:
              zone: deck
              owner: controller
              amount: 1
              archetype: Blue-Eyes
            specialSummonSelectedTo: controllerMonsterZone
      limits:
        key: sage.transformMonster
        scope: perControllerPerTurn
```

Implementatienotitie: het doel wordt bij activatie vastgelegd. Het sturen van het doel naar de GY is onderdeel van de resolutie, niet van de kosten.

---

## 4. The White Stone of Ancients

```yaml
databaseId: "71039903"
name: The White Stone of Ancients
cardClass: Tuner Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 1
attack: 600
defense: 500
effectHandlerId: stone_ancients_clean
displayText: >-
  During the End Phase of a turn in which this card reached the GY, you may
  Special Summon a Blue-Eyes monster from your Deck. You may also banish this
  card from the GY to return a Blue-Eyes monster from your GY to your hand.
  Each ability is limited to once per turn.
effectSpec:
  effects:
    - id: endPhaseBlueEyesSummon
      kind: trigger
      trigger:
        event: phaseEntered
        phase: end
      activationZone: graveyard
      conditions:
        - selfEnteredGraveyardThisTurn: true
        - controllerHasOpenMonsterZone: true
      selections:
        - zone: deck
          owner: controller
          amount: 1
          archetype: Blue-Eyes
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
      limits:
        key: whiteStoneAncients.endPhaseSummon
        scope: perControllerPerTurn

    - id: recoverBlueEyes
      kind: ignition
      activationZone: graveyard
      costs:
        - banishSelfFrom: graveyard
      targets:
        - controller: self
          zone: graveyard
          archetype: Blue-Eyes
          amount: 1
      operations:
        - moveTargetTo: hand
      limits:
        key: whiteStoneAncients.recover
        scope: perControllerPerTurn
```

---

## 5. Maiden of White

```yaml
databaseId: "17947697"
name: Maiden of White
cardClass: Tuner Effect Monster
attribute: LIGHT
monsterType: Spellcaster
level: 1
attack: 0
defense: 0
effectHandlerId: maiden_white
displayText: >-
  Send this card from your hand or field to the GY to place True Light face-up
  from your hand, Deck, or GY. If Blue-Eyes White Dragon is Special Summoned
  while this card is in the GY, you may Special Summon this card. When this
  face-up card becomes the target of an attack, card, or effect, you may Special
  Summon Blue-Eyes White Dragon or a Level 1 LIGHT Tuner from your GY. Each
  ability is usable once per turn.
effectSpec:
  effects:
    - id: placeTrueLight
      kind: ignition
      activationZones: [hand, monsterZone]
      costs:
        - moveSelf:
            from: activationZone
            to: graveyard
      selections:
        - zones: [hand, deck, graveyard]
          owner: controller
          amount: 1
          cardId: "62089826"
      operations:
        - placeSelectedFaceUpIn: spellTrapZone
      limits:
        key: maidenWhite.placeTrueLight
        scope: perControllerPerTurn

    - id: reviveAfterBlueEyesSummon
      kind: trigger
      activationZone: graveyard
      trigger:
        event: monsterSpecialSummoned
        cardId: "89631139"
        controller: self
      conditions:
        - controllerHasOpenMonsterZone: true
      operations:
        - specialSummonSelfFrom: graveyard
      limits:
        key: maidenWhite.reviveSelf
        scope: perControllerPerTurn

    - id: summonFromGraveyardWhenTargeted
      kind: trigger
      activationZone: monsterZone
      triggerAny:
        - event: selectedAsAttackTarget
          card: self
        - event: selectedAsCardOrEffectTarget
          card: self
      selections:
        - zone: graveyard
          owner: controller
          amount: 1
          matchAny:
            - cardId: "89631139"
            - level: 1
              attribute: LIGHT
              tuner: true
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
      limits:
        key: maidenWhite.targetResponse
        scope: perControllerPerTurn
```

---

## 6. Dictator of D.

```yaml
databaseId: "66961194"
name: Dictator of D.
cardClass: Effect Monster
attribute: DARK
monsterType: Spellcaster
level: 4
attack: 1200
defense: 1100
effectHandlerId: dictator_clean
displayText: >-
  While you control a Blue-Eyes monster, you decide which of your monsters the
  opponent attacks. To Special Summon this card from your hand, send Blue-Eyes
  White Dragon from your hand or Deck to the GY. You may also discard Blue-Eyes
  White Dragon, or a card that refers to it, then choose and Special Summon a
  Blue-Eyes monster from your GY. Each activated ability is usable once per turn.
effectSpec:
  effects:
    - id: redirectOpponentAttacks
      kind: continuous
      activationZone: monsterZone
      conditions:
        - controllerControlsArchetype: Blue-Eyes
      modifiers:
        - attackTargetChooser: controller

    - id: specialSummonSelf
      kind: ignition
      activationZone: hand
      costs:
        - selectAndMove:
            zones: [hand, deck]
            owner: controller
            cardId: "89631139"
            amount: 1
            destination: graveyard
      operations:
        - specialSummonSelfFrom: hand
      limits:
        key: dictator.specialSummonSelf
        scope: perControllerPerTurn

    - id: reviveBlueEyes
      kind: ignition
      activationZone: monsterZone
      costs:
        - discardFromHand:
            amount: 1
            matchAny:
              - cardId: "89631139"
              - mentionsCardId: "89631139"
      targets:
        - controller: self
          zone: graveyard
          archetype: Blue-Eyes
          amount: 1
      operations:
        - specialSummonTargetTo: controllerMonsterZone
      limits:
        key: dictator.reviveBlueEyes
        scope: perControllerPerTurn
```

---

## 7. Blue-Eyes Jet Dragon

```yaml
databaseId: "30576089"
name: Blue-Eyes Jet Dragon
cardClass: Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 8
attack: 3000
defense: 0
effectHandlerId: jet_clean
displayText: >-
  Your other cards are protected from destruction by opposing card effects.
  While Blue-Eyes White Dragon is on your field or in your GY, the following
  abilities are available once per turn: after a card on the field is destroyed
  by battle or an effect, Special Summon this card from your hand or GY; and at
  the start of the Damage Step when this card battles, return one opposing card
  to its owner's hand.
effectSpec:
  effects:
    - id: protectOtherControlledCards
      kind: continuous
      activationZone: monsterZone
      affectedCards:
        controller: self
        excludeSelf: true
      modifiers:
        - preventDestruction:
            cause: opponentCardEffect

    - id: specialSummonAfterDestruction
      kind: trigger
      activationZones: [hand, graveyard]
      trigger:
        event: cardDestroyed
        zone: field
        causeAny: [battle, cardEffect]
      conditions:
        - controllerFieldOrGraveyardContainsCardId: "89631139"
        - ifActivationZoneIsGraveyard:
            selfWasAlreadyInGraveyardWhenTriggerOccurred: true
        - controllerHasOpenMonsterZone: true
      operations:
        - specialSummonSelfFrom: activationZone
      limits:
        key: jetDragon.specialSummonSelf
        scope: perControllerPerTurn

    - id: returnOpposingCardAfterBattleStarts
      kind: trigger
      activationZone: monsterZone
      trigger:
        event: damageStepStarted
        battlingCard: self
      conditions:
        - controllerFieldOrGraveyardContainsCardId: "89631139"
      targets:
        - controller: opponent
          zone: field
          amount: 1
      operations:
        - returnTargetToOwnerHand: true
      limits:
        key: jetDragon.returnCard
        scope: perControllerPerTurn
```

---

## 8. Blue-Eyes Alternative White Dragon

```yaml
databaseId: "38517737"
name: Blue-Eyes Alternative White Dragon
cardClass: Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 8
attack: 3000
defense: 2500
effectHandlerId: alternative_clean
displayText: >-
  This monster cannot be Normal Summoned or Set. First Special Summon it from
  your hand by revealing Blue-Eyes White Dragon there; this procedure is limited
  to once per turn. On the field and in the GY, this card is treated as Blue-Eyes
  White Dragon. Once per turn, it may destroy one opposing monster, but it cannot
  attack during the turn that ability is used.
effectSpec:
  summonProcedure:
    kind: inherentSpecialSummon
    from: hand
    cannotNormalSummonOrSet: true
    mustBeProperlySpecialSummonedFirst: true
    revealFromHand:
      cardId: "89631139"
      amount: 1
    limits:
      key: alternativeWhiteDragon.summonProcedure
      scope: perControllerPerTurn
  effects:
    - id: substituteName
      kind: continuous
      activeZones: [monsterZone, graveyard]
      treatedNameCardId: "89631139"

    - id: destroyOpponentMonster
      kind: ignition
      activationZone: monsterZone
      targets:
        - controller: opponent
          zone: monsterZone
          amount: 1
      activationConsequences:
        - selfCannotAttackForTurn: true
      operations:
        - destroyTarget:
            cause: cardEffect
      limits:
        key: alternativeWhiteDragon.destroyMonster
        scope: perControllerPerTurn
```

---

## 9. Blue-Eyes Abyss Dragon

```yaml
databaseId: "64202399"
name: Blue-Eyes Abyss Dragon
cardClass: Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 8
attack: 2500
defense: 2500
effectHandlerId: abyss_clean
displayText: >-
  Its abilities can be used only while Blue-Eyes White Dragon is on your field
  or in your GY. When this card is Special Summoned, you may take a Ritual Spell
  or Polymerization from your Deck into your hand. During your End Phase, you
  may take a Level 8 or higher Dragon from your Deck into your hand. You may also
  banish this card from the GY to grant 1000 ATK to your Level 8 or higher
  Dragons. Each ability is usable once per turn.
effectSpec:
  sharedActivationConditions:
    - controllerFieldOrGraveyardContainsCardId: "89631139"
  effects:
    - id: searchSummoningSpell
      kind: trigger
      trigger:
        event: specialSummoned
        card: self
      optional: true
      selections:
        - zone: deck
          owner: controller
          amount: 1
          matchAny:
            - spellSubtype: Ritual
            - cardId: "24094653"
      operations:
        - moveSelectedCardTo: hand
      limits:
        key: abyssDragon.searchSummoningSpell
        scope: perControllerPerTurn

    - id: endPhaseDragonSearch
      kind: trigger
      activationZone: monsterZone
      trigger:
        event: phaseEntered
        phase: end
        turnPlayer: controller
      selections:
        - zone: deck
          owner: controller
          amount: 1
          monsterType: Dragon
          minimumLevel: 8
      operations:
        - moveSelectedCardTo: hand
      limits:
        key: abyssDragon.endPhaseSearch
        scope: perControllerPerTurn

    - id: empowerHighLevelDragons
      kind: ignition
      activationZone: graveyard
      costs:
        - banishSelfFrom: graveyard
      affectedCards:
        controller: self
        zone: monsterZone
        monsterType: Dragon
        minimumLevel: 8
      operations:
        - modifyAttack:
            amount: 1000
            duration: whileAffectedCardRemainsFaceUp
      limits:
        key: abyssDragon.attackBoost
        scope: perControllerPerTurn
```

Implementatienotitie: de ATK-verhoging wordt als een toegepast modifier-event opgeslagen. Later opgeroepen monsters krijgen de verhoging niet met terugwerkende kracht.

---

## 10. Blue-Eyes Chaos MAX Dragon

```yaml
databaseId: "55410871"
name: Blue-Eyes Chaos MAX Dragon
cardClass: Ritual Effect Monster
attribute: DARK
monsterType: Dragon
level: 8
attack: 4000
defense: 0
effectHandlerId: chaos_max_clean
displayText: >-
  This monster must enter play through a Ritual Summon and can be Ritual
  Summoned with Chaos Form. Opposing effects cannot choose it as a target or
  destroy it. When it attacks a Defense Position monster, it deals twice the
  normal amount of piercing battle damage.
effectSpec:
  summonProcedure:
    kind: ritualSummon
    mustBeRitualSummoned: true
    compatibleRitualSpellCardIds:
      - "21082832"
  effects:
    - id: targetingProtection
      kind: continuous
      activationZone: monsterZone
      modifiers:
        - cannotBeTargetedBy:
            controller: opponent
            source: cardEffect

    - id: destructionProtection
      kind: continuous
      activationZone: monsterZone
      modifiers:
        - preventDestruction:
            cause: opponentCardEffect

    - id: doublePiercingDamage
      kind: continuous
      activationZone: monsterZone
      conditions:
        - selfIsAttacking: true
        - battleTargetPosition: defense
      modifiers:
        - piercingBattleDamage: true
        - piercingDamageMultiplier: 2
```

---

## Acceptatiecriteria voor deze batch

1. Geen `displayText` is nodig voor de daadwerkelijke effectresolutie.
2. Kosten worden betaald voordat een chain link wordt opgebouwd of opgelost, afhankelijk van het bestaande commandmodel.
3. Doelen worden bij activatie gekozen en bij resolutie opnieuw op geldigheid gecontroleerd.
4. `perControllerPerTurn`-sleutels zijn onafhankelijk per speler en worden bij de juiste turn boundary gewist.
5. Trigger-effecten onthouden de relevante gebeurtenisgegevens, waaronder vorige zone en aanwezigheid op het moment van de trigger.
6. Blue-Eyes White Dragon levert nooit een uitvoerbare actie op basis van zijn smaakbeschrijving.
7. De bestaande handlers blijven gekoppeld via `effectHandlerId`; deze batch wijzigt geen runtimecode.

## Bekende implementatiegrens

Deze specificatie beschrijft de bedoelde werking uit de al aanwezige lokale bron. Het bestaan van een handler betekent niet automatisch dat ieder timing-, chain- en randgeval al door een test bewezen is. Die bewijsstatus hoort per effectfamilie in EditMode- of smoke-tests te worden vastgelegd.
