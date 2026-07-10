# CleanDuel Yu-Gi-Oh Knowledge Audit

Generated UTC: 2026-07-10T13:40:41Z
Unity deck source: CleanCardLibrary local Blue-Eyes recipe
Card database source: CardDatabase/cards.cdb (14880 records)
Lua source: D:\unity\script
Official reference priority: Konami Rulebook + Yu-Gi-Oh! Neuron/Card Database, then local cards.cdb, then local c*.lua scripts.

## Baseline Rules To Keep

- Do not expose Activate on Normal Monsters just because they have card art or text.
- Ritual Monsters require a Ritual Spell/effect and valid Level/material payment.
- Fusion Monsters require a Fusion Summon/effect unless their own text allows Special Summon.
- Synchro Summons require at least 1 Tuner and at least 1 non-Tuner with the exact required Level total.
- Link Monsters use Link Materials and should not be treated as Level monsters.
- Field Spells stay in the Field Spell Zone after activation unless replaced or removed.
- Quick Effects are Spell Speed 2 monster effects and can be activated during the opponent's turn when their condition is legal.
- Continuous Effects do not activate; they apply only while the card is face-up and not negated, then stop when that condition ends.
- Trigger Effects activate from specific events such as Summon, attack target, destruction, Graveyard timing, or End Phase; do not expose them as generic Activate buttons.
- Flip Effects are Trigger Effects that start with FLIP and trigger when a face-down monster is flipped face-up by Flip Summon, battle, or card effect.
- Link Monsters use printed Link Materials and Link Rating; Extra Deck Link Summons use the Extra Monster Zone first, then linked Main Monster Zones opened by Link Arrows.
- Link Arrows mark linked zones/monsters; two Link Monsters pointing at each other are co-linked.
- Pendulum Monsters can be activated as Spells only in the left/right Pendulum Zones; Pendulum Summon is once per turn, uses open scales, and face-up Pendulum Monsters from the Extra Deck must use the Extra Monster Zone or a linked Main Monster Zone.
- Pendulum Monsters on the field go face-up to the Extra Deck instead of the GY when they would be sent to the GY.
- If a card text says choose/add/send/target, prefer a player choice prompt over automatic best-card selection.

## Blue-Eyes Deck Audit

Cards with Main/Extra count 0 are not in the current recipe, but are tracked because CleanDuel has rules that search or resolve them.

