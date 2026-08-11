using System.Collections.Generic;
using IcarusGame.MasterDuel;

namespace IcarusGame.CleanDuel
{
    public static class CleanCardLibrary
    {
        private static readonly Dictionary<int, CleanCardDefinition> fallbackCards =
            new Dictionary<int, CleanCardDefinition>
            {
                { 89631139, Monster(89631139, "Blue-Eyes White Dragon", 8, 3000, 2500, string.Empty, "LIGHT / Dragon / Normal") },
                { 46986414, Monster(46986414, "Dark Magician", 7, 2500, 2100, string.Empty, "DARK / Spellcaster / Normal") },
                { 54332792, Monster(54332792, "Neo Kaiser Sea Horse", 4, 1700, 1650, "neo_kaiser_clean", "LIGHT / Dragon / Tuner / Effect") },
                { 88241506, Monster(88241506, "Maiden with Eyes of Blue", 1, 0, 0, "maiden_with_eyes_clean", "Spellcaster / Tuner / Effect") },
                { 8240199, Monster(8240199, "Sage with Eyes of Blue", 1, 0, 1500, "sage_clean", "LIGHT / Spellcaster / Tuner / Effect") },
                { 71039903, Monster(71039903, "The White Stone of Ancients", 1, 600, 500, "stone_ancients_clean", "LIGHT / Dragon / Tuner / Effect") },
                { 17947697, Monster(17947697, "Maiden of White", 1, 0, 0, "maiden_white", "LIGHT / Spellcaster / Tuner / Effect") },
                { 23434538, Monster(23434538, "Maxx \"C\"", 2, 500, 200, "maxx_c_clean", "Insect / Effect") },
                { 97268402, Monster(97268402, "Effect Veiler", 1, 0, 0, "veiler_clean", "LIGHT / Spellcaster / Tuner / Effect") },
                { 14558127, Monster(14558127, "Ash Blossom & Joyous Spring", 3, 0, 1800, "ash_clean", "Zombie / Tuner / Effect") },
                { 59438930, Monster(59438930, "Ghost Ogre & Snow Rabbit", 3, 0, 1800, "ghost_ogre_clean", "LIGHT / Psychic / Tuner / Effect") },
                { 66961194, Monster(66961194, "Dictator of D.", 4, 1200, 1100, "dictator_clean", "DARK / Spellcaster / Effect") },
                { 30576089, Monster(30576089, "Blue-Eyes Jet Dragon", 8, 3000, 0, "jet_clean", "LIGHT / Dragon / Effect") },
                { 38517737, Monster(38517737, "Blue-Eyes Alternative White Dragon", 8, 3000, 2500, "alternative_clean", "LIGHT / Dragon / Effect") },
                { 64202399, Monster(64202399, "Blue-Eyes Abyss Dragon", 8, 2500, 2500, "abyss_clean", "LIGHT / Dragon / Effect") },
                { 45467446, Monster(45467446, "Dragon Spirit of White", 8, 2500, 2000, "dragon_spirit_white_clean", "Dragon / Effect") },
                { 55410871, RitualMonster(55410871, "Blue-Eyes Chaos MAX Dragon", 8, 4000, 0, "chaos_max_clean", "DARK / Dragon / Ritual / Effect") },
                { 20654247, RitualMonster(20654247, "Blue-Eyes Chaos Dragon", 8, 3000, 0, "chaos_dragon_clean", "Dragon / Ritual / Effect") },
                { 70551291, RitualMonster(70551291, "Black Luster Soldier - Legendary Swordsman", 8, 3000, 2500, string.Empty, "Warrior / Ritual / Effect") },
                { 23995346, Monster(23995346, "Blue-Eyes Ultimate Dragon", 12, 4500, 3800, string.Empty, "LIGHT / Dragon / Fusion / Normal") },
                { 12381100, Monster(12381100, "Dragon Master Magia", 12, 5000, 4000, "magia_clean", "LIGHT / Dragon / Fusion / Effect") },
                { 59822133, Monster(59822133, "Blue-Eyes Spirit Dragon", 9, 2500, 3000, "spirit_dragon_clean", "LIGHT / Dragon / Synchro / Effect") },
                { 89604813, Monster(89604813, "Blue-Eyes Ultimate Spirit Dragon", 12, 3500, 4000, "ultimate_spirit_clean", "LIGHT / Dragon / Synchro / Effect") },
                { 40908371, Monster(40908371, "Azure-Eyes Silver Dragon", 9, 2500, 3000, "azure_eyes_clean", "Dragon / LIGHT / Synchro / Effect") },
                { 42097666, Monster(42097666, "Spirit with Eyes of Blue", 1, 300, 0, "spirit_with_eyes_clean", "LIGHT / Dragon / Link / Effect") },
                { 63767246, Monster(63767246, "Number 38: Hope Harbinger Dragon Titanic Galaxy", 8, 3000, 2500, "hope_harbinger_clean", "Dragon / Xyz / Effect") },
                { 18963306, Monster(18963306, "Galaxy-Eyes Cipher Dragon", 8, 3000, 2500, "cipher_dragon_clean", "Dragon / Xyz / Effect") },
                { 24382602, Spell(24382602, "Mausoleum of White", "mausoleum_clean") },
                { 93437091, Spell(93437091, "Bingo Machine, Go!!!", "bingo_clean") },
                { 83764719, Spell(83764719, "Monster Reborn", "monster_reborn_clean") },
                { 80326401, Spell(80326401, "Wishes for Eyes of Blue", "wishes_clean") },
                { 17725109, Spell(17725109, "Roar of the Blue-Eyed Dragons", "roar_clean") },
                { 12580477, Spell(12580477, "Raigeki", "raigeki_clean") },
                { 24094653, Spell(24094653, "Polymerization", "polymerization_clean") },
                { 40089744, Spell(40089744, "Gateway to Chaos", "gateway_chaos_clean") },
                { 2783661, Spell(2783661, "Majesty with Eyes of Blue", "majesty_blue_clean") },
                { 21082832, Spell(21082832, "Chaos Form", "chaos_form_clean") },
                { 41620959, Spell(41620959, "Dragon Shrine", "dragon_shrine_clean") },
                { 71143015, QuickPlaySpell(71143015, "Ultimate Fusion", "ultimate_fusion_clean") },
                { 62089826, Trap(62089826, "True Light", "true_light_clean") }
            };

