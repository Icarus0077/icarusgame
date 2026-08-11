# Volledig Master Duel-kaarteffectcorpus

> Snapshot: `2026-08-10` | cataloguskaarten: `13820` | handmatig uitgewerkt: `30` | automatisch concept: `13790`.

## Wat deze run oplevert

Alle cataloguskaarten die niet al in `Documentation/CardEffectBatches/` handmatig zijn uitgewerkt, staan in de shards hieronder. Iedere kaart bevat statische feiten, een nieuw geformuleerde spelbeschrijving, een hash van de gebruikte Engelstalige brontekst, een officiele verificatielink en een automatisch `effectSpecDraft` met clause-, timing-, zone-, cost-, target-, operation- en limitinformatie.

De oorspronkelijke kaarttekst wordt niet opgenomen. Een automatisch concept is geen claim van uitvoerbare regelnauwkeurigheid: PSCT-conjuncties, chains, missing-the-timing, materiaalvervanging, zone-identiteit en kaart-specifieke rulings vereisen tests en waar nodig handmatige modellering. De 30 kaarten in de drie handmatige batches blijven de hogere kwaliteitslaag.

## Dekking

| Eigenschap | Aantal |
| --- | ---: |
| Catalogus totaal | 13820 |
| Handmatige overrides | 30 |
| Automatische kaartconcepten | 13790 |
| Geen uitvoerbaar effect volgens metadata | 648 |
| Confidence High | 661 |
| Confidence Medium | 10310 |
| Confidence Low | 2819 |
| Markdown-shards | 138 |

Volledige zoekindex: [ALL_CARDS_INDEX.md](ALL_CARDS_INDEX.md). Handmatige specificaties: [../CardEffectBatches/README.md](../CardEffectBatches/README.md).

## Gebruik door een implementatie-agent

1. Zoek de kaart op ID in `ALL_CARDS_INDEX.md`.
2. Open de genoemde shard en lees `displayText`, `effectSpecDraft.clauses` en `reviewReasons`.
3. Controleer de officiele bronlink en eventuele rulings.
4. Modelleer een herbruikbare effectfamilie; maak geen centrale switch per kaart-ID.
5. Voeg rules tests toe voor kosten, targets, chain-resolutie, zonebeweging, limieten en expiry.
6. Zet pas na bewezen tests een kaartstatus om van concept naar uitvoerbaar.

## Reproduceerbaarheid

- Bron-JSON SHA-256: `5bda4054067906c8a1be0fbe4ee8c1794af17167d635244bd96644a6e8c0628d`
- Catalogus-TSV SHA-256: `a39cbd6948c696155f468bf9d04b0e32aa8dace3bf8729d4d3d0296bfb3d4288`
- Generator: `Tools/YGOProDeck/GenerateCardEffectCorpus.ps1`

## Shards

