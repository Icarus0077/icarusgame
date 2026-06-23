using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

namespace IcarusGame.MasterDuel
{
    public enum PlayerId
    {
        Player,
        Opponent
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

    public enum CardKind
    {
        Monster,
        Spell,
        Trap
    }

    public enum MonsterFrame
    {
        Normal,
        Effect,
        Ritual,
        Fusion,
        Synchro,
        Xyz,
        Link
    }

    public enum MonsterPosition
    {
        Attack,
        DefenseFaceUp,
        DefenseFaceDown
    }

    public enum CardLocation
    {
        Deck,
        ExtraDeck,
        Hand,
        MonsterZone,
        SpellTrapZone,
        OverlayMaterial,
        Graveyard,
        Banished
    }

    public sealed class CardData
    {
        public CardData(
            int id,
            string name,
            CardKind kind,
            MonsterFrame frame,
            int level,
            int attack,
            int defense,
            string typeLine,
            string text,
            string archetype,
            string effectId)
        {
            Id = id;
            Name = name;
            Kind = kind;
            Frame = frame;
            Level = level;
            Attack = attack;
            Defense = defense;
            TypeLine = typeLine;
            Text = text;
            Archetype = archetype;
            EffectId = effectId;
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public CardKind Kind { get; private set; }
        public MonsterFrame Frame { get; private set; }
        public int Level { get; private set; }
        public int Attack { get; private set; }
        public int Defense { get; private set; }
        public string TypeLine { get; private set; }
        public string Text { get; private set; }
        public string Archetype { get; private set; }
        public string EffectId { get; private set; }

        public bool IsMonster
        {
            get { return Kind == CardKind.Monster; }
        }

        public bool IsExtraDeckMonster
        {
            get
            {
                return Kind == CardKind.Monster &&
                       (Frame == MonsterFrame.Fusion ||
                        Frame == MonsterFrame.Synchro ||
                        Frame == MonsterFrame.Xyz ||
                        Frame == MonsterFrame.Link);
            }
        }

        public bool IsLinkMonster
        {
            get { return Frame == MonsterFrame.Link; }
        }
    }

    public sealed class CardInstance
    {
        public CardInstance(CardData data, PlayerId owner)
        {
            Data = data;
            Owner = owner;
            Controller = owner;
            Location = CardLocation.Deck;
            Position = MonsterPosition.Attack;
        }

        public CardData Data { get; private set; }
        public PlayerId Owner { get; private set; }
        public PlayerId Controller { get; set; }
        public CardLocation Location { get; set; }
        public MonsterPosition Position { get; set; }
        public bool FaceDown { get; set; }
        public bool HasAttacked { get; set; }
        public int SummonedTurn { get; set; }
        public int SetTurn { get; set; }
        public int TemporaryAttack { get; set; }
        public readonly List<CardInstance> Materials = new List<CardInstance>();

        public int CurrentAttack
        {
            get { return Mathf.Max(0, Data.Attack + TemporaryAttack); }
        }

        public int CurrentDefense
        {
            get { return Mathf.Max(0, Data.Defense); }
        }

        public string PublicName
        {
            get { return FaceDown ? "Face-down card" : Data.Name; }
        }
    }

    public sealed class PlayerState
    {
        public const int ZoneCount = 5;
        public const int MainDeckSize = 40;
        public const int ExtraDeckSize = 15;

        public PlayerState(PlayerId id, string name)
        {
            Id = id;
            Name = name;
        }

        public PlayerId Id { get; private set; }
        public string Name { get; private set; }
        public int LifePoints = 8000;
        public bool NormalSummonUsed;
        public readonly List<CardInstance> Deck = new List<CardInstance>();
        public readonly List<CardInstance> ExtraDeck = new List<CardInstance>();
        public readonly List<CardInstance> Hand = new List<CardInstance>();
        public readonly CardInstance[] Monsters = new CardInstance[ZoneCount];
        public readonly CardInstance[] SpellsTraps = new CardInstance[ZoneCount];
        public readonly List<CardInstance> Graveyard = new List<CardInstance>();
        public readonly List<CardInstance> Banished = new List<CardInstance>();
    }

    public sealed class DuelState
    {
        public PlayerState Player = new PlayerState(PlayerId.Player, "Dr, Wheelie");
        public PlayerState Opponent = new PlayerState(PlayerId.Opponent, "Messenger of Justice");
        public PlayerId TurnPlayer = PlayerId.Player;
        public DuelPhase Phase = DuelPhase.Draw;
        public int TurnNumber;
        public bool GameOver;
        public string WinnerText = string.Empty;
        public readonly List<string> Log = new List<string>();
    }

    public sealed class PhaseManager
    {
        public bool IsMainPhase(DuelPhase phase)
        {
            return phase == DuelPhase.Main1 || phase == DuelPhase.Main2;
        }

        public DuelPhase Next(DuelPhase phase, int turnNumber)
        {
            switch (phase)
            {
                case DuelPhase.Draw:
                    return DuelPhase.Standby;
                case DuelPhase.Standby:
                    return DuelPhase.Main1;
                case DuelPhase.Main1:
                    return CanEnterBattlePhase(turnNumber) ? DuelPhase.Battle : DuelPhase.End;
                case DuelPhase.Battle:
                    return DuelPhase.Main2;
                case DuelPhase.Main2:
                    return DuelPhase.End;
                default:
                    return DuelPhase.Draw;
            }
        }

        public bool CanEnterBattlePhase(int turnNumber)
        {
            return turnNumber > 1;
        }

        public string Label(DuelPhase phase)
        {
            switch (phase)
            {
                case DuelPhase.Draw:
                    return "DRAW PHASE";
                case DuelPhase.Standby:
                    return "STANDBY PHASE";
                case DuelPhase.Main1:
                    return "MAIN PHASE 1";
                case DuelPhase.Battle:
                    return "BATTLE PHASE";
                case DuelPhase.Main2:
                    return "MAIN PHASE 2";
                case DuelPhase.End:
                    return "END PHASE";
                default:
                    return phase.ToString().ToUpperInvariant();
            }
        }

        public string ShortLabel(DuelPhase phase)
        {
            switch (phase)
            {
                case DuelPhase.Draw:
                    return "Draw";
                case DuelPhase.Standby:
                    return "Standby";
                case DuelPhase.Main1:
                    return "Main 1";
                case DuelPhase.Battle:
                    return "Battle";
                case DuelPhase.Main2:
                    return "Main 2";
                case DuelPhase.End:
                    return "End";
                default:
                    return phase.ToString();
            }
        }
    }

    public sealed class TurnManager
    {
        public PlayerId Other(PlayerId id)
        {
            return id == PlayerId.Player ? PlayerId.Opponent : PlayerId.Player;
        }

        public bool DrawsForTurn(int turnNumber)
        {
            return turnNumber > 1;
        }
    }

