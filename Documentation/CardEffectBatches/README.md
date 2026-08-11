# Card-effectbatches

Dit register houdt bij welke kaarten een nieuw geschreven `displayText` en een technische `EffectSpec` hebben gekregen. De batchdocumenten zijn specificaties voor implementatie en tests; ze wijzigen zelf geen runtimegedrag.

| Batch | Thema | Kaarten | Status |
| --- | --- | ---: | --- |
| [001](BATCH_001_BLUE_EYES_CORE.md) | Blue-Eyes-kernmonsters | 10 | Gespecificeerd |
| [002](BATCH_002_BLUE_EYES_SPELLS_TRAPS.md) | Blue-Eyes-deck: Spells en Traps | 10 | Gespecificeerd |
| [003](BATCH_003_BLUE_EYES_SUMMON_EXTRA_DECK.md) | Ritual-, Fusion-, Synchro-, Xyz- en Link-kaarten | 10 | Gespecificeerd |

Handmatig op effectniveau gespecificeerd: **30 kaarten**.

## Volledige catalogusdekking

De eenmalige volledige documentatierun staat in [../CardEffectCorpus/README.md](../CardEffectCorpus/README.md):

- catalogus totaal: **13.820 kaarten**;
- handmatige overrides in deze map: **30 kaarten**;
- automatisch gegenereerde intentieconcepten: **13.790 kaarten**;
- ontbrekende of dubbel opgenomen IDs: **0**.

Gebruik voor zoeken eerst [../CardEffectCorpus/ALL_CARDS_INDEX.md](../CardEffectCorpus/ALL_CARDS_INDEX.md). Kaarten uit de drie handmatige batches zijn bewust niet nogmaals in de automatische shards geplaatst. De handmatige `EffectSpec` is voor die kaarten leidend.

## Conventies

- `displayText` is nieuw geschreven, uitsluitend bedoeld voor spelers.
- `effectSpec` is leidend voor voorwaarden, kosten, doelen, keuzes, resolutie en limieten.
- `databaseId` blijft een string, zodat eventuele voorloopnullen behouden blijven.
- `effectHandlerId` verwijst naar een bestaande runtimekoppeling en bewijst op zichzelf geen volledige implementatie.
- Iedere volgende batch moet vóór toevoeging op dubbele `databaseId`-waarden worden gecontroleerd.
- Automatische corpuskaarten blijven `AutomatedIntentDraft` totdat kaart- en effectfamilietests de runtimewerking bewijzen.
