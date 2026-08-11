# Card-effectbatch 003 — Summon- en Extra Deck-laag

Status: **afgebakende batch van 10 kaarten**  
Omvang: 1 Ritual, 1 Main Deck Effect Monster, 2 Fusion, 3 Synchro, 2 Xyz en 1 Link.  
Bron voor de werking: de door de gebruiker als toegestaan aangemerkte officiële kaartinformatie, gecontroleerd tegen de bestaande lokale catalogus en handlers.  
Runtime-impact: geen; dit document specificeert gewenst gedrag voor latere implementatie en tests.

## 1. Blue-Eyes Ultimate Dragon

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=4386&request_locale=en)

```yaml
databaseId: "23995346"
name: Blue-Eyes Ultimate Dragon
cardClass: Fusion Monster
attribute: LIGHT
monsterType: Dragon
level: 12
attack: 4500
defense: 3800
effectHandlerId: ""
displayText: >-
  Fusion Summon this three-headed dragon by combining three monsters whose
  current name is Blue-Eyes White Dragon. It has no additional card effect.
effectSpec:
  summonProcedure:
    kind: fusionSummon
    materials:
      - effectiveCardId: "89631139"
        exactAmount: 3
  effects: []
```

Implementatienotitie: de materiaalregel is geen uitvoerbaar kaarteffect. Algemene Fusion-regels en toegestane materiaalvervangers bepalen of de summon legaal is.

---

## 2. Dragon Master Magia

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=19718&request_locale=en)

```yaml
databaseId: "12381100"
name: Dragon Master Magia
cardClass: Fusion Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 12
attack: 5000
defense: 4000
effectHandlerId: magia_clean
displayText: >-
  Fusion Summon this monster with either Blue-Eyes Ultimate Dragon or three
  Blue-Eyes monsters, together with a Chaos or Black Luster Soldier Ritual
  Monster. When the opponent activates a card or effect, you may negate that
  activation and destroy its source. This response is available once per turn
  for each source category: Monster, Spell, and Trap. If an opponent's card
  makes this face-up monster leave your field, you may Special Summon an
  eligible Blue-Eyes, Chaos, or Black Luster Soldier monster from your Extra
  Deck or GY.
effectSpec:
  summonProcedure:
    kind: fusionSummon
    mustBeFusionSummoned: true
    materialGroups:
      - chooseExactlyOne:
          - effectiveCardId: "23995346"
            exactAmount: 1
          - archetype: Blue-Eyes
            exactAmount: 3
      - cardClass: RitualMonster
        nameFamilyAny: [Chaos, BlackLusterSoldier]
        exactAmount: 1
  effects:
    - id: negateOpponentActivation
      kind: quick
      activationZone: monsterZone
      trigger:
        event: opponentCardOrEffectActivated
        sourceCardKindAny: [monster, spell, trap]
      operations:
        - negatePendingActivation: true
        - ifNegationSucceeded:
            destroyPendingSource:
              cause: cardEffect
      limits:
        dynamicKeyFromTriggerSourceKind:
          monster: dragonMasterMagia.negateMonster
          spell: dragonMasterMagia.negateSpell
          trap: dragonMasterMagia.negateTrap
        scope: perControllerPerTurn

    - id: summonSuccessorAfterOpponentRemoval
      kind: trigger
      trigger:
        event: cardLeftField
        card: self
        previousController: owner
        previousFaceState: faceUp
        causedByOpponentCard: true
      selections:
        - zones: [extraDeck, graveyard]
          owner: controller
          cardKind: monster
          amount: 1
          matchAny:
            - archetype: Blue-Eyes
            - cardClass: RitualMonster
              nameFamilyAny: [Chaos, BlackLusterSoldier]
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
          summonMethod: effect
          properlySummoned: false
          obeyPrintedSummonRestrictions: true
```

Implementatienotitie: de drie negaties hebben afzonderlijke limietsleutels. Een mislukte vernietiging maakt een geslaagde negatie niet ongedaan.

---

## 3. Blue-Eyes Spirit Dragon

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=12324&request_locale=en)

