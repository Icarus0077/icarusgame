using System;
using System.Collections.Generic;

namespace IcarusGame.DuelPrototype
{
    public sealed partial class DuelEngine
    {
        public const int ZoneCount = DuelPlayerState.ZoneCount;

        private readonly Random random = new Random();

        public DuelPlayerState Player { get; private set; }
        public DuelPlayerState Opponent { get; private set; }
        public DuelPlayerId ActivePlayerId { get; private set; }
        public DuelPhase CurrentPhase { get; private set; }
        public int TurnNumber { get; private set; }
        public bool IsGameOver { get; private set; }
        public string WinnerText { get; private set; }
        public readonly List<string> Log = new List<string>();

        public void StartNewDuel(IList<DuelCardDefinition> playerDeck = null, IList<DuelCardDefinition> opponentDeck = null)
        {
            IsGameOver = false;
            WinnerText = string.Empty;
            TurnNumber = 0;
            Log.Clear();

            Player = BuildPlayer(DuelPlayerId.Player, "Speler", playerDeck);
            Opponent = BuildPlayer(DuelPlayerId.Opponent, "Tegenstander", opponentDeck ?? playerDeck);

            Shuffle(Player.Deck);
            Shuffle(Opponent.Deck);

            DrawStartingHand(Player);
            DrawStartingHand(Opponent);

            ActivePlayerId = random.Next(0, 2) == 0 ? DuelPlayerId.Player : DuelPlayerId.Opponent;
            AddLog("Duel gestart. Beide spelers trekken 5 kaarten.");
            AddLog(GetActivePlayer().Name + " begint. De eerste beurt heeft geen Battle Phase.");
            BeginTurn(ActivePlayerId);
        }

        public DuelPlayerState GetPlayer(DuelPlayerId id)
        {
            return id == DuelPlayerId.Player ? Player : Opponent;
        }

        public DuelPlayerState GetOpponent(DuelPlayerId id)
        {
            return id == DuelPlayerId.Player ? Opponent : Player;
        }

        public DuelPlayerState GetActivePlayer()
        {
            return GetPlayer(ActivePlayerId);
        }

        public DuelPlayerState GetInactivePlayer()
        {
            return GetOpponent(ActivePlayerId);
        }

        public bool IsMainPhase()
        {
            return CurrentPhase == DuelPhase.Main1 || CurrentPhase == DuelPhase.Main2;
        }

        public bool CanPlayerAct()
        {
            return !IsGameOver && ActivePlayerId == DuelPlayerId.Player;
        }

        public void AdvancePhase()
        {
            if (IsGameOver)
            {
                return;
            }

            switch (CurrentPhase)
            {
                case DuelPhase.Draw:
                    EnterPhase(DuelPhase.Standby);
                    break;
                case DuelPhase.Standby:
                    EnterPhase(DuelPhase.Main1);
                    break;
                case DuelPhase.Main1:
                    if (TurnNumber == 1)
                    {
                        AddLog("Battle Phase en Main Phase 2 overgeslagen: de eerste speler mag in de eerste beurt geen Battle Phase voeren.");
                        EnterPhase(DuelPhase.End);
                    }
                    else
                    {
                        EnterPhase(DuelPhase.Battle);
                    }
                    break;
                case DuelPhase.Battle:
                    EnterPhase(DuelPhase.Main2);
                    break;
                case DuelPhase.Main2:
                    EnterPhase(DuelPhase.End);
                    break;
                case DuelPhase.End:
                    ResolveEndPhase(GetActivePlayer());
                    BeginTurn(OtherPlayer(ActivePlayerId));
                    break;
                default:
                    EnterPhase(DuelPhase.Draw);
                    break;
            }
        }

        public void EndCurrentTurn()
        {
            if (IsGameOver)
            {
                return;
            }

            if (CurrentPhase != DuelPhase.End)
            {
                EnterPhase(DuelPhase.End);
            }

            AdvancePhase();
        }

        public bool CanNormalSummonFromHand(int handIndex)
        {
            return CanNormalSummonFromHand(handIndex, false);
        }

        public bool CanNormalSummonFromHand(int handIndex, bool setDefense)
        {
            if (!CanPlayerAct() || !IsMainPhase())
            {
                return false;
            }

            return CanNormalSummon(GetActivePlayer(), handIndex, setDefense);
        }

        public bool NormalSummonFromHand(int handIndex, bool setDefense)
        {
            if (!CanPlayerAct() || !IsMainPhase())
            {
                return false;
            }

            return NormalSummon(GetActivePlayer(), handIndex, setDefense);
        }

        public bool CanSetSpellTrapFromHand(int handIndex)
        {
            if (!CanPlayerAct() || !IsMainPhase())
            {
                return false;
            }

            DuelPlayerState player = GetActivePlayer();
            if (!IsValidHandIndex(player, handIndex))
            {
                return false;
            }

            DuelCard card = player.Hand[handIndex];
            if (card.Definition.Kind == DuelCardKind.Monster)
            {
                return false;
            }

            return card.Definition.IsFieldSpell || FirstEmptyZone(player.SpellTrapZones) >= 0;
        }

        public bool SetSpellTrapFromHand(int handIndex)
        {
            if (!CanSetSpellTrapFromHand(handIndex))
            {
                return false;
            }

            DuelPlayerState player = GetActivePlayer();
            DuelCard card = player.Hand[handIndex];
            if (card.Definition.IsFieldSpell)
            {
                player.Hand.RemoveAt(handIndex);
                PlaceInFieldZone(player, card, true);
                AddLog(player.Name + " zet een Field Spell face-down in de Field Zone.");
                return true;
            }

            int zone = FirstEmptyZone(player.SpellTrapZones);
            player.Hand.RemoveAt(handIndex);
            card.IsFaceDown = true;
            card.SetTurn = TurnNumber;
            player.SpellTrapZones[zone] = card;
            AddLog(player.Name + " zet " + DescribeHidden(card) + " in Spell/Trap Zone " + (zone + 1) + ".");
            return true;
        }

        public bool CanActivateSpellFromHand(int handIndex)
        {
            if (!CanPlayerAct())
            {
                return false;
            }

            DuelPlayerState player = GetActivePlayer();
            if (!IsValidHandIndex(player, handIndex))
            {
                return false;
            }

            DuelCard card = player.Hand[handIndex];
            if (card.Definition.Kind != DuelCardKind.Spell)
            {
                return false;
            }

            if (!CanActivateSpellInCurrentPhase(card))
            {
                return false;
            }

            return card.Definition.IsFieldSpell || FirstEmptyZone(player.SpellTrapZones) >= 0;
        }