        private static readonly Dictionary<int, string> fallbackTexts =
            new Dictionary<int, string>
            {
                { 89631139, "This legendary dragon is a powerful engine of destruction. Virtually invincible, very few have faced this awesome creature and lived to tell the tale." },
                { 46986414, "The ultimate wizard in terms of attack and defense." },
                { 54332792, "If you control \"Blue-Eyes White Dragon\": You can Special Summon this card from your hand. You can target 1 LIGHT Tuner you control; increase or decrease its Level by 1. If this card is sent from the field to the GY: You can send 1 \"Blue-Eyes\" monster or 1 monster that mentions \"Blue-Eyes White Dragon\" from your Deck to the GY, except \"Neo Kaiser Sea Horse\". You can only use each effect of \"Neo Kaiser Sea Horse\" once per turn." },
                { 8240199, "When this card is Normal Summoned: You can add 1 Level 1 LIGHT Tuner from your Deck to your hand, except \"Sage with Eyes of Blue\". You can discard this card, then target 1 Effect Monster you control; send it to the GY, and if you do, Special Summon 1 \"Blue-Eyes\" monster from your Deck. You can only use this effect of \"Sage with Eyes of Blue\" once per turn." },
                { 71039903, "Once per turn, during the End Phase, if this card is in the GY because it was sent there this turn: You can Special Summon 1 \"Blue-Eyes\" monster from your Deck. You can banish this card from your GY, then target 1 \"Blue-Eyes\" monster in your GY; add it to your hand. You can only use this effect of \"The White Stone of Ancients\" once per turn." },
                { 17947697, "You can send this card from your hand or field to the GY; place 1 \"True Light\" from your hand, Deck, or GY, face-up in your Spell & Trap Zone. If you Special Summon \"Blue-Eyes White Dragon\" while this card is in your GY: You can Special Summon this card. When a card or effect is activated that targets this card on the field, or when this card is targeted for an attack: You can Special Summon 1 \"Blue-Eyes White Dragon\" or 1 Level 1 LIGHT Tuner from your GY. You can only use each effect of \"Maiden of White\" once per turn." },
                { 23434538, "During either player's turn: You can send this card from your hand to the GY; this turn, each time your opponent Special Summons a monster(s), immediately draw 1 card. You can only use 1 \"Maxx \\\"C\\\"\" per turn." },
                { 14558127, "When a card or effect is activated that includes any of these effects: You can discard this card; negate that effect. Add a card from the Deck to the hand, Special Summon from the Deck, or send a card from the Deck to the GY. You can only use this effect of \"Ash Blossom & Joyous Spring\" once per turn." },
                { 59438930, "When a monster on the field activates its effect, or when a Spell/Trap that is already face-up on the field activates its effect: You can send this card from your hand or field to the GY; destroy that card on the field. You can only use this effect of \"Ghost Ogre & Snow Rabbit\" once per turn." },
                { 97268402, "During your opponent's Main Phase: You can send this card from your hand to the GY, then target 1 Effect Monster your opponent controls; negate the effects of that face-up monster your opponent controls, until the end of this turn." },
                { 66961194, "While you control a \"Blue-Eyes\" monster, you choose the attack targets for your opponent's attacks. You can only use each of the following effects of \"Dictator of D.\" once per turn. You can send 1 \"Blue-Eyes White Dragon\" from your hand or Deck to the GY; Special Summon this card from your hand. You can discard 1 \"Blue-Eyes White Dragon\", or 1 card that mentions it, then target 1 \"Blue-Eyes\" monster in your GY; Special Summon it." },
                { 30576089, "Other cards you control cannot be destroyed by your opponent's card effects. You can only use each of the following effects of \"Blue-Eyes Jet Dragon\" once per turn, and can only activate them while \"Blue-Eyes White Dragon\" is on your field or in your GY. If a card(s) on the field is destroyed by battle or card effect: You can Special Summon this card from the GY (if it was there when the card was destroyed) or hand (even if not). At the start of the Damage Step, if this card battles: You can target 1 card your opponent controls; return it to the hand." },
                { 38517737, "Cannot be Normal Summoned/Set. Must first be Special Summoned (from your hand) by revealing \"Blue-Eyes White Dragon\" in your hand. You can only Special Summon \"Blue-Eyes Alternative White Dragon\" once per turn this way. This card's name becomes \"Blue-Eyes White Dragon\" while on the field or in the GY. Once per turn: You can target 1 monster your opponent controls; destroy it. This card cannot attack the turn this effect is activated." },
                { 64202399, "If this card is Special Summoned: You can add 1 Ritual Spell or 1 \"Polymerization\" from your Deck to your hand. During your End Phase: You can add 1 Level 8 or higher Dragon monster from your Deck to your hand. You can banish this card from your GY; all Level 8 or higher Dragon monsters you control gain 1000 ATK. You can only use each effect of \"Blue-Eyes Abyss Dragon\" once per turn, and can only activate them while \"Blue-Eyes White Dragon\" is on your field or in your GY." },
                { 55410871, "You can Ritual Summon this card with \"Chaos Form\". Must be Ritual Summoned. Your opponent cannot target this card with card effects, also it cannot be destroyed by your opponent's card effects. If this card attacks a Defense Position monster, inflict double piercing battle damage." },
                { 83764719, "Target 1 monster in either GY; Special Summon it." },
                { 93437091, "Reveal 3 cards from your Deck that each meet at least 1 of the criteria listed below, your opponent randomly picks 1 for you to add to your hand, and you shuffle the rest into your Deck. You can only activate 1 \"Bingo Machine, Go!!!\" per turn. \"Blue-Eyes\" monster. Spell/Trap that mentions \"Blue-Eyes White Dragon\" or \"Blue-Eyes Ultimate Dragon\", except \"Bingo Machine, Go!!!\"." },
                { 80326401, "Discard 1 card; add 1 Level 1 LIGHT Tuner and 1 Spell/Trap that mentions \"Blue-Eyes White Dragon\" from your Deck to your hand, except \"Wishes for Eyes of Blue\". You can banish this card from your GY, then target 1 \"Blue-Eyes White Dragon\" you control; equip 1 \"Blue-Eyes\" monster from your Extra Deck to it as an Equip Spell that gives it 400 ATK. You can only use each effect of \"Wishes for Eyes of Blue\" once per turn." },
                { 17725109, "Special Summon 1 \"Blue-Eyes\" monster from your Deck, GY, or banishment in Defense Position, but you can only Special Summon \"Blue-Eyes White Dragon\" with this effect if you control no \"Blue-Eyes White Dragon\". You can banish this card from your GY; Fusion Summon 1 Fusion Monster from your Extra Deck, using monsters from your hand or field, including a \"Blue-Eyes\" monster. For the rest of this turn after this card resolves, you cannot Special Summon from the Extra Deck, except Dragon monsters. You can only use 1 \"Roar of the Blue-Eyed Dragons\" effect per turn, and only once that turn." },
                { 12580477, "Destroy all monsters your opponent controls." },
                { 24094653, "Fusion Summon 1 Fusion Monster from your Extra Deck, using monsters from your hand or field as Fusion Material." },
                { 21082832, "This card can be used to Ritual Summon any \"Chaos\" or \"Black Luster Soldier\" Ritual Monster. You must also Tribute monsters from your hand or field and/or banish \"Blue-Eyes White Dragon\" or \"Dark Magician\" from your GY, whose total Levels exactly equal the Level of the Ritual Monster you Ritual Summon." },
                { 41620959, "Send 1 Dragon monster from your Deck to the GY, then, if that monster in your GY is a Dragon Normal Monster, you can send 1 more Dragon monster from your Deck to the GY. You can only activate 1 \"Dragon Shrine\" per turn." },
                { 71143015, "During the Main Phase: Fusion Summon 1 Fusion Monster from your Extra Deck that mentions \"Blue-Eyes White Dragon\" or \"Blue-Eyes Ultimate Dragon\" as material, by shuffling the Fusion Materials listed on it from your hand, field, and/or GY into the Deck, then, you can destroy face-up cards your opponent controls, up to the number of \"Blue-Eyes White Dragon\" and \"Blue-Eyes Ultimate Dragon\" used from the field as material. You can only activate 1 \"Ultimate Fusion\" per turn." },
                { 62089826, "Your opponent cannot target \"Blue-Eyes White Dragon\" in your Monster Zone with card effects. You can only use the following effect of \"True Light\" once per turn. You can activate 1 of these effects: Special Summon 1 \"Blue-Eyes White Dragon\" from your hand or GY; Set 1 Spell/Trap directly from your Deck, that mentions \"Blue-Eyes White Dragon\". If this face-up card is sent from the Spell & Trap Zone to the GY: Destroy all monsters you control." },
                { 23995346, "\"Blue-Eyes White Dragon\" + \"Blue-Eyes White Dragon\" + \"Blue-Eyes White Dragon\"" },
                { 12381100, "\"Blue-Eyes Ultimate Dragon\" or 3 \"Blue-Eyes\" monsters + 1 \"Chaos\" or \"Black Luster Soldier\" Ritual Monster. Must be Fusion Summoned. When your opponent activates a card or effect: You can negate the activation, and if you do, destroy that card. You can only use this effect of \"Dragon Master Magia\" in response to each card type (Monster, Spell, or Trap) once per turn. If this face-up card in its owner's control leaves the field because of an opponent's card: You can Special Summon 1 \"Blue-Eyes\" monster or 1 \"Chaos\" or \"Black Luster Soldier\" Ritual Monster from your Extra Deck or GY." },
                { 59822133, "1 Tuner + 1+ non-Tuner \"Blue-Eyes\" monsters. Neither player can Special Summon 2 or more monsters at the same time. Once per turn, when an effect of a card in the GY is activated: You can negate the activation. You can Tribute this Synchro Summoned card; Special Summon 1 LIGHT Dragon Synchro Monster from your Extra Deck in Defense Position, except \"Blue-Eyes Spirit Dragon\", but destroy it during the End Phase of this turn." },
                { 89604813, "2+ Tuners + 1 non-Tuner \"Blue-Eyes\" monster. Your opponent cannot banish cards from your GY. You can only use each of the following effects of \"Blue-Eyes Ultimate Spirit Dragon\" once per turn. When a card or effect is activated on the field: You can negate the activation, and if you do, this card gains 1000 ATK until the end of this turn. If this card is destroyed by battle or card effect: You can Special Summon 1 LIGHT Dragon monster from your GY, except \"Blue-Eyes Ultimate Spirit Dragon\"." },
                { 42097666, "1 Level 4 or lower Dragon or Spellcaster monster. You cannot Special Summon monsters, except Dragon monsters. You can only use each of the following effects of \"Spirit with Eyes of Blue\" once per turn. If this card is Link Summoned: You can take 1 \"Mausoleum of White\" from your Deck, and either add it to your hand or send it to the GY. You can Tribute this card; Special Summon 1 \"Blue-Eyes\" monster from your hand or GY, but if you Special Summon an Effect Monster from your GY by this effect, it cannot attack, also its effects are negated." }
            };