```yaml
databaseId: "59822133"
name: Blue-Eyes Spirit Dragon
cardClass: Synchro Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 9
attack: 2500
defense: 3000
effectHandlerId: spirit_dragon_clean
displayText: >-
  Synchro Summon this monster with one Tuner and one or more non-Tuner
  Blue-Eyes monsters. While it remains face-up, neither player may Special
  Summon multiple monsters in a single event. Once per turn, it can negate an
  effect activated in a GY. As a Quick Effect, you may Tribute this Synchro
  Summoned card to Special Summon a different LIGHT Dragon Synchro Monster from
  your Extra Deck in Defense Position; destroy that monster during this turn's
  End Phase.
effectSpec:
  summonProcedure:
    kind: synchroSummon
    materials:
      - tuner: true
        exactAmount: 1
      - tuner: false
        archetype: Blue-Eyes
        minimumAmount: 1
    totalLevelsMustEqualSelfLevel: true
  effects:
    - id: preventMultiMonsterSpecialSummons
      kind: continuous
      activationZone: monsterZone
      modifiers:
        - maximumMonstersPerSpecialSummonEvent: 1
          affectedPlayers: both

    - id: negateGraveyardEffect
      kind: quick
      activationZone: monsterZone
      trigger:
        event: cardEffectActivated
        activationZone: graveyard
      operations:
        - negatePendingActivation: true
      limits:
        key: blueEyesSpiritDragon.graveyardNegate
        scope: perControllerPerTurn

    - id: replaceSelfWithLightDragonSynchro
      kind: quick
      activationZone: monsterZone
      conditions:
        - selfWasSynchroSummoned: true
        - controllerHasLegalExtraDeckSummonZone: true
      costs:
        - tributeSelfFrom: monsterZone
      selections:
        - zone: extraDeck
          owner: controller
          cardClass: SynchroMonster
          attribute: LIGHT
          monsterType: Dragon
          excludeCardId: "59822133"
          amount: 1
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
          position: faceUpDefense
          summonMethod: effect
          properlySummoned: false
        - scheduleOperation:
            event: currentTurnEndPhase
            operation:
              destroySummonedCard:
                cause: cardEffect
```

Implementatienotitie: de summonbeperking geldt per summon-event. Een effect dat meerdere monsters tegelijk wil oproepen is volledig illegaal en wordt niet gereduceerd tot één monster.

---

## 4. Blue-Eyes Ultimate Spirit Dragon

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=20607&request_locale=en)

```yaml
databaseId: "89604813"
name: Blue-Eyes Ultimate Spirit Dragon
cardClass: Synchro Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 12
attack: 3500
defense: 4000
effectHandlerId: ultimate_spirit_clean
displayText: >-
  Synchro Summon this monster with at least two Tuners and one non-Tuner
  Blue-Eyes monster. The opponent cannot banish cards from your GY. Once per
  turn, when a card or effect is activated on the field, you may negate its
  activation; after a successful negation this monster gains 1000 ATK through
  the end of the turn. Once per turn, if it is destroyed in battle or by an
  effect, you may Special Summon a different LIGHT Dragon from your GY.
effectSpec:
  summonProcedure:
    kind: synchroSummon
    materials:
      - tuner: true
        minimumAmount: 2
      - tuner: false
        archetype: Blue-Eyes
        exactAmount: 1
    totalLevelsMustEqualSelfLevel: true
  effects:
    - id: protectOwnGraveyardFromBanishment
      kind: continuous
      activationZone: monsterZone
      affectedPlayer: opponent
      modifiers:
        - cannotBanishCardsFromZone:
            owner: controller
            zone: graveyard

    - id: negateFieldActivation
      kind: quick
      activationZone: monsterZone
      trigger:
        event: cardOrEffectActivated
        sourceZone: field
      operations:
        - negatePendingActivation: true
        - ifNegationSucceeded:
            modifySelfAttack:
              amount: 1000
              duration: untilEndOfCurrentTurn
      limits:
        key: ultimateSpiritDragon.fieldNegate
        scope: perControllerPerTurn

    - id: reviveLightDragonAfterDestruction
      kind: trigger
      trigger:
        event: cardDestroyed
        card: self
        causeAny: [battle, cardEffect]
      selections:
        - zone: graveyard
          owner: controller
          attribute: LIGHT
          monsterType: Dragon
          excludeCardId: "89604813"
          amount: 1
          summonLegality: standardRevivalRules
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
      limits:
        key: ultimateSpiritDragon.reviveLightDragon
        scope: perControllerPerTurn
```

---

## 5. Azure-Eyes Silver Dragon

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=10593&request_locale=en)

