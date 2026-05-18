# Master Duel Prototype

Deze branch voegt een eerste Unity-prototype toe dat automatisch start wanneer je in Unity op Play drukt.

## Starten

1. Open het project in Unity `6000.4.7f1`.
2. Open de bestaande `Assets/Scenes/SampleScene.unity`.
3. Druk op Play.

Het script `Assets/Scripts/MasterDuelPrototype.cs` maakt zelf een UI-canvas, bord, zones, handkaarten en duelbesturing aan. Er hoeft dus geen prefab of scene-object handmatig geplaatst te worden.

## Inhoud

- Master Duel-achtige tafel met twee spelers.
- 8000 LP per speler.
- Deck, hand, monsterzones, spell/trap-zones en graveyard-teller.
- Automatische Draw en Standby.
- Main Phase-acties: monster oproepen, tribute summons, spells activeren, traps/quick-spells zetten.
- Battle Phase met directe aanvallen, ATK vs ATK en ATK vs DEF.
- Reactie-momenten tijdens AI-aanvallen: je kunt een getriggerde trap of Quick-Play spell activeren of annuleren.
- Simpele AI-tegenstander die kaarten speelt en aanvalt.

Dit is bewust een speelbaar prototype met originele voorbeeldkaarten, geen officiele Yu-Gi-Oh!-kaartdatabase of Master Duel-assets.