        private static YgoCardDatabase database;
        private static bool triedDatabase;
        private static MasterDuelCardCatalog catalog;
        private static bool triedCatalog;

        public static List<CleanCard> BuildDeck(CleanPlayerId owner)
        {
            int[] recipe = owner == CleanPlayerId.Local ? LocalRecipe() : RivalRecipe();
            return BuildCards(recipe, owner);
        }

        public static List<CleanCard> BuildExtraDeck(CleanPlayerId owner)
        {
            return BuildCards(ExtraRecipe(), owner);
        }

        public static CleanCardDefinition Get(int id)
        {
            CleanCardDefinition fallback;
            bool hasFallback = fallbackCards.TryGetValue(id, out fallback);
            MasterDuelCardDefinition canonical = CatalogDefinition(id);
            if (!hasFallback && canonical != null)
            {
                fallback = FromCatalog(canonical, string.Empty, string.Empty);
            }
            else if (!hasFallback)
            {
                fallback = Monster(id, "Card " + id, 4, 1500, 1500, string.Empty, "Monster");
            }

            YgoCardDatabase loaded = LoadDatabase();
            if (loaded == null)
            {
                return WithCatalogFactsAndFallbackText(fallback, canonical);
            }

            YgoCardRecord record;
            if (!loaded.TryGetRecord(id, out record))
            {
                return WithCatalogFactsAndFallbackText(fallback, canonical);
            }

            CardData data = YgoCardDatabase.CreateCardData(record, string.Empty, fallback.EffectKey);
            string effectKey = ResolveEffectKey(fallback.EffectKey);
            return new CleanCardDefinition(
                data.Id,
                data.Name,
                MapKind(data.Kind),
                data.Level,
                canonical != null ? canonical.Rank ?? 0 : 0,
                canonical != null ? canonical.LinkRating ?? 0 : 0,
                data.Attack,
                data.Defense,
                data.TypeLine,
                data.Text,
                effectKey,
                canonical);
        }