        public bool ActivateSpellFromHand(int handIndex)
        {
            if (!CanActivateSpellFromHand(handIndex))
            {
                return false;
            }

            DuelPlayerState player = GetActivePlayer();
            DuelCard card = player.Hand[handIndex];
            if (card.Definition.IsFieldSpell)
            {
                player.Hand.RemoveAt(handIndex);
                PlaceInFieldZone(player, card, false);
                AddLog("Chain Link 1 (Spell Speed " + SpellSpeed(card) + "): " + player.Name + " activeert Field Spell " + card.Definition.Name + ".");
                ResolveEffect(card, player, null);
                AddLog(card.Definition.Name + " blijft face-up in de Field Zone.");
                return true;
            }

            bool staysOnField = StaysOnFieldAfterActivation(card);
            int zone = FirstEmptyZone(player.SpellTrapZones);
            player.Hand.RemoveAt(handIndex);
            card.IsFaceDown = false;
            card.SetTurn = TurnNumber;
            player.SpellTrapZones[zone] = card;

            AddLog("Chain Link 1 (Spell Speed " + SpellSpeed(card) + "): " + player.Name + " activeert " + card.Definition.Name + ".");
            ResolveEffect(card, player, null);
            if (staysOnField)
            {
                AddLog(card.Definition.Name + " blijft face-up in de Spell/Trap Zone.");
            }
            else
            {
                player.SpellTrapZones[zone] = null;
                SendToGraveyard(player, card);
            }

            return true;
        }

        public bool CanActivateSetSpellTrap(int zoneIndex)
        {
            if (!CanPlayerAct())
            {
                return false;
            }

            return CanActivateSetSpellTrapFor(GetActivePlayer(), zoneIndex);
        }

        public bool ActivateSetSpellTrap(int zoneIndex)
        {
            if (!CanActivateSetSpellTrap(zoneIndex))
            {
                return false;
            }

            return ActivateSetSpellTrapFor(GetActivePlayer(), zoneIndex);
        }

        public bool CanActivateFieldZone()
        {
            if (!CanPlayerAct())
            {
                return false;
            }

            DuelCard card = Player.FieldZone;
            return card != null &&
                   card.IsFaceDown &&
                   card.Definition.Kind == DuelCardKind.Spell &&
                   CanActivateSpellInCurrentPhase(card);
        }

        public bool ActivateFieldZone()
        {
            if (!CanActivateFieldZone())
            {
                return false;
            }

            DuelCard card = Player.FieldZone;
            card.IsFaceDown = false;
            AddLog("Chain Link 1 (Spell Speed " + SpellSpeed(card) + "): " + Player.Name + " activeert Field Spell " + card.Definition.Name + ".");
            ResolveEffect(card, Player, null);
            AddLog(card.Definition.Name + " blijft face-up in de Field Zone.");
            return true;
        }

        public bool CanAttackWithMonster(int attackerZone)
        {
            if (!CanPlayerAct() || CurrentPhase != DuelPhase.Battle || TurnNumber == 1)
            {
                return false;
            }

            DuelPlayerState attacker = GetActivePlayer();
            if (!IsValidZoneIndex(attackerZone))
            {
                return false;
            }

            DuelCard card = attacker.MonsterZones[attackerZone];
            return card != null && card.Position == MonsterBattlePosition.Attack && !card.HasAttacked;
        }

        public bool AttackMonster(int attackerZone, int defenderZone)
        {
            if (!CanAttackWithMonster(attackerZone) || !IsValidZoneIndex(defenderZone))
            {
                return false;
            }

            DuelPlayerState attackerPlayer = GetActivePlayer();
            DuelPlayerState defenderPlayer = GetInactivePlayer();
            DuelCard attacker = attackerPlayer.MonsterZones[attackerZone];
            DuelCard defender = defenderPlayer.MonsterZones[defenderZone];
            if (defender == null)
            {
                return false;
            }

            AddLog("Attack Declaration: " + attacker.Definition.Name + " kiest " + defender.PublicName + " als doelwit.");
            if (ResolveAttackTrap(defenderPlayer, attackerPlayer, attackerZone))
            {
                return true;
            }

            attacker.HasAttacked = true;
            if (defender.IsFaceDown)
            {
                AddLog("Before Damage Calculation: " + defender.PublicName + " wordt face-up geflipt.");
            }

            defender.IsFaceDown = false;

            AddLog("Damage Step: " + attacker.Definition.Name + " valt " + defender.Definition.Name + " aan.");
            if (defender.Position == MonsterBattlePosition.Attack)
            {
                ResolveAttackAgainstAttack(attackerPlayer, attackerZone, defenderPlayer, defenderZone);
            }
            else
            {
                ResolveAttackAgainstDefense(attackerPlayer, attackerZone, defenderPlayer, defenderZone);
            }

            CheckLifePointWin();
            return true;
        }

        public bool DirectAttack(int attackerZone)
        {
            if (!CanAttackWithMonster(attackerZone) || CountMonsters(GetInactivePlayer()) > 0)
            {
                return false;
            }

            DuelPlayerState attackerPlayer = GetActivePlayer();
            DuelPlayerState defenderPlayer = GetInactivePlayer();
            DuelCard attacker = attackerPlayer.MonsterZones[attackerZone];

            AddLog("Attack Declaration: " + attacker.Definition.Name + " valt direct aan.");
            if (ResolveAttackTrap(defenderPlayer, attackerPlayer, attackerZone))
            {
                return true;
            }

            attacker.HasAttacked = true;
            defenderPlayer.LifePoints -= Math.Max(0, attacker.CurrentAttack);
            AddLog("Damage Step: " + attacker.Definition.Name + " doet " + attacker.CurrentAttack + " directe schade.");
            CheckLifePointWin();
            return true;
        }

        public bool CanChangePlayerMonsterPosition(int zoneIndex)
        {
            if (!CanPlayerAct() || !IsMainPhase() || !IsValidZoneIndex(zoneIndex))
            {
                return false;
            }

            return CanChangeMonsterPosition(Player, zoneIndex);
        }

