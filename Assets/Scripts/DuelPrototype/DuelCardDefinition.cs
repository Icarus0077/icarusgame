using System;
using System.Collections.Generic;

namespace IcarusGame.DuelPrototype
{
    public enum DuelPlayerId
    {
        Player = 0,
        Opponent = 1
    }

    public enum DuelPhase
    {
        Draw,
        Standby,
        Main1,
        Battle,
        Main2,
        End
    }

    public enum DuelCardKind
    {
        Monster,
        Spell,
        Trap
    }

    public enum MonsterBattlePosition
    {
        Attack,
        DefenseFaceUp,
        DefenseFaceDown
    }

    public enum DuelEffectKind
    {
        None,
        DrawCards,
        GainLife,
        DealDamage,
        BoostOwnMonsters,
        DestroyWeakestOpponentMonster,
        NegateAttack,
        DestroyAttacker
    }

    public sealed class DuelCardDefinition
    {
        public DuelCardDefinition(
            string name,
            DuelCardKind kind,
            int level,
            int attack,
            int defense,
            DuelEffectKind effect,
            int effectValue,
            string rulesText)
            : this(0, name, string.Empty, string.Empty, kind, kind.ToString(), string.Empty, string.Empty, string.Empty, level, attack, defense, effect, effectValue, rulesText)
        {
        }

        public DuelCardDefinition(
            int databaseId,
            string name,
            string imageUrl,
            string cardBack,
            DuelCardKind kind,
            string cardTypeText,
            string attribute,
            string race,
            string archetype,
            int level,
            int attack,
            int defense,
            DuelEffectKind effect,
            int effectValue,
            string rulesText)
        {
            DatabaseId = databaseId;
            Name = name;
            ImageUrl = imageUrl;
            CardBack = cardBack;
            Kind = kind;
            CardTypeText = cardTypeText;
            Attribute = attribute;
            Race = race;
            Archetype = archetype;
            Level = level;
            Attack = attack;
            Defense = defense;
            Effect = effect;
            EffectValue = effectValue;
            RulesText = rulesText;
        }

        public int DatabaseId { get; private set; }
        public string Name { get; private set; }
        public string ImageUrl { get; private set; }
        public string CardBack { get; private set; }
        public DuelCardKind Kind { get; private set; }
        public string CardTypeText { get; private set; }
        public string Attribute { get; private set; }
        public string Race { get; private set; }
        public string Archetype { get; private set; }
        public int Level { get; private set; }
        public int Attack { get; private set; }
        public int Defense { get; private set; }
        public DuelEffectKind Effect { get; private set; }
        public int EffectValue { get; private set; }
        public string RulesText { get; private set; }

        public bool IsMonster
        {
            get { return Kind == DuelCardKind.Monster; }
        }

        public bool IsLinkMonster
        {
            get { return Kind == DuelCardKind.Monster && ContainsCardType("Link"); }
        }

        public bool IsQuickPlaySpell
        {
            get { return Kind == DuelCardKind.Spell && ContainsCardType("Quick-Play"); }
        }

        public bool IsFieldSpell
        {
            get { return Kind == DuelCardKind.Spell && ContainsCardType("Field"); }
        }

        public bool IsEquipSpell
        {
            get { return Kind == DuelCardKind.Spell && ContainsCardType("Equip"); }
        }

        public bool IsContinuousSpellOrTrap
        {
            get
            {
                return (Kind == DuelCardKind.Spell || Kind == DuelCardKind.Trap) &&
                       (ContainsCardType("Continuous") || IsFieldSpell || IsEquipSpell);
            }
        }

        public bool IsCounterTrap
        {
            get { return Kind == DuelCardKind.Trap && ContainsCardType("Counter"); }
        }

        public bool IsExtraDeckMonster
        {
            get
            {
                return Kind == DuelCardKind.Monster &&
                       (ContainsCardType("Fusion") ||
                        ContainsCardType("Synchro") ||
                        ContainsCardType("Xyz") ||
                        ContainsCardType("Link"));
            }
        }

        public bool HasImage
        {
            get { return !string.IsNullOrEmpty(ImageUrl); }
        }

        private bool ContainsCardType(string value)
        {
            return !string.IsNullOrEmpty(CardTypeText) &&
                   CardTypeText.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    public sealed class DuelCard
    {
        public DuelCard(DuelCardDefinition definition, DuelPlayerId ownerId)
        {
            Definition = definition;
            OwnerId = ownerId;
            Position = MonsterBattlePosition.Attack;
            SetTurn = -1;
            SummonedTurn = -1;
            PositionChangedTurn = -1;
        }

        public DuelCardDefinition Definition { get; private set; }
        public DuelPlayerId OwnerId { get; private set; }
        public bool IsFaceDown { get; set; }
        public bool HasAttacked { get; set; }
        public int SetTurn { get; set; }
        public int SummonedTurn { get; set; }
        public int PositionChangedTurn { get; set; }
        public int TemporaryAttackBonus { get; set; }
        public MonsterBattlePosition Position { get; set; }

        public int CurrentAttack
        {
            get { return Definition.Attack + TemporaryAttackBonus; }
        }

        public int CurrentDefense
        {
            get { return Definition.Defense; }
        }

        public bool IsMonster
        {
            get { return Definition.Kind == DuelCardKind.Monster; }
        }

        public string PublicName
        {
            get { return IsFaceDown ? "Face-down" : Definition.Name; }
        }
    }

    public sealed class DuelPlayerState
    {
        public const int ZoneCount = 5;

        public DuelPlayerState(DuelPlayerId id, string name)
        {
            Id = id;
            Name = name;
        }

        public DuelPlayerId Id { get; private set; }
        public string Name { get; private set; }
        public int LifePoints { get; set; }
        public bool NormalSummonUsed { get; set; }

        public readonly List<DuelCardDefinition> Deck = new List<DuelCardDefinition>();
        public readonly List<DuelCard> Hand = new List<DuelCard>();
        public readonly DuelCard[] MonsterZones = new DuelCard[ZoneCount];
        public readonly DuelCard[] SpellTrapZones = new DuelCard[ZoneCount];
        public readonly List<DuelCard> Graveyard = new List<DuelCard>();
        public readonly List<DuelCard> Banished = new List<DuelCard>();
    }
}