```yaml
databaseId: "40908371"
name: Azure-Eyes Silver Dragon
cardClass: Synchro Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 9
attack: 2500
defense: 3000
effectHandlerId: azure_eyes_clean
displayText: >-
  Synchro Summon this monster with one Tuner and one or more non-Tuner Normal
  Monsters. When it is Special Summoned, protect every Dragon you currently
  control from card-effect targeting and destruction through the end of the
  next turn. Once per turn during your Standby Phase, you may Special Summon a
  Normal Monster from your GY.
effectSpec:
  summonProcedure:
    kind: synchroSummon
    materials:
      - tuner: true
        exactAmount: 1
      - tuner: false
        cardClass: NormalMonster
        minimumAmount: 1
    totalLevelsMustEqualSelfLevel: true
  effects:
    - id: protectCurrentDragons
      kind: trigger
      trigger:
        event: specialSummoned
        card: self
      affectedCardsSnapshot:
        controller: self
        zone: monsterZone
        monsterType: Dragon
      operations:
        - applyModifierToSnapshot:
            cannotBeTargetedByCardEffects: true
            cannotBeDestroyedByCardEffects: true
            affectedPlayers: both
            expiresAt: endOfNextTurn

    - id: reviveNormalMonster
      kind: trigger
      activationZone: monsterZone
      trigger:
        event: phaseEntered
        phase: standby
        turnPlayer: controller
      optional: true
      targets:
        - zone: graveyard
          owner: controller
          cardClass: NormalMonster
          amount: 1
          summonLegality: standardRevivalRules
      operations:
        - specialSummonTargetTo: controllerMonsterZone
      limits:
        key: azureEyes.reviveNormalMonster
        scope: perControllerPerTurn
```

Implementatienotitie: de bescherming gebruikt een snapshot. Draken die later op het veld komen, ontvangen deze beschermingslaag niet.

---

## 6. Spirit with Eyes of Blue

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=20609&request_locale=en)

```yaml
databaseId: "42097666"
name: Spirit with Eyes of Blue
cardClass: Link Effect Monster
attribute: LIGHT
monsterType: Dragon
linkRating: 1
linkArrows: [southwest]
attack: 300
defense: null
effectHandlerId: spirit_with_eyes_clean
displayText: >-
  Link Summon this monster with one Level 4 or lower Dragon or Spellcaster.
  While it is face-up, you may Special Summon only Dragon monsters. After its
  Link Summon, you may either add Mausoleum of White from your Deck to your hand
  or send it to the GY. You may also Tribute this card to Special Summon a
  Blue-Eyes monster from your hand or GY; an Effect Monster revived from the GY
  this way has its effects negated and cannot attack. Each activated ability is
  usable once per turn.
effectSpec:
  summonProcedure:
    kind: linkSummon
    requiredLinkMaterialTotal: 1
    materials:
      - maximumLevel: 4
        monsterTypeAny: [Dragon, Spellcaster]
        exactAmount: 1
  effects:
    - id: restrictSpecialSummonsToDragons
      kind: continuous
      activationZone: monsterZone
      affectedPlayer: controller
      modifiers:
        - specialSummonAllowedMonsterType: Dragon

    - id: routeMausoleum
      kind: trigger
      trigger:
        event: linkSummoned
        card: self
      selections:
        - zone: deck
          owner: controller
          cardId: "24382602"
          amount: 1
      choices:
        - moveSelectedTo: hand
        - moveSelectedTo: graveyard
      limits:
        key: spiritWithEyes.routeMausoleum
        scope: perControllerPerTurn

    - id: tributeForBlueEyes
      kind: ignition
      activationZone: monsterZone
      costs:
        - tributeSelfFrom: monsterZone
      selections:
        - zones: [hand, graveyard]
          owner: controller
          archetype: Blue-Eyes
          amount: 1
          summonLegality: standardRulesForSourceZone
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
        - ifAll:
            - selectedPreviousZone: graveyard
            - selectedCardClassIsNot: NormalMonster
          then:
            applyToSummonedCard:
              effectsNegated: true
              cannotAttack: true
              duration: whileFaceUpOnField
      limits:
        key: spiritWithEyes.summonBlueEyes
        scope: perControllerPerTurn
```

---

## 7. Number 38: Hope Harbinger Dragon Titanic Galaxy

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=12260&request_locale=en)

