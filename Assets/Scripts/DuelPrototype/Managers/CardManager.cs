using System;

namespace IcarusGame.DuelPrototype
{
    public sealed class CardManager
    {
        public bool CanNormalSummonMonster(DuelCard card, bool setDefense)
        {
            if (card == null ||
                !card.IsMonster ||
                card.Definition.IsExtraDeckMonster ||
                card.Definition.IsToken ||
                card.Definition.CannotBeNormalSummonedOrSet)
            {
                return false;
            }

            return !setDefense || !card.Definition.IsLinkMonster;
        }

        public int TributeCount(int level)
        {
            if (level <= 4)
            {
                return 0;
            }

            if (level <= 6)
            {
                return 1;
            }

            if (level <= 8)
            {
                return 2;
            }

            return 3;
        }

        public int RequiredTributeCount(DuelPlayerState player, DuelPlayerState opponent, DuelCard card)
        {
            if (player == null || card == null || !card.IsMonster)
            {
                return 0;
            }

            return CanNormalSummonWithoutTributeByCardText(player, opponent, card)
                ? 0
                : TributeCount(card.Definition.Level);
        }

        public bool CanNormalSummonWithoutTributeByCardText(DuelPlayerState player, DuelPlayerState opponent, DuelCard card)
        {
            if (player == null || card == null || !card.IsMonster)
            {
                return false;
            }

            string rulesText = card.Definition.RulesText ?? string.Empty;
            if (card.Definition.CannotBeNormalSummonedOrSet)
            {
                return false;
            }

            if (rulesText.IndexOf("Normal Summon this card without Tributing", StringComparison.OrdinalIgnoreCase) < 0 &&
                rulesText.IndexOf("Normal Summon/Set this card without Tributing", StringComparison.OrdinalIgnoreCase) < 0 &&
                rulesText.IndexOf("can be Normal Summoned without Tributing", StringComparison.OrdinalIgnoreCase) < 0 &&
                rulesText.IndexOf("can be Normal Summoned/Set without Tributing", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            if (rulesText.IndexOf("opponent has 2 or more cards in their hand", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return opponent != null && opponent.Hand.Count >= 2;
            }

            if (rulesText.IndexOf("your opponent controls a monster, and you control no monsters", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return opponent != null && CountMonsters(opponent) > 0 && CountMonsters(player) == 0;
            }

            return true;
        }

        public bool StaysOnFieldAfterActivation(DuelCard card)
        {
            return card != null && card.Definition.IsContinuousSpellOrTrap;
        }

        public bool CanBeInMainDeck(DuelCardDefinition definition)
        {
            return definition != null && definition.CanBeInMainDeck;
        }

        public bool CanBeInExtraDeck(DuelCardDefinition definition)
        {
            return definition != null && definition.CanBeInExtraDeck;
        }

        private int CountMonsters(DuelPlayerState player)
        {
            if (player == null)
            {
                return 0;
            }

            int count = 0;
            for (int zone = 0; zone < DuelPlayerState.ZoneCount; zone++)
            {
                if (player.MonsterZones[zone] != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