        public bool TogglePlayerMonsterPosition(int zoneIndex)
        {
            if (!CanChangePlayerMonsterPosition(zoneIndex))
            {
                return false;
            }

            DuelCard card = Player.MonsterZones[zoneIndex];
            if (card.Position == MonsterBattlePosition.Attack)
            {
                card.Position = MonsterBattlePosition.DefenseFaceUp;
                card.IsFaceDown = false;
            }
            else
            {
                card.Position = MonsterBattlePosition.Attack;
                card.IsFaceDown = false;
            }

            card.PositionChangedTurn = TurnNumber;
            AddLog(card.Definition.Name + " wisselt van battle position.");
            return true;
        }

        private bool CanActivateSetSpellTrapFor(DuelPlayerState player, int zoneIndex)
        {
            if (player == null || !IsValidZoneIndex(zoneIndex))
            {
                return false;
            }

            DuelCard card = player.SpellTrapZones[zoneIndex];
            if (card == null || !card.IsFaceDown)
            {
                return false;
            }

            if (card.Definition.Kind == DuelCardKind.Trap && card.SetTurn >= TurnNumber)
            {
                return false;
            }

            if (card.Definition.IsQuickPlaySpell && card.SetTurn >= TurnNumber)
            {
                return false;
            }

            if (card.Definition.Kind == DuelCardKind.Spell)
            {
                return CanActivateSpellInCurrentPhase(card);
            }

            return card.Definition.Kind == DuelCardKind.Trap &&
                   card.Definition.IsContinuousCard &&
                   IsMainPhase();
        }

        private bool ActivateSetSpellTrapFor(DuelPlayerState player, int zoneIndex)
        {
            if (!CanActivateSetSpellTrapFor(player, zoneIndex))
            {
                return false;
            }

            DuelCard card = player.SpellTrapZones[zoneIndex];
            bool staysOnField = StaysOnFieldAfterActivation(card);
            if (!staysOnField)
            {
                player.SpellTrapZones[zoneIndex] = null;
            }

            card.IsFaceDown = false;
            AddLog("Chain Link 1 (Spell Speed " + SpellSpeed(card) + "): " + player.Name + " activeert " + card.Definition.Name + ".");
            ResolveEffect(card, player, null);
            if (staysOnField)
            {
                AddLog(card.Definition.Name + " blijft face-up in de Spell/Trap Zone.");
            }
            else
            {
                SendToGraveyard(player, card);
            }

            return true;
        }

        private void PlaceInFieldZone(DuelPlayerState player, DuelCard card, bool faceDown)
        {
            if (player.FieldZone != null)
            {
                DuelCard previous = player.FieldZone;
                player.FieldZone = null;
                SendToGraveyard(player, previous);
                AddLog(player.Name + " stuurt de vorige Field Spell naar de Graveyard om een nieuwe Field Spell te gebruiken.");
            }

            card.IsFaceDown = faceDown;
            card.SetTurn = TurnNumber;
            player.FieldZone = card;
        }

        public bool CanFlipSummonPlayerMonster(int zoneIndex)
        {
            if (!CanPlayerAct() || !IsMainPhase() || !IsValidZoneIndex(zoneIndex))
            {
                return false;
            }

            DuelCard card = Player.MonsterZones[zoneIndex];
            return card != null &&
                   card.Position == MonsterBattlePosition.DefenseFaceDown &&
                   card.IsFaceDown &&
                   card.SetTurn < TurnNumber &&
                   !card.Definition.IsLinkMonster;
        }

        public bool FlipSummonPlayerMonster(int zoneIndex)
        {
            if (!CanFlipSummonPlayerMonster(zoneIndex))
            {
                return false;
            }

            DuelCard card = Player.MonsterZones[zoneIndex];
            card.IsFaceDown = false;
            card.Position = MonsterBattlePosition.Attack;
            card.PositionChangedTurn = TurnNumber;
            AddLog("Flip Summon: " + card.Definition.Name + " wordt face-up Attack Position.");
            AddLog("Trigger timing: flip-effecten kunnen nu een chain starten als de kaarttekst dat zegt.");
            return true;
        }

        public void RunOpponentMainPhase()
        {
            if (IsGameOver || ActivePlayerId != DuelPlayerId.Opponent || !IsMainPhase())
            {
                return;
            }

            DuelPlayerState opponent = Opponent;
            ActivateBestOpponentSpell(opponent);
            ActivateBestOpponentSetSpellTrap(opponent);
            SetFirstOpponentTrap(opponent);
            SummonBestOpponentMonster(opponent);
            ActivateBestOpponentSpell(opponent);
            ActivateBestOpponentSetSpellTrap(opponent);
        }

        public void RunOpponentBattlePhase()
        {
            if (IsGameOver || ActivePlayerId != DuelPlayerId.Opponent || CurrentPhase != DuelPhase.Battle)
            {
                return;
            }

            for (int zone = 0; zone < ZoneCount; zone++)
            {
                if (IsGameOver)
                {
                    return;
                }

                DuelCard attacker = Opponent.MonsterZones[zone];
                if (attacker == null || attacker.Position != MonsterBattlePosition.Attack || attacker.HasAttacked)
                {
                    continue;
                }

                int targetZone = ChooseOpponentAttackTarget(attacker);
                if (targetZone >= 0)
                {
                    OpponentAttackMonster(zone, targetZone);
                }
                else if (CountMonsters(Player) == 0)
                {
                    OpponentDirectAttack(zone);
                }
            }
        }

        private void BeginTurn(DuelPlayerId playerId)
        {
            TurnNumber++;
            ActivePlayerId = playerId;

            DuelPlayerState active = GetActivePlayer();
            active.NormalSummonUsed = false;
            ResetAttackFlags(active);

            AddLog("--- Beurt " + TurnNumber + ": " + active.Name + " ---");
            EnterPhase(DuelPhase.Draw);
        }

        private void EnterPhase(DuelPhase phase)
        {
            if (IsGameOver)
            {
                return;
            }

            CurrentPhase = phase;
            AddLog("Fase: " + PhaseName(phase) + ".");

            if (phase == DuelPhase.Draw)
            {
                if (TurnNumber == 1)
                {
                    AddLog("Eerste beurt: de beginnende speler trekt geen kaart in de Draw Phase.");
                }
                else
                {
                    DrawCard(GetActivePlayer(), true);
                }
            }
            else if (phase == DuelPhase.Standby)
            {
                AddLog("Standby timing verwerkt.");
                ApplyContinuousStandbyEffects(GetActivePlayer());
            }
            else if (phase == DuelPhase.End)
            {
                AddLog("End Phase: effecten die in deze fase afgaan kunnen nu resolven.");
            }
        }