| Bereik | Eerste kaart | Laatste kaart | Bestand | SHA-256 (kort) |
| ---: | --- | --- | --- | --- |
| 1-100 | 1st Movement Solo | Abyss-sting Triaina | [CARDS_00001_00100.md](CARDS_00001_00100.md) | `344556cc928a` |
| 101-200 | Abyss-strom | Alghoul Mazera | [CARDS_00101_00200.md](CARDS_00101_00200.md) | `cfc6fc41e1b6` |
| 201-300 | Alice, Lady of Lament | Amaze Attraction Horror House | [CARDS_00201_00300.md](CARDS_00201_00300.md) | `42b0da421a77` |
| 301-400 | Amaze Attraction Majestic Merry-Go-Round | Ancient Gear Box | [CARDS_00301_00400.md](CARDS_00301_00400.md) | `f0ae5d88c097` |
| 401-500 | Ancient Gear Cannon | Ape Magician | [CARDS_00401_00500.md](CARDS_00401_00500.md) | `dcb485a1094a` |
| 501-600 | Apex Predation | Archfiend's Advent | [CARDS_00501_00600.md](CARDS_00501_00600.md) | `f763764ad652` |
| 601-700 | Archfiend's Ascent | Aromage Jasmine | [CARDS_00601_00700.md](CARDS_00601_00700.md) | `55abc139d29d` |
| 701-800 | Aromage Laurel | Asymmetaphys | [CARDS_00701_00800.md](CARDS_00701_00800.md) | `4accd4771b06` |
| 801-900 | At One With the Sword | Backup Warrior | [CARDS_00801_00900.md](CARDS_00801_00900.md) | `80e4218d1722` |
| 901-1000 | Bacon Saver | Battlewasp - Hama the Conquering Bow | [CARDS_00901_01000.md](CARDS_00901_01000.md) | `5df22e081e78` |
| 1001-1100 | Battlewasp - Nest | Berserker Crush | [CARDS_01001_01100.md](CARDS_01001_01100.md) | `5c839625d19c` |
| 1101-1200 | Berserker of the Tenyi | Blackwing - Abrolhos the Megaquake | [CARDS_01101_01200.md](CARDS_01101_01200.md) | `408bdd39f37d` |
| 1201-1300 | Blackwing - Aurora the Northern Lights | Blizzard Thunderbird | [CARDS_01201_01300.md](CARDS_01201_01300.md) | `5c397126d28d` |
| 1301-1400 | Blizzard Warrior | Boulder Tortoise | [CARDS_01301_01400.md](CARDS_01301_01400.md) | `c15e555fc559` |
| 1401-1500 | Bound Wand | Brron, Mad King of Dark World | [CARDS_01401_01500.md](CARDS_01401_01500.md) | `c6968233a7de` |
| 1501-1600 | Brushfire Knight | Bystial Druiswurm | [CARDS_01501_01600.md](CARDS_01501_01600.md) | `08c0daca2192` |
| 1601-1700 | Bystial Magnamhut | Celestial Transformation | [CARDS_01601_01700.md](CARDS_01601_01700.md) | `a1baee43e38b` |
| 1701-1800 | Celestial Wolf Lord, Blue Sirius | Charioteer of Prophecy | [CARDS_01701_01800.md](CARDS_01701_01800.md) | `33fa41f01a03` |
| 1801-1900 | Charm of Shabti | Claw Reacher | [CARDS_01801_01900.md](CARDS_01801_01900.md) | `5bc06df478ef` |
| 1901-2000 | Clay Charge | Compulsory Evacuation Device | [CARDS_01901_02000.md](CARDS_01901_02000.md) | `4bb6a93f3cd0` |
| 2001-2100 | Comrade Swordsman of Landstar | Cosmo Queen the Queen of Prayers | [CARDS_02001_02100.md](CARDS_02001_02100.md) | `08d058a91798` |
| 2101-2200 | Cosmos Channelling | Crystal Beast Sapphire Pegasus | [CARDS_02101_02200.md](CARDS_02101_02200.md) | `8ede8bc37d6e` |
| 2201-2300 | Crystal Beast Topaz Tiger | CXyz N.As.Ch. Knight | [CARDS_02201_02300.md](CARDS_02201_02300.md) | `4762d2b81030` |
| 2301-2400 | CXyz Simon the Great Moral Leader | Cyberse White Hat | [CARDS_02301_02400.md](CARDS_02301_02400.md) | `be6c7635fc56` |
| 2401-2500 | Cyberse Wicckid | D/D/D Alfred the Divine Sage King | [CARDS_02401_02500.md](CARDS_02401_02500.md) | `35a74bfa2d9e` |
| 2501-2600 | D/D/D Chaos King Apocalypse | Dark Bribe | [CARDS_02501_02600.md](CARDS_02501_02600.md) | `2ea1ddd0a581` |
| 2601-2700 | Dark Bug | Dark Rebellion Xyz Dragon | [CARDS_02601_02700.md](CARDS_02601_02700.md) | `f43aa3188696` |
| 2701-2800 | Dark Red Enchanter | Dawn Angel Kambi | [CARDS_02701_02800.md](CARDS_02701_02800.md) | `020244b63c6b` |
| 2801-2900 | Dawn Knight | Deskbot 005 | [CARDS_02801_02900.md](CARDS_02801_02900.md) | `4254a1b55d7f` |
| 2901-3000 | Deskbot 006 | Diamond Duston | [CARDS_02901_03000.md](CARDS_02901_03000.md) | `8367b67676a0` |
| 3001-3100 | Diamond-Dust Cyclone | Dinowrestler Giga Spinosavate | [CARDS_03001_03100.md](CARDS_03001_03100.md) | `42649465966a` |
| 3101-3200 | Dinowrestler Iguanodraka | Dogmatikalamity | [CARDS_03101_03200.md](CARDS_03101_03200.md) | `844aa6466794` |
| 3201-3300 | Dogmatikamacabre | Double-Edged Sword | [CARDS_03201_03300.md](CARDS_03201_03300.md) | `4953845fa8b0` |
| 3301-3400 | Double-Edged Sword Technique | Dragon's Rebirth | [CARDS_03301_03400.md](CARDS_03301_03400.md) | `0bddfa2f6780` |
| 3401-3500 | Dragoons of Draconia | Drytron Meteonis Draconids | [CARDS_03401_03500.md](CARDS_03401_03500.md) | `ace9307dcb47` |
| 3501-3600 | Drytron Meteonis Quadrantids | Earthshaker | [CARDS_03501_03600.md](CARDS_03501_03600.md) | `90f7f5c19e18` |
| 3601-3700 | Earthshattering Event | Elemental HERO Dark Neos | [CARDS_03601_03700.md](CARDS_03601_03700.md) | `ad5c226ce265` |
| 3701-3800 | Elemental HERO Darkbright | Emergency Assistance | [CARDS_03701_03800.md](CARDS_03701_03800.md) | `e50502cd62f7` |
| 3801-3900 | Emergency Provisions | Eva | [CARDS_03801_03900.md](CARDS_03801_03900.md) | `c6cdf5ad288d` |
| 3901-4000 | Evening Twilight Knight | Evolution Burst | [CARDS_03901_04000.md](CARDS_03901_04000.md) | `315375f8a180` |
| 4001-4100 | Evolution End Burst | Fabled Grimro | [CARDS_04001_04100.md](CARDS_04001_04100.md) | `c25ce56fed73` |
| 4101-4200 | Fabled Krus | Fengsheng Mirror | [CARDS_04101_04200.md](CARDS_04101_04200.md) | `e46617e9722f` |
| 4201-4300 | Fenrir | Fire Princess | [CARDS_04201_04300.md](CARDS_04201_04300.md) | `65680c430043` |
| 4301-4400 | Fire Prison | Flipping Feline | [CARDS_04301_04400.md](CARDS_04301_04400.md) | `43e351b51802` |
| 4401-4500 | Flock Together | Formud Skipper | [CARDS_04401_04500.md](CARDS_04401_04500.md) | `179299091322` |
| 4501-4600 | Formula Synchron | Full-Force Strike | [CARDS_04501_04600.md](CARDS_04501_04600.md) | `b3841c117e1e` |
| 4601-4700 | Fullmetalfoes Alkahest | Gaia The Fierce Knight | [CARDS_04601_04700.md](CARDS_04601_04700.md) | `ffd8d5f1d84b` |
| 4701-4800 | Gaia the Fierce Knight Origin | Gazelle the King of Mythical Claws | [CARDS_04701_04800.md](CARDS_04701_04800.md) | `3cacc4bf3958` |
| 4801-4900 | Gazer Shark | Genex Ally Duradark | [CARDS_04801_04900.md](CARDS_04801_04900.md) | `7c49528f8495` |
| 4901-5000 | Genex Ally Powercell | Ghoul with an Appetite | [CARDS_04901_05000.md](CARDS_04901_05000.md) | `f69cb47989b3` |
| 5001-5100 | Giant Axe Mummy | Gizmek Taniguku, the Immobile Intellect | [CARDS_05001_05100.md](CARDS_05001_05100.md) | `a3780e5de9f5` |
| 5101-5200 | Gizmek Uka, the Festive Fox of Fecundity | Goblin Freefall Squad | [CARDS_05101_05200.md](CARDS_05101_05200.md) | `8eb4a3ec44f2` |
| 5201-5300 | Goblin King | Gouki Finishing Move | [CARDS_05201_05300.md](CARDS_05201_05300.md) | `f89291a53d87` |
| 5301-5400 | Gouki Gameface | Graveyard and the Hand of Invitation | [CARDS_05301_05400.md](CARDS_05301_05400.md) | `fa8d18a9b74f` |
| 5401-5500 | Graveyard of Wandering Souls | Guardian Tryce | [CARDS_05401_05500.md](CARDS_05401_05500.md) | `a4232d0c87d5` |
| 5501-5600 | Guardragon Agarpain | Harpie Lady 1 | [CARDS_05501_05600.md](CARDS_05501_05600.md) | `f6cdcef2653f` |
| 5601-5700 | Harpie Lady 2 | Herald of Ultimateness | [CARDS_05601_05700.md](CARDS_05601_05700.md) | `24e2a5568954` |
| 5701-5800 | Heraldic Beast Aberconway | High Ritual Art | [CARDS_05701_05800.md](CARDS_05701_05800.md) | `7b7855be29fd` |
| 5801-5900 | High Tide Gyojin | Hydrojet Shark | [CARDS_05801_05900.md](CARDS_05801_05900.md) | `6ee9d1801c0a` |
| 5901-6000 | Hydrotortoise, the Empowered Warrior | Impcantation Bookstone | [CARDS_05901_06000.md](CARDS_05901_06000.md) | `7556e78507ef` |
| 6001-6100 | Impcantation Candoll | Infernoid Piaty | [CARDS_06001_06100.md](CARDS_06001_06100.md) | `c38b9849b77c` |
| 6101-6200 | Infernoid Pirmais | Inzektor Hornet | [CARDS_06101_06200.md](CARDS_06101_06200.md) | `2eb0ad0abc03` |
| 6201-6300 | Inzektor Ladybug | Jumbo Drill | [CARDS_06201_06300.md](CARDS_06201_06300.md) | `3bc8f1e9d113` |
| 6301-6400 | Jungle Dweller | Kairyu-Shin's Dark Reef | [CARDS_06301_06400.md](CARDS_06301_06400.md) | `9e60f6ce7539` |
| 6401-6500 | Kaiser Colosseum | Kidmodo Dragon | [CARDS_06401_06500.md](CARDS_06401_06500.md) | `ca0a68003d4a` |
| 6501-6600 | Kikinagashi Fucho | Krawler Dendrite | [CARDS_06501_06600.md](CARDS_06501_06600.md) | `15825cf5b3ae` |
| 6601-6700 | Krawler Glial | Laughing Flower | [CARDS_06601_06700.md](CARDS_06601_06700.md) | `9bf85974e770` |
| 6701-6800 | Laughing Puffin | Level Retuner | [CARDS_06701_06800.md](CARDS_06701_06800.md) | `df5488b6396b` |
| 6801-6900 | Level Tuning | Link Bumper | [CARDS_06801_06900.md](CARDS_06801_06900.md) | `4af9a28234db` |
| 6901-7000 | Link Burst | Lunalight Crimson Fox | [CARDS_06901_07000.md](CARDS_06901_07000.md) | `0ad13b0bd547` |
| 7001-7100 | Lunalight Emerald Bird | Madolche Messengelato | [CARDS_07001_07100.md](CARDS_07001_07100.md) | `a98b3e6b0fdd` |
| 7101-7200 | Madolche Mewfeuille | Magicians' Souls | [CARDS_07101_07200.md](CARDS_07101_07200.md) | `9e8b4dc20a5a` |
| 7201-7300 | Magicians Unite | Malefic Blue-Eyes White Dragon | [CARDS_07201_07300.md](CARDS_07201_07300.md) | `535759775148` |
| 7301-7400 | Malefic Claw Stream | Marincess Sleepy Maiden | [CARDS_07301_07400.md](CARDS_07301_07400.md) | `2fab2f3f147b` |
| 7401-7500 | Marincess Snow | Max Metalmorph | [CARDS_07401_07500.md](CARDS_07401_07500.md) | `8442fec755cc` |
| 7501-7600 | Max Warrior | Meklord Emperor Wisel | [CARDS_07501_07600.md](CARDS_07501_07600.md) | `ec06e4a76a5e` |
| 7601-7700 | Meklord Emperor Wisel - Synchro Absorption | Metalfoes Fusion | [CARDS_07601_07700.md](CARDS_07601_07700.md) | `518385108f89` |
| 7701-7800 | Metalfoes Goldriver | Mimighoul Throne | [CARDS_07701_07800.md](CARDS_07701_07800.md) | `38592ff48e39` |
| 7801-7900 | Mimikuril | Mistaken Accusation | [CARDS_07801_07900.md](CARDS_07801_07900.md) | `a752d79a68de` |
| 7901-8000 | Mistaken Arrest | Morphtronic Monitron | [CARDS_07901_08000.md](CARDS_07901_08000.md) | `30f456bd509e` |
| 8001-8100 | Morphtronic Radion | Mystical Shine Ball | [CARDS_08001_08100.md](CARDS_08001_08100.md) | `856742ca5ca1` |
| 8101-8200 | Mystical Space Typhoon | Necroquip Princess | [CARDS_08101_08200.md](CARDS_08101_08200.md) | `78cdfd1c14f2` |
| 8201-8300 | Necroquip Prism | Night Beam | [CARDS_08201_08300.md](CARDS_08201_08300.md) | `e1c488f366d8` |
| 8301-8400 | Night Dragolich | Nordic Beast Gullinbursti | [CARDS_08301_08400.md](CARDS_08301_08400.md) | `2cffbfefba79` |
| 8401-8500 | Nordic Relic Brisingamen | Number 69: Heraldry Crest - Shatter Stream | [CARDS_08401_08500.md](CARDS_08401_08500.md) | `481deb036b15` |
| 8501-8600 | Number 7: Lucky Straight | Obelisk the Tormentor | [CARDS_08501_08600.md](CARDS_08501_08600.md) | `db740948f72f` |
| 8601-8700 | Obese Marmot of Nefariousness | One-Eyed Skill Gainer | [CARDS_08601_08700.md](CARDS_08601_08700.md) | `5f9c61cf853c` |
| 8701-8800 | Oneiros, the Dream Mirror Erlking | Oxygeddon | [CARDS_08701_08800.md](CARDS_08701_08800.md) | `437174ddc5e7` |
| 8801-8900 | Oyster Meister | Pendulum Pendant | [CARDS_08801_08900.md](CARDS_08801_08900.md) | `641a0a08238d` |
| 8901-9000 | Pendulum Reborn | Performapal Odd-Eyes Butler | [CARDS_08901_09000.md](CARDS_08901_09000.md) | `a546b90138a7` |
| 9001-9100 | Performapal Odd-Eyes Dissolver | Pharaonic Advent | [CARDS_09001_09100.md](CARDS_09001_09100.md) | `ca08d177b2e8` |
| 9101-9200 | Pharaonic Guardian Sphinx | Poissonniere de Nouvelles | [CARDS_09101_09200.md](CARDS_09101_09200.md) | `365e30f48935` |
| 9201-9300 | Poki Draco | Predaplant Triantis | [CARDS_09201_09300.md](CARDS_09201_09300.md) | `ec24797e096d` |
| 9301-9400 | Predaplant Triphyoverutum | Proxy Dragon | [CARDS_09301_09400.md](CARDS_09301_09400.md) | `75dcae0bde5e` |
| 9401-9500 | Proxy F Magician | Pyro Clock of Destiny | [CARDS_09401_09500.md](CARDS_09401_09500.md) | `4710d7d37938` |
| 9501-9600 | Pyrorex the Elemental Lord | Ragnaraika Samurai Beetle | [CARDS_09501_09600.md](CARDS_09501_09600.md) | `8411ceedadc9` |
| 9601-9700 | Ragnaraika Selene Snapper | Rank-Up-Magic Raid Force | [CARDS_09601_09700.md](CARDS_09601_09700.md) | `a261f3522d7b` |
| 9701-9800 | Rank-Up-Magic Raptor's Force | Red Screen | [CARDS_09701_09800.md](CARDS_09701_09800.md) | `a4ab92b30cae` |
| 9801-9900 | Red Sparrow Summoner | Repair Genex Controller | [CARDS_09801_09900.md](CARDS_09801_09900.md) | `83f4fd4357c7` |
| 9901-10000 | Replicard LAD | Revoke Fusion | [CARDS_09901_10000.md](CARDS_09901_10000.md) | `0b0cfd82c213` |
| 10001-10100 | Revolution Synchron | Rock Bombardment | [CARDS_10001_10100.md](CARDS_10001_10100.md) | `fe1df98daa7e` |
| 10101-10200 | Rock of the Vanquisher | Ryu-Ge End | [CARDS_10101_10200.md](CARDS_10101_10200.md) | `295976686d9b` |
| 10201-10300 | Ryu-Ge Realm - Dino Domains | Salamangreat Sunlight Wolf | [CARDS_10201_10300.md](CARDS_10201_10300.md) | `8ee3415f6a76` |
| 10301-10400 | Salamangreat Tiger | Score the Melodious Diva | [CARDS_10301_10400.md](CARDS_10301_10400.md) | `14beceded054` |
| 10401-10500 | Scourge of the White Forest | Segmental Dragon | [CARDS_10401_10500.md](CARDS_10401_10500.md) | `8c7fbd8b712a` |
| 10501-10600 | Seismic Crasher | Share the Pain | [CARDS_10501_10600.md](CARDS_10501_10600.md) | `c37464ad2ced` |
| 10601-10700 | Shared Ride | Shore Knight | [CARDS_10601_10700.md](CARDS_10601_10700.md) | `b30d0450d96c` |
| 10701-10800 | Short Circuit | Skill Prisoner | [CARDS_10701_10800.md](CARDS_10701_10800.md) | `a9daa519131e` |
| 10801-10900 | Skill Successor | Snake-Eyes Flamberge Dragon | [CARDS_10801_10900.md](CARDS_10801_10900.md) | `02d3579b3691` |
| 10901-11000 | Snake-Eyes Poplar | Soul Servant | [CARDS_10901_11000.md](CARDS_10901_11000.md) | `0816cbb27344` |
| 11001-11100 | Soul Strike | Spellbook of Miracles | [CARDS_11001_11100.md](CARDS_11001_11100.md) | `a1d999203caf` |
| 11101-11200 | Spellbook of Power | Springans Blast! | [CARDS_11101_11200.md](CARDS_11101_11200.md) | `c0ec8f666595` |
| 11201-11300 | Springans Booty | Starry Knight Balefire | [CARDS_11201_11300.md](CARDS_11201_11300.md) | `dc74793d8a9c` |
| 11301-11400 | Starry Knight Blast | Stronghold the Hidden Fortress | [CARDS_11301_11400.md](CARDS_11301_11400.md) | `1b37ecbb184b` |
| 11401-11500 | Stronghold the Moving Fortress | Super Quantal Alphan Spike | [CARDS_11401_11500.md](CARDS_11401_11500.md) | `6396c2d2b438` |
| 11501-11600 | Super Quantal Alphancall Appeal | Supply Soldier of the Indestructible Insects | [CARDS_11501_11600.md](CARDS_11501_11600.md) | `b83132474585` |
| 11601-11700 | Supply Squad | Sylvan Lotuswain | [CARDS_11601_11700.md](CARDS_11601_11700.md) | `2b580c835140` |
| 11701-11800 | Sylvan Marshalleaf | Tails of the Fairy Tails | [CARDS_11701_11800.md](CARDS_11701_11800.md) | `5f3499ccb828` |
| 11801-11900 | Tailwind of Gusto | Terra Firma Gravity | [CARDS_11801_11900.md](CARDS_11801_11900.md) | `37170ea0467b` |
| 11901-12000 | Terra the Terrible | The Fabled Cerburrel | [CARDS_11901_12000.md](CARDS_11901_12000.md) | `be6ab8647fee` |
| 12001-12100 | The Fabled Chawa | The Reliable Guardian | [CARDS_12001_12100.md](CARDS_12001_12100.md) | `4fc7a18db762` |
| 12101-12200 | The Resolute Meklord Army | Thestalos the Mega Monarch | [CARDS_12101_12200.md](CARDS_12101_12200.md) | `fe1d54fbafda` |
| 12201-12300 | Thestalos the Shadowfire Monarch | Timegazer Magician | [CARDS_12201_12300.md](CARDS_12201_12300.md) | `96306f68a689` |
| 12301-12400 | Timelord Progenitor Vorpgate | Toy Magician | [CARDS_12301_12400.md](CARDS_12301_12400.md) | `1e80eb7767e7` |
| 12401-12500 | Toy Parade | Tri-Brigade Arms Mouser | [CARDS_12401_12500.md](CARDS_12401_12500.md) | `ff03eb62844c` |
| 12501-12600 | Tri-Brigade Bearbrumm the Rampant Rampager | Tsuru-Puru-Purun | [CARDS_12501_12600.md](CARDS_12501_12600.md) | `2c13f57a956d` |
| 12601-12700 | Tualatin | UFOroid | [CARDS_12601_12700.md](CARDS_12601_12700.md) | `2fc5207efdf3` |
| 12701-12800 | UFOroid Fighter | Unmasked Dragon | [CARDS_12701_12800.md](CARDS_12701_12800.md) | `9c965ed16b2f` |
| 12801-12900 | Unpossessed | Vampiric Koala | [CARDS_12801_12900.md](CARDS_12801_12900.md) | `e96cc6fe37bc` |
| 12901-13000 | Vampiric Orchis | Vic Viper T301 | [CARDS_12901_13000.md](CARDS_12901_13000.md) | `3e84115227fa` |
| 13001-13100 | Vic Viper Type-L | Vorse Raider | [CARDS_13001_13100.md](CARDS_13001_13100.md) | `1992e61bff2d` |
| 13101-13200 | Vortex of Time | Water Magician | [CARDS_13101_13200.md](CARDS_13101_13200.md) | `2e0ff5f51e36` |
| 13201-13300 | Water of Life | Whitebeard, the Plunder Patroll Helm | [CARDS_13201_13300.md](CARDS_13201_13300.md) | `f62cf0da5998` |
| 13301-13400 | Whitefish Salvage | Witchcrafter Pittore | [CARDS_13301_13400.md](CARDS_13301_13400.md) | `df8334255eec` |
| 13401-13500 | Witchcrafter Potterie | Worm Warlord | [CARDS_13401_13500.md](CARDS_13401_13500.md) | `1ee803704cad` |
| 13501-13600 | Worm Xex | Yaguramaru the Armor Ninja | [CARDS_13501_13600.md](CARDS_13501_13600.md) | `b518c2657629` |
| 13601-13700 | Yaiba Robo | Zeman the Ape King | [CARDS_13601_13700.md](CARDS_13601_13700.md) | `8e37a2e49fd9` |
| 13701-13790 | Zenmaiday | Z-Zillion Tank | [CARDS_13701_13790.md](CARDS_13701_13790.md) | `be575bd8083f` |