        public static bool TryGetCatalogDefinition(int id, out MasterDuelCardDefinition definition)
        {
            definition = CatalogDefinition(id);
            return definition != null;
        }

        private static CleanCardDefinition WithCatalogFactsAndFallbackText(CleanCardDefinition fallback, MasterDuelCardDefinition canonical)
        {
            string text;
            fallbackTexts.TryGetValue(fallback.Id, out text);
            if (canonical == null)
            {
                return new CleanCardDefinition(
                    fallback.Id,
                    fallback.Name,
                    fallback.Kind,
                    fallback.Level,
                    fallback.Rank,
                    fallback.LinkRating,
                    fallback.Attack,
                    fallback.Defense,
                    fallback.TypeLine,
                    text ?? fallback.Text ?? string.Empty,
                    fallback.EffectKey,
                    null);
            }

            string selectedText = !string.IsNullOrEmpty(canonical.OfficialText)
                ? canonical.OfficialText
                : text ?? fallback.Text ?? string.Empty;
            string effectKey = canonical.HasExecutableEffect
                ? canonical.EffectHandlerId
                : fallback.EffectKey;
            return FromCatalog(canonical, selectedText, effectKey);
        }

        private static CleanCardDefinition FromCatalog(MasterDuelCardDefinition canonical, string text, string effectKey)
        {
            int attack = canonical.Attack.HasFixedValue ? canonical.Attack.Value : 0;
            int defense = canonical.Defense.HasFixedValue ? canonical.Defense.Value : 0;
            return new CleanCardDefinition(
                checked((int)canonical.NumericDatabaseId),
                canonical.Name,
                CatalogKind(canonical),
                canonical.Level ?? 0,
                canonical.Rank ?? 0,
                canonical.LinkRating ?? 0,
                attack,
                defense,
                CatalogTypeLine(canonical),
                text ?? string.Empty,
                ResolveEffectKey(effectKey),
                canonical);
        }