        private void ResolveEndPhase(DuelPlayerState player)
        {
            ExpireTemporaryEffects(player);
            ApplyHandSizeLimit(player);
            AddLog("Einde van de End Phase: tijdelijke boosts verlopen en handlimiet wordt toegepast.");
        }

        private bool CanNormalSummon(DuelPlayerState player, int handIndex, bool setDefense)
        {
            if (!IsValidHandIndex(player, handIndex) || player.NormalSummonUsed)
            {
                return false;
            }

            DuelCard card = player.Hand[handIndex];
            if (!card.IsMonster || FirstEmptyZone(player.MonsterZones) < 0)
            {
                return false;
            }

            if (card.Definition.IsExtraDeckMonster)
            {
                return false;
            }

            if (setDefense && card.Definition.IsLinkMonster)
            {
                return false;
            }

            return CountMonsters(player) >= RequiredTributeCount(player, card);
        }

        private bool NormalSummon(DuelPlayerState player, int handIndex, bool setDefense)
        {
            if (!CanNormalSummon(player, handIndex, setDefense))
            {
                return false;
            }

            DuelCard card = player.Hand[handIndex];
            int tributeCount = RequiredTributeCount(player, card);
            player.Hand.RemoveAt(handIndex);

            if (tributeCount > 0)
            {
                TributeFirstMonsters(player, tributeCount);
            }

            int zone = FirstEmptyZone(player.MonsterZones);
            card.IsFaceDown = setDefense;
            card.Position = setDefense ? MonsterBattlePosition.DefenseFaceDown : MonsterBattlePosition.Attack;
            card.SetTurn = TurnNumber;
            card.SummonedTurn = TurnNumber;
            card.PositionChangedTurn = TurnNumber;
            player.MonsterZones[zone] = card;
            player.NormalSummonUsed = true;

            string action = setDefense ? "zet een monster in verdedigingspositie" : "normal summont";
            AddLog(player.Name + " " + action + ": " + (setDefense ? "Face-down monster" : card.Definition.Name) + ".");
            if (CanNormalSummonWithoutTributeByCardText(player, card))
            {
                AddLog(card.Definition.Name + " gebruikt zijn kaarttekst en heeft geen tribute nodig.");
            }

            return true;
        }

        private void ResolveAttackAgainstAttack(
            DuelPlayerState attackerPlayer,
            int attackerZone,
            DuelPlayerState defenderPlayer,
            int defenderZone)
        {
            DuelCard attacker = attackerPlayer.MonsterZones[attackerZone];
            DuelCard defender = defenderPlayer.MonsterZones[defenderZone];
            int difference = attacker.CurrentAttack - defender.CurrentAttack;

            if (difference > 0)
            {
                defenderPlayer.LifePoints -= difference;
                DestroyMonster(defenderPlayer, defenderZone);
                AddLog(defenderPlayer.Name + " verliest " + difference + " LP.");
            }
            else if (difference < 0)
            {
                attackerPlayer.LifePoints -= -difference;
                DestroyMonster(attackerPlayer, attackerZone);
                AddLog(attackerPlayer.Name + " verliest " + (-difference) + " LP.");
            }
            else
            {
                if (attacker.CurrentAttack == 0 && defender.CurrentAttack == 0)
                {
                    AddLog("Beide monsters hebben 0 ATK: volgens de regels wordt geen monster vernietigd.");
                    return;
                }

                DestroyMonster(attackerPlayer, attackerZone);
                DestroyMonster(defenderPlayer, defenderZone);
                AddLog("Gelijke ATK: beide monsters worden vernietigd.");
            }
        }

        private void ResolveAttackAgainstDefense(
            DuelPlayerState attackerPlayer,
            int attackerZone,
            DuelPlayerState defenderPlayer,
            int defenderZone)
        {
            DuelCard attacker = attackerPlayer.MonsterZones[attackerZone];
            DuelCard defender = defenderPlayer.MonsterZones[defenderZone];
            int difference = attacker.CurrentAttack - defender.CurrentDefense;

            if (difference > 0)
            {
                DestroyMonster(defenderPlayer, defenderZone);
                AddLog(defender.Definition.Name + " wordt vernietigd in DEF.");
            }
            else if (difference < 0)
            {
                attackerPlayer.LifePoints -= -difference;
                AddLog(attackerPlayer.Name + " verliest " + (-difference) + " LP door DEF verschil.");
            }
            else
            {
                AddLog("ATK en DEF zijn gelijk. Geen monster wordt vernietigd.");
            }
        }

        private bool ResolveAttackTrap(DuelPlayerState defender, DuelPlayerState attacker, int attackerZone)
        {
            int zone = FindAttackTrapZone(defender, true);
            if (zone < 0)
            {
                zone = FindAttackTrapZone(defender, false);
            }

            if (zone < 0)
            {
                return false;
            }

            DuelCard trap = defender.SpellTrapZones[zone];
            defender.SpellTrapZones[zone] = null;
            trap.IsFaceDown = false;
            AddLog("Chain Link 2 (Spell Speed " + SpellSpeed(trap) + "): " + defender.Name + " activeert trap " + trap.Definition.Name + ".");
            AddLog("Chain resolve: chain links lossen achterstevoren op.");

            DuelLuaExecutionResult luaResult;
            if (TryResolveLuaEffect(trap, defender, attacker.MonsterZones[attackerZone], DuelLuaEvent.AttackTrap, out luaResult))
            {
                SendToGraveyard(defender, trap);
                return luaResult.AttackStopped;
            }

            if (trap.Definition.Effect == DuelEffectKind.DestroyAttacker)
            {
                DestroyMonster(attacker, attackerZone);
                SendToGraveyard(defender, trap);
                AddLog("De aanval stopt en de aanvaller wordt vernietigd.");
                return true;
            }

            DuelCard attackingCard = attacker.MonsterZones[attackerZone];
            if (attackingCard != null)
            {
                attackingCard.HasAttacked = true;
            }

            SendToGraveyard(defender, trap);
            AddLog("De aanval wordt genegeerd.");
            return true;
        }

