using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcarusGame.DuelPrototype
{
    public static class YugiohCardDatabase
    {
        private const string ResourcePath = "DuelPrototype/yugioh_cards";

        private static readonly List<DuelCardDefinition> cachedCards = new List<DuelCardDefinition>();
        private static bool hasLoaded;

        public static List<DuelCardDefinition> BuildStarterDeck()
        {
            EnsureLoaded();

            List<DuelCardDefinition> deck = new List<DuelCardDefinition>(40);
            if (cachedCards.Count == 0)
            {
                return deck;
            }

            AddStrongestMatchingCards(deck, 18, IsLowLevelMonster);
            AddStrongestMatchingCards(deck, 4, IsTributeMonster);
            AddMatchingCards(deck, 10, card => card.Kind == DuelCardKind.Spell);
            AddMatchingCards(deck, 8, card => card.Kind == DuelCardKind.Trap);

            if (deck.Count < 40)
            {
                AddMatchingCards(deck, 40 - deck.Count, card => true);
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