    public sealed class ZoneManager
    {
        public int FirstEmpty(CardInstance[] zones)
        {
            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        public int CountMonsters(PlayerState player)
        {
            int count = 0;
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public sealed class CardManager
    {
        public bool CanNormalSummon(CardInstance card, PlayerState player, int turnNumber)
        {
            if (card == null || !card.Data.IsMonster || card.Data.IsExtraDeckMonster || player.NormalSummonUsed)
            {
                return false;
            }

            return RequiredTributes(card.Data) <= CountMonsters(player);
        }

        public int RequiredTributes(CardData data)
        {
            if (!data.IsMonster || data.Level <= 4)
            {
                return 0;
            }

            if (data.Level <= 6)
            {
                return 1;
            }

            if (data.Level <= 8)
            {
                return 2;
            }

            return 3;
        }

        public bool IsFastEffect(CardData data)
        {
            return data.Kind == CardKind.Trap ||
                   IsQuickPlay(data) ||
                   string.Equals(data.EffectId, "quick_special", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanActivateFromHand(CardInstance card, DuelPhase phase, PhaseManager phaseManager)
        {
            if (card == null || card.Data.Kind == CardKind.Trap)
            {
                return false;
            }

            if (card.Data.Kind == CardKind.Spell && IsQuickPlay(card.Data))
            {
                return true;
            }

            return phaseManager.IsMainPhase(phase);
        }

        public bool CanActivateSet(CardInstance card, DuelPhase phase, PhaseManager phaseManager, int turnNumber)
        {
            if (card == null || card.Location != CardLocation.SpellTrapZone)
            {
                return false;
            }

            if (card.SetTurn == turnNumber && (card.Data.Kind == CardKind.Trap || IsQuickPlay(card.Data)))
            {
                return false;
            }

            if (card.Data.Kind == CardKind.Trap || IsQuickPlay(card.Data))
            {
                return true;
            }

            return phaseManager.IsMainPhase(phase);
        }

        public bool StaysOnField(CardData data)
        {
            return IsContinuous(data) || IsField(data);
        }

        public bool IsQuickPlay(CardData data)
        {
            return data.TypeLine.IndexOf("Quick-Play", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool IsContinuous(CardData data)
        {
            return data.TypeLine.IndexOf("Continuous", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool IsField(CardData data)
        {
            return data.TypeLine.IndexOf("Field", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int CountMonsters(PlayerState player)
        {
            int count = 0;
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    public sealed class ChainLink
    {
        public ChainLink(int index, CardInstance source, PlayerId controller, int speed)
        {
            Index = index;
            Source = source;
            Controller = controller;
            Speed = speed;
        }

        public int Index { get; private set; }
        public CardInstance Source { get; private set; }
        public PlayerId Controller { get; private set; }
        public int Speed { get; private set; }
    }

    public sealed class ChainSystem
    {
        private readonly List<ChainLink> links = new List<ChainLink>();

        public int SpellSpeed(CardInstance card)
        {
            if (card == null)
            {
                return 0;
            }

            if (card.Data.Kind == CardKind.Trap)
            {
                return card.Data.TypeLine.IndexOf("Counter", StringComparison.OrdinalIgnoreCase) >= 0 ? 3 : 2;
            }

            return card.Data.TypeLine.IndexOf("Quick-Play", StringComparison.OrdinalIgnoreCase) >= 0 ? 2 : 1;
        }

        public ChainLink Start(CardInstance card, PlayerId controller)
        {
            links.Clear();
            ChainLink link = new ChainLink(1, card, controller, SpellSpeed(card));
            links.Add(link);
            return link;
        }

        public void Clear()
        {
            links.Clear();
        }
    }

    public sealed class BattleSystem
    {
        public bool CanAttack(CardInstance monster, DuelPhase phase, int turnNumber)
        {
            return monster != null &&
                   !monster.FaceDown &&
                   monster.Data.IsMonster &&
                   monster.Position == MonsterPosition.Attack &&
                   !monster.HasAttacked &&
                   phase == DuelPhase.Battle &&
                   turnNumber > 1;
        }

        public int ResolveAttack(CardInstance attacker, CardInstance defender, PlayerState attackerPlayer, PlayerState defenderPlayer)
        {
            attacker.HasAttacked = true;
            if (defender == null)
            {
                int damage = attacker.CurrentAttack;
                defenderPlayer.LifePoints -= damage;
                return damage;
            }

            defender.FaceDown = false;
            int defenderValue = defender.Position == MonsterPosition.Attack ? defender.CurrentAttack : defender.CurrentDefense;
            int difference = attacker.CurrentAttack - defenderValue;
            if (defender.Position == MonsterPosition.Attack)
            {
                if (difference > 0)
                {
                    defenderPlayer.LifePoints -= difference;
                    return difference;
                }

                if (difference < 0)
                {
                    attackerPlayer.LifePoints += difference;
                    return difference;
                }
            }
            else if (difference < 0)
            {
                attackerPlayer.LifePoints += difference;
                return difference;
            }

            return 0;
        }
    }

    public sealed class LuaEffectBridge
    {
        private readonly Dictionary<string, Action<DuelManager, CardInstance, PlayerState>> callbacks =
            new Dictionary<string, Action<DuelManager, CardInstance, PlayerState>>(StringComparer.OrdinalIgnoreCase);

        public void Register(string effectId, Action<DuelManager, CardInstance, PlayerState> callback)
        {
            callbacks[effectId] = callback;
        }

        public bool TryResolve(DuelManager duel, CardInstance source, PlayerState controller)
        {
            if (source == null || string.IsNullOrEmpty(source.Data.EffectId))
            {
                return false;
            }

            Action<DuelManager, CardInstance, PlayerState> callback;
            if (!callbacks.TryGetValue(source.Data.EffectId, out callback))
            {
                return false;
            }

            callback(duel, source, controller);
            return true;
        }
    }

    public sealed class DuelManager
    {
        private readonly System.Random random = new System.Random();
        public readonly DuelState State = new DuelState();
        public readonly PhaseManager PhaseManager = new PhaseManager();
        public readonly TurnManager TurnManager = new TurnManager();
        public readonly ZoneManager ZoneManager = new ZoneManager();
        public readonly CardManager CardManager = new CardManager();
        public readonly ChainSystem ChainSystem = new ChainSystem();
        public readonly BattleSystem BattleSystem = new BattleSystem();
        public readonly LuaEffectBridge Lua = new LuaEffectBridge();

        public DuelManager()
        {
            Lua.Register("draw_two", (duel, source, controller) => duel.Draw(controller, 2, true));
            Lua.Register("boost_team", (duel, source, controller) => duel.BoostTeam(controller, 600));
            Lua.Register("quick_special", (duel, source, controller) => duel.SpecialSummonArchetypeFromHand(controller, source));
            Lua.Register("burn_800", (duel, source, controller) =>
            {
                PlayerState opponent = duel.OpponentOf(controller.Id);
                opponent.LifePoints -= 800;
                duel.Log(opponent.Name + " takes 800 effect damage.");
                duel.CheckWin();
            });
        }

        public PlayerState ActivePlayer
        {
            get { return Player(State.TurnPlayer); }
        }

        public PlayerState InactivePlayer
        {
            get { return OpponentOf(State.TurnPlayer); }
        }

        public void StartNewDuel()
        {
            State.Player = new PlayerState(PlayerId.Player, "Dr, Wheelie");
            State.Opponent = new PlayerState(PlayerId.Opponent, "Messenger of Justice");
            State.Log.Clear();
            State.GameOver = false;
            State.WinnerText = string.Empty;
            State.TurnPlayer = PlayerId.Player;
            State.TurnNumber = 0;
            BuildDecks();
            Shuffle(State.Player.Deck);
            Shuffle(State.Opponent.Deck);
            Draw(State.Player, 5, false);
            Draw(State.Opponent, 5, false);
            Log("Duel start. Both players draw 5 cards.");
            BeginTurn(PlayerId.Player);
        }

        public PlayerState Player(PlayerId id)
        {
            return id == PlayerId.Player ? State.Player : State.Opponent;
        }

        public PlayerState OpponentOf(PlayerId id)
        {
            return id == PlayerId.Player ? State.Opponent : State.Player;
        }

        public void BeginTurn(PlayerId playerId)
        {
            State.TurnNumber++;
            State.TurnPlayer = playerId;
            ActivePlayer.NormalSummonUsed = false;
            ResetAttackFlags(ActivePlayer);
            Log("Turn " + State.TurnNumber + ": " + ActivePlayer.Name);
            EnterPhase(DuelPhase.Draw);
            AutoAdvanceTurnStartToMainPhase();
        }

        private void AutoAdvanceTurnStartToMainPhase()
        {
            if (State.GameOver || State.Phase != DuelPhase.Draw)
            {
                return;
            }

            EnterPhase(DuelPhase.Standby);
            if (!State.GameOver && State.Phase == DuelPhase.Standby)
            {
                EnterPhase(DuelPhase.Main1);
            }
        }

        public void AdvancePhase()
        {
            if (State.GameOver)
            {
                return;
            }

            if (State.Phase == DuelPhase.End)
            {
                CleanupEndPhase(ActivePlayer);
                BeginTurn(TurnManager.Other(State.TurnPlayer));
                return;
            }

            DuelPhase next = PhaseManager.Next(State.Phase, State.TurnNumber);
            if (State.Phase == DuelPhase.Main1 && next == DuelPhase.End)
            {
                Log("First turn: Battle Phase is skipped.");
            }

            EnterPhase(next);
        }

        public void EndTurn()
        {
            if (State.GameOver)
            {
                return;
            }

            if (State.Phase != DuelPhase.End)
            {
                EnterPhase(DuelPhase.End);
            }

            if (!State.GameOver)
            {
                AdvancePhase();
            }
        }

        public void EnterPhase(DuelPhase phase)
        {
            State.Phase = phase;
            Log(PhaseManager.Label(phase));
            if (phase == DuelPhase.Draw)
            {
                if (TurnManager.DrawsForTurn(State.TurnNumber))
                {
                    Draw(ActivePlayer, 1, true);
                }
                else
                {
                    Log("First turn player does not draw.");
                }
            }
        }

        public bool NormalSummon(int handIndex, bool setDefense)
        {
            PlayerState player = ActivePlayer;
            if (!IsHandIndex(player, handIndex) || !PhaseManager.IsMainPhase(State.Phase))
            {
                return false;
            }

            CardInstance card = player.Hand[handIndex];
            if (!CardManager.CanNormalSummon(card, player, State.TurnNumber))
            {
                return false;
            }

            int tributes = CardManager.RequiredTributes(card.Data);
            for (int i = 0; i < tributes; i++)
            {
                TributeFirstMonster(player);
            }

            int zone = ZoneManager.FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                return false;
            }

            player.Hand.RemoveAt(handIndex);
            card.Controller = player.Id;
            card.Location = CardLocation.MonsterZone;
            card.FaceDown = setDefense;
            card.Position = setDefense ? MonsterPosition.DefenseFaceDown : MonsterPosition.Attack;
            card.SummonedTurn = State.TurnNumber;
            player.Monsters[zone] = card;
            player.NormalSummonUsed = true;
            Log(player.Name + (setDefense ? " sets a monster." : " Normal Summons " + card.Data.Name + "."));
            return true;
        }

        public bool SetSpellTrap(int handIndex)
        {
            PlayerState player = ActivePlayer;
            if (!IsHandIndex(player, handIndex) || !PhaseManager.IsMainPhase(State.Phase))
            {
                return false;
            }

            CardInstance card = player.Hand[handIndex];
            if (card.Data.Kind == CardKind.Monster)
            {
                return false;
            }

            int zone = ZoneManager.FirstEmpty(player.SpellsTraps);
            if (zone < 0)
            {
                return false;
            }

            player.Hand.RemoveAt(handIndex);
            card.FaceDown = true;
            card.SetTurn = State.TurnNumber;
            card.Location = CardLocation.SpellTrapZone;
            player.SpellsTraps[zone] = card;
            Log(player.Name + " Sets a Spell/Trap.");
            return true;
        }

        public bool ActivateFromHand(int handIndex)
        {
            PlayerState player = ActivePlayer;
            if (!IsHandIndex(player, handIndex))
            {
                return false;
            }

            CardInstance card = player.Hand[handIndex];
            if (!CardManager.CanActivateFromHand(card, State.Phase, PhaseManager))
            {
                return false;
            }

            bool staysOnField = CardManager.StaysOnField(card.Data);
            int zone = -1;
            if (staysOnField)
            {
                zone = ZoneManager.FirstEmpty(player.SpellsTraps);
                if (zone < 0)
                {
                    return false;
                }
            }

            player.Hand.RemoveAt(handIndex);
            card.FaceDown = false;
            card.Controller = player.Id;
            ChainSystem.Start(card, player.Id);
            Log("Chain Link 1: " + player.Name + " activates " + card.Data.Name + ".");
            if (staysOnField)
            {
                card.Location = CardLocation.SpellTrapZone;
                player.SpellsTraps[zone] = card;
            }

            ResolveSimpleEffect(card, player);
            if (!staysOnField)
            {
                SendToGraveyard(player, card);
            }

            ChainSystem.Clear();
            return true;
        }

        public bool ActivateSetSpellTrap(int zoneIndex)
        {
            PlayerState player = ActivePlayer;
            if (zoneIndex < 0 || zoneIndex >= PlayerState.ZoneCount || !PhaseManager.IsMainPhase(State.Phase) && State.Phase != DuelPhase.Battle)
            {
                return false;
            }

            CardInstance card = player.SpellsTraps[zoneIndex];
            if (!CardManager.CanActivateSet(card, State.Phase, PhaseManager, State.TurnNumber))
            {
                return false;
            }

            bool staysOnField = CardManager.StaysOnField(card.Data);
            card.FaceDown = false;
            ChainSystem.Start(card, player.Id);
            Log("Chain Link 1: " + player.Name + " activates " + card.Data.Name + ".");
            ResolveSimpleEffect(card, player);
            if (!staysOnField)
            {
                player.SpellsTraps[zoneIndex] = null;
                SendToGraveyard(player, card);
            }

            ChainSystem.Clear();
            return true;
        }

        public void ResolveSimpleEffect(CardInstance card, PlayerState controller)
        {
            if (Lua.TryResolve(this, card, controller))
            {
                return;
            }

            if (card.Data.EffectId.StartsWith("search_", StringComparison.OrdinalIgnoreCase))
            {
                CardInstance found = FindInDeck(controller, c => c.Data.Archetype == card.Data.Archetype && !c.Data.IsExtraDeckMonster);
                if (found != null)
                {
                    MoveDeckCardToHand(controller, found);
                    Log(controller.Name + " adds " + found.Data.Name + " from Deck to hand.");
                }
            }
            else if (string.Equals(card.Data.EffectId, "special_blueeyes", StringComparison.OrdinalIgnoreCase))
            {
                SpecialSummonArchetypeFromHand(controller, card);
            }
            else if (string.Equals(card.Data.EffectId, "counter", StringComparison.OrdinalIgnoreCase))
            {
                Log(card.Data.Name + " is ready as chain interaction in this prototype.");
            }
        }

        public void ResolveDeckChoiceFromHand(CardInstance source, CardInstance selected)
        {
            PlayerState player = ActivePlayer;
            if (source == null || selected == null || !player.Hand.Contains(source) || !player.Deck.Contains(selected))
            {
                return;
            }

            player.Hand.Remove(source);
            source.FaceDown = false;
            source.Controller = player.Id;
            ChainSystem.Start(source, player.Id);
            Log("Chain Link 1: " + player.Name + " activates " + source.Data.Name + ".");
            SendToGraveyard(player, source);
            MoveDeckCardToHand(player, selected);
            Log(player.Name + " adds " + selected.Data.Name + " from Deck to hand.");
            ChainSystem.Clear();
        }

        public bool SpecialSummonArchetypeFromHand(PlayerState player, CardInstance source)
        {
            CardInstance target = FindInHand(player, c =>
                c != source &&
                c.Data.IsMonster &&
                c.Data.Archetype == source.Data.Archetype &&
                !c.Data.IsExtraDeckMonster &&
                c.Data.Level >= 7);

            if (target == null)
            {
                target = FindInHand(player, c =>
                    c != source &&
                    c.Data.IsMonster &&
                    c.Data.Archetype == source.Data.Archetype &&
                    !c.Data.IsExtraDeckMonster);
            }

            return target != null && SpecialSummonFromHand(player, target);
        }

        public bool SpecialSummonFromHand(PlayerState player, CardInstance card)
        {
            int zone = ZoneManager.FirstEmpty(player.Monsters);
            if (zone < 0 || card == null || !card.Data.IsMonster)
            {
                return false;
            }

            player.Hand.Remove(card);
            card.Location = CardLocation.MonsterZone;
            card.Controller = player.Id;
            card.FaceDown = false;
            card.Position = MonsterPosition.Attack;
            card.SummonedTurn = State.TurnNumber;
            player.Monsters[zone] = card;
            Log(player.Name + " Special Summons " + card.Data.Name + ".");
            return true;
        }

        public bool CanSummonFromExtra(CardInstance extra, PlayerState player)
        {
            if (extra == null || !extra.Data.IsExtraDeckMonster || ZoneManager.FirstEmpty(player.Monsters) < 0)
            {
                return false;
            }

            return SelectExtraMaterials(player, extra.Data).Count > 0;
        }

        public bool SummonFromExtraDeck(int extraIndex)
        {
            PlayerState player = ActivePlayer;
            if (extraIndex < 0 || extraIndex >= player.ExtraDeck.Count || !PhaseManager.IsMainPhase(State.Phase))
            {
                return false;
            }

            CardInstance extra = player.ExtraDeck[extraIndex];
            List<int> materials = SelectExtraMaterials(player, extra.Data);
            int zone = ZoneManager.FirstEmpty(player.Monsters);
            if (materials.Count == 0 || zone < 0)
            {
                return false;
            }

            materials.Sort();
            for (int i = materials.Count - 1; i >= 0; i--)
            {
                int materialZone = materials[i];
                CardInstance material = player.Monsters[materialZone];
                player.Monsters[materialZone] = null;
                if (extra.Data.Frame == MonsterFrame.Xyz)
                {
                    material.Location = CardLocation.OverlayMaterial;
                    material.FaceDown = false;
                    extra.Materials.Add(material);
                }
                else
                {
                    SendToGraveyard(player, material);
                }
            }

            player.ExtraDeck.RemoveAt(extraIndex);
            extra.Location = CardLocation.MonsterZone;
            extra.Controller = player.Id;
            extra.FaceDown = false;
            extra.Position = MonsterPosition.Attack;
            extra.SummonedTurn = State.TurnNumber;
            player.Monsters[zone] = extra;
            Log(player.Name + " " + ExtraSummonLabel(extra.Data.Frame) + " Summons " + extra.Data.Name + ".");
            return true;
        }

        private List<int> SelectExtraMaterials(PlayerState player, CardData extra)
        {
            switch (extra.Frame)
            {
                case MonsterFrame.Fusion:
                    return FirstFaceUpMaterials(player, 2);
                case MonsterFrame.Synchro:
                    return SelectSynchroMaterials(player, extra.Level);
                case MonsterFrame.Xyz:
                    return SelectXyzMaterials(player, extra.Level);
                case MonsterFrame.Link:
                    return FirstFaceUpMaterials(player, RequiredLinkMaterials(extra));
                default:
                    return new List<int>();
            }
        }

        private List<int> FirstFaceUpMaterials(PlayerState player, int count)
        {
            List<int> materials = new List<int>();
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                CardInstance card = player.Monsters[i];
                if (card != null && !card.FaceDown)
                {
                    materials.Add(i);
                    if (materials.Count == count)
                    {
                        return materials;
                    }
                }
            }

            return new List<int>();
        }

        private List<int> SelectSynchroMaterials(PlayerState player, int targetLevel)
        {
            for (int tuner = 0; tuner < PlayerState.ZoneCount; tuner++)
            {
                CardInstance tunerCard = player.Monsters[tuner];
                if (tunerCard == null || tunerCard.FaceDown || !IsTuner(tunerCard) || tunerCard.Data.Level >= targetLevel)
                {
                    continue;
                }

                List<int> materials = new List<int> { tuner };
                if (TrySelectSynchroNonTuners(player, 0, tuner, targetLevel - tunerCard.Data.Level, materials))
                {
                    return materials;
                }
            }

            return new List<int>();
        }

        private bool TrySelectSynchroNonTuners(PlayerState player, int startZone, int tunerZone, int remainingLevel, List<int> materials)
        {
            if (remainingLevel == 0)
            {
                return materials.Count > 1;
            }

            for (int zone = startZone; zone < PlayerState.ZoneCount; zone++)
            {
                CardInstance card = player.Monsters[zone];
                if (zone == tunerZone || !IsFaceUpNonTuner(card) || card.Data.Level > remainingLevel)
                {
                    continue;
                }

                materials.Add(zone);
                if (TrySelectSynchroNonTuners(player, zone + 1, tunerZone, remainingLevel - card.Data.Level, materials))
                {
                    return true;
                }

                materials.RemoveAt(materials.Count - 1);
            }

            return false;
        }

        private List<int> SelectXyzMaterials(PlayerState player, int rank)
        {
            for (int first = 0; first < PlayerState.ZoneCount; first++)
            {
                CardInstance firstCard = player.Monsters[first];
                if (firstCard == null || firstCard.FaceDown || firstCard.Data.Level != rank)
                {
                    continue;
                }

                for (int second = first + 1; second < PlayerState.ZoneCount; second++)
                {
                    CardInstance secondCard = player.Monsters[second];
                    if (secondCard != null && !secondCard.FaceDown && secondCard.Data.Level == rank)
                    {
                        return new List<int> { first, second };
                    }
                }
            }

            return new List<int>();
        }

        private bool IsTuner(CardInstance card)
        {
            return card != null && card.Data.TypeLine.IndexOf("Tuner", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsFaceUpNonTuner(CardInstance card)
        {
            return card != null && !card.FaceDown && !IsTuner(card);
        }

        private int RequiredLinkMaterials(CardData data)
        {
            if (data.Text.StartsWith("1 ", StringComparison.OrdinalIgnoreCase))
            {
                return 1;
            }

            if (data.Text.StartsWith("3 ", StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            return 2;
        }

        private string ExtraSummonLabel(MonsterFrame frame)
        {
            switch (frame)
            {
                case MonsterFrame.Fusion:
                    return "Fusion";
                case MonsterFrame.Synchro:
                    return "Synchro";
                case MonsterFrame.Xyz:
                    return "Xyz";
                case MonsterFrame.Link:
                    return "Link";
                default:
                    return "Special";
            }
        }

        public int Attack(int attackerZone, int defenderZone)
        {
            CardInstance attacker = ActivePlayer.Monsters[attackerZone];
            if (!BattleSystem.CanAttack(attacker, State.Phase, State.TurnNumber))
            {
                return 0;
            }

            CardInstance defender = defenderZone >= 0 ? InactivePlayer.Monsters[defenderZone] : null;
            int damage = BattleSystem.ResolveAttack(attacker, defender, ActivePlayer, InactivePlayer);
            Log(defender == null
                ? attacker.Data.Name + " declares a direct attack."
                : attacker.Data.Name + " attacks " + defender.PublicName + ".");
            ResolveBattleDestruction(attackerZone, defenderZone, attacker, defender);
            CheckWin();
            return damage;
        }

        public void Draw(PlayerState player, int count, bool mandatory)
        {
            for (int i = 0; i < count; i++)
            {
                if (player.Deck.Count == 0)
                {
                    if (mandatory)
                    {
                        SetWinner(OpponentOf(player.Id), player.Name + " cannot draw.");
                    }

                    return;
                }

                CardInstance card = player.Deck[0];
                player.Deck.RemoveAt(0);
                card.Location = CardLocation.Hand;
                player.Hand.Add(card);
            }
        }

        public void BoostTeam(PlayerState player, int amount)
        {
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                CardInstance monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown)
                {
                    monster.TemporaryAttack += amount;
                }
            }

            Log(player.Name + "'s monsters gain " + amount + " ATK this turn.");
        }

        public void Log(string message)
        {
            State.Log.Add(message);
            if (State.Log.Count > 80)
            {
                State.Log.RemoveAt(0);
            }
        }

        public void CheckWin()
        {
            if (State.Player.LifePoints <= 0)
            {
                SetWinner(State.Opponent, State.Opponent.Name + " wins.");
            }
            else if (State.Opponent.LifePoints <= 0)
            {
                SetWinner(State.Player, State.Player.Name + " wins.");
            }
        }

        private void BuildDecks()
        {
            List<CardData> blueEyes = SampleCards.BlueEyesMainDeck();
            List<CardData> sforce = SampleCards.SForceMainDeck();
            AddDeck(State.Player, blueEyes, PlayerId.Player);
            AddDeck(State.Opponent, sforce, PlayerId.Opponent);
            AddExtra(State.Player, SampleCards.BlueEyesExtraDeck(), PlayerId.Player);
            AddExtra(State.Opponent, SampleCards.SForceExtraDeck(), PlayerId.Opponent);
        }

        private void AddDeck(PlayerState player, List<CardData> cards, PlayerId owner)
        {
            List<CardData> mainDeckCards = cards.FindAll(card => card != null && !card.IsExtraDeckMonster);
            if (mainDeckCards.Count == 0)
            {
                throw new InvalidOperationException("A Main Deck requires at least one non-Extra Deck card.");
            }

            int index = 0;
            while (player.Deck.Count < PlayerState.MainDeckSize)
            {
                player.Deck.Add(new CardInstance(mainDeckCards[index % mainDeckCards.Count], owner));
                index++;
            }
        }

        private void AddExtra(PlayerState player, List<CardData> cards, PlayerId owner)
        {
            List<CardData> extraDeckCards = cards.FindAll(card => card != null && card.IsExtraDeckMonster);
            if (extraDeckCards.Count == 0)
            {
                throw new InvalidOperationException("An Extra Deck requires at least one Fusion, Synchro, Xyz, or Link monster.");
            }

            int index = 0;
            while (player.ExtraDeck.Count < PlayerState.ExtraDeckSize)
            {
                CardInstance card = new CardInstance(extraDeckCards[index % extraDeckCards.Count], owner);
                card.Location = CardLocation.ExtraDeck;
                player.ExtraDeck.Add(card);
                index++;
            }
        }

        private void Shuffle(List<CardInstance> cards)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int swap = random.Next(i + 1);
                CardInstance current = cards[i];
                cards[i] = cards[swap];
                cards[swap] = current;
            }
        }

        private void CleanupEndPhase(PlayerState player)
        {
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null)
                {
                    player.Monsters[i].TemporaryAttack = 0;
                }
            }
        }

        private void ResetAttackFlags(PlayerState player)
        {
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null)
                {
                    player.Monsters[i].HasAttacked = false;
                }
            }
        }

        private void ResolveBattleDestruction(int attackerZone, int defenderZone, CardInstance attacker, CardInstance defender)
        {
            if (defender == null)
            {
                return;
            }

            int defenderValue = defender.Position == MonsterPosition.Attack ? defender.CurrentAttack : defender.CurrentDefense;
            if (attacker.CurrentAttack > defenderValue)
            {
                DestroyMonster(InactivePlayer, defenderZone);
            }
            else if (defender.Position == MonsterPosition.Attack && attacker.CurrentAttack < defender.CurrentAttack)
            {
                DestroyMonster(ActivePlayer, attackerZone);
            }
            else if (defender.Position == MonsterPosition.Attack && attacker.CurrentAttack == defender.CurrentAttack && attacker.CurrentAttack > 0)
            {
                DestroyMonster(ActivePlayer, attackerZone);
                DestroyMonster(InactivePlayer, defenderZone);
            }
        }

        private void DestroyMonster(PlayerState player, int zone)
        {
            CardInstance monster = player.Monsters[zone];
            if (monster == null)
            {
                return;
            }

            player.Monsters[zone] = null;
            SendToGraveyard(player, monster);
            Log(monster.Data.Name + " is sent to the Graveyard.");
        }

        private void SendToGraveyard(PlayerState owner, CardInstance card)
        {
            card.Location = CardLocation.Graveyard;
            card.FaceDown = false;
            card.HasAttacked = false;
            owner.Graveyard.Add(card);
        }

        private void TributeFirstMonster(PlayerState player)
        {
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null)
                {
                    CardInstance tribute = player.Monsters[i];
                    player.Monsters[i] = null;
                    SendToGraveyard(player, tribute);
                    Log(player.Name + " Tributes " + tribute.Data.Name + ".");
                    return;
                }
            }
        }

        private CardInstance FindInDeck(PlayerState player, Predicate<CardInstance> predicate)
        {
            for (int i = 0; i < player.Deck.Count; i++)
            {
                if (predicate(player.Deck[i]))
                {
                    return player.Deck[i];
                }
            }

            return null;
        }

        private CardInstance FindInHand(PlayerState player, Predicate<CardInstance> predicate)
        {
            for (int i = 0; i < player.Hand.Count; i++)
            {
                if (predicate(player.Hand[i]))
                {
                    return player.Hand[i];
                }
            }

            return null;
        }

        private void MoveDeckCardToHand(PlayerState player, CardInstance card)
        {
            player.Deck.Remove(card);
            card.Location = CardLocation.Hand;
            player.Hand.Add(card);
            Shuffle(player.Deck);
        }

        private bool IsHandIndex(PlayerState player, int handIndex)
        {
            return handIndex >= 0 && handIndex < player.Hand.Count;
        }

        private void SetWinner(PlayerState winner, string reason)
        {
            State.GameOver = true;
            State.WinnerText = winner.Name + " WINS";
            Log(reason);
        }
    }

    public static class SampleCards
    {
        public static List<CardData> BlueEyesMainDeck()
        {
            return new List<CardData>
            {
                Monster(8240199, "Sage with Eyes of Blue", MonsterFrame.Effect, 1, 0, 1500, "Spellcaster / Tuner / Effect", "When this card is Normal Summoned: add 1 Blue-Eyes monster from your Deck to your hand.", "Blue-Eyes", "search_blueeyes"),
                Monster(36734924, "Priestess with Eyes of Blue", MonsterFrame.Effect, 1, 0, 0, "Spellcaster / Tuner / Effect", "Quick Effect: send an Effect Monster you control to the GY; add Blue-Eyes cards from your Deck to your hand.", "Blue-Eyes", "search_blueeyes"),
                Monster(89631139, "Blue-Eyes White Dragon", MonsterFrame.Normal, 8, 3000, 2500, "Dragon / Normal", "This legendary dragon is a powerful engine piece for Fusion and Synchro plays.", "Blue-Eyes", ""),
                Monster(30576089, "Blue-Eyes Jet Dragon", MonsterFrame.Effect, 8, 3000, 0, "Dragon / Effect", "If a card is destroyed: Special Summon this card. Your other cards are protected from destruction.", "Blue-Eyes", "special_blueeyes"),
                Monster(66961194, "Dictator of D.", MonsterFrame.Effect, 4, 1200, 1100, "Spellcaster / Effect", "Send a Blue-Eyes monster from Deck to GY; Special Summon this card from your hand.", "Blue-Eyes", "special_blueeyes"),
                Spell(71143015, "Ultimate Fusion", "Quick-Play Spell", "Fusion Summon using monsters from your hand or field. Prototype: Special Summon a Blue-Eyes monster from hand.", "Blue-Eyes", "quick_special"),
                Spell(41620959, "Dragon Shrine", "Normal Spell", "Send a Dragon monster from Deck to GY, then set up a Blue-Eyes line.", "Blue-Eyes", "search_blueeyes"),
                Spell(48800175, "The Melody of Awakening Dragon", "Normal Spell", "Discard 1 card; add high-Level Dragons from your Deck to your hand.", "Blue-Eyes", "search_blueeyes"),
                Spell(38120068, "Trade-In", "Normal Spell", "Discard 1 Level 8 monster; draw 2 cards.", "Blue-Eyes", "draw_two"),
                Spell(17655904, "Burst Stream of Destruction", "Normal Spell", "Your monsters gain ATK for this turn.", "Blue-Eyes", "boost_team"),
                Trap(62089826, "True Light", "Continuous Trap", "Special Summon a Blue-Eyes White Dragon or Set support from Deck.", "Blue-Eyes", "special_blueeyes"),
                Trap(56920308, "The Ultimate Creature of Destruction", "Normal Trap", "Negate an attack or protect a Blue-Eyes monster.", "Blue-Eyes", "counter"),
                Monster(88241506, "Maiden with Eyes of Blue", MonsterFrame.Effect, 1, 0, 0, "Spellcaster / Tuner / Effect", "When targeted for an attack or effect: Special Summon a Blue-Eyes monster.", "Blue-Eyes", "special_blueeyes"),
                Monster(48048590, "Keeper of Dragon Magic", MonsterFrame.Effect, 4, 1800, 1300, "Dragon / Effect", "Searches Fusion cards and helps setup Extra Deck plays.", "Blue-Eyes", "search_blueeyes")
            };
        }

        public static List<CardData> BlueEyesExtraDeck()
        {
            return new List<CardData>
            {
                Monster(59822133, "Blue-Eyes Spirit Dragon", MonsterFrame.Synchro, 9, 2500, 3000, "Dragon / Synchro / Effect", "1 Tuner + 1+ non-Tuner Blue-Eyes monsters. Can negate GY effects.", "Blue-Eyes", "counter"),
                Monster(89604813, "Blue-Eyes Ultimate Spirit Dragon", MonsterFrame.Synchro, 12, 3500, 4000, "Dragon / Synchro / Effect", "A large Synchro boss monster for the prototype summon animation.", "Blue-Eyes", ""),
                Monster(2129638, "Blue-Eyes Twin Burst Dragon", MonsterFrame.Fusion, 10, 3000, 2500, "Dragon / Fusion / Effect", "2 Blue-Eyes White Dragon. Can make multiple attacks.", "Blue-Eyes", ""),
                Monster(63767246, "Number 38: Hope Harbinger Dragon Titanic Galaxy", MonsterFrame.Xyz, 8, 3000, 2500, "Dragon / Xyz / Effect", "2 Level 8 monsters. Negates Spell effects.", "Blue-Eyes", "counter"),
                Monster(24361622, "Hieratic Seal of the Heavenly Spheres", MonsterFrame.Link, 0, 0, 0, "Dragon / Link / Effect", "2 Dragon monsters. Bounces cards and summons Dragons.", "Blue-Eyes", "special_blueeyes")
            };
        }

        public static List<CardData> SForceMainDeck()
        {
            return new List<CardData>
            {
                Monster(3001, "S-Force Rappa Chiyomaru", MonsterFrame.Effect, 2, 800, 1000, "Warrior / Effect", "Banish an S-Force card from hand; Special Summon an S-Force from Deck.", "S-Force", "search_sforce"),
                Monster(3002, "S-Force Lapcewell", MonsterFrame.Effect, 6, 2400, 2500, "Fiend / Effect", "If Special Summoned: target monsters based on S-Force columns.", "S-Force", "burn_800"),
                Monster(3003, "S-Force Professor DiGamma", MonsterFrame.Effect, 3, 1000, 1500, "Psychic / Effect", "Column based disruption.", "S-Force", "boost_team"),
                Monster(3004, "S-Force Orrafist", MonsterFrame.Effect, 4, 1800, 1000, "Cyberse / Effect", "Banish an opponent's monster when it targets an S-Force.", "S-Force", "counter"),
                Spell(3101, "S-Force Bridgehead", "Field Spell", "When activated: add 1 S-Force monster from Deck to hand.", "S-Force", "search_sforce"),
                Spell(3102, "S-Force Showdown", "Quick-Play Spell", "Special Summon or return S-Force cards.", "S-Force", "quick_special"),
                Trap(3201, "S-Force Chase", "Normal Trap", "Return opponent monsters in the same columns as your S-Force monsters.", "S-Force", "counter"),
                Trap(3202, "S-Force Specimen", "Normal Trap", "Special Summon a banished S-Force monster.", "S-Force", "quick_special"),
                Spell(3103, "Reinforcement Command", "Normal Spell", "Add a Warrior monster from Deck to hand.", "S-Force", "search_sforce"),
                Monster(3005, "S-Force Dog Tag", MonsterFrame.Effect, 5, 1600, 2100, "Machine / Effect", "Special Summons itself when opponent summons.", "S-Force", "special_blueeyes"),
                Monster(3006, "S-Force Gravitino", MonsterFrame.Effect, 5, 2000, 1400, "Psychic / Effect", "Adds an S-Force card and pressures columns.", "S-Force", "search_sforce"),
                Spell(3104, "Dimensional Pursuit", "Normal Spell", "Deal effect damage and move cards between zones.", "S-Force", "burn_800"),
                Trap(3203, "Column Lock", "Continuous Trap", "Locks opponent actions in occupied columns.", "S-Force", "boost_team"),
                Monster(3007, "S-Force Pla-Tina", MonsterFrame.Effect, 6, 2200, 2000, "Spellcaster / Effect", "Special Summons banished S-Force monsters.", "S-Force", "quick_special")
            };
        }

        public static List<CardData> SForceExtraDeck()
        {
            return new List<CardData>
            {
                Monster(4001, "S-Force Nightchaser", MonsterFrame.Link, 0, 800, 0, "Cyberse / Link / Effect", "1 non-Link S-Force monster. Adds S-Force follow-up.", "S-Force", "search_sforce"),
                Monster(4002, "S-Force Justify", MonsterFrame.Link, 0, 2600, 0, "Cyberse / Link / Effect", "3 Effect Monsters. Column-based boss monster.", "S-Force", "counter"),
                Monster(4003, "Number 41: Bagooska", MonsterFrame.Xyz, 4, 2100, 2000, "Fiend / Xyz / Effect", "2 Level 4 monsters. Slows the game.", "S-Force", "counter"),
                Monster(4004, "PSY-Framelord Omega", MonsterFrame.Synchro, 8, 2800, 2200, "Psychic / Synchro / Effect", "A Synchro utility monster.", "S-Force", "counter"),
                Monster(4005, "Guardian Chimera", MonsterFrame.Fusion, 9, 3300, 3300, "Beast / Fusion / Effect", "Fusion boss used as Extra Deck reference.", "S-Force", "draw_two")
            };
        }

        private static CardData Monster(int id, string name, MonsterFrame frame, int level, int attack, int defense, string typeLine, string text, string archetype, string effectId)
        {
            return new CardData(id, name, CardKind.Monster, frame, level, attack, defense, typeLine, text, archetype, effectId);
        }

        private static CardData Spell(int id, string name, string typeLine, string text, string archetype, string effectId)
        {
            return new CardData(id, name, CardKind.Spell, MonsterFrame.Normal, 0, 0, 0, typeLine, text, archetype, effectId);
        }

        private static CardData Trap(int id, string name, string typeLine, string text, string archetype, string effectId)
        {
            return new CardData(id, name, CardKind.Trap, MonsterFrame.Normal, 0, 0, 0, typeLine, text, archetype, effectId);
        }
    }

    public sealed class DuelUIManager
    {
        public string EmptyInspectorText()
        {
            return "Select a card to inspect its type, stats and effect text.\n\nThe new core is built around GameState, zones, phases, chains, effects and a Lua bridge.";
        }

        public string InspectorText(CardInstance card)
        {
            if (card == null)
            {
                return EmptyInspectorText();
            }

            if (card.FaceDown)
            {
                return "Face-down card.";
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine(card.Data.TypeLine);
            builder.AppendLine(card.Data.IsExtraDeckMonster
                ? "Origin: Extra Deck (" + ExtraDeckType(card.Data.Frame) + ")."
                : "Origin: Main Deck.");
            if (card.Data.IsMonster)
            {
                builder.AppendLine("Level/Rank/Link " + card.Data.Level + "   ATK " + card.CurrentAttack + " / DEF " + card.CurrentDefense);
                if (card.Data.Frame == MonsterFrame.Synchro)
                {
                    builder.AppendLine("Synchro rule: exactly 1 face-up Tuner + 1 or more face-up non-Tuners; their Levels must exactly total " + card.Data.Level + ".");
                }

                if (card.Materials.Count > 0)
                {
                    builder.AppendLine("Materials attached: " + card.Materials.Count);
                }
            }

            builder.AppendLine("Lua hook: c" + card.Data.Id + ".lua / " + (string.IsNullOrEmpty(card.Data.EffectId) ? "no scripted effect" : card.Data.EffectId));
            builder.AppendLine();
            builder.Append(card.Data.Text);
            return builder.ToString();
        }

        private string ExtraDeckType(MonsterFrame frame)
        {
            switch (frame)
            {
                case MonsterFrame.Fusion:
                    return "Fusion";
                case MonsterFrame.Synchro:
                    return "Synchro";
                case MonsterFrame.Xyz:
                    return "Xyz";
                case MonsterFrame.Link:
                    return "Link";
                default:
                    return "Unknown";
            }
        }

        public Color CardColor(CardInstance card)
        {
            if (card == null || card.FaceDown)
            {
                return new Color(0.1f, 0.14f, 0.25f, 1f);
            }

            if (card.Data.Kind == CardKind.Spell)
            {
                return new Color(0.08f, 0.42f, 0.34f, 1f);
            }

            if (card.Data.Kind == CardKind.Trap)
            {
                return new Color(0.45f, 0.12f, 0.38f, 1f);
            }

            if (card.Data.IsExtraDeckMonster)
            {
                switch (card.Data.Frame)
                {
                    case MonsterFrame.Fusion:
                        return new Color(0.48f, 0.24f, 0.62f, 1f);
                    case MonsterFrame.Synchro:
                        return new Color(0.82f, 0.84f, 0.86f, 1f);
                    case MonsterFrame.Xyz:
                        return new Color(0.08f, 0.08f, 0.1f, 1f);
                    case MonsterFrame.Link:
                        return new Color(0.08f, 0.28f, 0.62f, 1f);
                }
            }

            return new Color(0.58f, 0.3f, 0.12f, 1f);
        }
    }

    public sealed class MasterDuelRuntime : MonoBehaviour
    {
        private const float UiUnitsPerCentimeter = 37.8f;
        private const float CardAspectRatio = 813f / 1185f;
        private const float HandCardHeight = 6.8f * UiUnitsPerCentimeter;
        private const float HandCardWidth = HandCardHeight * CardAspectRatio;
        private const float HandRowHeight = HandCardHeight + 12f;
        private const float BottomBarHeight = HandRowHeight + 82f;
        private const float ChoiceCardHeight = 136f;
        private const float ChoiceCardWidth = ChoiceCardHeight * CardAspectRatio;

        private readonly DuelManager duel = new DuelManager();
        private readonly DuelUIManager duelUi = new DuelUIManager();
        private readonly Dictionary<Collider, ZoneClickTarget> targets = new Dictionary<Collider, ZoneClickTarget>();
        private readonly Dictionary<int, Sprite> artworkSprites = new Dictionary<int, Sprite>();

        private Font font;
        private CardArtworkCatalog artworkCatalog;
        private Camera arenaCamera;
        private Transform arenaRoot;
        private Canvas canvas;
        private Text bannerText;
        private Text phaseText;
        private Text playerLpText;
        private Text opponentLpText;
        private Text inspectorTitle;
        private Text inspectorBody;
        private Text logText;
        private RectTransform handRow;
        private RectTransform actionRow;
        private RectTransform choicePanel;
        private BoardSlot[] playerMonsterSlots;
        private BoardSlot[] playerSpellSlots;
        private BoardSlot[] opponentMonsterSlots;
        private BoardSlot[] opponentSpellSlots;
        private DeckPileVisual playerMainDeckPile;
        private DeckPileVisual playerExtraDeckPile;
        private DeckPileVisual opponentMainDeckPile;
        private DeckPileVisual opponentExtraDeckPile;

        private int selectedHand = -1;
        private int selectedMonster = -1;
        private int selectedSpellTrap = -1;
        private bool opponentRoutineRunning;
        private bool playerAutoPhaseRunning;
        private bool awaitingChoice;

        private sealed class BoardSlot
        {
            public Renderer Plate;
            public Renderer Card;
            public TextMesh Label;
            public PlayerId Owner;
            public bool Monster;
            public int Index;
        }

        private sealed class ZoneClickTarget
        {
            public PlayerId Owner;
            public bool Monster;
            public int Index;
        }

        private sealed class DeckPileVisual
        {
            public Transform Root;
            public Renderer Stack;
            public TextMesh Label;
            public PlayerId Owner;
            public bool ExtraDeck;
        }

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            artworkCatalog = Resources.Load<CardArtworkCatalog>("BlueEyesArtworkCatalog");
            EnsureEventSystem();
            Build3DArena();
            BuildUI();
            duel.StartNewDuel();
            RefreshAll();
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            bool pointerPressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            Vector2 pointerPosition = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
#else
            bool pointerPressed = Input.GetMouseButtonDown(0);
            Vector2 pointerPosition = Input.mousePosition;
#endif
            if (arenaCamera == null || !pointerPressed || EventSystem.current == null || EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = arenaCamera.ScreenPointToRay(pointerPosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f))
            {
                ZoneClickTarget target;
                if (targets.TryGetValue(hit.collider, out target))
                {
                    HandleZoneClick(target);
                }
            }
        }

        private void Build3DArena()
        {
            arenaRoot = new GameObject("Master Duel Arena").transform;
            arenaRoot.SetParent(transform, false);
            arenaCamera = Camera.main;
            if (arenaCamera == null)
            {
                arenaCamera = new GameObject("Arena Camera", typeof(Camera)).GetComponent<Camera>();
            }

            arenaCamera.transform.position = new Vector3(0f, 7.2f, -8.2f);
            arenaCamera.transform.LookAt(new Vector3(0f, 0f, 0.15f));
            arenaCamera.fieldOfView = 47f;
            arenaCamera.backgroundColor = new Color(0.02f, 0.02f, 0.035f, 1f);
            arenaCamera.clearFlags = CameraClearFlags.SolidColor;

            Light mainLight = new GameObject("Arena Key Light", typeof(Light)).GetComponent<Light>();
            mainLight.transform.SetParent(arenaRoot, false);
            mainLight.transform.rotation = Quaternion.Euler(52f, -28f, 0f);
            mainLight.type = LightType.Directional;
            mainLight.intensity = 1.25f;

            CreateBox("Table", Vector3.zero, new Vector3(14.6f, 0.16f, 8.0f), new Color(0.02f, 0.28f, 0.22f, 1f));
            CreateBox("Center Rail", new Vector3(0f, 0.12f, 0f), new Vector3(13.8f, 0.08f, 0.16f), new Color(0.95f, 0.55f, 0.12f, 1f));
            CreateBox("Player Neon Rail", new Vector3(0f, 0.14f, -3.82f), new Vector3(14.4f, 0.12f, 0.22f), new Color(0.62f, 0.08f, 0.9f, 1f));
            CreateBox("Opponent Neon Rail", new Vector3(0f, 0.14f, 3.82f), new Vector3(14.4f, 0.12f, 0.22f), new Color(0.95f, 0.24f, 0.08f, 1f));
            playerExtraDeckPile = CreateDeckPile(PlayerId.Player, true, -6.25f, -2.62f);
            playerMainDeckPile = CreateDeckPile(PlayerId.Player, false, 6.25f, -2.62f);
            opponentMainDeckPile = CreateDeckPile(PlayerId.Opponent, false, -6.25f, 2.62f);
            opponentExtraDeckPile = CreateDeckPile(PlayerId.Opponent, true, 6.25f, 2.62f);

            playerMonsterSlots = new BoardSlot[PlayerState.ZoneCount];
            playerSpellSlots = new BoardSlot[PlayerState.ZoneCount];
            opponentMonsterSlots = new BoardSlot[PlayerState.ZoneCount];
            opponentSpellSlots = new BoardSlot[PlayerState.ZoneCount];
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                float x = -4.8f + i * 2.4f;
                opponentMonsterSlots[i] = CreateSlot(PlayerId.Opponent, true, i, x, 1.18f);
                opponentSpellSlots[i] = CreateSlot(PlayerId.Opponent, false, i, x, 2.62f);
                playerMonsterSlots[i] = CreateSlot(PlayerId.Player, true, i, x, -1.18f);
                playerSpellSlots[i] = CreateSlot(PlayerId.Player, false, i, x, -2.62f);
            }
        }

        private void BuildUI()
        {
            GameObject canvasObject = new GameObject("Master Duel UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = canvasObject.GetComponent<RectTransform>();
            Stretch(root);

            RectTransform top = Panel("Top HUD", root, new Color(0.008f, 0.012f, 0.02f, 0.78f));
            Anchor(top, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -104f), new Vector2(-24f, -20f));
            AddHorizontal(top.gameObject, 18, 18, 0, 0, 18f, TextAnchor.MiddleCenter);
            playerLpText = Label("Player LP", top, 28, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            AddLayout(playerLpText.gameObject, 420f, 70f, 0f, 0f);
            phaseText = Label("Phase", top, 31, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.45f, 1f));
            AddLayout(phaseText.gameObject, -1f, 70f, 1f, 0f);
            opponentLpText = Label("Opponent LP", top, 28, FontStyle.Bold, TextAnchor.MiddleRight, Color.white);
            AddLayout(opponentLpText.gameObject, 420f, 70f, 0f, 0f);

            RectTransform inspector = Panel("Card Inspector", root, new Color(0.015f, 0.022f, 0.035f, 0.84f));
            Anchor(inspector, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(20f, BottomBarHeight + 30f), new Vector2(400f, -122f));
            AddVertical(inspector.gameObject, 14, 14, 14, 14, 7f);
            inspectorTitle = Label("Inspector Title", inspector, 24, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            AddLayout(inspectorTitle.gameObject, -1f, 46f, 0f, 0f);
            inspectorBody = Label("Inspector Body", inspector, 18, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.9f, 0.96f, 1f, 1f));
            AddLayout(inspectorBody.gameObject, -1f, -1f, 1f, 1f);

            RectTransform bottom = Panel("Bottom Bar", root, new Color(0.008f, 0.01f, 0.016f, 0.88f));
            Anchor(bottom, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(24f, 14f), new Vector2(-24f, BottomBarHeight));
            AddVertical(bottom.gameObject, 18, 18, 10, 12, 8f);
            handRow = Panel("Hand", bottom, new Color(0f, 0f, 0f, 0f));
            AddHorizontal(handRow.gameObject, 0, 0, 0, 0, 10f, TextAnchor.MiddleCenter);
            AddLayout(handRow.gameObject, -1f, HandRowHeight, 1f, 0f);
            actionRow = Panel("Actions", bottom, new Color(0f, 0f, 0f, 0f));
            AddHorizontal(actionRow.gameObject, 0, 0, 0, 0, 8f, TextAnchor.MiddleCenter);
            AddLayout(actionRow.gameObject, -1f, 54f, 0f, 0f);

            RectTransform logPanel = Panel("Log", root, new Color(0.012f, 0.017f, 0.028f, 0.80f));
            Anchor(logPanel, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-402f, BottomBarHeight + 30f), new Vector2(-20f, -122f));
            AddVertical(logPanel.gameObject, 14, 14, 14, 14, 6f);
            logText = Label("Log Text", logPanel, 15, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.8f, 0.9f, 1f, 1f));
            AddLayout(logText.gameObject, -1f, -1f, 1f, 1f);

            bannerText = Label("Banner", root, 48, FontStyle.BoldAndItalic, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.65f, 0.50f));
            RectTransform bannerRect = bannerText.GetComponent<RectTransform>();
            Anchor(bannerRect, new Vector2(0.24f, 0.48f), new Vector2(0.76f, 0.60f), Vector2.zero, Vector2.zero);
            bannerText.raycastTarget = false;

            choicePanel = Panel("Choice Panel", root, new Color(0.01f, 0.015f, 0.025f, 0.94f));
            Anchor(choicePanel, new Vector2(0.24f, 0.36f), new Vector2(0.76f, 0.64f), Vector2.zero, Vector2.zero);
            AddVertical(choicePanel.gameObject, 16, 16, 14, 14, 10f);
            choicePanel.gameObject.SetActive(false);
        }

        private void RefreshAll()
        {
            playerLpText.text = "LP " + Mathf.Max(0, duel.State.Player.LifePoints) + "   " + duel.State.Player.Name;
            opponentLpText.text = duel.State.Opponent.Name + "   LP " + Mathf.Max(0, duel.State.Opponent.LifePoints);
            phaseText.text = "TURN " + duel.State.TurnNumber + " | " + duel.PhaseManager.Label(duel.State.Phase);
            bannerText.text = duel.State.GameOver ? duel.State.WinnerText : duel.PhaseManager.Label(duel.State.Phase);
            Refresh3D();
            RefreshHand();
            RefreshActions();
            RefreshInspector();
            RefreshLog();

            if (duel.State.TurnPlayer == PlayerId.Opponent && !opponentRoutineRunning && !duel.State.GameOver)
            {
                StartCoroutine(OpponentRoutine());
            }
            else if (duel.State.TurnPlayer == PlayerId.Player && !playerAutoPhaseRunning && IsAutoPlayerPhase())
            {
                StartCoroutine(PlayerAutoPhaseRoutine());
            }
        }

        private void Refresh3D()
        {
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                RefreshSlot(playerMonsterSlots[i], duel.State.Player.Monsters[i]);
                RefreshSlot(playerSpellSlots[i], duel.State.Player.SpellsTraps[i]);
                RefreshSlot(opponentMonsterSlots[i], duel.State.Opponent.Monsters[i]);
                RefreshSlot(opponentSpellSlots[i], duel.State.Opponent.SpellsTraps[i]);
            }

            RefreshDeckPile(playerMainDeckPile, duel.State.Player);
            RefreshDeckPile(playerExtraDeckPile, duel.State.Player);
            RefreshDeckPile(opponentMainDeckPile, duel.State.Opponent);
            RefreshDeckPile(opponentExtraDeckPile, duel.State.Opponent);
        }