        private static List<CleanCard> BuildCards(int[] recipe, CleanPlayerId owner)
        {
            List<CleanCard> cards = new List<CleanCard>(recipe.Length);
            for (int i = 0; i < recipe.Length; i++)
            {
                cards.Add(new CleanCard(Get(recipe[i]), owner));
            }

            return cards;
        }

        private static int[] LocalRecipe()
        {
            return new[]
            {
                89631139, 89631139, 89631139,
                54332792, 54332792, 54332792,
                8240199, 8240199, 8240199,
                71039903, 71039903, 71039903,
                17947697, 17947697,
                23434538,
                97268402,
                14558127,
                59438930,
                66961194,
                30576089, 30576089,
                38517737,
                64202399,
                55410871,
                83764719,
                93437091,
                80326401, 80326401,
                17725109, 17725109, 17725109,
                12580477,
                24094653,
                21082832,
                41620959,
                71143015, 71143015, 71143015,
                62089826, 62089826, 62089826
            };
        }

        private static int[] RivalRecipe()
        {
            return LocalRecipe();
        }

        private static int[] ExtraRecipe()
        {
            return new[]
            {
                23995346,
                12381100, 12381100, 12381100,
                59822133, 59822133,
                89604813,
                42097666, 42097666, 42097666
            };
        }

