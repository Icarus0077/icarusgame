using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcarusGame.DuelPrototype
{
    public static class YugiohCardDatabase
    {
        private const string ResourcePath = "DuelPrototype/yugioh_cards";
        private const int DeckSize = 40;
        private const int MaxCopiesPerCard = 3;
        private const string PlayerTheme = "K9";
        private const string OpponentTheme = "Six Samurai";

        private static readonly List<DuelCardDefinition> cachedCards = new List<DuelCardDefinition>();
        private static bool hasLoaded;

        public static List<DuelCardDefinition> BuildStarterDeck()
        {
            return BuildPlayerDeck();
        }

        public static List<DuelCardDefinition> BuildPlayerDeck()
        {
            return BuildThemeDeck(PlayerTheme, "Nekroz");
        }

        public static List<DuelCardDefinition> BuildOpponentDeck()
        {
            return BuildThemeDeck(OpponentTheme, "Blackwing");
        }

        private static List<DuelCardDefinition> BuildThemeDeck(string theme, string fallbackTheme)
        {
            EnsureLoaded();

            List<DuelCardDefinition> deck = BuildThemeDeckCandidate(theme);
            if (deck.Count >= DeckSize)
            {
                return deck;
            }

            deck = BuildThemeDeckCandidate(fallbackTheme);
            if (deck.Count >= DeckSize)
            {
                return deck;
            }

            return BuildBalancedFallbackDeck();
        }

        private static List<DuelCardDefinition> BuildThemeDeckCandidate(string theme)
        {
            List<DuelCardDefinition> deck = new List<DuelCardDefinition>(DeckSize);
            if (string.IsNullOrEmpty(theme))
            {
                return deck;
            }

            EnsureLoaded();
            if (cachedCards.Count == 0)
            {
                return deck;
            }

            List<DuelCardDefinition> themeMonsters = new List<DuelCardDefinition>();
            List<DuelCardDefinition> themeSpellsTraps = new List<DuelCardDefinition>();
            List<DuelCardDefinition> namedSupport = new List<DuelCardDefinition>();

            for (int i = 0; i < cachedCards.Count; i++)
            {
                DuelCardDefinition card = cachedCards[i];
                if (!IsMainDeckCard(card))
                {
                    continue;
                }

                if (IsThemeCard(card, theme))
                {
                    if (card.Kind == DuelCardKind.Monster)
                    {
                        themeMonsters.Add(card);
                    }
                    else
                    {
                        themeSpellsTraps.Add(card);
                    }
                }
                else if (MentionsTheme(card, theme))
                {
                    namedSupport.Add(card);
                }
            }

            if (themeMonsters.Count == 0)
            {
                return deck;
            }

            SortForDeck(themeMonsters);
            SortForDeck(themeSpellsTraps);
            SortForDeck(namedSupport);

            AddCopiesFromCandidates(deck, themeMonsters, Math.Min(24, themeMonsters.Count * MaxCopiesPerCard));
            AddCopiesFromCandidates(deck, themeSpellsTraps, Math.Min(14, themeSpellsTraps.Count * MaxCopiesPerCard));
            AddCopiesFromCandidates(deck, namedSupport, 6);

            if (CountInDeck(deck, card => card.Kind == DuelCardKind.Monster) < 20)
            {
                AddCopiesFromCards(deck, 20 - CountInDeck(deck, card => card.Kind == DuelCardKind.Monster), card =>
                    IsLowLevelMonster(card) && !MentionsDifferentTheme(card, theme));
            }

            if (CountInDeck(deck, card => card.Kind != DuelCardKind.Monster) < 12)
            {
                AddCopiesFromCards(deck, 12 - CountInDeck(deck, card => card.Kind != DuelCardKind.Monster), card =>
                    card.Kind != DuelCardKind.Monster && IsMainDeckCard(card) && !MentionsDifferentTheme(card, theme));
            }

            if (deck.Count < DeckSize)
            {
                AddCopiesFromCards(deck, DeckSize - deck.Count, card =>
                    IsMainDeckCard(card) && !MentionsDifferentTheme(card, theme));
            }

            TrimDeck(deck);
            return deck;
        }

        private static List<DuelCardDefinition> BuildBalancedFallbackDeck()
        {
            EnsureLoaded();

            List<DuelCardDefinition> deck = new List<DuelCardDefinition>(DeckSize);
            if (cachedCards.Count == 0)
            {
                return deck;
            }

            AddStrongestMatchingCards(deck, 18, IsLowLevelMonster);
            AddStrongestMatchingCards(deck, 4, IsTributeMonster);
            AddMatchingCards(deck, 10, card => card.Kind == DuelCardKind.Spell);
            AddMatchingCards(deck, 8, card => card.Kind == DuelCardKind.Trap);

            if (deck.Count < DeckSize)
            {
                AddMatchingCards(deck, DeckSize - deck.Count, IsMainDeckCard);
            }

            return deck;
        }

        private static void EnsureLoaded()
        {
            if (hasLoaded)
            {
                return;
            }

            hasLoaded = true;
            TextAsset json = Resources.Load<TextAsset>(ResourcePath);
            if (json == null)
            {
                Debug.LogWarning("Yu-Gi-Oh card database not found at Resources/" + ResourcePath + ". Falling back to prototype deck.");
                return;
            }

            YugiohCardCollection collection = JsonUtility.FromJson<YugiohCardCollection>(json.text);
            if (collection == null || collection.cards == null)
            {
                Debug.LogWarning("Yu-Gi-Oh card database could not be parsed. Falling back to prototype deck.");
                return;
            }

            for (int i = 0; i < collection.cards.Length; i++)
            {
                YugiohCardRecord record = collection.cards[i];
                if (record == null || string.IsNullOrEmpty(record.name))
                {
                    continue;
                }

                cachedCards.Add(ToDefinition(record, i));
            }
        }

        private static DuelCardDefinition ToDefinition(YugiohCardRecord record, int index)
        {
            DuelCardKind kind = MapKind(record.type);
            int effectValue;
            DuelEffectKind effect = MapEffect(record, kind, index, out effectValue);
            int level = NormalizeLevel(record.level, kind);
            int attack = NormalizeStat(record.atk);
            int defense = NormalizeStat(record.def);

            string cardTypeText = string.IsNullOrEmpty(record.humanReadableCardType)
                ? record.type
                : record.humanReadableCardType;

            return new DuelCardDefinition(
                record.databaseId,
                record.name,
                record.imageUrl,
                record.cardBack,
                kind,
                cardTypeText,
                record.attribute,
                record.race,
                record.archetype,
                level,
                attack,
                defense,
                effect,
                effectValue,
                BuildRulesText(record, kind, effect, effectValue));
        }

        private static DuelCardKind MapKind(string type)
        {
            string lowerType = (type ?? string.Empty).ToLowerInvariant();
            if (lowerType.Contains("spell"))
            {
                return DuelCardKind.Spell;
            }

            if (lowerType.Contains("trap"))
            {
                return DuelCardKind.Trap;
            }

            return DuelCardKind.Monster;
        }

        private static DuelEffectKind MapEffect(YugiohCardRecord record, DuelCardKind kind, int index, out int effectValue)
        {
            int seed = record.databaseId != 0 ? Math.Abs(record.databaseId) : index + 1;
            effectValue = 0;

            if (kind == DuelCardKind.Spell)
            {
                switch (seed % 5)
                {
                    case 0:
                        effectValue = 2;
                        return DuelEffectKind.DrawCards;
                    case 1:
                        effectValue = 800;
                        return DuelEffectKind.DealDamage;
                    case 2:
                        effectValue = 500;
                        return DuelEffectKind.BoostOwnMonsters;
                    case 3:
                        return DuelEffectKind.DestroyWeakestOpponentMonster;
                    default:
                        effectValue = 1200;
                        return DuelEffectKind.GainLife;
                }
            }

            if (kind == DuelCardKind.Trap)
            {
                return seed % 2 == 0 ? DuelEffectKind.NegateAttack : DuelEffectKind.DestroyAttacker;
            }

            return DuelEffectKind.None;
        }

        private static string BuildRulesText(YugiohCardRecord record, DuelCardKind kind, DuelEffectKind effect, int effectValue)
        {
            if (!string.IsNullOrEmpty(record.description))
            {
                return record.description;
            }

            if (kind == DuelCardKind.Monster)
            {
                return record.type + " uit de Excel kaartdatabase. Archetype: " + EmptyAsDash(record.archetype) + ".";
            }

            switch (effect)
            {
                case DuelEffectKind.DrawCards:
                    return record.type + ": trek " + effectValue + " kaarten.";
                case DuelEffectKind.GainLife:
                    return record.type + ": krijg " + effectValue + " LP.";
                case DuelEffectKind.DealDamage:
                    return record.type + ": doe " + effectValue + " effectschade.";
                case DuelEffectKind.BoostOwnMonsters:
                    return record.type + ": eigen monsters krijgen +" + effectValue + " ATK tot de End Phase.";
                case DuelEffectKind.DestroyWeakestOpponentMonster:
                    return record.type + ": vernietig het zwakste monster van de tegenstander.";
                case DuelEffectKind.NegateAttack:
                    return record.type + ": negate een aanval nadat deze kaart een beurt gezet is.";
                case DuelEffectKind.DestroyAttacker:
                    return record.type + ": vernietig een aanvallend monster nadat deze kaart een beurt gezet is.";
                default:
                    return record.type + " uit de Excel kaartdatabase.";
            }
        }

        private static bool IsLowLevelMonster(DuelCardDefinition card)
        {
            return card.Kind == DuelCardKind.Monster &&
                   IsMainDeckMonster(card) &&
                   card.Level >= 1 &&
                   card.Level <= 4 &&
                   card.Attack > 0;
        }

        private static bool IsTributeMonster(DuelCardDefinition card)
        {
            return card.Kind == DuelCardKind.Monster &&
                   IsMainDeckMonster(card) &&
                   card.Level >= 5 &&
                   card.Level <= 7 &&
                   card.Attack > 0;
        }

        private static bool IsMainDeckMonster(DuelCardDefinition card)
        {
            string type = (card.CardTypeText ?? string.Empty).ToLowerInvariant();
            return !type.Contains("fusion") &&
                   !type.Contains("synchro") &&
                   !type.Contains("xyz") &&
                   !type.Contains("link") &&
                   !type.Contains("token");
        }

        private static bool IsMainDeckCard(DuelCardDefinition card)
        {
            return card.Kind != DuelCardKind.Monster || IsMainDeckMonster(card);
        }

        private static bool IsThemeCard(DuelCardDefinition card, string theme)
        {
            return string.Equals(card.Archetype, theme, StringComparison.OrdinalIgnoreCase) ||
                   ContainsIgnoreCase(card.Name, theme);
        }

        private static bool MentionsTheme(DuelCardDefinition card, string theme)
        {
            return ContainsIgnoreCase(card.RulesText, "\"" + theme + "\"") ||
                   ContainsIgnoreCase(card.RulesText, theme);
        }

        private static bool MentionsDifferentTheme(DuelCardDefinition card, string theme)
        {
            return !string.IsNullOrEmpty(card.Archetype) &&
                   !string.Equals(card.Archetype, theme, StringComparison.OrdinalIgnoreCase);
        }

        private static void AddCopiesFromCards(List<DuelCardDefinition> deck, int count, Predicate<DuelCardDefinition> predicate)
        {
            List<DuelCardDefinition> candidates = new List<DuelCardDefinition>();
            for (int i = 0; i < cachedCards.Count; i++)
            {
                DuelCardDefinition card = cachedCards[i];
                if (predicate(card))
                {
                    candidates.Add(card);
                }
            }

            SortForDeck(candidates);
            AddCopiesFromCandidates(deck, candidates, count);
        }

        private static void AddCopiesFromCandidates(List<DuelCardDefinition> deck, List<DuelCardDefinition> candidates, int count)
        {
            if (count <= 0 || deck.Count >= DeckSize)
            {
                return;
            }

            Dictionary<int, int> copyCounts = BuildCopyCounts(deck);
            for (int i = 0; i < candidates.Count && count > 0 && deck.Count < DeckSize; i++)
            {
                DuelCardDefinition card = candidates[i];
                int key = CardKey(card);
                int currentCopies;
                copyCounts.TryGetValue(key, out currentCopies);

                while (currentCopies < MaxCopiesPerCard && count > 0 && deck.Count < DeckSize)
                {
                    deck.Add(card);
                    currentCopies++;
                    copyCounts[key] = currentCopies;
                    count--;
                }
            }
        }

        private static Dictionary<int, int> BuildCopyCounts(List<DuelCardDefinition> deck)
        {
            Dictionary<int, int> copyCounts = new Dictionary<int, int>();
            for (int i = 0; i < deck.Count; i++)
            {
                int key = CardKey(deck[i]);
                int count;
                copyCounts.TryGetValue(key, out count);
                copyCounts[key] = count + 1;
            }

            return copyCounts;
        }

        private static int CountInDeck(List<DuelCardDefinition> deck, Predicate<DuelCardDefinition> predicate)
        {
            int count = 0;
            for (int i = 0; i < deck.Count; i++)
            {
                if (predicate(deck[i]))
                {
                    count++;
                }
            }

            return count;
        }

        private static void TrimDeck(List<DuelCardDefinition> deck)
        {
            while (deck.Count > DeckSize)
            {
                deck.RemoveAt(deck.Count - 1);
            }
        }

        private static void SortForDeck(List<DuelCardDefinition> cards)
        {
            cards.Sort((left, right) =>
            {
                int score = DeckScore(right).CompareTo(DeckScore(left));
                return score != 0 ? score : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
            });
        }

        private static int DeckScore(DuelCardDefinition card)
        {
            if (card.Kind == DuelCardKind.Monster)
            {
                int score = card.Attack;
                if (card.Level >= 1 && card.Level <= 4)
                {
                    score += 100000;
                }
                else if (card.Level <= 6)
                {
                    score += 80000;
                }
                else
                {
                    score += 45000;
                }

                return score;
            }

            int supportScore = 60000;
            if (card.IsQuickPlaySpell || card.IsContinuousSpellOrTrap)
            {
                supportScore += 5000;
            }

            supportScore += Math.Min(4000, card.EffectValue);
            return supportScore;
        }

        private static bool ContainsIgnoreCase(string text, string value)
        {
            return !string.IsNullOrEmpty(text) &&
                   !string.IsNullOrEmpty(value) &&
                   text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int CardKey(DuelCardDefinition card)
        {
            return card.DatabaseId != 0 ? card.DatabaseId : card.Name.GetHashCode();
        }

        private static void AddMatchingCards(List<DuelCardDefinition> deck, int count, Predicate<DuelCardDefinition> predicate)
        {
            if (count <= 0)
            {
                return;
            }

            HashSet<int> existingIds = new HashSet<int>();
            for (int i = 0; i < deck.Count; i++)
            {
                existingIds.Add(deck[i].DatabaseId);
            }

            for (int i = 0; i < cachedCards.Count && count > 0; i++)
            {
                DuelCardDefinition card = cachedCards[i];
                if (!predicate(card) || existingIds.Contains(card.DatabaseId))
                {
                    continue;
                }

                deck.Add(card);
                existingIds.Add(card.DatabaseId);
                count--;
            }
        }

        private static void AddStrongestMatchingCards(List<DuelCardDefinition> deck, int count, Predicate<DuelCardDefinition> predicate)
        {
            List<DuelCardDefinition> candidates = new List<DuelCardDefinition>();
            for (int i = 0; i < cachedCards.Count; i++)
            {
                DuelCardDefinition card = cachedCards[i];
                if (predicate(card))
                {
                    candidates.Add(card);
                }
            }

            candidates.Sort((left, right) => right.Attack.CompareTo(left.Attack));
            AddFromCandidates(deck, candidates, count);
        }

        private static void AddFromCandidates(List<DuelCardDefinition> deck, List<DuelCardDefinition> candidates, int count)
        {
            if (count <= 0)
            {
                return;
            }

            HashSet<int> existingIds = new HashSet<int>();
            for (int i = 0; i < deck.Count; i++)
            {
                existingIds.Add(deck[i].DatabaseId);
            }

            for (int i = 0; i < candidates.Count && count > 0; i++)
            {
                DuelCardDefinition card = candidates[i];
                if (existingIds.Contains(card.DatabaseId))
                {
                    continue;
                }

                deck.Add(card);
                existingIds.Add(card.DatabaseId);
                count--;
            }
        }

        private static int NormalizeLevel(int level, DuelCardKind kind)
        {
            if (kind != DuelCardKind.Monster)
            {
                return 0;
            }

            return level > 0 ? level : 4;
        }

        private static int NormalizeStat(int value)
        {
            return value >= 0 ? value : 0;
        }

        private static string EmptyAsDash(string value)
        {
            return string.IsNullOrEmpty(value) ? "-" : value;
        }

        [Serializable]
        private sealed class YugiohCardCollection
        {
            public YugiohCardRecord[] cards;
        }

        [Serializable]
        private sealed class YugiohCardRecord
        {
            public int databaseId;
            public string name;
            public string imageUrl;
            public string cardBack;
            public string type;
            public string humanReadableCardType;
            public string description;
            public string race;
            public string frameType;
            public string attribute;
            public int atk;
            public int def;
            public int level;
            public string archetype;
        }
    }
}