        private void RefreshSlot(BoardSlot slot, CardInstance card)
        {
            bool hasCard = card != null;
            slot.Card.gameObject.SetActive(hasCard);
            slot.Label.gameObject.SetActive(hasCard);
            if (!hasCard)
            {
                SetColor(slot.Plate, slot.Owner == PlayerId.Player ? new Color(0.08f, 0.45f, 0.55f, 1f) : new Color(0.58f, 0.14f, 0.09f, 1f));
                return;
            }

            Color color = CardColor(card);
            bool selected = slot.Owner == PlayerId.Player &&
                            ((slot.Monster && selectedMonster == slot.Index) || (!slot.Monster && selectedSpellTrap == slot.Index));
            if (selected)
            {
                color = new Color(1f, 0.75f, 0.18f, 1f);
            }

            SetCardAppearance(slot.Card, card, color, selected);
            slot.Card.transform.localRotation = card.Data.IsMonster && card.Position != MonsterPosition.Attack
                ? Quaternion.Euler(0f, 90f, 0f)
                : Quaternion.identity;
            slot.Label.text = ShortName(card.PublicName) + (card.Data.IsMonster && !card.FaceDown ? "\n" + card.CurrentAttack + "/" + card.CurrentDefense : string.Empty);
            FaceToCamera(slot.Label.transform);
        }