        private static YgoCardDatabase LoadDatabase()
        {
            if (triedDatabase)
            {
                return database;
            }

            triedDatabase = true;
            string error;
            YgoCardDatabase.TryLoadDefault(out database, out error);
            return database;
        }

        private static MasterDuelCardDefinition CatalogDefinition(int id)
        {
            if (!triedCatalog)
            {
                triedCatalog = true;
                string ignored;
                MasterDuelCardCatalog.TryLoadDefault(out catalog, out ignored);
            }

            MasterDuelCardDefinition definition;
            return catalog != null && catalog.TryGet(id, out definition) ? definition : null;
        }

        private static CleanCardKind CatalogKind(MasterDuelCardDefinition definition)
        {
            string frame = definition.Frame ?? string.Empty;
            if (string.Equals(frame, "spell", System.StringComparison.OrdinalIgnoreCase))
            {
                return CleanCardKind.Spell;
            }

            if (string.Equals(frame, "trap", System.StringComparison.OrdinalIgnoreCase))
            {
                return CleanCardKind.Trap;
            }

            return CleanCardKind.Monster;
        }

        private static string CatalogTypeLine(MasterDuelCardDefinition definition)
        {
            CleanCardKind kind = CatalogKind(definition);
            if (kind == CleanCardKind.Spell)
            {
                return string.IsNullOrEmpty(definition.RaceOrType) ? "Spell" : definition.RaceOrType + " Spell";
            }

            if (kind == CleanCardKind.Trap)
            {
                return string.IsNullOrEmpty(definition.RaceOrType) ? "Trap" : definition.RaceOrType + " Trap";
            }

            List<string> parts = new List<string>();
            if (!string.IsNullOrEmpty(definition.Attribute))
            {
                parts.Add(definition.Attribute);
            }

            if (!string.IsNullOrEmpty(definition.RaceOrType))
            {
                parts.Add(definition.RaceOrType);
            }

            string cardClass = (definition.CardClass ?? string.Empty).Replace(" Monster", string.Empty);
            string[] subtypes = cardClass.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < subtypes.Length; i++)
            {
                if (!parts.Contains(subtypes[i]))
                {
                    parts.Add(subtypes[i]);
                }
            }