        private int FindAttackTrapZone(DuelPlayerState defender, bool counterOnly)
        {
            for (int zone = 0; zone < ZoneCount; zone++)
            {
                DuelCard trap = defender.SpellTrapZones[zone];
                if (trap == null || trap.Definition.Kind != DuelCardKind.Trap || !trap.IsFaceDown)
                {
                    continue;
                }

                if (trap.SetTurn >= TurnNumber)
                {
                    continue;
                }

                if (counterOnly && !trap.Definition.IsCounterTrap)
                {
                    continue;
                }

                if (!counterOnly && trap.Definition.IsCounterTrap)
                {
                    continue;
                }

                if (trap.Definition.Effect == DuelEffectKind.NegateAttack ||
                    trap.Definition.Effect == DuelEffectKind.DestroyAttacker ||
                    HasLuaEffect(trap, DuelLuaEvent.AttackTrap))
                {
                    return zone;
                }
            }

            return -1;
        }

        private void ResolveEffect(DuelCard card, DuelPlayerState controller, DuelCard optionalTarget)
        {
            DuelPlayerState opponent = GetOpponent(controller.Id);
            AddLog("Chain resolve: " + card.Definition.Name + ".");

            DuelLuaExecutionResult luaResult;
            if (TryResolveLuaEffect(card, controller, optionalTarget, DuelLuaEvent.Activate, out luaResult))
            {
                ResolveSubtypeExtraEffect(card, controller);
                return;
            }

            switch (card.Definition.Effect)
            {
                case DuelEffectKind.DrawCards:
                    for (int i = 0; i < card.Definition.EffectValue; i++)
                    {
                        DrawCard(controller, true);
                    }
                    break;
                case DuelEffectKind.GainLife:
                    controller.LifePoints += card.Definition.EffectValue;
                    AddLog(controller.Name + " krijgt " + card.Definition.EffectValue + " LP.");
                    break;
                case DuelEffectKind.DealDamage:
                    opponent.LifePoints -= card.Definition.EffectValue;
                    AddLog(opponent.Name + " krijgt " + card.Definition.EffectValue + " effectschade.");
                    CheckLifePointWin();
                    break;
                case DuelEffectKind.BoostOwnMonsters:
                    BoostMonsters(controller, card.Definition.EffectValue);
                    break;
                case DuelEffectKind.DestroyWeakestOpponentMonster:
                    DestroyWeakestMonster(opponent);
                    break;
                case DuelEffectKind.None:
                case DuelEffectKind.NegateAttack:
                case DuelEffectKind.DestroyAttacker:
                default:
                    break;
            }

            ResolveSubtypeExtraEffect(card, controller);
        }

        private void BoostMonsters(DuelPlayerState player, int amount)
        {
            bool boosted = false;
            for (int zone = 0; zone < ZoneCount; zone++)
            {
                DuelCard monster = player.MonsterZones[zone];
                if (monster == null)
                {
                    continue;
                }

                if (monster.IsFaceDown)
                {
                    continue;
                }

                monster.TemporaryAttackBonus += amount;
                boosted = true;
            }

            AddLog(boosted
                ? player.Name + " geeft eigen face-up monsters +" + amount + " ATK tot de End Phase."
                : "Er waren geen face-up monsters om te boosten.");
        }

        private void ResolveSubtypeExtraEffect(DuelCard card, DuelPlayerState controller)
        {
            if (IsGameOver)
            {
                return;
            }

            if (card.Definition.IsQuickPlaySpell)
            {
                AddLog("Quick-Play extra effect: snelle kaart geeft eigen face-up monsters +500 ATK.");
                BoostMonsters(controller, 500);
            }

            if (card.Definition.IsContinuousCard)
            {
                AddLog("Continuous extra effect: zolang deze kaart face-up blijft, triggert hij in je Standby Phase.");
            }
        }

        private void ApplyContinuousStandbyEffects(DuelPlayerState player)
        {
            int continuousCount = 0;
            DuelCard fieldSpell = player.FieldZone;
            if (fieldSpell != null && !fieldSpell.IsFaceDown)
            {
                DuelLuaExecutionResult fieldLuaResult;
                TryResolveLuaEffect(fieldSpell, player, null, DuelLuaEvent.Standby, out fieldLuaResult);
            }

            for (int zone = 0; zone < ZoneCount; zone++)
            {
                DuelCard card = player.SpellTrapZones[zone];
                if (card == null || card.IsFaceDown || !card.Definition.IsContinuousCard)
                {
                    continue;
                }

                DuelLuaExecutionResult luaResult;
                if (TryResolveLuaEffect(card, player, null, DuelLuaEvent.Standby, out luaResult))
                {
                    continue;
                }

                continuousCount++;
            }

            if (continuousCount == 0)
            {
                return;
            }

            int lifeGain = continuousCount * 300;
            int attackBoost = continuousCount * 200;
            player.LifePoints += lifeGain;
            AddLog("Continuous extra effect: " + player.Name + " krijgt " + lifeGain + " LP van " + continuousCount + " face-up Continuous kaart(en).");
            BoostMonsters(player, attackBoost);
        }

        private void DestroyWeakestMonster(DuelPlayerState player)
        {
            int weakestZone = -1;
            int weakestAttack = int.MaxValue;
            for (int zone = 0; zone < ZoneCount; zone++)
            {
                DuelCard monster = player.MonsterZones[zone];
                if (monster == null)
                {
                    continue;
                }

                int attack = monster.CurrentAttack;
                if (attack < weakestAttack)
                {
                    weakestAttack = attack;
                    weakestZone = zone;
                }
            }

            if (weakestZone < 0)
            {
                AddLog("Geen doelwit gevonden voor vernietiging.");
                return;
            }

            AddLog(player.MonsterZones[weakestZone].Definition.Name + " wordt door een effect vernietigd.");
            DestroyMonster(player, weakestZone);
        }

        private void DestroyMonster(DuelPlayerState player, int zone)
        {
            DuelCard monster = player.MonsterZones[zone];
            if (monster == null)
            {
                return;
            }

            player.MonsterZones[zone] = null;
            monster.IsFaceDown = false;
            SendToGraveyard(player, monster);
            AddLog(monster.Definition.Name + " gaat naar de Graveyard.");
        }

        private void SendToGraveyard(DuelPlayerState owner, DuelCard card)
        {
            card.IsFaceDown = false;
            card.TemporaryAttackBonus = 0;
            card.HasAttacked = false;
            card.SetTurn = -1;
            card.SummonedTurn = -1;
            card.PositionChangedTurn = -1;
            owner.Graveyard.Add(card);
        }