        private void RefreshHand()
        {
            ClearChildren(handRow);
            for (int i = 0; i < duel.State.Player.Hand.Count; i++)
            {
                int index = i;
                CardInstance card = duel.State.Player.Hand[i];
                Button button = CardButton(card, handRow, selectedHand == i, true);
                button.onClick.AddListener(() =>
                {
                    selectedHand = selectedHand == index ? -1 : index;
                    selectedMonster = -1;
                    selectedSpellTrap = -1;
                    RefreshAll();
                });
            }
        }

        private void RefreshActions()
        {
            ClearChildren(actionRow);
            if (duel.State.GameOver)
            {
                AddButton("New Duel", actionRow, Restart, new Color(0.45f, 0.16f, 0.2f, 1f), 140f);
                return;
            }

            if (duel.State.TurnPlayer != PlayerId.Player)
            {
                Text wait = Label("Wait", actionRow, 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
                wait.text = "Opponent turn";
                AddLayout(wait.gameObject, -1f, 46f, 1f, 0f);
                return;
            }

            if (selectedHand >= 0 && selectedHand < duel.State.Player.Hand.Count)
            {
                CardInstance card = duel.State.Player.Hand[selectedHand];
                if (card.Data.Kind == CardKind.Monster)
                {
                    if (duel.PhaseManager.IsMainPhase(duel.State.Phase) && duel.CardManager.CanNormalSummon(card, duel.State.Player, duel.State.TurnNumber))
                    {
                        AddButton("Summon", actionRow, () => { duel.NormalSummon(selectedHand, false); ClearSelection(); RefreshAll(); }, new Color(0.17f, 0.42f, 0.25f, 1f), 120f);
                        AddButton("Set DEF", actionRow, () => { duel.NormalSummon(selectedHand, true); ClearSelection(); RefreshAll(); }, new Color(0.15f, 0.32f, 0.48f, 1f), 120f);
                    }
                }
                else
                {
                    if (duel.CardManager.CanActivateFromHand(card, duel.State.Phase, duel.PhaseManager))
                    {
                        AddButton("Activate", actionRow, ActivateSelectedHandCard, new Color(0.44f, 0.32f, 0.12f, 1f), 130f);
                    }

                    if (duel.PhaseManager.IsMainPhase(duel.State.Phase))
                    {
                        AddButton("Set", actionRow, () => { duel.SetSpellTrap(selectedHand); ClearSelection(); RefreshAll(); }, new Color(0.24f, 0.28f, 0.46f, 1f), 100f);
                    }
                }

                if (CanChooseEndTurn())
                {
                    AddEndTurnButton();
                }

                AddButton("Cancel", actionRow, () => { ClearSelection(); RefreshAll(); }, new Color(0.18f, 0.2f, 0.24f, 1f), 110f);
                return;
            }

            if (selectedSpellTrap >= 0 && selectedSpellTrap < PlayerState.ZoneCount)
            {
                CardInstance card = duel.State.Player.SpellsTraps[selectedSpellTrap];
                if (duel.CardManager.CanActivateSet(card, duel.State.Phase, duel.PhaseManager, duel.State.TurnNumber))
                {
                    AddButton("Activate", actionRow, () => { duel.ActivateSetSpellTrap(selectedSpellTrap); ClearSelection(); RefreshAll(); }, new Color(0.44f, 0.32f, 0.12f, 1f), 130f);
                }

                AddButton("Next Phase", actionRow, () => { duel.AdvancePhase(); ClearSelection(); RefreshAll(); }, new Color(0.18f, 0.34f, 0.58f, 1f), 150f);
                if (CanChooseEndTurn())
                {
                    AddEndTurnButton();
                }

                AddButton("Cancel", actionRow, () => { ClearSelection(); RefreshAll(); }, new Color(0.18f, 0.2f, 0.24f, 1f), 110f);
                return;
            }

            if (selectedMonster >= 0)
            {
                if (duel.BattleSystem.CanAttack(duel.State.Player.Monsters[selectedMonster], duel.State.Phase, duel.State.TurnNumber))
                {
                    AddButton("Direct", actionRow, () => { duel.Attack(selectedMonster, -1); ClearSelection(); RefreshAll(); }, new Color(0.56f, 0.16f, 0.14f, 1f), 110f);
                }

                AddButton("Next Phase", actionRow, () => { duel.AdvancePhase(); ClearSelection(); RefreshAll(); }, new Color(0.18f, 0.34f, 0.58f, 1f), 150f);
                if (CanChooseEndTurn())
                {
                    AddEndTurnButton();
                }

                AddButton("Cancel", actionRow, () => { ClearSelection(); RefreshAll(); }, new Color(0.18f, 0.2f, 0.24f, 1f), 110f);
                return;
            }

            if (duel.PhaseManager.IsMainPhase(duel.State.Phase) && HasExtraDeckOptions())
            {
                AddButton("Extra Deck", actionRow, ShowExtraDeckChoice, new Color(0.38f, 0.28f, 0.56f, 1f), 140f);
            }

            AddButton("Next Phase", actionRow, () => { duel.AdvancePhase(); RefreshAll(); }, new Color(0.18f, 0.34f, 0.58f, 1f), 150f);
            AddEndTurnButton();
        }

        private bool CanChooseEndTurn()
        {
            return duel.State.Phase == DuelPhase.Battle || duel.State.Phase == DuelPhase.Main2;
        }

        private void AddEndTurnButton()
        {
            string label = duel.State.Phase == DuelPhase.Battle ? "End" : "End Turn";
            AddButton(label, actionRow, EndCurrentTurn, new Color(0.28f, 0.24f, 0.34f, 1f), 130f);
        }

        private void EndCurrentTurn()
        {
            ClearSelection();
            duel.EndTurn();
            RefreshAll();
        }

        private void ActivateSelectedHandCard()
        {
            if (selectedHand < 0 || selectedHand >= duel.State.Player.Hand.Count)
            {
                return;
            }

            CardInstance card = duel.State.Player.Hand[selectedHand];
            if (card.Data.EffectId.StartsWith("search_", StringComparison.OrdinalIgnoreCase))
            {
                ShowDeckChoice(card, c => c.Data.Archetype == card.Data.Archetype && !c.Data.IsExtraDeckMonster);
                return;
            }

            duel.ActivateFromHand(selectedHand);
            ClearSelection();
            RefreshAll();
        }

        private void ShowDeckChoice(CardInstance source, Predicate<CardInstance> predicate)
        {
            awaitingChoice = true;
            choicePanel.gameObject.SetActive(true);
            ClearChildren(choicePanel);
            Text title = Label("Choice Title", choicePanel, 22, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            title.text = "Select card(s) to add from your Deck to your hand.";
            AddLayout(title.gameObject, -1f, 38f, 0f, 0f);

            RectTransform row = Panel("Choice Cards", choicePanel, new Color(0.02f, 0.05f, 0.12f, 1f));
            AddHorizontal(row.gameObject, 12, 12, 8, 8, 10f, TextAnchor.MiddleCenter);
            AddLayout(row.gameObject, -1f, ChoiceCardHeight + 22f, 1f, 0f);
            int shown = 0;
            for (int i = 0; i < duel.State.Player.Deck.Count && shown < 6; i++)
            {
                CardInstance candidate = duel.State.Player.Deck[i];
                if (!predicate(candidate))
                {
                    continue;
                }

                CardInstance captured = candidate;
                Button button = CardButton(candidate, row, false);
                button.onClick.AddListener(() =>
                {
                    duel.ResolveDeckChoiceFromHand(source, captured);
                    choicePanel.gameObject.SetActive(false);
                    awaitingChoice = false;
                    ClearSelection();
                    RefreshAll();
                });
                shown++;
            }

            AddButton("Cancel", choicePanel, () =>
            {
                choicePanel.gameObject.SetActive(false);
                awaitingChoice = false;
                RefreshAll();
            }, new Color(0.18f, 0.2f, 0.24f, 1f), 140f);
        }

        private bool HasExtraDeckOptions()
        {
            for (int i = 0; i < duel.State.Player.ExtraDeck.Count; i++)
            {
                if (duel.CanSummonFromExtra(duel.State.Player.ExtraDeck[i], duel.State.Player))
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowExtraDeckChoice()
        {
            awaitingChoice = true;
            choicePanel.gameObject.SetActive(true);
            ClearChildren(choicePanel);
            Text title = Label("Extra Deck Title", choicePanel, 22, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            title.text = "Choose an Extra Deck Summon.";
            AddLayout(title.gameObject, -1f, 38f, 0f, 0f);

            RectTransform row = Panel("Extra Deck Cards", choicePanel, new Color(0.02f, 0.05f, 0.12f, 1f));
            AddHorizontal(row.gameObject, 12, 12, 8, 8, 10f, TextAnchor.MiddleCenter);
            AddLayout(row.gameObject, -1f, ChoiceCardHeight + 22f, 1f, 0f);

            for (int i = 0; i < duel.State.Player.ExtraDeck.Count; i++)
            {
                int capturedIndex = i;
                CardInstance candidate = duel.State.Player.ExtraDeck[i];
                if (!duel.CanSummonFromExtra(candidate, duel.State.Player))
                {
                    continue;
                }

                Button button = CardButton(candidate, row, false);
                button.onClick.AddListener(() =>
                {
                    duel.SummonFromExtraDeck(capturedIndex);
                    choicePanel.gameObject.SetActive(false);
                    awaitingChoice = false;
                    ClearSelection();
                    RefreshAll();
                });
            }

            AddButton("Cancel", choicePanel, () =>
            {
                choicePanel.gameObject.SetActive(false);
                awaitingChoice = false;
                RefreshAll();
            }, new Color(0.18f, 0.2f, 0.24f, 1f), 140f);
        }

        private void RefreshInspector()
        {
            CardInstance card = GetFocusedCard();
            if (card == null)
            {
                inspectorTitle.text = "No card selected";
                inspectorBody.text = duelUi.EmptyInspectorText();
                return;
            }

            inspectorTitle.text = card.PublicName;
            inspectorBody.text = duelUi.InspectorText(card);
        }

        private void RefreshLog()
        {
            StringBuilder builder = new StringBuilder();
            int start = Mathf.Max(0, duel.State.Log.Count - 8);
            for (int i = duel.State.Log.Count - 1; i >= start; i--)
            {
                builder.Append("- ");
                builder.AppendLine(duel.State.Log[i]);
            }

            logText.text = builder.ToString();
        }

        private bool IsAutoPlayerPhase()
        {
            return !duel.State.GameOver &&
                   !awaitingChoice &&
                   (duel.State.Phase == DuelPhase.Draw || duel.State.Phase == DuelPhase.Standby);
        }

        private IEnumerator PlayerAutoPhaseRoutine()
        {
            playerAutoPhaseRunning = true;
            yield return new WaitForSeconds(0.65f);
            while (duel.State.TurnPlayer == PlayerId.Player && IsAutoPlayerPhase())
            {
                duel.AdvancePhase();
                RefreshAll();
                yield return new WaitForSeconds(0.45f);
            }

            playerAutoPhaseRunning = false;
        }

        private IEnumerator OpponentRoutine()
        {
            opponentRoutineRunning = true;
            yield return new WaitForSeconds(0.65f);
            while (!duel.State.GameOver && duel.State.TurnPlayer == PlayerId.Opponent && !awaitingChoice)
            {
                if (duel.State.Phase == DuelPhase.Draw || duel.State.Phase == DuelPhase.Standby)
                {
                    duel.AdvancePhase();
                }
                else if (duel.PhaseManager.IsMainPhase(duel.State.Phase))
                {
                    OpponentPlayMain();
                    duel.AdvancePhase();
                }
                else if (duel.State.Phase == DuelPhase.Battle)
                {
                    OpponentBattle();
                    duel.AdvancePhase();
                }
                else
                {
                    duel.AdvancePhase();
                }

                RefreshAll();
                yield return new WaitForSeconds(0.8f);
            }

            opponentRoutineRunning = false;
        }

        private void OpponentPlayMain()
        {
            PlayerState opponent = duel.State.Opponent;
            for (int i = 0; i < opponent.Hand.Count; i++)
            {
                CardInstance card = opponent.Hand[i];
                if (card.Data.Kind != CardKind.Monster || !duel.CardManager.CanNormalSummon(card, opponent, duel.State.TurnNumber))
                {
                    continue;
                }

                duel.NormalSummon(i, false);
                return;
            }

            for (int i = 0; i < opponent.Hand.Count; i++)
            {
                CardInstance card = opponent.Hand[i];
                if (card.Data.Kind == CardKind.Spell && duel.CardManager.CanActivateFromHand(card, duel.State.Phase, duel.PhaseManager))
                {
                    duel.ActivateFromHand(i);
                    return;
                }
            }

            for (int i = 0; i < opponent.ExtraDeck.Count; i++)
            {
                if (duel.CanSummonFromExtra(opponent.ExtraDeck[i], opponent))
                {
                    duel.SummonFromExtraDeck(i);
                    return;
                }
            }

            for (int i = 0; i < opponent.Hand.Count; i++)
            {
                CardInstance card = opponent.Hand[i];
                if (card.Data.Kind != CardKind.Monster)
                {
                    duel.SetSpellTrap(i);
                    return;
                }
            }
        }

        private void OpponentBattle()
        {
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                if (duel.BattleSystem.CanAttack(duel.State.Opponent.Monsters[i], duel.State.Phase, duel.State.TurnNumber))
                {
                    int target = FirstPlayerMonsterZone();
                    duel.Attack(i, target);
                    return;
                }
            }
        }

        private int FirstPlayerMonsterZone()
        {
            for (int i = 0; i < PlayerState.ZoneCount; i++)
            {
                if (duel.State.Player.Monsters[i] != null)
                {
                    return i;
                }
            }

            return -1;
        }

        private void HandleZoneClick(ZoneClickTarget target)
        {
            if (target.Owner == PlayerId.Player && target.Monster && duel.State.Player.Monsters[target.Index] != null && duel.State.TurnPlayer == PlayerId.Player)
            {
                selectedMonster = selectedMonster == target.Index ? -1 : target.Index;
                selectedHand = -1;
                selectedSpellTrap = -1;
                RefreshAll();
                return;
            }

            if (target.Owner == PlayerId.Player && !target.Monster && duel.State.Player.SpellsTraps[target.Index] != null && duel.State.TurnPlayer == PlayerId.Player)
            {
                selectedSpellTrap = selectedSpellTrap == target.Index ? -1 : target.Index;
                selectedHand = -1;
                selectedMonster = -1;
                RefreshAll();
                return;
            }

            if (target.Owner == PlayerId.Opponent && target.Monster && selectedMonster >= 0 && duel.State.Opponent.Monsters[target.Index] != null)
            {
                duel.Attack(selectedMonster, target.Index);
                ClearSelection();
                RefreshAll();
            }
        }

        private CardInstance GetFocusedCard()
        {
            if (selectedHand >= 0 && selectedHand < duel.State.Player.Hand.Count)
            {
                return duel.State.Player.Hand[selectedHand];
            }

            if (selectedMonster >= 0 && selectedMonster < PlayerState.ZoneCount)
            {
                return duel.State.Player.Monsters[selectedMonster];
            }

            if (selectedSpellTrap >= 0 && selectedSpellTrap < PlayerState.ZoneCount)
            {
                return duel.State.Player.SpellsTraps[selectedSpellTrap];
            }

            return null;
        }

        private void ClearSelection()
        {
            selectedHand = -1;
            selectedMonster = -1;
            selectedSpellTrap = -1;
        }

        private void Restart()
        {
            StopAllCoroutines();
            opponentRoutineRunning = false;
            awaitingChoice = false;
            choicePanel.gameObject.SetActive(false);
            ClearSelection();
            duel.StartNewDuel();
            RefreshAll();
        }

        private BoardSlot CreateSlot(PlayerId owner, bool monster, int index, float x, float z)
        {
            Transform root = new GameObject((owner == PlayerId.Player ? "Player" : "Opponent") + (monster ? " Monster " : " Spell ") + index).transform;
            root.SetParent(arenaRoot, false);
            root.localPosition = new Vector3(x, 0.12f, z);
            GameObject plate = CreateBox("Zone Plate", root, Vector3.zero, new Vector3(1.52f, 0.06f, 1.84f), new Color(0.08f, 0.12f, 0.18f, 1f));
            CreateCylinder("Zone Core", root, new Vector3(0f, 0.08f, 0f), new Vector3(0.54f, 0.045f, 0.54f), monster ? new Color(0.08f, 0.45f, 0.38f, 1f) : new Color(0.1f, 0.22f, 0.48f, 1f));
            GameObject card = CreateBox("Card", root, new Vector3(0f, 0.16f, 0f), new Vector3(1.02f, 0.06f, 1.48f), new Color(0.52f, 0.26f, 0.1f, 1f));
            TextMesh label = CreateTextMesh("Card Label", root, new Vector3(-0.47f, 0.42f, -0.55f), string.Empty);
            card.SetActive(false);
            label.gameObject.SetActive(false);
            RegisterTarget(plate.GetComponent<Collider>(), owner, monster, index);
            RegisterTarget(card.GetComponent<Collider>(), owner, monster, index);
            return new BoardSlot
            {
                Plate = plate.GetComponent<Renderer>(),
                Card = card.GetComponent<Renderer>(),
                Label = label,
                Owner = owner,
                Monster = monster,
                Index = index
            };
        }

        private DeckPileVisual CreateDeckPile(PlayerId owner, bool extraDeck, float x, float z)
        {
            string ownerName = owner == PlayerId.Player ? "Player" : "Opponent";
            string deckName = extraDeck ? "Extra Deck" : "Main Deck";
            Transform root = new GameObject(ownerName + " " + deckName).transform;
            root.SetParent(arenaRoot, false);
            root.localPosition = new Vector3(x, 0.12f, z);
            if (owner == PlayerId.Opponent)
            {
                root.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            Color zoneColor = extraDeck
                ? new Color(0.42f, 0.44f, 0.52f, 1f)
                : new Color(0.12f, 0.28f, 0.52f, 1f);
            CreateBox("Deck Zone", root, Vector3.zero, new Vector3(1.44f, 0.055f, 1.88f), zoneColor);
            GameObject stack = CreateBox("Card Stack", root, new Vector3(0f, 0.16f, 0f), new Vector3(1.08f, 0.16f, 1.5f), new Color(0.17f, 0.07f, 0.04f, 1f));
            CreateBox("Orientation Mark", stack.transform, new Vector3(0f, 0.54f, -0.35f), new Vector3(0.72f, 0.08f, 0.1f), zoneColor);
            TextMesh label = CreateTextMesh("Deck Label", root, new Vector3(-0.62f, 0.52f, -0.68f), string.Empty);

            return new DeckPileVisual
            {
                Root = root,
                Stack = stack.GetComponent<Renderer>(),
                Label = label,
                Owner = owner,
                ExtraDeck = extraDeck
            };
        }

        private void RefreshDeckPile(DeckPileVisual pile, PlayerState player)
        {
            int count = pile.ExtraDeck ? player.ExtraDeck.Count : player.Deck.Count;
            int capacity = pile.ExtraDeck ? PlayerState.ExtraDeckSize : PlayerState.MainDeckSize;
            float stackHeight = 0.055f + count * 0.006f;
            pile.Stack.gameObject.SetActive(count > 0);
            pile.Stack.transform.localScale = new Vector3(1.08f, stackHeight, 1.5f);
            pile.Stack.transform.localPosition = new Vector3(0f, 0.06f + stackHeight * 0.5f, 0f);
            pile.Label.text = (pile.ExtraDeck ? "EXTRA DECK" : "MAIN DECK") + "\n" + count + " / " + capacity;
            FaceToCamera(pile.Label.transform);
        }

        private Button CardButton(CardInstance card, Transform parent, bool selected, bool handCard = false)
        {
            GameObject go = new GameObject("Card Button", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform cardRect = go.GetComponent<RectTransform>();
            cardRect.localRotation = Quaternion.identity;

            float width = handCard ? HandCardWidth : ChoiceCardWidth;
            float height = handCard ? HandCardHeight : ChoiceCardHeight;
            AddLayout(go, width, height, 0f, 0f);

            Image clickSurface = go.GetComponent<Image>();
            clickSurface.raycastTarget = true;
            clickSurface.color = selected
                ? new Color(1f, 0.72f, 0.18f, 1f)
                : new Color(0.025f, 0.035f, 0.055f, 1f);

            Button button = go.GetComponent<Button>();
            button.targetGraphic = clickSurface;
            button.interactable = true;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            Sprite artwork = GetArtworkSprite(card);
            if (artwork != null)
            {
                GameObject artworkObject = new GameObject("Artwork", typeof(RectTransform), typeof(Image));
                artworkObject.transform.SetParent(go.transform, false);
                Image artworkImage = artworkObject.GetComponent<Image>();
                artworkImage.sprite = artwork;
                artworkImage.preserveAspect = true;
                artworkImage.raycastTarget = false;
                artworkImage.color = Color.white;
                Anchor(
                    artworkImage.rectTransform,
                    Vector2.zero,
                    Vector2.one,
                    new Vector2(4f, 4f),
                    new Vector2(-4f, -4f));
                return button;
            }

            clickSurface.color = selected ? new Color(1f, 0.72f, 0.18f, 1f) : CardColor(card);
            AddVertical(go, 5, 5, 5, 5, 2f);
            Text name = Label("Name", go.transform, 12, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            name.text = ShortName(card.PublicName);
            name.raycastTarget = false;
            AddLayout(name.gameObject, -1f, 48f, 0f, 0f);
            Text stats = Label("Stats", go.transform, 11, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.95f, 1f, 1f));
            stats.text = card.Data.IsMonster && !card.FaceDown ? "Lv " + card.Data.Level + "\nATK " + card.CurrentAttack : card.Data.Kind.ToString();
            stats.raycastTarget = false;
            AddLayout(stats.gameObject, -1f, 46f, 1f, 0f);
            return button;
        }

        private Button AddButton(string text, Transform parent, UnityEngine.Events.UnityAction action, Color color, float width)
        {
            GameObject go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(action);
            Text label = Label("Label", go.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            label.text = text;
            Stretch(label.GetComponent<RectTransform>());
            AddLayout(go, width, 46f, 0f, 0f);
            return button;
        }

        private Color CardColor(CardInstance card)
        {
            return duelUi.CardColor(card);
        }

        private string ShortName(string value)
        {
            return value.Length <= 18 ? value : value.Substring(0, 17) + ".";
        }

        private void RegisterTarget(Collider collider, PlayerId owner, bool monster, int index)
        {
            if (collider == null)
            {
                return;
            }

            targets[collider] = new ZoneClickTarget { Owner = owner, Monster = monster, Index = index };
        }

        private GameObject CreateBox(string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            return CreateBox(name, arenaRoot, localPosition, scale, color);
        }

        private GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = Material(color);
            return go;
        }

        private GameObject CreateCylinder(string name, Vector3 localPosition, Vector3 scale, Color color)
        {
            return CreateCylinder(name, arenaRoot, localPosition, scale, color);
        }

        private GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = Material(color);
            return go;
        }

        private Material Material(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            SetMaterialColor(material, color);
            return material;
        }

        private void SetColor(Renderer renderer, Color color)
        {
            if (renderer != null)
            {
                SetMaterialColor(renderer.material, color);
            }
        }

        private void SetCardAppearance(Renderer renderer, CardInstance card, Color fallbackColor, bool selected)
        {
            if (renderer == null)
            {
                return;
            }

            Texture2D artwork = GetArtworkTexture(card);
            Material material = renderer.material;
            SetMaterialTexture(material, artwork);
            SetMaterialColor(material, artwork == null
                ? fallbackColor
                : selected ? new Color(1f, 0.82f, 0.52f, 1f) : Color.white);
        }

        private Texture2D GetArtworkTexture(CardInstance card)
        {
            if (card == null || card.FaceDown || artworkCatalog == null)
            {
                return null;
            }

            Texture2D texture;
            return artworkCatalog.TryGetTexture(card.Data.Id, out texture) ? texture : null;
        }

        private Sprite GetArtworkSprite(CardInstance card)
        {
            Texture2D texture = GetArtworkTexture(card);
            if (texture == null)
            {
                return null;
            }

            Sprite sprite;
            if (!artworkSprites.TryGetValue(card.Data.Id, out sprite) || sprite == null)
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0u,
                    SpriteMeshType.FullRect);
                artworkSprites[card.Data.Id] = sprite;
            }

            return sprite;
        }

        private void SetMaterialTexture(Material material, Texture texture)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }
        }

        private void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private TextMesh CreateTextMesh(string name, Transform parent, Vector3 position, string text)
        {
            GameObject go = new GameObject(name, typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            TextMesh mesh = go.GetComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 46;
            mesh.characterSize = 0.035f;
            mesh.anchor = TextAnchor.MiddleLeft;
            mesh.alignment = TextAlignment.Left;
            mesh.color = Color.white;
            return mesh;
        }

        private void FaceToCamera(Transform target)
        {
            if (arenaCamera == null || target == null)
            {
                return;
            }

            Vector3 direction = arenaCamera.transform.position - target.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                target.rotation = Quaternion.LookRotation(direction);
            }
        }

        private RectTransform Panel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > 0.05f;
            return go.GetComponent<RectTransform>();
        }