            return string.Join(" / ", parts.ToArray());
        }

        private static CleanCardKind MapKind(CardKind kind)
        {
            switch (kind)
            {
                case CardKind.Spell:
                    return CleanCardKind.Spell;
                case CardKind.Trap:
                    return CleanCardKind.Trap;
                default:
                    return CleanCardKind.Monster;
            }
        }

        private static string ResolveEffectKey(string configuredKey)
        {
            if (!string.IsNullOrEmpty(configuredKey))
            {
                return configuredKey;
            }

            // Lua source requires the full duel engine; never guess gameplay from it.
            return string.Empty;
        }

        private static CleanCardDefinition Monster(int id, string name, int level, int attack, int defense, string effectKey, string typeLine)
        {
            bool xyz = typeLine.IndexOf("Xyz", System.StringComparison.OrdinalIgnoreCase) >= 0;
            bool link = typeLine.IndexOf("Link", System.StringComparison.OrdinalIgnoreCase) >= 0;
            return new CleanCardDefinition(
                id,
                name,
                CleanCardKind.Monster,
                xyz || link ? 0 : level,
                xyz ? level : 0,
                link ? level : 0,
                attack,
                defense,
                typeLine,
                string.Empty,
                effectKey,
                null);
        }

        private static CleanCardDefinition RitualMonster(int id, string name, int level, int attack, int defense, string effectKey, string typeLine)
        {
            return new CleanCardDefinition(id, name, CleanCardKind.Monster, level, attack, defense, typeLine, string.Empty, effectKey);
        }

        private static CleanCardDefinition Spell(int id, string name, string effectKey)
        {
            return Spell(id, name, effectKey, "Spell");
        }

        private static CleanCardDefinition QuickPlaySpell(int id, string name, string effectKey)
        {
            return Spell(id, name, effectKey, "Quick-Play Spell");
        }

        private static CleanCardDefinition Spell(int id, string name, string effectKey, string typeLine)
        {
            return new CleanCardDefinition(id, name, CleanCardKind.Spell, 0, 0, 0, typeLine, string.Empty, effectKey);
        }

        private static CleanCardDefinition Trap(int id, string name, string effectKey)
        {
            return new CleanCardDefinition(id, name, CleanCardKind.Trap, 0, 0, 0, "Trap", string.Empty, effectKey);
        }
    }
}
