using System.Collections.Generic;
using IcarusGame.MasterDuel;

namespace IcarusGame.CleanDuel
{
    public static class CleanCardLibrary
    {
        private static readonly Dictionary<int, CleanCardDefinition> fallbackCards =
            new Dictionary<int, CleanCardDefinition>
            {
                { 89631139, Monster(89631139, "Blue-Eyes White Dragon", 8, 3000, 2500, string.Empty, "Dragon / Normal") },
                { 88241506, Monster(88241506, "Maiden with Eyes of Blue", 1, 0, 0, "maiden_with_eyes_clean", "Spellcaster / Tuner / Effect") },
                { 8240199, Monster(8240199, "Sage with Eyes of Blue", 1, 0, 1500, "sage_clean", "Spellcaster / Tuner / Effect") },
                { 71039903, Monster(71039903, "The White Stone of Ancients", 1, 600, 500, "stone_ancients_clean", "Dragon / Tuner / Effect") },
                { 17947697, Monster(17947697, "Maiden of White", 1, 0, 0, "maiden_white", "Spellcaster / Tuner / Effect") },
                { 23434538, Monster(23434538, "Maxx \"C\"", 2, 500, 200, "maxx_c_clean", "Insect / Effect") },
                { 97268402, Monster(97268402, "Effect Veiler", 1, 0, 0, "veiler_clean", "Spellcaster / Tuner / Effect") },
                { 14558127, Monster(14558127, "Ash Blossom & Joyous Spring", 3, 0, 1800, "ash_clean", "Zombie / Tuner / Effect") },
                { 59438930, Monster(59438930, "Ghost Ogre & Snow Rabbit", 3, 0, 1800, "ghost_ogre_clean", "Psychic / Tuner / Effect") },
                { 66961194, Monster(66961194, "Dictator of D.", 4, 1200, 1100, "dictator_clean", "Spellcaster / Effect") },
                { 30576089, Monster(30576089, "Blue-Eyes Jet Dragon", 8, 3000, 0, "jet_clean", "Dragon / Effect") },
                { 45467446, Monster(45467446, "Dragon Spirit of White", 8, 2500, 2000, "dragon_spirit_white_clean", "Dragon / Effect") },
                { 55410871, RitualMonster(55410871, "Blue-Eyes Chaos MAX Dragon", 8, 4000, 0, "chaos_max_clean", "Dragon / Ritual / Effect") },
                { 20654247, RitualMonster(20654247, "Blue-Eyes Chaos Dragon", 8, 3000, 0, "chaos_dragon_clean", "Dragon / Ritual / Effect") },
                { 70551291, RitualMonster(70551291, "Black Luster Soldier - Legendary Swordsman", 8, 3000, 2500, string.Empty, "Warrior / Ritual / Effect") },
                { 23995346, Monster(23995346, "Blue-Eyes Ultimate Dragon", 12, 4500, 3800, string.Empty, "Dragon / Fusion / Normal") },
                { 12381100, Monster(12381100, "Dragon Master Magia", 12, 5000, 4000, "magia_clean", "Dragon / Fusion / Effect") },
                { 59822133, Monster(59822133, "Blue-Eyes Spirit Dragon", 9, 2500, 3000, "spirit_dragon_clean", "Dragon / LIGHT / Synchro / Effect") },
                { 89604813, Monster(89604813, "Blue-Eyes Ultimate Spirit Dragon", 12, 3500, 4000, "ultimate_spirit_clean", "Dragon / LIGHT / Synchro / Effect") },
                { 40908371, Monster(40908371, "Azure-Eyes Silver Dragon", 9, 2500, 3000, "azure_eyes_clean", "Dragon / LIGHT / Synchro / Effect") },
                { 42097666, Monster(42097666, "Spirit with Eyes of Blue", 1, 300, 0, "spirit_with_eyes_clean", "Dragon / Link / Effect") },
                { 24382602, Spell(24382602, "Mausoleum of White", "mausoleum_clean") },
                { 93437091, Spell(93437091, "Bingo Machine, Go!!!", "bingo_clean") },
                { 80326401, Spell(80326401, "Wishes for Eyes of Blue", "wishes_clean") },
                { 17725109, Spell(17725109, "Roar of the Blue-Eyed Dragons", "roar_clean") },
                { 40089744, Spell(40089744, "Gateway to Chaos", "gateway_chaos_clean") },
                { 2783661, Spell(2783661, "Majesty with Eyes of Blue", "majesty_blue_clean") },
                { 21082832, Spell(21082832, "Chaos Form", "chaos_form_clean") },
                { 41620959, Spell(41620959, "Dragon Shrine", "dragon_shrine_clean") },
                { 71143015, QuickPlaySpell(71143015, "Ultimate Fusion", "ultimate_fusion_clean") },
                { 62089826, Trap(62089826, "True Light", "true_light_clean") }
            };

        private static YgoCardDatabase database;
        private static bool triedDatabase;

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
            if (!fallbackCards.TryGetValue(id, out fallback))
            {
                fallback = Monster(id, "Card " + id, 4, 1500, 1500, string.Empty, "Monster");
            }

            YgoCardDatabase loaded = LoadDatabase();
            if (loaded == null)
            {
                return fallback;
            }

            YgoCardRecord record;
            if (!loaded.TryGetRecord(id, out record))
            {
                return fallback;
            }

            CardData data = YgoCardDatabase.CreateCardData(record, string.Empty, fallback.EffectKey);
            string effectKey = ResolveEffectKey(fallback.EffectKey, data);
            return new CleanCardDefinition(
                data.Id,
                data.Name,
                MapKind(data.Kind),
                data.Level,
                data.Attack,
                data.Defense,
                data.TypeLine,
                data.Text,
                effectKey);
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
                88241506, 88241506, 88241506,
                8240199, 8240199, 8240199,
                71039903, 71039903, 71039903,
                17947697, 17947697,
                23434538,
                97268402,
                14558127,
                59438930,
                66961194,
                30576089, 30576089,
                45467446,
                55410871,
                20654247,
                24382602,
                93437091,
                80326401, 80326401,
                17725109, 17725109, 17725109,
                40089744,
                2783661,
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

        private static string ResolveEffectKey(string configuredKey, CardData data)
        {
            if (!string.IsNullOrEmpty(configuredKey))
            {
                return configuredKey;
            }

            if (data == null || !CleanLuaScriptRepository.HasCardScript(data.Id))
            {
                return string.Empty;
            }

            return "lua_" + data.Id;
        }

        private static CleanCardDefinition Monster(int id, string name, int level, int attack, int defense, string effectKey, string typeLine)
        {
            return new CleanCardDefinition(id, name, CleanCardKind.Monster, level, attack, defense, typeLine, string.Empty, effectKey);
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
