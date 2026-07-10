using System.Collections.Generic;

namespace IcarusGame.CleanDuel
{
    public enum CleanPlayerId
    {
        Local,
        Rival
    }

    public enum CleanDuelPhase
    {
        Draw,
        Main,
        Battle,
        Main2,
        End
    }

    public enum CleanCardKind
    {
        Monster,
        Spell,
        Trap
    }

    public enum CleanEffectTiming
    {
        None,
        Ignition,
        Quick,
        Trigger,
        Continuous
    }

    public enum CleanMonsterStance
    {
        Attack,
        Defense
    }

    public enum CleanZoneKind
    {
        Monster,
        SpellTrap,
        Deck,
        Graveyard,
        Banished,
        ExtraDeck,
        FieldSpell,
        ExtraMonster
    }


    public sealed class CleanChoiceOption
    {
        public CleanChoiceOption(int id, string label, string detail, bool isAvailable, string disabledReason)
        {
            Id = id;
            Label = label;
            Detail = detail;
            IsAvailable = isAvailable;
            DisabledReason = disabledReason;
        }

        public int Id { get; private set; }
        public string Label { get; private set; }
        public string Detail { get; private set; }
        public bool IsAvailable { get; private set; }
        public string DisabledReason { get; private set; }
    }

    public sealed class CleanChoicePrompt
    {
        public CleanChoicePrompt(int sourceCardId, string title, string body, List<CleanChoiceOption> options)
        {
            SourceCardId = sourceCardId;
            Title = title;
            Body = body;
            Options = options;
        }

        public int SourceCardId { get; private set; }
        public string Title { get; private set; }
        public string Body { get; private set; }
        public List<CleanChoiceOption> Options { get; private set; }
    }

    public sealed class CleanCardDefinition
    {
        public CleanCardDefinition(
            int id,
            string name,
            CleanCardKind kind,
            int level,
            int attack,
            int defense,
            string typeLine,
            string text,
            string effectKey)
        {
            Id = id;
            Name = name;
            Kind = kind;
            Level = level;
            Attack = attack;
            Defense = defense;
            TypeLine = typeLine;
            Text = text;
            EffectKey = effectKey;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public CleanCardKind Kind { get; private set; }
        public int Level { get; private set; }
        public int Attack { get; private set; }
        public int Defense { get; private set; }
        public string TypeLine { get; private set; }
        public string Text { get; private set; }
        public string EffectKey { get; private set; }
    }

    public sealed class CleanCard
    {
        public CleanCard(CleanCardDefinition definition, CleanPlayerId owner)
        {
            Definition = definition;
            Owner = owner;
            Controller = owner;
            Stance = CleanMonsterStance.Attack;
        }

        public CleanCardDefinition Definition { get; private set; }
        public CleanPlayerId Owner { get; private set; }
        public CleanPlayerId Controller { get; set; }
        public bool FaceDown { get; set; }
        public bool HasBattled { get; set; }
        public bool SummonedThisTurn { get; set; }
        public CleanMonsterStance Stance { get; set; }
        public int AttackBonus { get; set; }
        public int DefenseBonus { get; set; }
        public int AttackOverride { get; set; } = -1;
        public int DefenseOverride { get; set; } = -1;
        public bool CannotAttack { get; set; }
        public bool EffectsNegated { get; set; }
        public bool PiercingThisTurn { get; set; }
        public bool RitualSummonedUsingBlueEyesWhiteDragon { get; set; }
        public bool SentToGraveThisTurn { get; set; }
        public bool FusionSummoned { get; set; }
        public bool SynchroSummoned { get; set; }
        public bool LinkSummoned { get; set; }
        public bool FaceUpExtraDeck { get; set; }
        public bool DestroyDuringEndPhase { get; set; }

        public int CurrentAttack
        {
            get { return (AttackOverride >= 0 ? AttackOverride : Definition.Attack) + AttackBonus; }
        }

        public int CurrentDefense
        {
            get { return (DefenseOverride >= 0 ? DefenseOverride : Definition.Defense) + DefenseBonus; }
        }

        public string PublicName
        {
            get { return FaceDown ? "Face-down card" : Definition.Name; }
        }
    }

    public sealed class CleanPlayerState
    {
        public const int ZoneCount = 5;

        public CleanPlayerState(CleanPlayerId id, string name)
        {
            Id = id;
            Name = name;
        }

        public CleanPlayerId Id { get; private set; }
        public string Name { get; private set; }
        public int LifePoints = 8000;
        public bool NormalSummonUsed;
        public bool PendulumSummonUsed;
        public int GatewayToChaosCounters;
        public bool ExtraDeckDragonOnlyThisTurn;
        public readonly HashSet<string> UsedOncePerTurn = new HashSet<string>();
        public readonly List<CleanCard> Deck = new List<CleanCard>();
        public readonly List<CleanCard> ExtraDeck = new List<CleanCard>();
        public readonly List<CleanCard> Hand = new List<CleanCard>();
        public readonly CleanCard[] Monsters = new CleanCard[ZoneCount];
        public readonly CleanCard[] SpellsTraps = new CleanCard[ZoneCount];
        public CleanCard FieldSpell;
        public readonly List<CleanCard> Graveyard = new List<CleanCard>();
        public readonly List<CleanCard> Banished = new List<CleanCard>();
    }

    public sealed class CleanDuelState
    {
        public readonly CleanPlayerState Local = new CleanPlayerState(CleanPlayerId.Local, "Dr, Wheelie");
        public readonly CleanPlayerState Rival = new CleanPlayerState(CleanPlayerId.Rival, "Messenger of Justice");
        public readonly List<string> Log = new List<string>();
        public readonly CleanCard[] ExtraMonsters = new CleanCard[2];

        public CleanPlayerId TurnPlayer = CleanPlayerId.Local;
        public CleanDuelPhase Phase = CleanDuelPhase.Draw;
        public int TurnNumber = 1;
        public bool GameOver;
        public CleanChoicePrompt PendingChoice;
        public string WinnerText = string.Empty;
    }
}