```yaml
databaseId: "63767246"
name: "Number 38: Hope Harbinger Dragon Titanic Galaxy"
cardClass: Xyz Effect Monster
attribute: LIGHT
monsterType: Dragon
rank: 8
attack: 3000
defense: 2500
effectHandlerId: hope_harbinger_clean
displayText: >-
  Xyz Summon this monster with two Level 8 monsters. Once per turn, when a Spell
  Card or a Spell effect is activated on the field, you may negate that effect
  and, when possible, place its source beneath this card as material. When an
  opposing monster attacks, you may detach one material to redirect that attack
  to this monster and immediately calculate damage. If another face-up Xyz
  Monster you control is destroyed, choose one of your remaining face-up Xyz
  Monsters; it gains ATK equal to the original ATK of one destroyed monster.
effectSpec:
  summonProcedure:
    kind: xyzSummon
    materials:
      - level: 8
        exactAmount: 2
  effects:
    - id: negateFieldSpellEffect
      kind: quick
      activationZone: monsterZone
      trigger:
        event: spellCardOrSpellEffectActivated
        sourceZone: field
      operations:
        - negatePendingEffect: true
        - ifNegationSucceededAndSourceRemainsOnField:
            attachPendingSourceToSelfAsXyzMaterial: true
      limits:
        key: hopeHarbinger.negateSpell
        scope: perControllerPerTurn

    - id: redirectOpponentAttack
      kind: trigger
      activationZone: monsterZone
      trigger:
        event: opponentAttackDeclared
      costs:
        - detachXyzMaterialFromSelf:
            amount: 1
            destination: graveyard
      operations:
        - changeAttackTargetTo: self
        - immediatelyPerformDamageCalculation: true

    - id: inheritDestroyedXyzAttack
      kind: trigger
      activationZone: monsterZone
      trigger:
        event: controlledCardsDestroyed
        causeAny: [battle, cardEffect]
        requiredPreviousState:
          faceUp: true
          cardClass: XyzMonster
          excludeSelf: true
      choices:
        - chooseOneDestroyedMonsterAsAttackReference: true
      targets:
        - controller: self
          zone: monsterZone
          faceUp: true
          cardClass: XyzMonster
          amount: 1
      operations:
        - increaseTargetAttackBy:
            value: originalAttackOfChosenDestroyedMonster
            duration: whileTargetRemainsFaceUp
```

Implementatienotitie: de tweede en derde effecten hebben geen gedrukte once-per-turn-limiet. De Spell-bron wordt alleen materiaal wanneer deze bij resolutie nog op het veld aanwezig en verplaatsbaar is.

---

## 8. Galaxy-Eyes Cipher Dragon

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=12552&request_locale=en)

```yaml
databaseId: "18963306"
name: Galaxy-Eyes Cipher Dragon
cardClass: Xyz Effect Monster
attribute: LIGHT
monsterType: Dragon
rank: 8
attack: 3000
defense: 2500
effectHandlerId: cipher_dragon_clean
displayText: >-
  Xyz Summon this monster with two Level 8 monsters. Once per turn, detach one
  material and choose a face-up opposing monster. Take control of that monster
  through the End Phase; while controlled this way, its effects are negated,
  its ATK is 3000, and its name is Galaxy-Eyes Cipher Dragon. For the remainder
  of the turn, your other monsters cannot attack the opponent directly.
effectSpec:
  summonProcedure:
    kind: xyzSummon
    materials:
      - level: 8
        exactAmount: 2
  effects:
    - id: temporarilyStealMonster
      kind: ignition
      activationZone: monsterZone
      costs:
        - detachXyzMaterialFromSelf:
            amount: 1
            destination: graveyard
      targets:
        - controller: opponent
          zone: monsterZone
          faceUp: true
          amount: 1
          controlCanChange: true
      activationConsequences:
        - directAttackRestriction:
            affectedCards:
              controller: self
              excludeSelf: true
            duration: restOfTurn
      operations:
        - transferControlOfTargetTo: controller
          duration: untilEndPhase
        - applyToTarget:
            effectsNegated: true
            attackBecomes: 3000
            effectiveNameBecomesCardId: "18963306"
            duration: whileControlChangeFromThisEffectApplies
      expiry:
        event: endPhase
        operations:
          - returnTargetToPreviousControllerIfPossible: true
          - removeModifiersAppliedByThisEffect: true
      limits:
        key: galaxyEyesCipher.controlChange
        scope: perControllerPerTurn
```

---

## 9. Blue-Eyes Chaos Dragon

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=13842&request_locale=en)