        private void DrawStartingHand(DuelPlayerState player)
        {
            for (int i = 0; i < 5; i++)
            {
                DrawCard(player, false);
            }
        }

        private void DrawCard(DuelPlayerState player, bool mandatory)
        {
            if (player.Deck.Count == 0)
            {
                if (mandatory)
                {
                    SetWinner(GetOpponent(player.Id), player.Name + " kan geen verplichte kaart trekken.");
                }

                return;
            }

            DuelCardDefinition definition = player.Deck[0];
            player.Deck.RemoveAt(0);
            player.Hand.Add(new DuelCard(definition, player.Id));
            AddLog(player.Name + " trekt 1 kaart.");
        }

        private void ApplyHandSizeLimit(DuelPlayerState player)
        {
            while (player.Hand.Count > 6)
            {
                DuelCard discarded = player.Hand[player.Hand.Count - 1];
                player.Hand.RemoveAt(player.Hand.Count - 1);
                SendToGraveyard(player, discarded);
                AddLog("Handlimiet: " + player.Name + " discardt " + discarded.Definition.Name + " tot maximaal 6 kaarten.");
            }
        }

        private void CheckLifePointWin()
        {
            if (IsGameOver)
            {
                return;
            }

            if (Player.LifePoints <= 0 && Opponent.LifePoints <= 0)
            {
                SetWinner(null, "Beide spelers bereiken 0 LP.");
            }
            else if (Player.LifePoints <= 0)
            {
                SetWinner(Opponent, "Speler heeft 0 LP.");
            }
            else if (Opponent.LifePoints <= 0)
            {
                SetWinner(Player, "Tegenstander heeft 0 LP.");
            }
        }

        private void SetWinner(DuelPlayerState winner, string reason)
        {
            IsGameOver = true;
            WinnerText = winner == null ? "Gelijkspel" : winner.Name + " wint";
            AddLog(WinnerText + ". Reden: " + reason);
        }

        private void ExpireTemporaryEffects(DuelPlayerState player)
        {
            for (int zone = 0; zone < ZoneCount; zone++)
            {
                DuelCard monster = player.MonsterZones[zone];
                if (monster != null)
                {
                    monster.TemporaryAttackBonus = 0;
                }
            }
        }

        private bool CanChangeMonsterPosition(DuelPlayerState player, int zoneIndex)
        {
            if (!IsValidZoneIndex(zoneIndex))
            {
                return false;
            }

            DuelCard card = player.MonsterZones[zoneIndex];
            if (card == null || card.IsFaceDown || card.Definition.IsLinkMonster)
            {
                return false;
            }

            if (card.SummonedTurn == TurnNumber || card.PositionChangedTurn == TurnNumber || card.HasAttacked)
            {
                return false;
            }

            return card.Position == MonsterBattlePosition.Attack ||
                   card.Position == MonsterBattlePosition.DefenseFaceUp;
        }

        private bool StaysOnFieldAfterActivation(DuelCard card)
        {
            return card != null && card.Definition.IsContinuousSpellOrTrap;
        }

        private bool CanActivateSpellInCurrentPhase(DuelCard card)
        {
            if (card.Definition.IsQuickPlaySpell)
            {
                return true;
            }

            return IsMainPhase();
        }

        private int SpellSpeed(DuelCard card)
        {
            if (card.Definition.IsCounterTrap)
            {
                return 3;
            }

            if (card.Definition.Kind == DuelCardKind.Trap || card.Definition.IsQuickPlaySpell)
            {
                return 2;
            }

            return 1;
        }

        private void ResetAttackFlags(DuelPlayerState player)
        {
            for (int zone = 0; zone < ZoneCount; zone++)
            {
                DuelCard monster = player.MonsterZones[zone];
                if (monster != null)
                {
                    monster.HasAttacked = false;
                }
            }
        }

        private int TributeCount(int level)
        {
            if (level <= 4)
            {
                return 0;
            }

            if (level <= 6)
            {
                return 1;
            }

            return 2;
        }

        private int RequiredTributeCount(DuelPlayerState player, DuelCard card)
        {
            return CanNormalSummonWithoutTributeByCardText(player, card)
                ? 0
                : TributeCount(card.Definition.Level);
        }