| Main | Extra | Id | Card | Kind | Type Line | Effect Key | Timing | Speed | Lua Families | Issues |
| ---: | ---: | ---: | --- | --- | --- | --- | --- | ---: | --- | --- |
| 1 | 0 | 2783661 | Majesty with Eyes of Blue | Spell | Quick-Play Spell | majesty_blue_clean | Ignition | 2 | loaded | OK |
| 3 | 0 | 8240199 | Sage with Eyes of Blue | Monster | Spellcaster / Tuner / Effect | sage_clean | Ignition | 1 | special_summon, search_or_recover, send_to_grave | OK |
| 0 | 3 | 12381100 | Dragon Master Magia | Monster | Dragon / Fusion / Effect | magia_clean | Ignition | 1 | destroy, special_summon | OK |
| 1 | 0 | 14558127 | Ash Blossom & Joyous Spring | Monster | Zombie / Tuner / Effect | ash_clean | Quick | 2 | draw, special_summon, search_or_recover, send_to_grave, mill | OK |
| 3 | 0 | 17725109 | Roar of the Blue-Eyed Dragons | Spell | Normal Spell | roar_clean | Ignition | 1 | special_summon, fusion_summon | OK |
| 2 | 0 | 17947697 | Maiden of White | Monster | Spellcaster / Tuner / Effect | maiden_white | Ignition | 1 | special_summon | OK |
| 1 | 0 | 20654247 | Blue-Eyes Chaos Dragon | Monster | Dragon / Ritual / Effect | chaos_dragon_clean | Ignition | 1 | position | OK |
| 1 | 0 | 21082832 | Chaos Form | Spell | Ritual Spell | chaos_form_clean | Ignition | 1 | loaded | OK |
| 1 | 0 | 23434538 | Maxx "C" | Monster | Insect / Effect | maxx_c_clean | Quick | 2 | draw | OK |
| 0 | 1 | 23995346 | Blue-Eyes Ultimate Dragon | Monster | Dragon / Fusion | lua_23995346 | None | 0 | loaded | OK |
| 1 | 0 | 24382602 | Mausoleum of White | Spell | Field Spell | mausoleum_clean | Continuous | 0 | search_or_recover, send_to_grave | OK |
| 2 | 0 | 30576089 | Blue-Eyes Jet Dragon | Monster | Dragon / Effect | jet_clean | Continuous | 0 | special_summon, search_or_recover | OK |
| 1 | 0 | 40089744 | Gateway to Chaos | Spell | Field Spell | gateway_chaos_clean | Continuous | 0 | search_or_recover | OK |
| 1 | 0 | 41620959 | Dragon Shrine | Spell | Normal Spell | dragon_shrine_clean | Ignition | 1 | send_to_grave | OK |
| 0 | 3 | 42097666 | Spirit with Eyes of Blue | Monster | Dragon / Link / Effect | spirit_with_eyes_clean | Continuous | 0 | special_summon, search_or_recover, send_to_grave | OK |
| 1 | 0 | 45467446 | Dragon Spirit of White | Monster | Dragon / Effect | dragon_spirit_white_clean | Ignition | 1 | special_summon, banish | OK |
| 1 | 0 | 55410871 | Blue-Eyes Chaos MAX Dragon | Monster | Dragon / Ritual / Effect | chaos_max_clean | Ignition | 1 | loaded | OK |
| 1 | 0 | 59438930 | Ghost Ogre & Snow Rabbit | Monster | Psychic / Tuner / Effect | ghost_ogre_clean | Quick | 2 | destroy | OK |
| 0 | 2 | 59822133 | Blue-Eyes Spirit Dragon | Monster | Dragon / Synchro / Effect | spirit_dragon_clean | Quick | 2 | special_summon, synchro_summon | OK |
| 3 | 0 | 62089826 | True Light | Trap | Continuous Trap | true_light_clean | Continuous | 2 | destroy, special_summon, send_to_grave | OK |
| 1 | 0 | 66961194 | Dictator of D. | Monster | Spellcaster / Effect | dictator_clean | Ignition | 1 | special_summon | OK |
| 0 | 0 | 70551291 | Black Luster Soldier - Legendary Swordsman | Monster | Warrior / Ritual / Effect | lua_70551291 | Ignition | 1 | search_or_recover | OK |
| 3 | 0 | 71039903 | The White Stone of Ancients | Monster | Dragon / Tuner / Effect | stone_ancients_clean | Trigger | 1 | special_summon, search_or_recover | OK |
| 3 | 0 | 71143015 | Ultimate Fusion | Spell | Quick-Play Spell | ultimate_fusion_clean | Ignition | 2 | special_summon, fusion_summon, graveyard_action | OK |
| 2 | 0 | 80326401 | Wishes for Eyes of Blue | Spell | Normal Spell | wishes_clean | Ignition | 1 | search_or_recover | OK |
| 3 | 0 | 88241506 | Maiden with Eyes of Blue | Monster | Spellcaster / Tuner / Effect | maiden_with_eyes_clean | Trigger | 1 | special_summon, position, mill | OK |
| 0 | 1 | 89604813 | Blue-Eyes Ultimate Spirit Dragon | Monster | Dragon / Synchro / Effect | ultimate_spirit_clean | Quick | 2 | special_summon, synchro_summon, stat_change | OK |
| 3 | 0 | 89631139 | Blue-Eyes White Dragon | Monster | Dragon / Normal | none | None | 0 | no c89631139.lua | OK |
| 1 | 0 | 93437091 | Bingo Machine, Go!!! | Spell | Normal Spell | bingo_clean | Ignition | 1 | search_or_recover | OK |
| 1 | 0 | 97268402 | Effect Veiler | Monster | Spellcaster / Tuner / Effect | veiler_clean | Quick | 2 | loaded | OK |

## Summary

- Main Deck cards: 41
- Extra Deck cards: 10
- Unique cards audited: 30
- Audit issues: 0
- Lua status: Lua source loaded: 19067 card scripts / 19085 lua files from D:\unity\script