        private Text Label(string name, Transform parent, int size, FontStyle style, TextAnchor anchor, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private void AddHorizontal(GameObject go, int left, int right, int top, int bottom, float spacing, TextAnchor alignment)
        {
            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(left, right, top, bottom);
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private void AddVertical(GameObject go, int left, int right, int top, int bottom, float spacing)
        {
            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(left, right, top, bottom);
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private void AddLayout(GameObject go, float width, float height, float flexWidth, float flexHeight)
        {
            LayoutElement layout = go.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = go.AddComponent<LayoutElement>();
            }

            if (width >= 0f)
            {
                layout.preferredWidth = width;
            }

            if (height >= 0f)
            {
                layout.preferredHeight = height;
            }

            layout.flexibleWidth = flexWidth;
            layout.flexibleHeight = flexHeight;
        }

        private void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private void Stretch(RectTransform rect)
        {
            Anchor(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private void EnsureEventSystem()
        {
            GameObject go = EventSystem.current != null
                ? EventSystem.current.gameObject
                : new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule legacyInputModule = go.GetComponent<StandaloneInputModule>();
            if (legacyInputModule != null)
            {
                legacyInputModule.enabled = false;
            }

            InputSystemUIInputModule inputModule = go.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = go.AddComponent<InputSystemUIInputModule>();
            }

            inputModule.enabled = true;
#else
            StandaloneInputModule inputModule = go.GetComponent<StandaloneInputModule>();
            if (inputModule == null)
            {
                inputModule = go.AddComponent<StandaloneInputModule>();
            }

            inputModule.enabled = true;
#endif
        }
    }
}