        private bool CanNormalSummonWithoutTributeByCardText(DuelPlayerState player, DuelCard card)
        {
            if (player == null || card == null || !card.IsMonster)
            {
                return false;
            }

            string rulesText = card.Definition.RulesText ?? string.Empty;
            if (rulesText.IndexOf("Normal Summon this card without Tributing", StringComparison.OrdinalIgnoreCase) < 0 &&
                rulesText.IndexOf("Normal Summon/Set this card without Tributing", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            if (rulesText.IndexOf("opponent has 2 or more cards in their hand", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return GetOpponent(player.Id).Hand.Count >= 2;
            }

            return true;
        }

        private void TributeFirstMonsters(DuelPlayerState player, int count)
        {
            int remaining = count;
            for (int zone = 0; zone < ZoneCount && remaining > 0; zone++)
            {
                DuelCard monster = player.MonsterZones[zone];
                if (monster == null)
                {
                    continue;
                }

                player.MonsterZones[zone] = null;
                SendToGraveyard(player, monster);
                AddLog(player.Name + " tribute " + monster.Definition.Name + ".");
                remaining--;
            }
        }

        private int FirstEmptyZone(DuelCard[] zones)
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

        private int CountMonsters(DuelPlayerState player)
        {
            int count = 0;
            for (int zone = 0; zone < ZoneCount; zone++)
            {
                if (player.MonsterZones[zone] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsValidHandIndex(DuelPlayerState player, int handIndex)
        {
            return player != null && handIndex >= 0 && handIndex < player.Hand.Count;
        }

        private bool IsValidZoneIndex(int zoneIndex)
        {
            return zoneIndex >= 0 && zoneIndex < ZoneCount;
        }

        private DuelPlayerId OtherPlayer(DuelPlayerId id)
        {
            return id == DuelPlayerId.Player ? DuelPlayerId.Opponent : DuelPlayerId.Player;
        }

        private string DescribeHidden(DuelCard card)
        {
            return card.Definition.Kind == DuelCardKind.Trap ? "een Trap Card" : "een Spell Card";
        }

        private string PhaseName(DuelPhase phase)
        {
            switch (phase)
            {
                case DuelPhase.Draw:
                    return "Draw Phase";
                case DuelPhase.Standby:
                    return "Standby Phase";
                case DuelPhase.Main1:
                    return "Main Phase 1";
                case DuelPhase.Battle:
                    return "Battle Phase";
                case DuelPhase.Main2:
                    return "Main Phase 2";
                case DuelPhase.End:
                    return "End Phase";
                default:
                    return phase.ToString();
            }
        }

        private void AddLog(string message)
        {
            Log.Add(message);
            if (Log.Count > 80)
            {
                Log.RemoveAt(0);
            }
        }

        private DuelPlayerState BuildPlayer(DuelPlayerId id, string name, IList<DuelCardDefinition> starterDeck)
        {
            DuelPlayerState player = new DuelPlayerState(id, name);
            player.LifePoints = 8000;

            if (starterDeck != null && starterDeck.Count > 0)
            {
                for (int i = 0; i < starterDeck.Count; i++)
                {
                    player.Deck.Add(starterDeck[i]);
                }

                return player;
            }

            AddDeckCopies(player.Deck, Monster("Luchtzwaardvechter", 4, 1800, 1200, "Een betrouwbaar Level 4 monster."), 3);
            AddDeckCopies(player.Deck, Monster("Steenwachter", 4, 1300, 2100, "Sterke verdediging voor DEF-gevechten."), 3);
            AddDeckCopies(player.Deck, Monster("Vonkdraak", 4, 1600, 1000, "Snel aanvallend monster."), 3);
            AddDeckCopies(player.Deck, Monster("Kristalwijze", 4, 1500, 1500, "Gebalanceerd monster."), 2);
            AddDeckCopies(player.Deck, Monster("Maanwachter", 4, 1700, 1300, "Extra Level 4 monster voor een rulebook-legaal Main Deck."), 3);
            AddDeckCopies(player.Deck, Monster("Stormsoldaat", 4, 1900, 900, "Aanvallend Level 4 monster."), 3);
            AddDeckCopies(player.Deck, Monster("Rode Wachter", 4, 1400, 1800, "Verdedigend Level 4 monster."), 2);
            AddDeckCopies(player.Deck, Monster("Zonnepaladijn", 5, 2100, 1600, "Level 5: vereist 1 tribute."), 2);
            AddDeckCopies(player.Deck, Monster("Klokridder", 6, 2300, 1500, "Level 6: vereist 1 tribute."), 1);
            AddDeckCopies(player.Deck, Monster("Azuren Titan", 7, 2600, 2100, "Level 7: vereist 2 tributes."), 1);

            AddDeckCopies(player.Deck, Spell("Pot van Momentum", DuelEffectKind.DrawCards, 2, "Trek 2 kaarten."), 2);
            AddDeckCopies(player.Deck, Spell("Vonkstraal", DuelEffectKind.DealDamage, 800, "Doe 800 effectschade."), 2);
            AddDeckCopies(player.Deck, Spell("Strijdbanier", DuelEffectKind.BoostOwnMonsters, 500, "Eigen monsters krijgen +500 ATK tot de End Phase."), 2);
            AddDeckCopies(player.Deck, Spell("Zuiverende Bliksem", DuelEffectKind.DestroyWeakestOpponentMonster, 0, "Vernietig het zwakste monster van de tegenstander."), 1);
            AddDeckCopies(player.Deck, Spell("Herstelritueel", DuelEffectKind.GainLife, 1200, "Krijg 1200 LP."), 1);
            AddDeckCopies(player.Deck, Spell("Tactische Trek", DuelEffectKind.DrawCards, 1, "Trek 1 kaart."), 2);

            AddDeckCopies(player.Deck, Trap("Wachtzegel", DuelEffectKind.NegateAttack, 0, "Negate een aanval na minimaal 1 beurt gezet te zijn."), 2);
            AddDeckCopies(player.Deck, Trap("Breukval", DuelEffectKind.DestroyAttacker, 0, "Vernietig een aanvallend monster na minimaal 1 beurt gezet te zijn."), 2);
            AddDeckCopies(player.Deck, Trap("Verdedigingsval", DuelEffectKind.NegateAttack, 0, "Negate een aanval na minimaal 1 beurt gezet te zijn."), 3);

            return player;
        }

        private DuelCardDefinition Monster(string name, int level, int attack, int defense, string text)
        {
            return new DuelCardDefinition(name, DuelCardKind.Monster, level, attack, defense, DuelEffectKind.None, 0, text);
        }

        private DuelCardDefinition Spell(string name, DuelEffectKind effect, int value, string text)
        {
            return new DuelCardDefinition(name, DuelCardKind.Spell, 0, 0, 0, effect, value, text);
        }

        private DuelCardDefinition Trap(string name, DuelEffectKind effect, int value, string text)
        {
            return new DuelCardDefinition(name, DuelCardKind.Trap, 0, 0, 0, effect, value, text);
        }

        private void AddDeckCopies(List<DuelCardDefinition> deck, DuelCardDefinition card, int copies)
        {
            for (int i = 0; i < copies; i++)
            {
                deck.Add(card);
            }
        }

        private void Shuffle(List<DuelCardDefinition> deck)
        {
            for (int i = deck.Count - 1; i > 0; i--)
            {
                int swapIndex = random.Next(i + 1);
                DuelCardDefinition current = deck[i];
                deck[i] = deck[swapIndex];
                deck[swapIndex] = current;
            }
        }

        private void ActivateBestOpponentSpell(DuelPlayerState opponent)
        {
            for (int i = 0; i < opponent.Hand.Count; i++)
            {
                DuelCard card = opponent.Hand[i];
                if (card.Definition.Kind != DuelCardKind.Spell)
                {
                    continue;
                }

                if (card.Definition.Effect == DuelEffectKind.DestroyWeakestOpponentMonster && CountMonsters(Player) == 0)
                {
                    continue;
                }

                if (card.Definition.Effect == DuelEffectKind.BoostOwnMonsters && CountMonsters(Opponent) == 0)
                {
                    continue;
                }

                if (card.Definition.IsFieldSpell)
                {
                    opponent.Hand.RemoveAt(i);
                    PlaceInFieldZone(opponent, card, false);
                    AddLog("Chain Link 1 (Spell Speed " + SpellSpeed(card) + "): " + opponent.Name + " activeert Field Spell " + card.Definition.Name + ".");
                    ResolveEffect(card, opponent, null);
                    AddLog(card.Definition.Name + " blijft face-up in de Field Zone.");
                    return;
                }

                bool staysOnField = StaysOnFieldAfterActivation(card);
                int zone = FirstEmptyZone(opponent.SpellTrapZones);
                if (zone < 0)
                {
                    continue;
                }

                opponent.Hand.RemoveAt(i);
                card.IsFaceDown = false;
                card.SetTurn = TurnNumber;
                opponent.SpellTrapZones[zone] = card;

                AddLog("Chain Link 1 (Spell Speed " + SpellSpeed(card) + "): " + opponent.Name + " activeert " + card.Definition.Name + ".");
                ResolveEffect(card, opponent, null);
                if (staysOnField)
                {
                    AddLog(card.Definition.Name + " blijft face-up in de Spell/Trap Zone.");
                }
                else
                {
                    opponent.SpellTrapZones[zone] = null;
                    SendToGraveyard(opponent, card);
                }

                return;
            }
        }

        private void ActivateBestOpponentSetSpellTrap(DuelPlayerState opponent)
        {
            for (int zone = 0; zone < ZoneCount; zone++)
            {
                DuelCard card = opponent.SpellTrapZones[zone];
                if (card == null)
                {
                    continue;
                }

                if (!card.Definition.IsContinuousCard && !card.Definition.IsQuickPlaySpell)
                {
                    continue;
                }

                if (card.Definition.Effect == DuelEffectKind.DestroyWeakestOpponentMonster && CountMonsters(Player) == 0)
                {
                    continue;
                }

                if (card.Definition.Effect == DuelEffectKind.BoostOwnMonsters && CountMonsters(opponent) == 0)
                {
                    continue;
                }

                if (ActivateSetSpellTrapFor(opponent, zone))
                {
                    return;
                }
            }
        }

        private void SetFirstOpponentTrap(DuelPlayerState opponent)
        {
            int zone = FirstEmptyZone(opponent.SpellTrapZones);
            if (zone < 0)
            {
                return;
            }

            for (int i = 0; i < opponent.Hand.Count; i++)
            {
                DuelCard card = opponent.Hand[i];
                if (card.Definition.Kind != DuelCardKind.Trap)
                {
                    continue;
                }

                opponent.Hand.RemoveAt(i);
                card.IsFaceDown = true;
                card.SetTurn = TurnNumber;
                opponent.SpellTrapZones[zone] = card;
                AddLog(opponent.Name + " zet een kaart in de Spell/Trap Zone.");
                return;
            }
        }

        private void SummonBestOpponentMonster(DuelPlayerState opponent)
        {
            if (opponent.NormalSummonUsed)
            {
                return;
            }

            int bestHandIndex = -1;
            int bestAttack = -1;
            for (int i = 0; i < opponent.Hand.Count; i++)
            {
                DuelCard card = opponent.Hand[i];
                if (!card.IsMonster || !CanNormalSummon(opponent, i, false))
                {
                    continue;
                }

                if (card.Definition.Attack > bestAttack)
                {
                    bestAttack = card.Definition.Attack;
                    bestHandIndex = i;
                }
            }

            if (bestHandIndex >= 0)
            {
                NormalSummon(opponent, bestHandIndex, false);
            }
        }

        private int ChooseOpponentAttackTarget(DuelCard attacker)
        {
            int bestZone = -1;
            int bestScore = int.MinValue;
            for (int zone = 0; zone < ZoneCount; zone++)
            {
                DuelCard defender = Player.MonsterZones[zone];
                if (defender == null)
                {
                    continue;
                }

                int battleValue = defender.Position == MonsterBattlePosition.Attack
                    ? defender.CurrentAttack
                    : defender.CurrentDefense;
                int score = attacker.CurrentAttack - battleValue;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestZone = zone;
                }
            }

            return bestZone;
        }

        private bool OpponentAttackMonster(int attackerZone, int defenderZone)
        {
            if (ActivePlayerId != DuelPlayerId.Opponent || CurrentPhase != DuelPhase.Battle)
            {
                return false;
            }

            DuelPlayerState attackerPlayer = Opponent;
            DuelPlayerState defenderPlayer = Player;
            DuelCard attacker = attackerPlayer.MonsterZones[attackerZone];
            DuelCard defender = defenderPlayer.MonsterZones[defenderZone];
            if (attacker == null || defender == null || attacker.HasAttacked || attacker.Position != MonsterBattlePosition.Attack)
            {
                return false;
            }

            AddLog("Attack Declaration: " + attacker.Definition.Name + " kiest " + defender.PublicName + " als doelwit.");
            if (ResolveAttackTrap(defenderPlayer, attackerPlayer, attackerZone))
            {
                return true;
            }

            attacker.HasAttacked = true;
            if (defender.IsFaceDown)
            {
                AddLog("Before Damage Calculation: " + defender.PublicName + " wordt face-up geflipt.");
            }

            defender.IsFaceDown = false;
            AddLog("Damage Step: " + attacker.Definition.Name + " valt " + defender.Definition.Name + " aan.");
            if (defender.Position == MonsterBattlePosition.Attack)
            {
                ResolveAttackAgainstAttack(attackerPlayer, attackerZone, defenderPlayer, defenderZone);
            }
            else
            {
                ResolveAttackAgainstDefense(attackerPlayer, attackerZone, defenderPlayer, defenderZone);
            }

            CheckLifePointWin();
            return true;
        }

        private bool OpponentDirectAttack(int attackerZone)
        {
            if (ActivePlayerId != DuelPlayerId.Opponent || CurrentPhase != DuelPhase.Battle || CountMonsters(Player) > 0)
            {
                return false;
            }

            DuelCard attacker = Opponent.MonsterZones[attackerZone];
            if (attacker == null || attacker.HasAttacked || attacker.Position != MonsterBattlePosition.Attack)
            {
                return false;
            }

            AddLog("Attack Declaration: " + attacker.Definition.Name + " valt direct aan.");
            if (ResolveAttackTrap(Player, Opponent, attackerZone))
            {
                return true;
            }

            attacker.HasAttacked = true;
            Player.LifePoints -= Math.Max(0, attacker.CurrentAttack);
            AddLog("Damage Step: " + attacker.Definition.Name + " doet " + attacker.CurrentAttack + " directe schade.");
            CheckLifePointWin();
            return true;
        }
    }
}
