using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelGame
    {
        private const int MaidenOfWhiteCardId = 17947697;
        private const int TrueLightCardId = 62089826;
        private const int BlueEyesWhiteDragonCardId = 89631139;
        private const int BlueEyesUltimateDragonCardId = 23995346;
        private const int DragonMasterMagiaCardId = 12381100;
        private const int BlueEyesChaosMaxDragonCardId = 55410871;
        private const int TrueLightSummonOptionId = 1;
        private const int TrueLightSetOptionId = 2;
        private const string TrueLightOnceKey = "true_light_clean";

        private enum CleanActivationSource
        {
            Hand,
            MonsterZone,
            SpellTrapZone
        }

        private readonly System.Random random = new System.Random();
        private readonly CleanLuaEffectBridge luaBridge = new CleanLuaEffectBridge();

        public readonly CleanDuelState State = new CleanDuelState();

        public event Action Changed;

        public CleanPlayerState CurrentPlayer
        {
            get { return Player(State.TurnPlayer); }
        }

        public CleanPlayerState WaitingPlayer
        {
            get { return OpponentOf(State.TurnPlayer); }
        }

        public void StartNewDuel()
        {
            ResetPlayer(State.Local);
            ResetPlayer(State.Rival);
            State.Log.Clear();
            State.TurnPlayer = CleanPlayerId.Local;
            State.Phase = CleanDuelPhase.Draw;
            State.TurnNumber = 1;
            State.GameOver = false;
            State.PendingChoice = null;
            State.WinnerText = string.Empty;

            FillDeck(State.Local);
            FillDeck(State.Rival);
            luaBridge.Initialize();
            Draw(State.Local, 5);
            Draw(State.Rival, 5);
            Log("New clean duel started.");
            Log(luaBridge.Status);
            Log("Draw phase: use Next Phase to enter Main.");
            NotifyChanged();
        }

        public CleanPlayerState Player(CleanPlayerId id)
        {
            return id == CleanPlayerId.Local ? State.Local : State.Rival;
        }

        public CleanPlayerState OpponentOf(CleanPlayerId id)
        {
            return id == CleanPlayerId.Local ? State.Rival : State.Local;
        }

        public bool CanUseLocalActions()
        {
            return !State.GameOver && State.PendingChoice == null && State.TurnPlayer == CleanPlayerId.Local;
        }

        
        public string DescribeLua(CleanCard card)
        {
            if (IsNormalMonster(card))
            {
                return string.Empty;
            }

            return luaBridge.Describe(card);
        }

        public bool CanNormalSummon(int handIndex)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Main || State.Local.NormalSummonUsed)
            {
                return false;
            }

            return CanNormalPlayMonster(State.Local, CardInHand(State.Local, handIndex));
        }

        public bool CanSetMonster(int handIndex)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Main || State.Local.NormalSummonUsed)
            {
                return false;
            }

            return CanNormalPlayMonster(State.Local, CardInHand(State.Local, handIndex));
        }

        public bool CanSetSpellTrap(int handIndex)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Main)
            {
                return false;
            }

            CleanCard card = CardInHand(State.Local, handIndex);
            return card != null &&
                   card.Definition.Kind != CleanCardKind.Monster &&
                   FirstEmpty(State.Local.SpellsTraps) >= 0;
        }

        public bool CanActivateHandCard(int handIndex)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Main)
            {
                return false;
            }

            CleanCard card = CardInHand(State.Local, handIndex);
            if (card == null)
            {
                return false;
            }

            return card.Definition.Kind != CleanCardKind.Trap && CanActivateCleanCard(card, State.Local, CleanActivationSource.Hand);
        }

        public bool CanActivateMonsterZoneCard(int zoneIndex)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Main || zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            return card != null && !card.FaceDown && CanActivateCleanCard(card, State.Local, CleanActivationSource.MonsterZone);
        }

        public bool CanActivateSetCard(int zoneIndex)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Main || zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.SpellsTraps[zoneIndex];
            if (card == null)
            {
                return false;
            }

            if (card.Definition.Id == TrueLightCardId)
            {
                return CanOpenTrueLightChoice(State.Local);
            }

            return CanActivateCleanCard(card, State.Local, CleanActivationSource.SpellTrapZone);
        }

        public bool CanAttackWith(int monsterZone)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Battle || monsterZone < 0 || monsterZone >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.Monsters[monsterZone];
            return card != null &&
                   !card.FaceDown &&
                   card.Stance == CleanMonsterStance.Attack &&
                   !card.HasBattled;
        }

        public void NormalSummon(int handIndex)
        {
            if (!CanNormalSummon(handIndex))
            {
                return;
            }

            CleanCard card = State.Local.Hand[handIndex];
            int tributeCount = RequiredTributesForNormalPlay(card);
            if (!TributeForNormalPlay(State.Local, card))
            {
                return;
            }

            int zone = FirstEmpty(State.Local.Monsters);
            State.Local.Hand.RemoveAt(handIndex);
            SummonToZone(State.Local, card, zone, false, CleanMonsterStance.Attack);
            State.Local.NormalSummonUsed = true;
            Log((tributeCount > 0 ? "Tribute summoned " : "Normal summoned ") + card.Definition.Name + ".");
            NotifyChanged();
        }

        public void SetMonster(int handIndex)
        {
            if (!CanSetMonster(handIndex))
            {
                return;
            }

            CleanCard card = State.Local.Hand[handIndex];
            int tributeCount = RequiredTributesForNormalPlay(card);
            if (!TributeForNormalPlay(State.Local, card))
            {
                return;
            }

            int zone = FirstEmpty(State.Local.Monsters);
            State.Local.Hand.RemoveAt(handIndex);
            SummonToZone(State.Local, card, zone, true, CleanMonsterStance.Defense);
            State.Local.NormalSummonUsed = true;
            Log(tributeCount > 0 ? "Tribute set a monster." : "Set a monster.");
            NotifyChanged();
        }

        public void SetSpellTrap(int handIndex)
        {
            if (!CanSetSpellTrap(handIndex))
            {
                return;
            }

            CleanCard card = State.Local.Hand[handIndex];
            int zone = FirstEmpty(State.Local.SpellsTraps);
            State.Local.Hand.RemoveAt(handIndex);
            State.Local.SpellsTraps[zone] = card;
            card.FaceDown = true;
            Log("Set " + card.Definition.Kind + " card.");
            NotifyChanged();
        }

        public void ActivateHandCard(int handIndex)
        {
            if (!CanActivateHandCard(handIndex))
            {
                return;
            }

            CleanCard card = State.Local.Hand[handIndex];
            State.Local.Hand.RemoveAt(handIndex);
            if (ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.Hand))
            {
                State.Local.Graveyard.Add(card);
            }

            NotifyChanged();
        }

        public void ActivateMonsterZoneCard(int zoneIndex)
        {
            if (!CanActivateMonsterZoneCard(zoneIndex))
            {
                return;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            bool sendsSelf = card.Definition.Id == MaidenOfWhiteCardId;
            if (sendsSelf)
            {
                State.Local.Monsters[zoneIndex] = null;
                card.FaceDown = false;
                card.AttackBonus = 0;
                card.HasBattled = false;
            }

            if (ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.MonsterZone) && sendsSelf)
            {
                State.Local.Graveyard.Add(card);
            }

            NotifyChanged();
        }

        public void ActivateSetCard(int zoneIndex)
        {
            if (!CanActivateSetCard(zoneIndex))
            {
                return;
            }

            CleanCard card = State.Local.SpellsTraps[zoneIndex];
            if (card.Definition.Id == TrueLightCardId)
            {
                card.FaceDown = false;
                OpenTrueLightChoice(State.Local);
                NotifyChanged();
                return;
            }

            State.Local.SpellsTraps[zoneIndex] = null;
            card.FaceDown = false;
            if (ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.SpellTrapZone))
            {
                State.Local.Graveyard.Add(card);
            }

            NotifyChanged();
        }

        public void ChoosePendingOption(int optionIndex)
        {
            if (State.PendingChoice == null || optionIndex < 0 || optionIndex >= State.PendingChoice.Options.Count)
            {
                return;
            }

            CleanChoiceOption option = State.PendingChoice.Options[optionIndex];
            if (!option.IsAvailable)
            {
                Log(option.Label + " is not valid: " + option.DisabledReason);
                NotifyChanged();
                return;
            }

            if (State.PendingChoice.SourceCardId == TrueLightCardId)
            {
                ResolveTrueLightChoice(State.Local, option.Id);
            }

            State.PendingChoice = null;
            CheckWin();
            NotifyChanged();
        }

        public void CancelPendingChoice()
        {
            if (State.PendingChoice == null)
            {
                return;
            }

            Log("Choice cancelled.");
            State.PendingChoice = null;
            NotifyChanged();
        }

        public void ToggleLocalMonsterStance(int zoneIndex)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Main || zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            if (card == null || card.SummonedThisTurn || card.FaceDown)
            {
                return;
            }

            card.Stance = card.Stance == CleanMonsterStance.Attack ? CleanMonsterStance.Defense : CleanMonsterStance.Attack;
            Log(card.Definition.Name + " changed stance.");
            NotifyChanged();
        }

        public void Attack(int attackerZone, int targetZone)
        {
            if (!CanAttackWith(attackerZone))
            {
                return;
            }

            CleanCard attacker = State.Local.Monsters[attackerZone];
            if (targetZone < 0)
            {
                if (MonsterCount(State.Rival) > 0)
                {
                    Log("Direct attack blocked: rival still controls a monster.");
                    NotifyChanged();
                    return;
                }

                State.Rival.LifePoints -= attacker.CurrentAttack;
                attacker.HasBattled = true;
                Log(attacker.Definition.Name + " attacks directly for " + attacker.CurrentAttack + ".");
                CheckWin();
                NotifyChanged();
                return;
            }

            if (targetZone >= CleanPlayerState.ZoneCount)
            {
                return;
            }

            CleanCard defender = State.Rival.Monsters[targetZone];
            if (defender == null)
            {
                return;
            }

            ResolveBattle(State.Local, State.Rival, attackerZone, targetZone);
            CheckWin();
            NotifyChanged();
        }

        public void AdvancePhase()
        {
            if (State.GameOver)
            {
                return;
            }

            switch (State.Phase)
            {
                case CleanDuelPhase.Draw:
                    State.Phase = CleanDuelPhase.Main;
                    break;
                case CleanDuelPhase.Main:
                    State.Phase = CleanDuelPhase.Battle;
                    break;
                case CleanDuelPhase.Battle:
                    State.Phase = CleanDuelPhase.End;
                    break;
                default:
                    EndTurn();
                    break;
            }

            Log("Phase: " + State.Phase + ".");
            NotifyChanged();

            if (!State.GameOver && State.TurnPlayer == CleanPlayerId.Rival)
            {
                RunRivalTurn();
            }
        }

        public void EndTurn()
        {
            if (State.GameOver)
            {
                return;
            }

            CleanPlayerState ending = CurrentPlayer;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = ending.Monsters[i];
                if (monster != null)
                {
                    monster.HasBattled = false;
                    monster.SummonedThisTurn = false;
                }
            }

            ending.NormalSummonUsed = false;
            ending.UsedOncePerTurn.Clear();
            State.PendingChoice = null;
            State.TurnPlayer = State.TurnPlayer == CleanPlayerId.Local ? CleanPlayerId.Rival : CleanPlayerId.Local;
            State.TurnNumber++;
            State.Phase = CleanDuelPhase.Draw;
            Draw(CurrentPlayer, 1);
            Log(CurrentPlayer.Name + " draws for turn.");
            NotifyChanged();

            if (!State.GameOver && State.TurnPlayer == CleanPlayerId.Rival)
            {
                RunRivalTurn();
            }
        }

        private void RunRivalTurn()
        {
            if (State.GameOver)
            {
                return;
            }

            State.Phase = CleanDuelPhase.Main;
            RivalPlayMain();
            State.Phase = CleanDuelPhase.Battle;
            RivalBattle();
            EndTurn();
        }

        private void RivalPlayMain()
        {
            CleanPlayerState rival = State.Rival;
            CleanPlayerState local = State.Local;

            int spellIndex = FirstPlayableRivalSpell();
            if (spellIndex >= 0)
            {
                CleanCard spell = rival.Hand[spellIndex];
                rival.Hand.RemoveAt(spellIndex);
                if (ResolveEffect(rival, local, spell, CleanActivationSource.Hand))
                {
                    rival.Graveyard.Add(spell);
                }
            }

            if (rival.NormalSummonUsed)
            {
                return;
            }

            int bestMonster = BestMonsterInHand(rival);
            if (bestMonster >= 0)
            {
                CleanCard card = rival.Hand[bestMonster];
                int tributeCount = RequiredTributesForNormalPlay(card);
                if (!TributeForNormalPlay(rival, card))
                {
                    return;
                }

                rival.Hand.RemoveAt(bestMonster);
                SummonToZone(rival, card, FirstEmpty(rival.Monsters), false, CleanMonsterStance.Attack);
                rival.NormalSummonUsed = true;
                Log((tributeCount > 0 ? "Rival tribute summons " : "Rival summons ") + card.Definition.Name + ".");
            }
        }

        private void RivalBattle()
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard attacker = State.Rival.Monsters[i];
                if (attacker == null || attacker.FaceDown || attacker.HasBattled)
                {
                    continue;
                }

                int target = WeakestLocalMonsterZone();
                if (target >= 0)
                {
                    ResolveBattle(State.Rival, State.Local, i, target);
                }
                else
                {
                    State.Local.LifePoints -= attacker.CurrentAttack;
                    attacker.HasBattled = true;
                    Log("Rival attacks directly for " + attacker.CurrentAttack + ".");
                }

                CheckWin();
                if (State.GameOver)
                {
                    return;
                }
            }
        }

        private int StrongestMonsterZone(CleanPlayerState player)
        {
            int strongestZone = -1;
            int strongestAttack = -1;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && monster.CurrentAttack > strongestAttack)
                {
                    strongestAttack = monster.CurrentAttack;
                    strongestZone = i;
                }
            }

            return strongestZone;
        }

        private int StrongestEffectTargetZone(CleanPlayerState player)
        {
            int strongestZone = -1;
            int strongestAttack = -1;
            bool protectedBlueEyesSkipped = false;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster == null)
                {
                    continue;
                }

                if (IsProtectedByTrueLightFromTargeting(player, monster))
                {
                    protectedBlueEyesSkipped = true;
                    continue;
                }

                if (monster.CurrentAttack > strongestAttack)
                {
                    strongestAttack = monster.CurrentAttack;
                    strongestZone = i;
                }
            }

            if (protectedBlueEyesSkipped)
            {
                Log("True Light protects Blue-Eyes White Dragon from card effect targeting.");
            }

            return strongestZone;
        }

        private bool IsProtectedByTrueLightFromTargeting(CleanPlayerState player, CleanCard monster)
        {
            return monster != null &&
                   !monster.FaceDown &&
                   monster.Definition.Id == BlueEyesWhiteDragonCardId &&
                   HasFaceUpTrueLight(player);
        }

        private bool HasFaceUpTrueLight(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.SpellsTraps[i];
                if (card != null && card.Definition.Id == TrueLightCardId && !card.FaceDown)
                {
                    return true;
                }
            }

            return false;
        }

        private int BestMonsterInList(List<CleanCard> cards)
        {
            int bestIndex = -1;
            int bestAttack = -1;
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCard card = cards[i];
                if (card.Definition.Kind == CleanCardKind.Monster && !IsRitualMonster(card) && card.Definition.Attack > bestAttack)
                {
                    bestAttack = card.Definition.Attack;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private int BestCardInDeckForSearch(CleanPlayerState player)
        {
            int bestIndex = -1;
            int bestScore = -1;
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard card = player.Deck[i];
                int score = card.Definition.Kind == CleanCardKind.Monster ? 100000 + card.Definition.Attack : 50000;
                if (card.Definition.Id == 89631139 || card.Definition.Name.IndexOf("Blue-Eyes", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 250000;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private void ResolveBattle(CleanPlayerState attackingPlayer, CleanPlayerState defendingPlayer, int attackerZone, int defenderZone)
        {
            CleanCard attacker = attackingPlayer.Monsters[attackerZone];
            CleanCard defender = defendingPlayer.Monsters[defenderZone];
            if (attacker == null || defender == null)
            {
                return;
            }

            attacker.HasBattled = true;
            defender.FaceDown = false;

            int defenderPower = defender.Stance == CleanMonsterStance.Attack ? defender.CurrentAttack : defender.CurrentDefense;
            int difference = attacker.CurrentAttack - defenderPower;

            if (defender.Stance == CleanMonsterStance.Attack)
            {
                if (difference > 0)
                {
                    defendingPlayer.LifePoints -= difference;
                    MoveMonsterToGrave(defendingPlayer, defenderZone);
                    Log(attacker.Definition.Name + " destroys " + defender.Definition.Name + " for " + difference + " damage.");
                }
                else if (difference < 0)
                {
                    attackingPlayer.LifePoints += difference;
                    MoveMonsterToGrave(attackingPlayer, attackerZone);
                    Log(attacker.Definition.Name + " is destroyed. " + attackingPlayer.Name + " takes " + (-difference) + " damage.");
                }
                else
                {
                    MoveMonsterToGrave(attackingPlayer, attackerZone);
                    MoveMonsterToGrave(defendingPlayer, defenderZone);
                    Log("Both monsters are destroyed.");
                }
            }
            else if (difference > 0)
            {
                MoveMonsterToGrave(defendingPlayer, defenderZone);
                Log(attacker.Definition.Name + " destroys a defense monster.");
            }
            else if (difference < 0)
            {
                attackingPlayer.LifePoints += difference;
                Log(attackingPlayer.Name + " takes " + (-difference) + " battle damage.");
            }
            else
            {
                Log("The attack bounces off equal defense.");
            }
        }

        internal void LuaLog(string message)
        {
            Log(message);
        }

        internal void LuaDraw(CleanPlayerState player, int count)
        {
            int safeCount = Mathf.Clamp(count, 1, 5);
            Draw(player, safeCount);
            Log(player.Name + " draws " + safeCount + " card(s) by Lua.");
        }

        internal void LuaBoostTeam(CleanPlayerState player, int amount)
        {
            BoostTeam(player, amount);
        }

        internal void LuaDamage(CleanPlayerState player, int amount)
        {
            int safeAmount = Mathf.Max(0, amount);
            player.LifePoints -= safeAmount;
            Log(player.Name + " takes " + safeAmount + " Lua effect damage.");
            CheckWin();
        }

        internal void LuaRecover(CleanPlayerState player, int amount)
        {
            int safeAmount = Mathf.Max(0, amount);
            player.LifePoints += safeAmount;
            Log(player.Name + " recovers " + safeAmount + " LP by Lua.");
        }

        internal int LuaDestroyStrongest(CleanPlayerState player)
        {
            int strongestZone = StrongestEffectTargetZone(player);
            if (strongestZone < 0)
            {
                Log("Lua found no valid monster to target.");
                return 0;
            }

            string name = player.Monsters[strongestZone].PublicName;
            MoveMonsterToGrave(player, strongestZone);
            Log("Lua destroys " + name + ".");
            return 1;
        }

        internal int LuaDestroyAllMonsters(CleanPlayerState player)
        {
            int count = MonsterCount(player);
            if (count == 0)
            {
                Log("Lua found no monsters to destroy.");
                return 0;
            }

            DestroyAllMonsters(player);
            Log("Lua destroys " + count + " monster(s).");
            return count;
        }

        internal bool LuaReviveStrongest(CleanPlayerState player)
        {
            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                Log("Lua revive failed: no open monster zone.");
                return false;
            }

            int bestIndex = BestMonsterInList(player.Graveyard);
            if (bestIndex < 0)
            {
                Log("Lua revive failed: no monster in graveyard.");
                return false;
            }

            CleanCard revived = player.Graveyard[bestIndex];
            player.Graveyard.RemoveAt(bestIndex);
            SummonToZone(player, revived, zone, false, CleanMonsterStance.Attack);
            Log("Lua special summons " + revived.Definition.Name + " from the graveyard.");
            return true;
        }

        internal bool LuaSpecialSummonBestAvailable(CleanPlayerState player)
        {
            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                Log("Lua special summon failed: no open monster zone.");
                return false;
            }

            int handIndex = BestMonsterInList(player.Hand);
            if (handIndex >= 0)
            {
                CleanCard card = player.Hand[handIndex];
                player.Hand.RemoveAt(handIndex);
                SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
                Log("Lua special summons " + card.Definition.Name + " from the hand.");
                return true;
            }

            int graveIndex = BestMonsterInList(player.Graveyard);
            if (graveIndex >= 0)
            {
                CleanCard card = player.Graveyard[graveIndex];
                player.Graveyard.RemoveAt(graveIndex);
                SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
                Log("Lua special summons " + card.Definition.Name + " from the graveyard.");
                return true;
            }

            Log("Lua special summon failed: no monster in hand or graveyard.");
            return false;
        }

        internal bool LuaSearchBestCard(CleanPlayerState player)
        {
            int bestIndex = BestCardInDeckForSearch(player);
            if (bestIndex < 0)
            {
                Log("Lua search failed: no card left in deck.");
                return false;
            }

            CleanCard card = player.Deck[bestIndex];
            player.Deck.RemoveAt(bestIndex);
            card.FaceDown = false;
            player.Hand.Add(card);
            Log("Lua adds " + card.Definition.Name + " from deck to hand.");
            return true;
        }

        internal bool LuaSendDeckMonsterToGrave(CleanPlayerState player)
        {
            int bestIndex = BestMonsterInList(player.Deck);
            if (bestIndex < 0)
            {
                Log("Lua send-to-grave failed: no monster in deck.");
                return false;
            }

            CleanCard card = player.Deck[bestIndex];
            player.Deck.RemoveAt(bestIndex);
            card.FaceDown = false;
            player.Graveyard.Add(card);
            Log("Lua sends " + card.Definition.Name + " from deck to graveyard.");
            return true;
        }

        internal bool LuaPlaceTrueLight(CleanPlayerState player)
        {
            int zone = FirstEmpty(player.SpellsTraps);
            if (zone < 0)
            {
                Log("Maiden of White failed: no open Spell/Trap zone for True Light.");
                return false;
            }

            CleanCard trueLight;
            string source;
            if (!TryTakeTrueLight(player, out trueLight, out source))
            {
                Log("Maiden of White failed: True Light is not in hand, deck, or graveyard.");
                return false;
            }

            trueLight.Controller = player.Id;
            trueLight.FaceDown = false;
            trueLight.AttackBonus = 0;
            trueLight.HasBattled = false;
            player.SpellsTraps[zone] = trueLight;
            Log("Maiden of White places True Light face-up from " + source + ".");
            return true;
        }

        internal bool LuaChangeStrongestMonsterToDefense(CleanPlayerState player)
        {
            int zone = StrongestEffectTargetZone(player);
            if (zone < 0)
            {
                Log("Lua position change failed: no valid monster to target.");
                return false;
            }

            CleanCard card = player.Monsters[zone];
            card.FaceDown = false;
            card.Stance = CleanMonsterStance.Defense;
            Log("Lua changes " + card.Definition.Name + " to defense position.");
            return true;
        }

        private bool CanOpenTrueLightChoice(CleanPlayerState player)
        {
            return !player.UsedOncePerTurn.Contains(TrueLightOnceKey) &&
                   (CanTrueLightSummonBlueEyes(player) || CanTrueLightSetSpellTrap(player));
        }

        private void OpenTrueLightChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            options.Add(BuildTrueLightSummonOption(player));
            options.Add(BuildTrueLightSetOption(player));
            State.PendingChoice = new CleanChoicePrompt(
                TrueLightCardId,
                "True Light",
                "Choose 1 effect to activate.",
                options);
            Log("True Light awaits a choice.");
        }

        private CleanChoiceOption BuildTrueLightSummonOption(CleanPlayerState player)
        {
            string reason;
            bool available = ValidateTrueLightSummonBlueEyes(player, out reason);
            return new CleanChoiceOption(
                TrueLightSummonOptionId,
                "Special Summon Blue-Eyes White Dragon",
                "From your hand or GY.",
                available,
                reason);
        }

        private CleanChoiceOption BuildTrueLightSetOption(CleanPlayerState player)
        {
            string reason;
            bool available = ValidateTrueLightSetSpellTrap(player, out reason);
            return new CleanChoiceOption(
                TrueLightSetOptionId,
                "Set listed Spell/Trap from Deck",
                "Must mention Blue-Eyes White Dragon and have a different name from cards you control and in your GY.",
                available,
                reason);
        }

        private bool CanTrueLightSummonBlueEyes(CleanPlayerState player)
        {
            string reason;
            return ValidateTrueLightSummonBlueEyes(player, out reason);
        }

        private bool ValidateTrueLightSummonBlueEyes(CleanPlayerState player, out string reason)
        {
            if (FirstEmpty(player.Monsters) < 0)
            {
                reason = "no open Monster Zone";
                return false;
            }

            if (IndexOfCard(player.Hand, BlueEyesWhiteDragonCardId) < 0 && IndexOfCard(player.Graveyard, BlueEyesWhiteDragonCardId) < 0)
            {
                reason = "Blue-Eyes White Dragon is not in hand or GY";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private bool CanTrueLightSetSpellTrap(CleanPlayerState player)
        {
            string reason;
            return ValidateTrueLightSetSpellTrap(player, out reason);
        }

        private bool ValidateTrueLightSetSpellTrap(CleanPlayerState player, out string reason)
        {
            if (FirstEmpty(player.SpellsTraps) < 0)
            {
                reason = "no open Spell/Trap Zone";
                return false;
            }

            int index = BestTrueLightSetCandidateIndex(player);
            if (index < 0)
            {
                reason = "no valid listed Spell/Trap in deck";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private void ResolveTrueLightChoice(CleanPlayerState player, int optionId)
        {
            if (optionId == TrueLightSummonOptionId)
            {
                ResolveTrueLightSummonBlueEyes(player);
            }
            else if (optionId == TrueLightSetOptionId)
            {
                ResolveTrueLightSetSpellTrap(player);
            }

            player.UsedOncePerTurn.Add(TrueLightOnceKey);
        }

        private bool ResolveTrueLightSummonBlueEyes(CleanPlayerState player)
        {
            string reason;
            if (!ValidateTrueLightSummonBlueEyes(player, out reason))
            {
                Log("True Light cannot summon: " + reason + ".");
                return false;
            }

            CleanCard card;
            if (!TryTakeCard(player.Graveyard, BlueEyesWhiteDragonCardId, out card) && !TryTakeCard(player.Hand, BlueEyesWhiteDragonCardId, out card))
            {
                Log("True Light cannot find Blue-Eyes White Dragon.");
                return false;
            }

            SummonToZone(player, card, FirstEmpty(player.Monsters), false, CleanMonsterStance.Attack);
            Log("True Light special summons Blue-Eyes White Dragon.");
            return true;
        }

        private bool ResolveTrueLightSetSpellTrap(CleanPlayerState player)
        {
            string reason;
            if (!ValidateTrueLightSetSpellTrap(player, out reason))
            {
                Log("True Light cannot set a Spell/Trap: " + reason + ".");
                return false;
            }

            int deckIndex = BestTrueLightSetCandidateIndex(player);
            int zone = FirstEmpty(player.SpellsTraps);
            CleanCard card = player.Deck[deckIndex];
            player.Deck.RemoveAt(deckIndex);
            card.Controller = player.Id;
            card.FaceDown = true;
            player.SpellsTraps[zone] = card;
            Log("True Light sets " + card.Definition.Name + " from the deck.");
            return true;
        }

        private int BestTrueLightSetCandidateIndex(CleanPlayerState player)
        {
            int bestIndex = -1;
            int bestScore = -1;
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard card = player.Deck[i];
                if (!IsTrueLightSetCandidate(card) || NameExistsOnFieldOrGraveyard(player, card.Definition.Name))
                {
                    continue;
                }

                int score = TrueLightSetPriority(card);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private bool IsTrueLightSetCandidate(CleanCard card)
        {
            if (card.Definition.Kind == CleanCardKind.Monster)
            {
                return false;
            }

            string text = card.Definition.Text ?? string.Empty;
            if (text.IndexOf("Blue-Eyes White Dragon", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            switch (card.Definition.Id)
            {
                case 69145169:
                case 93437091:
                case 80326401:
                case 17725109:
                case 21082832:
                case 71143015:
                case 62089826:
                    return true;
                default:
                    return false;
            }
        }

        private int TrueLightSetPriority(CleanCard card)
        {
            switch (card.Definition.Id)
            {
                case 71143015:
                    return 100;
                case 17725109:
                    return 90;
                case 80326401:
                    return 80;
                case 93437091:
                    return 70;
                case 69145169:
                    return 60;
                case 21082832:
                    return 50;
                default:
                    return 10;
            }
        }

        private bool NameExistsOnFieldOrGraveyard(CleanPlayerState player, string name)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null && string.Equals(player.Monsters[i].Definition.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (player.SpellsTraps[i] != null && string.Equals(player.SpellsTraps[i].Definition.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            for (int i = 0; i < player.Graveyard.Count; i++)
            {
                if (string.Equals(player.Graveyard[i].Definition.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        private bool CanActivateCleanCard(CleanCard card, CleanPlayerState player, CleanActivationSource source)
        {
            if (card == null || IsNormalMonster(card) || string.IsNullOrEmpty(card.Definition.EffectKey))
            {
                return false;
            }

            if (IsRitualMonster(card))
            {
                return false;
            }

            switch (card.Definition.EffectKey)
            {
                case "maiden_white":
                    return CanPlaceTrueLight(player);
                case "blue_eyes_clean":
                case "jet_clean":
                case "bystial_clean":
                case "bystial_search_clean":
                    return source != CleanActivationSource.Hand || FirstEmpty(player.Monsters) >= 0;
                case "one_for_one_clean":
                case "mausoleum_clean":
                    return FirstEmpty(player.Monsters) >= 0 && HasMatchingCard(player.Deck, IsLevelOneMonster);
                case "chaos_form_clean":
                    return CanRitualSummonWithChaosForm(player);
                case "ultimate_fusion_clean":
                    return CanFusionSummonFromExtra(player, DragonMasterMagiaCardId);
                case "spirit_dragon_clean":
                    return source == CleanActivationSource.MonsterZone;
                default:
                    return true;
            }
        }

        private bool CanPlaceTrueLight(CleanPlayerState player)
        {
            return FirstEmpty(player.SpellsTraps) >= 0 &&
                   (IndexOfCard(player.Hand, TrueLightCardId) >= 0 ||
                    IndexOfCard(player.Deck, TrueLightCardId) >= 0 ||
                    IndexOfCard(player.Graveyard, TrueLightCardId) >= 0);
        }

        private bool ResolveCleanEffect(CleanPlayerState controller, CleanPlayerState rival, CleanCard card, CleanActivationSource source, out bool resolved)
        {
            resolved = true;
            switch (card.Definition.EffectKey)
            {
                case "blue_eyes_clean":
                    return ActivateSelfSummonOrBoost(controller, card, source, 300);
                case "maiden_white":
                    LuaPlaceTrueLight(controller);
                    return true;
                case "sage_clean":
                    LuaSearchBestBlueEyes(controller);
                    return source != CleanActivationSource.MonsterZone;
                case "stone_ancients_clean":
                    LuaSpecialSummonBestBlueEyes(controller);
                    return source != CleanActivationSource.MonsterZone;
                case "maiden_with_eyes_clean":
                    LuaSpecialSummonBestBlueEyes(controller);
                    return source != CleanActivationSource.MonsterZone;
                case "veiler_clean":
                    LuaChangeStrongestMonsterToDefense(rival);
                    return source != CleanActivationSource.MonsterZone;
                case "ash_clean":
                    DestroyOrPressure(rival, 500);
                    return source != CleanActivationSource.MonsterZone;
                case "droll_clean":
                    LuaDraw(controller, 1);
                    LuaDamage(rival, 500);
                    return source != CleanActivationSource.MonsterZone;
                case "dictator_clean":
                    LuaSendDeckMonsterToGrave(controller);
                    LuaSpecialSummonBestBlueEyes(controller);
                    return source != CleanActivationSource.MonsterZone;
                case "abyss_clean":
                    LuaSearchBestBlueEyes(controller);
                    return source != CleanActivationSource.MonsterZone;
                case "jet_clean":
                    return ActivateSelfSummonOrBoost(controller, card, source, 400);
                case "bystial_clean":
                    if (source == CleanActivationSource.Hand && LuaSpecialSummonSpecific(controller, card))
                    {
                        return false;
                    }

                    DestroyOrPressure(rival, 700);
                    return source != CleanActivationSource.MonsterZone;
                case "bystial_search_clean":
                    if (source == CleanActivationSource.Hand && LuaSpecialSummonSpecific(controller, card))
                    {
                        LuaSearchBestBlueEyes(controller);
                        return false;
                    }

                    LuaSearchBestBlueEyes(controller);
                    return source != CleanActivationSource.MonsterZone;
                case "one_for_one_clean":
                    LuaSpecialSummonBestLevelOne(controller);
                    return true;
                case "successor_soul_clean":
                    DestroyOrPressure(rival, 800);
                    LuaSpecialSummonBestBlueEyes(controller);
                    return true;
                case "bingo_clean":
                    LuaSearchBestBlueEyes(controller);
                    return true;
                case "wishes_clean":
                    LuaSearchBestBlueEyes(controller);
                    LuaSendDeckMonsterToGrave(controller);
                    return true;
                case "roar_clean":
                    LuaSpecialSummonBestBlueEyes(controller);
                    return true;
                case "dark_piercing_clean":
                    if (LuaDestroyAllMonsters(rival) == 0)
                    {
                        LuaDraw(controller, 1);
                    }
                    return true;
                case "mausoleum_clean":
                    if (!LuaSpecialSummonBestLevelOne(controller))
                    {
                        LuaBoostTeam(controller, 500);
                    }
                    return true;
                case "chaos_form_clean":
                    LuaRitualSummonWithChaosForm(controller);
                    return true;
                case "dragon_shrine_clean":
                    LuaSendDeckMonsterToGrave(controller);
                    return true;
                case "ultimate_fusion_clean":
                    int fieldBlueEyesMaterialCount;
                    if (!LuaFusionSummonBestAvailable(controller, DragonMasterMagiaCardId, true, out fieldBlueEyesMaterialCount))
                    {
                        return true;
                    }

                    for (int i = 0; i < fieldBlueEyesMaterialCount; i++)
                    {
                        LuaDestroyStrongest(rival);
                    }

                    return true;
                case "true_light_clean":
                    resolved = false;
                    return true;
                case "ultimate_dragon_clean":
                    DestroyOrPressure(rival, 1200);
                    return false;
                case "magia_clean":
                    DestroyOrPressure(rival, 1500);
                    LuaDraw(controller, 1);
                    LuaBoostTeam(controller, 500);
                    return false;
                case "spirit_dragon_clean":
                    if (!LuaSpecialSummonFromExtra(controller, 89604813))
                    {
                        LuaChangeStrongestMonsterToDefense(rival);
                    }
                    return false;
                case "ultimate_spirit_clean":
                    LuaBoostTeam(controller, 800);
                    DestroyOrPressure(rival, 1000);
                    return false;
                case "spirit_with_eyes_clean":
                    LuaSearchBestBlueEyes(controller);
                    return false;
                default:
                    resolved = false;
                    return true;
            }
        }

        private bool ActivateSelfSummonOrBoost(CleanPlayerState controller, CleanCard card, CleanActivationSource source, int boostAmount)
        {
            if (source == CleanActivationSource.Hand && LuaSpecialSummonSpecific(controller, card))
            {
                return false;
            }

            LuaBoostTeam(controller, boostAmount);
            return source != CleanActivationSource.MonsterZone;
        }

        private void DestroyOrPressure(CleanPlayerState player, int damageIfEmpty)
        {
            if (LuaDestroyStrongest(player) == 0 && MonsterCount(player) == 0)
            {
                LuaDamage(player, damageIfEmpty);
            }
        }

        internal bool LuaSpecialSummonSpecific(CleanPlayerState player, CleanCard card)
        {
            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                Log("Clean summon failed: no open monster zone.");
                return false;
            }

            SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
            Log("Clean special summons " + card.Definition.Name + ".");
            return true;
        }

        internal bool LuaSpecialSummonBestBlueEyes(CleanPlayerState player)
        {
            CleanCard card;
            string source;
            if (!TryTakeBestMonsterFromZones(player, IsBlueEyesMonster, out card, out source))
            {
                Log("Clean Blue-Eyes summon failed: no Blue-Eyes monster found.");
                return false;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                Log("Clean Blue-Eyes summon failed: no open monster zone.");
                ReturnCardToSource(player, card, source);
                return false;
            }

            SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
            Log("Clean special summons " + card.Definition.Name + " from " + source + ".");
            return true;
        }

        internal bool LuaSpecialSummonBestLevelOne(CleanPlayerState player)
        {
            CleanCard card;
            string source;
            if (!TryTakeBestMonsterFromZones(player, IsLevelOneMonster, out card, out source))
            {
                Log("Clean Level 1 summon failed: no Level 1 monster found.");
                return false;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                Log("Clean Level 1 summon failed: no open monster zone.");
                ReturnCardToSource(player, card, source);
                return false;
            }

            SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
            Log("Clean special summons " + card.Definition.Name + " from " + source + ".");
            return true;
        }

        private sealed class RitualMaterialCandidate
        {
            public CleanCard Card;
            public List<CleanCard> List;
            public int ZoneIndex = -1;

            public bool IsMonsterZone
            {
                get { return ZoneIndex >= 0; }
            }
        }

        private bool CanRitualSummonWithChaosForm(CleanPlayerState player)
        {
            int ritualIndex;
            List<RitualMaterialCandidate> materials;
            return TryFindChaosFormRitualSummon(player, out ritualIndex, out materials);
        }

        internal bool LuaRitualSummonWithChaosForm(CleanPlayerState player)
        {
            int ritualIndex;
            List<RitualMaterialCandidate> materials;
            if (!TryFindChaosFormRitualSummon(player, out ritualIndex, out materials))
            {
                Log("Chaos Form failed: no Ritual Monster, exact-level tribute, or open Monster Zone.");
                return false;
            }

            CleanCard ritualMonster = player.Hand[ritualIndex];
            player.Hand.RemoveAt(ritualIndex);
            MoveRitualMaterialsToGrave(player, materials);
            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                player.Hand.Add(ritualMonster);
                Log("Chaos Form failed: no open Monster Zone after tributes.");
                return false;
            }

            SummonToZone(player, ritualMonster, zone, false, CleanMonsterStance.Attack);
            Log("Chaos Form Ritual Summons " + ritualMonster.Definition.Name + " using " + materials.Count + " tribute(s).");
            return true;
        }

        private bool TryFindChaosFormRitualSummon(CleanPlayerState player, out int ritualIndex, out List<RitualMaterialCandidate> materials)
        {
            ritualIndex = -1;
            materials = null;
            int bestAttack = -1;
            for (int i = 0; i < player.Hand.Count; i++)
            {
                CleanCard ritualMonster = player.Hand[i];
                if (!IsChaosFormRitualTarget(ritualMonster))
                {
                    continue;
                }

                List<RitualMaterialCandidate> pool = BuildRitualMaterialPool(player, ritualMonster);
                List<RitualMaterialCandidate> selected;
                if (!TrySelectExactRitualMaterials(pool, ritualMonster.Definition.Level, out selected) || !HasRitualSummonSpace(player, selected))
                {
                    continue;
                }

                if (ritualMonster.Definition.Attack > bestAttack)
                {
                    bestAttack = ritualMonster.Definition.Attack;
                    ritualIndex = i;
                    materials = selected;
                }
            }

            return ritualIndex >= 0;
        }

        private List<RitualMaterialCandidate> BuildRitualMaterialPool(CleanPlayerState player, CleanCard ritualMonster)
        {
            List<RitualMaterialCandidate> pool = new List<RitualMaterialCandidate>();
            for (int i = 0; i < player.Hand.Count; i++)
            {
                CleanCard card = player.Hand[i];
                if (!object.ReferenceEquals(card, ritualMonster) && IsRitualTributeMaterial(card))
                {
                    pool.Add(new RitualMaterialCandidate { Card = card, List = player.Hand });
                }
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.Monsters[i];
                if (IsRitualTributeMaterial(card))
                {
                    pool.Add(new RitualMaterialCandidate { Card = card, ZoneIndex = i });
                }
            }

            return pool;
        }

        private bool TrySelectExactRitualMaterials(List<RitualMaterialCandidate> pool, int requiredLevel, out List<RitualMaterialCandidate> materials)
        {
            materials = null;
            List<RitualMaterialCandidate> selected = new List<RitualMaterialCandidate>();
            return TrySelectExactRitualMaterials(pool, requiredLevel, 0, 0, selected, out materials);
        }

        private bool TrySelectExactRitualMaterials(List<RitualMaterialCandidate> pool, int requiredLevel, int start, int currentLevel, List<RitualMaterialCandidate> selected, out List<RitualMaterialCandidate> materials)
        {
            if (currentLevel == requiredLevel)
            {
                materials = new List<RitualMaterialCandidate>(selected);
                return true;
            }

            if (currentLevel > requiredLevel)
            {
                materials = null;
                return false;
            }

            for (int i = start; i < pool.Count; i++)
            {
                int level = Mathf.Max(0, pool[i].Card.Definition.Level);
                if (level <= 0)
                {
                    continue;
                }

                selected.Add(pool[i]);
                if (TrySelectExactRitualMaterials(pool, requiredLevel, i + 1, currentLevel + level, selected, out materials))
                {
                    return true;
                }

                selected.RemoveAt(selected.Count - 1);
            }

            materials = null;
            return false;
        }

        private bool HasRitualSummonSpace(CleanPlayerState player, List<RitualMaterialCandidate> materials)
        {
            if (FirstEmpty(player.Monsters) >= 0)
            {
                return true;
            }

            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].IsMonsterZone)
                {
                    return true;
                }
            }

            return false;
        }

        private void MoveRitualMaterialsToGrave(CleanPlayerState player, List<RitualMaterialCandidate> materials)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                RitualMaterialCandidate material = materials[i];
                CleanCard card = material.Card;
                if (material.IsMonsterZone)
                {
                    player.Monsters[material.ZoneIndex] = null;
                }
                else if (material.List != null)
                {
                    material.List.Remove(card);
                }

                ResetCardForHiddenZone(card);
                player.Graveyard.Add(card);
            }
        }

        private bool IsChaosFormRitualTarget(CleanCard card)
        {
            if (!IsRitualMonster(card))
            {
                return false;
            }

            string name = card.Definition.Name ?? string.Empty;
            return name.IndexOf("Chaos", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Black Luster Soldier", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsRitualTributeMaterial(CleanCard card)
        {
            return card != null && card.Definition.Kind == CleanCardKind.Monster && !IsRitualMonster(card);
        }
        private sealed class FusionMaterialCandidate
        {
            public CleanCard Card;
            public List<CleanCard> List;
            public int ZoneIndex = -1;
            public string Source;

            public bool IsMonsterZone
            {
                get { return ZoneIndex >= 0; }
            }
        }

        private bool CanFusionSummonFromExtra(CleanPlayerState player, int preferredId)
        {
            int extraIndex;
            List<FusionMaterialCandidate> materials;
            return TryFindFusionSummon(player, preferredId, out extraIndex, out materials);
        }

        internal bool LuaFusionSummonBestAvailable(CleanPlayerState player, int preferredId, bool materialsToDeck)
        {
            int ignored;
            return LuaFusionSummonBestAvailable(player, preferredId, materialsToDeck, out ignored);
        }

        internal bool LuaFusionSummonBestAvailable(CleanPlayerState player, int preferredId, bool materialsToDeck, out int fieldBlueEyesMaterialCount)
        {
            fieldBlueEyesMaterialCount = 0;
            int extraIndex;
            List<FusionMaterialCandidate> materials;
            if (!TryFindFusionSummon(player, preferredId, out extraIndex, out materials))
            {
                Log("Fusion summon failed: no valid Fusion Monster, materials, or open Monster Zone.");
                return false;
            }

            CleanCard fusionMonster = player.ExtraDeck[extraIndex];
            player.ExtraDeck.RemoveAt(extraIndex);
            fieldBlueEyesMaterialCount = CountFieldBlueEyesMaterials(materials);
            MoveFusionMaterials(player, materials, materialsToDeck);

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                player.ExtraDeck.Add(fusionMonster);
                Log("Fusion summon failed: no open Monster Zone after materials were used.");
                return false;
            }

            SummonToZone(player, fusionMonster, zone, false, CleanMonsterStance.Attack);
            Log("Clean Fusion Summons " + fusionMonster.Definition.Name + " using " + materials.Count + " material(s).");
            return true;
        }

        private bool TryFindFusionSummon(CleanPlayerState player, int preferredId, out int extraIndex, out List<FusionMaterialCandidate> materials)
        {
            extraIndex = -1;
            materials = null;

            if (preferredId > 0)
            {
                int preferredIndex = IndexOfCard(player.ExtraDeck, preferredId);
                if (preferredIndex >= 0 && TrySelectFusionMaterials(player, player.ExtraDeck[preferredIndex], out materials) && HasFusionSummonSpace(player, materials))
                {
                    extraIndex = preferredIndex;
                    return true;
                }
            }

            int bestAttack = -1;
            List<FusionMaterialCandidate> bestMaterials = null;
            for (int i = 0; i < player.ExtraDeck.Count; i++)
            {
                CleanCard candidate = player.ExtraDeck[i];
                List<FusionMaterialCandidate> candidateMaterials;
                if (!IsFusionSummonCandidate(candidate) || !TrySelectFusionMaterials(player, candidate, out candidateMaterials) || !HasFusionSummonSpace(player, candidateMaterials))
                {
                    continue;
                }

                if (candidate.Definition.Attack > bestAttack)
                {
                    bestAttack = candidate.Definition.Attack;
                    extraIndex = i;
                    bestMaterials = candidateMaterials;
                }
            }

            materials = bestMaterials;
            return extraIndex >= 0;
        }

        private bool TrySelectFusionMaterials(CleanPlayerState player, CleanCard fusionMonster, out List<FusionMaterialCandidate> materials)
        {
            List<FusionMaterialCandidate> pool = BuildFusionMaterialPool(player);
            materials = new List<FusionMaterialCandidate>();

            switch (fusionMonster.Definition.Id)
            {
                case BlueEyesUltimateDragonCardId:
                    return TryAddMaterials(pool, materials, IsBlueEyesWhiteDragonMaterial, 3);
                case DragonMasterMagiaCardId:
                    return TrySelectDragonMasterMagiaMaterials(pool, materials);
                default:
                    return TryAddMaterials(pool, materials, IsFusionMaterialMonster, 2);
            }
        }

        private bool TrySelectDragonMasterMagiaMaterials(List<FusionMaterialCandidate> pool, List<FusionMaterialCandidate> materials)
        {
            materials.Clear();
            if (TryAddMaterials(pool, materials, IsBlueEyesUltimateDragonMaterial, 1) && TryAddMaterials(pool, materials, IsChaosRitualMaterial, 2))
            {
                return true;
            }

            materials.Clear();
            return TryAddMaterials(pool, materials, IsBlueEyesMonster, 3) && TryAddMaterials(pool, materials, IsChaosRitualMaterial, 4);
        }

        private List<FusionMaterialCandidate> BuildFusionMaterialPool(CleanPlayerState player)
        {
            List<FusionMaterialCandidate> pool = new List<FusionMaterialCandidate>();
            AddFusionMaterialsFromList(pool, player.Hand, "hand");
            AddFusionMaterialsFromList(pool, player.Graveyard, "graveyard");

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.Monsters[i];
                if (IsFusionMaterialMonster(card))
                {
                    pool.Add(new FusionMaterialCandidate { Card = card, ZoneIndex = i, Source = "field" });
                }
            }

            return pool;
        }

        private void AddFusionMaterialsFromList(List<FusionMaterialCandidate> pool, List<CleanCard> cards, string source)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCard card = cards[i];
                if (IsFusionMaterialMonster(card))
                {
                    pool.Add(new FusionMaterialCandidate { Card = card, List = cards, Source = source });
                }
            }
        }

        private bool TryAddMaterials(List<FusionMaterialCandidate> pool, List<FusionMaterialCandidate> selected, Predicate<CleanCard> predicate, int count)
        {
            for (int i = 0; i < pool.Count && selected.Count < count; i++)
            {
                FusionMaterialCandidate candidate = pool[i];
                if (!ContainsMaterial(selected, candidate.Card) && predicate(candidate.Card))
                {
                    selected.Add(candidate);
                }
            }

            return selected.Count >= count;
        }

        private bool ContainsMaterial(List<FusionMaterialCandidate> selected, CleanCard card)
        {
            for (int i = 0; i < selected.Count; i++)
            {
                if (object.ReferenceEquals(selected[i].Card, card))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasFusionSummonSpace(CleanPlayerState player, List<FusionMaterialCandidate> materials)
        {
            if (FirstEmpty(player.Monsters) >= 0)
            {
                return true;
            }

            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].IsMonsterZone)
                {
                    return true;
                }
            }

            return false;
        }

        private void MoveFusionMaterials(CleanPlayerState player, List<FusionMaterialCandidate> materials, bool materialsToDeck)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                FusionMaterialCandidate material = materials[i];
                CleanCard card = material.Card;
                if (material.IsMonsterZone)
                {
                    player.Monsters[material.ZoneIndex] = null;
                }
                else if (material.List != null)
                {
                    material.List.Remove(card);
                }

                ResetCardForHiddenZone(card);
                if (materialsToDeck)
                {
                    if (IsFusionMonsterDefinition(card.Definition))
                    {
                        player.ExtraDeck.Add(card);
                    }
                    else
                    {
                        player.Deck.Add(card);
                    }
                }
                else
                {
                    player.Graveyard.Add(card);
                }
            }
        }

        private void ResetCardForHiddenZone(CleanCard card)
        {
            card.FaceDown = false;
            card.AttackBonus = 0;
            card.HasBattled = false;
            card.SummonedThisTurn = false;
        }

        private int CountFieldBlueEyesMaterials(List<FusionMaterialCandidate> materials)
        {
            int count = 0;
            for (int i = 0; i < materials.Count; i++)
            {
                CleanCard card = materials[i].Card;
                if (materials[i].IsMonsterZone && (card.Definition.Id == BlueEyesWhiteDragonCardId || card.Definition.Id == BlueEyesUltimateDragonCardId))
                {
                    count++;
                }
            }

            return count;
        }

        private bool IsFusionSummonCandidate(CleanCard card)
        {
            return card != null && card.Definition.Kind == CleanCardKind.Monster && IsFusionMonsterDefinition(card.Definition);
        }

        private bool IsFusionMonsterDefinition(CleanCardDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            if (definition.Id == BlueEyesUltimateDragonCardId || definition.Id == DragonMasterMagiaCardId)
            {
                return true;
            }

            string typeLine = definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Fusion", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsFusionMaterialMonster(CleanCard card)
        {
            return card != null && card.Definition.Kind == CleanCardKind.Monster;
        }

        private bool IsBlueEyesWhiteDragonMaterial(CleanCard card)
        {
            return card != null && card.Definition.Id == BlueEyesWhiteDragonCardId;
        }

        private bool IsBlueEyesUltimateDragonMaterial(CleanCard card)
        {
            return card != null && card.Definition.Id == BlueEyesUltimateDragonCardId;
        }

        private bool IsChaosRitualMaterial(CleanCard card)
        {
            if (card == null)
            {
                return false;
            }

            string name = card.Definition.Name ?? string.Empty;
            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Ritual", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Chaos", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Black Luster Soldier", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        internal bool LuaSpecialSummonFromExtra(CleanPlayerState player, int preferredId)
        {
            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                Log("Extra Deck summon failed: no open monster zone.");
                return false;
            }

            int index = IndexOfCard(player.ExtraDeck, preferredId);
            if (index < 0)
            {
                index = BestMonsterInList(player.ExtraDeck);
            }

            if (index < 0)
            {
                Log("Extra Deck summon failed: no monster left in Extra Deck.");
                return false;
            }

            CleanCard card = player.ExtraDeck[index];
            player.ExtraDeck.RemoveAt(index);
            SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
            Log("Clean special summons " + card.Definition.Name + " from the Extra Deck.");
            return true;
        }

        internal bool LuaSearchBestBlueEyes(CleanPlayerState player)
        {
            int bestIndex = BestCardInDeckForSearch(player, IsBlueEyesCard);
            if (bestIndex < 0)
            {
                return LuaSearchBestCard(player);
            }

            CleanCard card = player.Deck[bestIndex];
            player.Deck.RemoveAt(bestIndex);
            card.FaceDown = false;
            player.Hand.Add(card);
            Log("Clean adds " + card.Definition.Name + " from deck to hand.");
            return true;
        }

        private bool TryTakeBestMonsterFromZones(CleanPlayerState player, Predicate<CleanCard> predicate, out CleanCard card, out string source)
        {
            if (TryTakeBestMonster(player.Graveyard, predicate, out card))
            {
                source = "graveyard";
                return true;
            }

            if (TryTakeBestMonster(player.Deck, predicate, out card))
            {
                source = "deck";
                return true;
            }

            if (TryTakeBestMonster(player.Hand, predicate, out card))
            {
                source = "hand";
                return true;
            }

            source = string.Empty;
            return false;
        }

        private bool TryTakeBestMonster(List<CleanCard> cards, Predicate<CleanCard> predicate, out CleanCard card)
        {
            int bestIndex = -1;
            int bestAttack = -1;
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCard candidate = cards[i];
                if (candidate.Definition.Kind != CleanCardKind.Monster || !predicate(candidate))
                {
                    continue;
                }

                if (candidate.Definition.Attack > bestAttack)
                {
                    bestAttack = candidate.Definition.Attack;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                card = null;
                return false;
            }

            card = cards[bestIndex];
            cards.RemoveAt(bestIndex);
            return true;
        }

        private void ReturnCardToSource(CleanPlayerState player, CleanCard card, string source)
        {
            if (string.Equals(source, "graveyard", StringComparison.OrdinalIgnoreCase))
            {
                player.Graveyard.Add(card);
            }
            else if (string.Equals(source, "hand", StringComparison.OrdinalIgnoreCase))
            {
                player.Hand.Add(card);
            }
            else
            {
                player.Deck.Add(card);
            }
        }

        private bool HasMatchingCard(List<CleanCard> cards, Predicate<CleanCard> predicate)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (predicate(cards[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsBlueEyesCard(CleanCard card)
        {
            return card.Definition.Id == 89631139 ||
                   card.Definition.Name.IndexOf("Blue-Eyes", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   card.Definition.Name.IndexOf("with Eyes of Blue", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsBlueEyesMonster(CleanCard card)
        {
            return card.Definition.Kind == CleanCardKind.Monster && !IsRitualMonster(card) && IsBlueEyesCard(card);
        }

        private bool IsLevelOneMonster(CleanCard card)
        {
            return card.Definition.Kind == CleanCardKind.Monster && card.Definition.Level <= 1;
        }

        private bool TryTakeTrueLight(CleanPlayerState player, out CleanCard card, out string source)
        {
            if (TryTakeCard(player.Hand, TrueLightCardId, out card))
            {
                source = "hand";
                return true;
            }

            if (TryTakeCard(player.Deck, TrueLightCardId, out card))
            {
                source = "deck";
                return true;
            }

            if (TryTakeCard(player.Graveyard, TrueLightCardId, out card))
            {
                source = "graveyard";
                return true;
            }

            source = string.Empty;
            return false;
        }

        private bool TryTakeCard(List<CleanCard> cards, int id, out CleanCard card)
        {
            int index = IndexOfCard(cards, id);
            if (index < 0)
            {
                card = null;
                return false;
            }

            card = cards[index];
            cards.RemoveAt(index);
            return true;
        }

        private int IndexOfCard(List<CleanCard> cards, int id)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Definition.Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private int BestCardInDeckForSearch(CleanPlayerState player, Predicate<CleanCard> predicate)
        {
            int bestIndex = -1;
            int bestScore = -1;
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard card = player.Deck[i];
                if (!predicate(card))
                {
                    continue;
                }

                int score = card.Definition.Kind == CleanCardKind.Monster ? 100000 + card.Definition.Attack : 50000;
                if (IsBlueEyesCard(card))
                {
                    score += 250000;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private bool ResolveEffect(CleanPlayerState controller, CleanPlayerState rival, CleanCard card, CleanActivationSource source)
        {
            string key = card.Definition.EffectKey;
            Log(controller.Name + " activates " + card.Definition.Name + ".");

            bool cleanResolved;
            bool sendSourceCardToGrave = ResolveCleanEffect(controller, rival, card, source, out cleanResolved);
            if (cleanResolved)
            {
                CheckWin();
                return sendSourceCardToGrave;
            }

            if (luaBridge.TryResolve(this, controller, rival, card))
            {
                CheckWin();
                return true;
            }

            switch (key)
            {
                case "draw_two":
                    Draw(controller, 2);
                    Log(controller.Name + " draws 2 cards.");
                    break;
                case "destroy_rival_strongest":
                    DestroyStrongestMonster(rival);
                    break;
                case "destroy_all_monsters":
                    DestroyAllMonsters(State.Local);
                    DestroyAllMonsters(State.Rival);
                    Log("All monsters are destroyed.");
                    break;
                case "revive_strongest":
                    ReviveStrongest(controller);
                    break;
                case "boost_team":
                    BoostTeam(controller, 500);
                    break;
                case "destroy_rival_attackers":
                    DestroyAllMonsters(rival);
                    Log("The rival field is cleared.");
                    break;
                case "burn_rival":
                    rival.LifePoints -= 1000;
                    Log(rival.Name + " takes 1000 effect damage.");
                    CheckWin();
                    break;
                default:
                    Log(card.Definition.Name + " has no scripted clean effect yet.");
                    break;
            }

            return true;
        }
        private void Draw(CleanPlayerState player, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (player.Deck.Count == 0)
                {
                    player.LifePoints = 0;
                    State.GameOver = true;
                    State.WinnerText = OpponentOf(player.Id).Name + " wins by deck-out.";
                    Log(State.WinnerText);
                    return;
                }

                CleanCard top = player.Deck[0];
                player.Deck.RemoveAt(0);
                top.FaceDown = false;
                player.Hand.Add(top);
            }
        }

        private void FillDeck(CleanPlayerState player)
        {
            player.Deck.AddRange(CleanCardLibrary.BuildDeck(player.Id));
            player.ExtraDeck.AddRange(CleanCardLibrary.BuildExtraDeck(player.Id));
        }

        private void Shuffle(List<CleanCard> cards)
        {
            for (int i = cards.Count - 1; i > 0; i--)
            {
                int swap = random.Next(i + 1);
                CleanCard temp = cards[i];
                cards[i] = cards[swap];
                cards[swap] = temp;
            }
        }

        private void ResetPlayer(CleanPlayerState player)
        {
            player.LifePoints = 8000;
            player.NormalSummonUsed = false;
            player.Deck.Clear();
            player.ExtraDeck.Clear();
            player.Hand.Clear();
            player.Graveyard.Clear();
            player.UsedOncePerTurn.Clear();
            Array.Clear(player.Monsters, 0, player.Monsters.Length);
            Array.Clear(player.SpellsTraps, 0, player.SpellsTraps.Length);
        }

        private void SummonToZone(CleanPlayerState player, CleanCard card, int zone, bool faceDown, CleanMonsterStance stance)
        {
            player.Monsters[zone] = card;
            card.Controller = player.Id;
            card.FaceDown = faceDown;
            card.Stance = stance;
            card.HasBattled = false;
            card.SummonedThisTurn = true;
        }

        private CleanCard CardInHand(CleanPlayerState player, int index)
        {
            return index >= 0 && index < player.Hand.Count ? player.Hand[index] : null;
        }

        private int FirstEmpty(CleanCard[] zones)
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

        private int MonsterCount(CleanPlayerState player)
        {
            int count = 0;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private bool CanNormalPlayMonster(CleanPlayerState player, CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster || IsRitualMonster(card))
            {
                return false;
            }

            int tributeCount = RequiredTributesForNormalPlay(card);
            return MonsterCount(player) >= tributeCount &&
                   (FirstEmpty(player.Monsters) >= 0 || tributeCount > 0);
        }

        private int RequiredTributesForNormalPlay(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster || card.Definition.Level <= 4)
            {
                return 0;
            }

            return card.Definition.Level <= 6 ? 1 : 2;
        }

        private bool TributeForNormalPlay(CleanPlayerState player, CleanCard summonedCard)
        {
            int tributeCount = RequiredTributesForNormalPlay(summonedCard);
            if (tributeCount == 0)
            {
                return true;
            }

            List<string> tributedNames = new List<string>();
            for (int i = 0; i < tributeCount; i++)
            {
                int zone = WeakestTributeZone(player);
                if (zone < 0)
                {
                    Log("Tribute summon failed: not enough monsters to tribute.");
                    return false;
                }

                CleanCard tribute = player.Monsters[zone];
                tributedNames.Add(tribute.PublicName);
                MoveMonsterToGrave(player, zone);
            }

            Log("Tributed " + string.Join(", ", tributedNames.ToArray()) + " for " + summonedCard.Definition.Name + ".");
            return true;
        }

        private int WeakestTributeZone(CleanPlayerState player)
        {
            int weakestZone = -1;
            int weakestPower = int.MaxValue;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster == null)
                {
                    continue;
                }

                int power = monster.Stance == CleanMonsterStance.Attack ? monster.CurrentAttack : monster.CurrentDefense;
                if (monster.FaceDown)
                {
                    power = monster.CurrentDefense;
                }

                if (power < weakestPower)
                {
                    weakestPower = power;
                    weakestZone = i;
                }
            }

            return weakestZone;
        }

        private bool IsNormalMonster(CleanCard card)
        {
            return card != null && IsNormalMonsterDefinition(card.Definition);
        }

        private bool IsRitualMonster(CleanCard card)
        {
            return card != null && IsRitualMonsterDefinition(card.Definition);
        }

        private bool IsRitualMonsterDefinition(CleanCardDefinition definition)
        {
            if (definition == null || definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            string typeLine = definition.TypeLine ?? string.Empty;
            return definition.Id == BlueEyesChaosMaxDragonCardId || typeLine.IndexOf("Ritual", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsNormalMonsterDefinition(CleanCardDefinition definition)
        {
            if (definition == null || definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            if (definition.Id == BlueEyesWhiteDragonCardId || definition.Id == 23995346)
            {
                return true;
            }

            string typeLine = definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0 &&
                   typeLine.IndexOf("Effect", StringComparison.OrdinalIgnoreCase) < 0;
        }
        private int BestMonsterInHand(CleanPlayerState player)
        {
            int bestIndex = -1;
            int bestAttack = -1;
            for (int i = 0; i < player.Hand.Count; i++)
            {
                CleanCard card = player.Hand[i];
                if (CanNormalPlayMonster(player, card) && card.Definition.Attack > bestAttack)
                {
                    bestAttack = card.Definition.Attack;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private int FirstPlayableRivalSpell()
        {
            for (int i = 0; i < State.Rival.Hand.Count; i++)
            {
                if (State.Rival.Hand[i].Definition.Kind == CleanCardKind.Spell)
                {
                    return i;
                }
            }

            return -1;
        }

        private int WeakestLocalMonsterZone()
        {
            int weakestZone = -1;
            int weakestPower = int.MaxValue;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = State.Local.Monsters[i];
                if (monster == null)
                {
                    continue;
                }

                int power = monster.Stance == CleanMonsterStance.Attack ? monster.CurrentAttack : monster.CurrentDefense;
                if (power < weakestPower)
                {
                    weakestPower = power;
                    weakestZone = i;
                }
            }

            return weakestZone;
        }

        private void DestroyStrongestMonster(CleanPlayerState player)
        {
            int strongestZone = StrongestEffectTargetZone(player);
            if (strongestZone >= 0)
            {
                string name = player.Monsters[strongestZone].PublicName;
                MoveMonsterToGrave(player, strongestZone);
                Log(name + " is destroyed.");
            }
            else
            {
                Log("No valid monster to target.");
            }
        }

        private void DestroyAllMonsters(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null)
                {
                    MoveMonsterToGrave(player, i);
                }
            }
        }

        private void ReviveStrongest(CleanPlayerState player)
        {
            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                Log("No open monster zone.");
                return;
            }

            int bestIndex = -1;
            int bestAttack = -1;
            for (int i = 0; i < player.Graveyard.Count; i++)
            {
                CleanCard card = player.Graveyard[i];
                if (card.Definition.Kind == CleanCardKind.Monster && card.Definition.Attack > bestAttack)
                {
                    bestAttack = card.Definition.Attack;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                Log("No monster in graveyard to revive.");
                return;
            }

            CleanCard revived = player.Graveyard[bestIndex];
            player.Graveyard.RemoveAt(bestIndex);
            SummonToZone(player, revived, zone, false, CleanMonsterStance.Attack);
            Log("Revived " + revived.Definition.Name + ".");
        }

        private void BoostTeam(CleanPlayerState player, int amount)
        {
            int boosted = 0;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null)
                {
                    monster.AttackBonus += amount;
                    boosted++;
                }
            }

            Log(boosted > 0 ? "Allied monsters gain " + amount + " ATK." : "No allied monsters to boost.");
        }

        private void MoveMonsterToGrave(CleanPlayerState player, int zone)
        {
            CleanCard card = player.Monsters[zone];
            if (card == null)
            {
                return;
            }

            card.FaceDown = false;
            card.AttackBonus = 0;
            card.HasBattled = false;
            player.Monsters[zone] = null;
            player.Graveyard.Add(card);
        }

        private void CheckWin()
        {
            if (State.Local.LifePoints <= 0)
            {
                State.Local.LifePoints = 0;
                State.GameOver = true;
                State.WinnerText = State.Rival.Name + " wins.";
                Log(State.WinnerText);
            }
            else if (State.Rival.LifePoints <= 0)
            {
                State.Rival.LifePoints = 0;
                State.GameOver = true;
                State.WinnerText = State.Local.Name + " wins.";
                Log(State.WinnerText);
            }
        }

        private void Log(string message)
        {
            State.Log.Add(message);
            while (State.Log.Count > 10)
            {
                State.Log.RemoveAt(0);
            }
        }

        private void NotifyChanged()
        {
            if (Changed != null)
            {
                Changed();
            }
        }
    }
}