```yaml
databaseId: "20654247"
name: Blue-Eyes Chaos Dragon
cardClass: Ritual Effect Monster
attribute: DARK
monsterType: Dragon
level: 8
attack: 3000
defense: 0
effectHandlerId: chaos_dragon_clean
displayText: >-
  This monster must be Ritual Summoned and is compatible with Chaos Form.
  Opposing card effects cannot target or destroy it. If Blue-Eyes White Dragon
  was used for its Ritual Summon, an attack declaration by this monster lets
  you change the battle position of every opposing monster that can legally be
  changed. Monsters changed this way have their ATK and DEF set to 0, and this
  monster deals piercing damage against Defense Position monsters for the rest
  of that turn.
effectSpec:
  summonProcedure:
    kind: ritualSummon
    mustBeRitualSummoned: true
    compatibleRitualSpellCardIds: ["21082832"]
    retainMaterialHistory: true
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

    - id: collapseOpponentBattleLines
      kind: trigger
      activationZone: monsterZone
      trigger:
        event: attackDeclared
        attacker: self
      conditions:
        - selfRitualSummonUsedEffectiveCardId: "89631139"
      affectedCards:
        controller: opponent
        zone: monsterZone
        battlePositionCanChange: true
      operations:
        - changeBattlePositionOfAsManyAffectedAsPossible: true
        - ifAtLeastOnePositionChanged:
            applyToChangedCards:
              attackBecomes: 0
              defenseBecomes: 0
              duration: whileFaceUpOnField
            applyToSelf:
              piercingBattleDamage: true
              duration: restOfTurn
```

Implementatienotitie: de engine moet Ritual-materiaalhistorie bewaren. Alleen monsters waarvan de positie werkelijk veranderde krijgen ATK/DEF 0.

---

## 10. Dragon Spirit of White

Bronverwijzing: [Yu-Gi-Oh! Neuron](https://www.db.yugioh-card.com/yugiohdb/card_search.action?ope=2&cid=12290&request_locale=en)

```yaml
databaseId: "45467446"
name: Dragon Spirit of White
cardClass: Effect Monster
attribute: LIGHT
monsterType: Dragon
level: 8
attack: 2500
defense: 2000
effectHandlerId: dragon_spirit_white_clean
displayText: >-
  This card always belongs to the Blue-Eyes family and is treated as a Normal
  Monster while in the hand or GY. When it is Normal or Special Summoned, you
  may choose and banish one opposing Spell/Trap. If the opponent controls a
  monster, you may use a Quick Effect to Tribute this card and Special Summon
  Blue-Eyes White Dragon from your hand.
effectSpec:
  classificationRules:
    - alwaysHasArchetype: Blue-Eyes
    - treatedAsNormalMonsterInZones: [hand, graveyard]
  effects:
    - id: banishOpponentSpellTrap
      kind: trigger
      triggerAny:
        - event: normalSummoned
          card: self
        - event: specialSummoned
          card: self
      optional: true
      targets:
        - controller: opponent
          zone: spellTrapZoneOrFieldZone
          cardKindAny: [spell, trap]
          amount: 1
      operations:
        - banishTarget: faceUpByDefault

    - id: tributeForBlueEyesWhiteDragon
      kind: quick
      activationZone: monsterZone
      conditions:
        - opponentControlsAtLeastOneMonster: true
        - controllerHasOpenMonsterZoneAfterCost: true
        - controllerHandContainsCardId: "89631139"
      costs:
        - tributeSelfFrom: monsterZone
      selections:
        - zone: hand
          owner: controller
          cardId: "89631139"
          amount: 1
      operations:
        - specialSummonSelectedTo: controllerMonsterZone
```

---

## Acceptatiecriteria voor batch 003

1. Fusion-, Synchro-, Xyz-, Ritual- en Link-recepten zijn afzonderlijke summonprocedures en worden niet als gewone effecten geactiveerd.
2. Effecten met materiaalhistorie kunnen gebruikte kaarten en hun effectieve namen achteraf betrouwbaar controleren.
3. `once per turn per card type` gebruikt drie onafhankelijke limietsleutels voor Monster, Spell en Trap.
4. Snapshot-effecten bewaren stabiele kaart-instance-ID’s en breiden zich niet uit naar later verschenen monsters.
5. Uitgestelde End Phase-acties verwijzen naar de werkelijk opgeroepen kaartinstantie en zijn replay-deterministisch.
6. Xyz-materialen zijn gekoppelde kaartinstanties, geen teller zonder identiteit.
7. Tijdelijke controlewissels bewaren de vorige controller en verwijderen alle door het effect toegepaste modifiers bij afloop.
8. Niet-targetende selecties en targetende effecten blijven onderscheiden.
9. Een bestaande `effectHandlerId` betekent alleen dat een lokale koppeling bestaat; lege of ontbrekende runtimecases worden niet als voltooid gemarkeerd.
10. `displayText` wordt nooit gebruikt om regels te parsen of gameplay te sturen.

## Bewijsstatus

De statische kaartgegevens zijn gecontroleerd tegen de lokale catalogus en de kaartwerking tegen de toegestane officiële kaartpagina’s. Dit document voegt geen runtimecode toe; daarom is geen Unity-compile, PlayMode-test of visuele controle uitgevoerd.
