using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelGame
    {
        public const int FieldSpellZoneIndex = CleanPlayerState.ZoneCount;
        private const int MaidenOfWhiteCardId = 17947697;
        private const int NeoKaiserSeaHorseCardId = 54332792;
        private const int TrueLightCardId = 62089826;
        private const int BlueEyesWhiteDragonCardId = 89631139;
        private const int DarkMagicianCardId = 46986414;
        public const int SignatureClimaxLifePointThreshold = 2000;
        private const int BlueEyesUltimateDragonCardId = 23995346;
        private const int DragonMasterMagiaCardId = 12381100;
        private const int BlueEyesChaosMaxDragonCardId = 55410871;
        private const int BlueEyesChaosDragonCardId = 20654247;
        private const int BlackLusterSoldierLegendarySwordsmanCardId = 70551291;
        private const int MaxxCCardId = 23434538;
        private const int EffectVeilerCardId = 97268402;
        private const int AshBlossomCardId = 14558127;
        private const int BingoMachineCardId = 93437091;
        private const int BingoChoicePromptId = -934370911;
        private const int UltimateFusionCardId = 71143015;
        private const int DragonShrineCardId = 41620959;
        private const int GhostOgreSnowRabbitCardId = 59438930;
        private const int SageWithEyesOfBlueCardId = 8240199;
        private const int BlueEyesJetDragonCardId = 30576089;
        private const int BlueEyesAlternativeWhiteDragonCardId = 38517737;
        private const int BlueEyesAbyssDragonCardId = 64202399;
        private const int MonsterRebornCardId = 83764719;
        private const int RaigekiCardId = 12580477;
        private const int PolymerizationCardId = 24094653;
        private const int WhiteStoneOfAncientsCardId = 71039903;
        private const int RoarOfBlueEyedDragonsCardId = 17725109;
        private const int BlueEyesSpiritDragonCardId = 59822133;
        private const int BlueEyesUltimateSpiritDragonCardId = 89604813;
        private const int AzureEyesSilverDragonCardId = 40908371;
        private const int SpiritWithEyesOfBlueCardId = 42097666;
        private const int MaidenWithEyesOfBlueCardId = 88241506;
        private const int DragonSpiritOfWhiteCardId = 45467446;
        private const int MausoleumOfWhiteCardId = 24382602;
        private const int WishesForEyesOfBlueCardId = 80326401;
        private const int GatewayToChaosCardId = 40089744;
        private const int MajestyWithEyesOfBlueCardId = 2783661;
        private const int ChaosFormCardId = 21082832;
        private const int BurstStreamOfDestructionCardId = 17655904;
        private const int TrueLightSummonOptionId = 1;
        private const int TrueLightSetOptionId = 2;
        private const int TrueLightSetCardChoicePromptId = -62089826;
        private const int BattlePhaseChoicePromptId = -2001;
        private const int BattlePhaseMain2OptionId = 1;
        private const int BattlePhaseEndOptionId = 2;
        private const int DragonShrineFirstChoicePromptId = -41620959;
        private const int DragonShrineSecondChoicePromptId = -41620960;
        private const int DragonSpiritBanishChoicePromptId = -454674461;
        private const int DictatorSendChoicePromptId = -669611941;
        private const int DictatorDiscardChoicePromptId = -669611942;
        private const int DictatorReviveChoicePromptId = -669611943;
        private const int DictatorAttackTargetChoicePromptId = -669611944;
        private const int JetBounceChoicePromptId = -305760891;
        private const int DragonSpiritFieldSpellChoiceId = 100;
        private const int DragonShrineSkipSecondOptionId = -1;
        private const int WishesDiscardChoicePromptId = -803264011;
        private const int WishesTunerChoicePromptId = -803264012;
        private const int WishesSupportChoicePromptId = -803264013;
        private const int StoneRecoverChoicePromptId = -710399031;
        private const int WishesEquipTargetChoicePromptId = -803264014;
        private const int WishesEquipMonsterChoicePromptId = -803264015;
        private const int MagiaLeaveChoicePromptId = -123811001;
        private const int VeilerTargetChoicePromptId = -972684021;
        private const int SageSearchChoicePromptId = -82401991;
        private const int SageTargetChoicePromptId = -82401992;
        private const int SageBlueEyesChoicePromptId = -82401993;
        private const int MausoleumTargetChoicePromptId = -243826021;
        private const int MajestySendChoicePromptId = -27836611;
        private const int MajestyTargetChoicePromptId = -27836612;
        private const int MausoleumNormalChoicePromptId = -243826022;
        private const int GatewaySearchChoicePromptId = -400897441;
        private const int SpiritEffectChoicePromptId = -420976661;
        private const int SpiritMausoleumDestinationChoicePromptId = -420976662;
        private const int SpiritSummonChoicePromptId = -420976663;
        private const int MaidenWhiteReviveChoicePromptId = -179476971;
        private const int NeoKaiserLevelChoicePromptId = -543327921;
        private const int NeoKaiserSendChoicePromptId = -543327922;
        private const int AlternativeDestroyChoicePromptId = -385177371;
        private const int AbyssSearchChoicePromptId = -642023991;
        private const int AbyssEndPhaseChoicePromptId = -642023992;
        private const int MonsterRebornChoicePromptId = -837647191;
        private const int MaidenWhiteReviveSkipOptionId = -1;
        private const int SpiritMausoleumEffectOptionId = 1;
        private const int SpiritTributeEffectOptionId = 2;
        private const int SpiritMausoleumToHandOptionId = 1;
        private const int SpiritMausoleumToGraveOptionId = 2;
        private const int LuaDestroyTargetChoicePromptId = -3001;
        private const int QuickEffectChoicePromptId = -3002;
        private const int QuickEffectPassOptionId = -1;
        private const int QuickEffectChoiceHandSource = 1;
        private const int QuickEffectChoiceMonsterSource = 2;
        private const int QuickEffectChoiceExtraMonsterSource = 3;
        private const int QuickEffectChoiceSpellTrapSource = 4;
        private const int ExtraSpiritZoneOffset = 100;
        private const int PendulumSummonChoicePromptId = -5001;
        private const int ExtraDeckSummonChoicePromptId = -5002;
        private const int FusionSummonChoicePromptId = -5003;
        private const int RitualSummonChoicePromptId = -5004;
        private const int TriggerEffectPromptId = -6001;
        private const int SpecialSummonPromptId = -6002;
        private const int CardEffectPromptId = -6003;
        private const int SurrenderPromptId = -7001;
        private const int PromptConfirmOptionId = 1;
        private const int PromptDeclineOptionId = 0;
        private const int PendulumFinishOptionId = -1;
        private const int ChoiceSourceDeck = 1;
        private const int ChoiceSourceHand = 2;
        private const string TrueLightOnceKey = "true_light_clean";
        private const string MausoleumBoostOnceKey = "mausoleum_boost_clean";
        private const string MausoleumExtraNormalOnceKey = "mausoleum_extra_normal_clean";
        private const string JetSummonOnceKey = "jet_summon_clean";
        private const string JetBounceOnceKey = "jet_bounce_clean";
        private const string WhiteStoneEndPhaseOnceKey = "stone_ancients_end_phase_clean";
        private const string BlueEyesSpiritGraveNegateOnceKey = "spirit_dragon_grave_negate_clean";
        private const string UltimateSpiritNegateOnceKey = "ultimate_spirit_negate_clean";
        private const string UltimateSpiritDestroyedOnceKey = "ultimate_spirit_destroyed_clean";
        private const string GatewayActivationOnceKey = "gateway_activation_clean";
        private const string GatewayRitualSearchOnceKey = "gateway_ritual_search_clean";
        private const string SpiritMausoleumOnceKey = "spirit_with_eyes_mausoleum_clean";
        private const string SpiritTributeOnceKey = "spirit_with_eyes_tribute_clean";
        private const string MagiaMonsterNegateOnceKey = "magia_monster_negate_clean";
        private const string MagiaSpellNegateOnceKey = "magia_spell_negate_clean";
        private const string MagiaTrapNegateOnceKey = "magia_trap_negate_clean";
        private const string MaidenWithEyesOnceKey = "maiden_with_eyes_once_clean";
        private const string MaidenWhitePlaceOnceKey = "maiden_white_place_once_clean";
        private const string MaidenWhiteReviveOnceKey = "maiden_white_revive_once_clean";
        private const string MaidenWhiteTargetOnceKey = "maiden_white_target_once_clean";
        private const string SageHandOnceKey = "sage_hand_once_clean";
        private const string StoneRecoverOnceKey = "stone_recover_once_clean";
        private const string MaxxCOnceKey = "maxx_c_once_clean";
        private const string AshBlossomOnceKey = "ash_blossom_once_clean";
        private const string GhostOgreOnceKey = "ghost_ogre_once_clean";
        private const string DictatorSummonOnceKey = "dictator_summon_once_clean";
        private const string DictatorReviveOnceKey = "dictator_revive_once_clean";
        private const string BingoActivationOnceKey = "bingo_activation_once_clean";
        private const string WishesSearchOnceKey = "wishes_search_once_clean";
        private const string WishesEquipOnceKey = "wishes_equip_once_clean";
        private const string RoarSharedOnceKey = "roar_shared_once_clean";
        private const string MajestyActivationOnceKey = "majesty_activation_once_clean";
        private const string DragonShrineActivationOnceKey = "dragon_shrine_activation_once_clean";
        private const string UltimateFusionActivationOnceKey = "ultimate_fusion_activation_once_clean";
        private const string NeoKaiserSummonOnceKey = "neo_kaiser_summon_once_clean";
        private const string NeoKaiserLevelOnceKey = "neo_kaiser_level_once_clean";
        private const string NeoKaiserSendOnceKey = "neo_kaiser_send_once_clean";
        private const string AlternativeSummonOnceKey = "alternative_summon_once_clean";
        private const string AlternativeDestroyOnceKey = "alternative_destroy_once_clean";
        private const string AbyssSummonSearchOnceKey = "abyss_summon_search_once_clean";
        private const string AbyssEndPhaseSearchOnceKey = "abyss_end_phase_search_once_clean";
        private const string AbyssGraveBoostOnceKey = "abyss_grave_boost_once_clean";
        private enum CleanActivationSource
        {
            Hand,
            MonsterZone,
            SpellTrapZone
        }

        private enum CleanSummonOrigin
        {
            Hand,
            Deck,
            Graveyard,
            Banished,
            ExtraDeck
        }

        private enum CleanSummonMethod
        {
            Effect,
            Ritual,
            Fusion,
            Synchro,
            Xyz,
            Link,
            AlternativeProcedure
        }

        [Flags]
        private enum CleanResponseKind
        {
            None = 0,
            DeckEffect = 1,
            FieldEffect = 2,
            GraveyardEffect = 4
        }

        private sealed class QueuedUiPrompt
        {
            public int PromptId;
            public string Title;
            public string Body;
            public string ConfirmDetail;
            public CleanPromptKind Kind;
            public bool Optional;
            public Action Confirm;
            public Action Decline;
        }
        private System.Random random;
        private System.Random rivalAiRandom;
        private readonly CleanDuelAiPlanner rivalAiPlanner = new CleanDuelAiPlanner();
        private readonly HashSet<int> rivalAiAttemptedEffectsThisTurn = new HashSet<int>();
        private readonly HashSet<int> unavailableCardIds = new HashSet<int>();
        private readonly CleanLuaEffectBridge luaBridge = new CleanLuaEffectBridge();
        private int pendingMausoleumTargetZone = -1;
        private int pendingSpiritZone = -1;
        private CleanPlayerId pendingLuaDestroyTargetPlayerId = CleanPlayerId.Rival;
        private int pendingLuaDestroyRemaining;
        private string pendingLuaDestroySource = string.Empty;
        private bool pendingLuaDestroyFaceUpCards;
        private bool pendingLuaDestroyMayStop;
        private CleanResponseKind pendingResponseKind;
        private CleanCard pendingResponseCard;
        private CleanPlayerState pendingResponseController;
        private bool pendingResponseSourceWasFaceUp;
        private bool pendingResponseNegated;
        private bool pendingResponseDestroyed;
        private CleanCard pendingRivalActivatedCard;
        private readonly List<int> pendingBingoRuntimeIds = new List<int>();
        private int pendingWishesEquipTargetZone = -1;
        private CleanCard pendingDictatorCard;
        private int pendingPendulumSummonCount;
        private int pendingFusionPreferredId;
        private bool pendingFusionIncludeGraveyard;
        private bool pendingFusionMaterialsToDeck;
        private bool pendingFusionDestroyTargets;
        private string pendingFusionSourceName = string.Empty;
        private int pendingRivalAttackerZone = -1;
        private CleanPlayerId pendingJetAttackingPlayerId = CleanPlayerId.Local;
        private int pendingJetAttackerZone = -1;
        private int pendingJetDefenderZone = -1;
        private bool localQuickWindowHandledForRivalTurn;
        private readonly Queue<QueuedUiPrompt> pendingUiPrompts = new Queue<QueuedUiPrompt>();
        private readonly HashSet<string> pendingTriggerPromptKeys = new HashSet<string>();
        private QueuedUiPrompt activeUiPrompt;
        private bool resolvingConfirmedSpecialSummon;
        private bool pendingEndTurnContinuation;
        private bool pendingRivalTurnContinuationAfterResponse;
        private bool suppressAttackTargetTriggerOnce;
        public readonly CleanDuelState State = new CleanDuelState();

        public event Action Changed;
        public event Action<CleanDuelCinematicEvent> CinematicRequested;

        public CleanPlayerState CurrentPlayer
        {
            get { return Player(State.TurnPlayer); }
        }

        public CleanPlayerState WaitingPlayer
        {
            get { return OpponentOf(State.TurnPlayer); }
        }

        public CleanDuelAiDifficulty RivalAiDifficulty { get; private set; } = CleanDuelAiDifficulty.Intermediate;
        private CleanDuelRankedAiProfile configuredRankedAiProfile = CleanDuelRankedAiProfile.ForStep(0);
        public bool RivalAiFollowsRank { get; private set; }
        public CleanDuelRankedAiProfile RivalRankedAiProfile { get { return RivalAiFollowsRank ? configuredRankedAiProfile : null; } }
        public string RivalAiModeLabel { get { return RivalAiFollowsRank ? "Auto (" + configuredRankedAiProfile.RankLabel + ")" : RivalAiDifficulty.ToString(); } }
        public bool CanChangeRivalAiMode { get { return !State.GameOver && State.TurnPlayer == CleanPlayerId.Local && State.PendingChoice == null && activeUiPrompt == null; } }

        public void ConfigureRankedOpponent(CleanDuelRankedSnapshot snapshot)
        {
            if (State.TurnNumber > 0 && !CanChangeRivalAiMode) return;
            configuredRankedAiProfile = CleanDuelRankedAiProfile.FromSnapshot(snapshot);
            UseRankedRivalAi();
        }

        private void UseRankedRivalAi()
        {
            RivalAiFollowsRank = true;
            RivalAiDifficulty = configuredRankedAiProfile.Difficulty;
            rivalAiRandom = CreateRivalAiRandom(State.RandomSeed);
            Log("Opponent AI follows rank: " + configuredRankedAiProfile.RankLabel + " (" + RivalAiDifficulty + ").");
            NotifyChanged();
        }

        public void CycleRivalAiMode()
        {
            if (!CanChangeRivalAiMode) return;
            if (RivalAiFollowsRank) SetRivalAiDifficulty(CleanDuelAiDifficulty.Beginner);
            else if (RivalAiDifficulty == CleanDuelAiDifficulty.Expert) UseRankedRivalAi();
            else CycleRivalAiDifficulty();
        }

        public void ConfigureAuthorizedPlayer(string displayName, IEnumerable<int> deniedCardIds)
        {
            State.Local.SetDisplayName(displayName);
            unavailableCardIds.Clear();
            if (deniedCardIds == null)
            {
                return;
            }

            foreach (int cardId in deniedCardIds)
            {
                unavailableCardIds.Add(cardId);
            }
        }

        public void SetRivalAiDifficulty(CleanDuelAiDifficulty difficulty)
        {
            if (RivalAiDifficulty == difficulty && !RivalAiFollowsRank)
            {
                return;
            }

            RivalAiFollowsRank = false;
            RivalAiDifficulty = difficulty;
            rivalAiRandom = CreateRivalAiRandom(State.RandomSeed);
            Log("Opponent AI difficulty: " + difficulty + ".");
            NotifyChanged();
        }

        public void CycleRivalAiDifficulty()
        {
            CleanDuelAiDifficulty next = RivalAiDifficulty == CleanDuelAiDifficulty.Beginner
                ? CleanDuelAiDifficulty.Intermediate
                : RivalAiDifficulty == CleanDuelAiDifficulty.Intermediate
                    ? CleanDuelAiDifficulty.Expert
                    : CleanDuelAiDifficulty.Beginner;
            SetRivalAiDifficulty(next);
        }

        public void StartNewDuel()
        {
            StartNewDuel(CreateMatchSeed());
        }

        public void StartNewDuel(int randomSeed)
        {
            ResetPlayer(State.Local);
            ResetPlayer(State.Rival);
            Array.Clear(State.ExtraMonsters, 0, State.ExtraMonsters.Length);
            State.Log.Clear();
            State.EventStream.Clear();
            State.TurnPlayer = CleanPlayerId.Local;
            State.Phase = CleanDuelPhase.Draw;
            State.TurnNumber = 1;
            State.GameOver = false;
            State.PendingChoice = null;
            State.Result = null;
            State.RandomSeed = randomSeed;
            State.StateSequence = 0;
            State.CinematicSequence = 0;
            State.RivalryClimaxShown = false;
            State.DeterministicStateHash = string.Empty;
            random = new System.Random(randomSeed);
            rivalAiRandom = CreateRivalAiRandom(randomSeed);
            rivalAiAttemptedEffectsThisTurn.Clear();
            pendingUiPrompts.Clear();
            pendingTriggerPromptKeys.Clear();
            activeUiPrompt = null;
            resolvingConfirmedSpecialSummon = false;
            pendingEndTurnContinuation = false;
            pendingRivalTurnContinuationAfterResponse = false;
            suppressAttackTargetTriggerOnce = false;
            ClearPendingEffectChoices();
            State.WinnerText = string.Empty;
            localQuickWindowHandledForRivalTurn = false;

            FillDeck(State.Local);
            FillDeck(State.Rival);
            Shuffle(State.Local.Deck);
            Shuffle(State.Rival.Deck);
            luaBridge.Initialize();
            Draw(State.Local, 5);
            if (!State.GameOver)
            {
                Draw(State.Rival, 5);
            }

            Log("New clean duel started.");
            Log(luaBridge.Status);
            if (!State.GameOver)
            {
                Log("The first player does not draw during the first Draw Phase.");
                EnterStandbyPhase();
                EnterMainPhase1();
            }
            NotifyChanged();
        }

        private System.Random CreateRivalAiRandom(int randomSeed)
        {
            unchecked
            {
                int rankSeed = RivalAiFollowsRank ? (configuredRankedAiProfile.LadderStep + 1) * 104729 : 0;
                return new System.Random(randomSeed ^ 0x5EED51A1 ^ ((int)RivalAiDifficulty * 7919) ^ rankSeed);
            }
        }

        private static int CreateMatchSeed()
        {
            unchecked
            {
                return Environment.TickCount * 397 ^ Guid.NewGuid().GetHashCode();
            }
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

        public void RequestSurrender()
        {
            if (State.GameOver)
            {
                return;
            }

            if (State.PendingChoice != null)
            {
                Log("Finish the current choice before opening the surrender confirmation.");
                NotifyChanged();
                return;
            }

            State.PendingChoice = new CleanChoicePrompt(
                SurrenderPromptId,
                "Surrender duel?",
                "Surrendering immediately ends the duel and awards the win to your opponent.",
                new List<CleanChoiceOption>
                {
                    new CleanChoiceOption(PromptConfirmOptionId, "Surrender", "End this duel now.", true, string.Empty),
                    new CleanChoiceOption(PromptDeclineOptionId, "Keep playing", "Return to the duel.", true, string.Empty)
                },
                CleanPromptKind.Choice,
                true);
            Log("Surrender confirmation opened.");
            NotifyChanged();
        }

        public void Surrender(CleanPlayerId actor)
        {
            if (State.GameOver)
            {
                return;
            }

            CleanPlayerState loser = Player(actor);
            CleanPlayerState winner = OpponentOf(actor);
            EndDuel(
                CleanDuelEndReason.Surrender,
                winner,
                loser,
                loser.Name + " surrendered. " + winner.Name + " wins.");
            NotifyChanged();
        }

        
        private static bool IsMainPhase(CleanDuelPhase phase)
        {
            return phase == CleanDuelPhase.Main || phase == CleanDuelPhase.Main2;
        }

        private bool IsFirstTurnOfDuel()
        {
            return State.TurnNumber == 1 && State.TurnPlayer == CleanPlayerId.Local;
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
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase))
            {
                return false;
            }

            CleanCard card = CardInHand(State.Local, handIndex);
            return CanNormalPlayMonster(State.Local, card) &&
                   (!State.Local.NormalSummonUsed || CanUseMausoleumExtraNormalSummon(State.Local, card));
        }
        public bool CanSetMonster(int handIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || State.Local.NormalSummonUsed)
            {
                return false;
            }

            return CanNormalPlayMonster(State.Local, CardInHand(State.Local, handIndex));
        }
        public bool CanSetSpellTrap(int handIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase))
            {
                return false;
            }

            CleanCard card = CardInHand(State.Local, handIndex);
            return card != null &&
                   card.Definition.Kind != CleanCardKind.Monster &&
                   (IsFieldSpellCard(card) || FirstEmpty(State.Local.SpellsTraps) >= 0);
        }

        public bool CanActivateGraveyardCard(int graveyardIndex)
        {
            if (!CanUseLocalActions() ||
                !IsMainPhase(State.Phase) ||
                graveyardIndex < 0 ||
                graveyardIndex >= State.Local.Graveyard.Count)
            {
                return false;
            }

            CleanCard card = State.Local.Graveyard[graveyardIndex];

            switch (card.Definition.Id)
            {
                case WhiteStoneOfAncientsCardId:
                    return !State.Local.UsedOncePerTurn.Contains(StoneRecoverOnceKey) &&
                           HasOtherMatchingCard(State.Local.Graveyard, card, IsBlueEyesMonster);
                case MausoleumOfWhiteCardId:
                    return IndexOfCard(State.Local.Deck, BurstStreamOfDestructionCardId) >= 0;
                case WishesForEyesOfBlueCardId:
                    return !State.Local.UsedOncePerTurn.Contains(WishesEquipOnceKey) &&
                           FirstEmpty(State.Local.SpellsTraps) >= 0 &&
                           HasFaceUpBlueEyesWhiteDragon(State.Local) &&
                           HasMatchingCard(State.Local.ExtraDeck, IsBlueEyesMonster);
                case RoarOfBlueEyedDragonsCardId:
                    return !State.Local.UsedOncePerTurn.Contains(RoarSharedOnceKey) &&
                           CanFusionSummonFromHandOrField(State.Local);
                case BlueEyesAbyssDragonCardId:
                    return !State.Local.UsedOncePerTurn.Contains(AbyssGraveBoostOnceKey) &&
                           HasBlueEyesWhiteDragonOnFieldOrGrave(State.Local) &&
                           HasLevelEightOrHigherDragonOnField(State.Local);
                default:
                    return false;
            }
        }

        public void RequestActivateGraveyardCard(int graveyardIndex)
        {
            if (!CanActivateGraveyardCard(graveyardIndex))
            {
                return;
            }

            CleanCard card = State.Local.Graveyard[graveyardIndex];
            QueueManualEffectPrompt(
                card,
                CleanActivationSource.Hand,
                "Activate this card's effect from your GY?",
                () =>
                {
                    int currentIndex = State.Local.Graveyard.IndexOf(card);
                    if (currentIndex >= 0)
                    {
                        ActivateGraveyardCard(currentIndex);
                    }
                });
        }

        public void ActivateGraveyardCard(int graveyardIndex)
        {
            if (!CanActivateGraveyardCard(graveyardIndex))
            {
                return;
            }

            CleanCard card = State.Local.Graveyard[graveyardIndex];
            if (!resolvingConfirmedSpecialSummon && RequiresSpecialSummonConfirmation(card, CleanActivationSource.Hand))
            {
                QueueSpecialSummonPrompt(
                    State.Local,
                    card.Definition.Name,
                    "Activate this GY effect and perform its Special Summon?",
                    () => ResolveConfirmedSpecialSummonAction(() =>
                    {
                        int currentIndex = State.Local.Graveyard.IndexOf(card);
                        if (currentIndex >= 0)
                        {
                            ActivateGraveyardCard(currentIndex);
                        }
                    }));
                NotifyChanged();
                return;
            }

            State.Local.Graveyard.RemoveAt(graveyardIndex);
            ResetCardForHiddenZone(card);
            State.Local.Banished.Add(card);
            Log(State.Local.Name + " banishes " + card.Definition.Name + " from the GY to activate its effect.");

            MarkGraveyardActivationOnce(State.Local, card.Definition.Id);

            if (TryNegateRivalGraveyardEffect(State.Local, card.Definition.Name))
            {
                NotifyChanged();
                return;
            }

            switch (card.Definition.Id)
            {
                case WhiteStoneOfAncientsCardId:
                    State.Local.UsedOncePerTurn.Add(StoneRecoverOnceKey);
                    OpenStoneRecoverChoice(State.Local);
                    break;
                case MausoleumOfWhiteCardId:
                    AddSpecificCardFromDeckToHand(State.Local, BurstStreamOfDestructionCardId, "Burst Stream of Destruction");
                    break;
                case WishesForEyesOfBlueCardId:
                    State.Local.UsedOncePerTurn.Add(WishesEquipOnceKey);
                    OpenWishesEquipTargetChoice(State.Local);
                    break;
                case RoarOfBlueEyedDragonsCardId:
                    State.Local.UsedOncePerTurn.Add(RoarSharedOnceKey);
                    FusionSummonFromHandOrField(State.Local);
                    break;
                case BlueEyesAbyssDragonCardId:
                    BoostLevelEightOrHigherDragons(State.Local, 1000);
                    break;
            }

            CheckWin();
            NotifyChanged();
        }

        private void MarkGraveyardActivationOnce(CleanPlayerState player, int cardId)
        {
            switch (cardId)
            {
                case WhiteStoneOfAncientsCardId:
                    player.UsedOncePerTurn.Add(StoneRecoverOnceKey);
                    break;
                case WishesForEyesOfBlueCardId:
                    player.UsedOncePerTurn.Add(WishesEquipOnceKey);
                    break;
                case RoarOfBlueEyedDragonsCardId:
                    player.UsedOncePerTurn.Add(RoarSharedOnceKey);
                    break;
                case BlueEyesAbyssDragonCardId:
                    player.UsedOncePerTurn.Add(AbyssGraveBoostOnceKey);
                    break;
            }
        }

        private bool HasOtherMatchingCard(List<CleanCard> cards, CleanCard excluded, Predicate<CleanCard> predicate)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (!object.ReferenceEquals(cards[i], excluded) && predicate(cards[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void OpenStoneRecoverChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < player.Graveyard.Count; i++)
            {
                CleanCard card = player.Graveyard[i];
                if (IsBlueEyesMonster(card))
                {
                    options.Add(new CleanChoiceOption(card.RuntimeId, card.Definition.Name, "Add from GY to hand", true, string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                StoneRecoverChoicePromptId,
                "The White Stone of Ancients",
                "Choose 1 Blue-Eyes monster in your GY to add to your hand.",
                options);
        }

        private void ResolveStoneRecoverChoice(CleanPlayerState player, int runtimeId)
        {
            CleanCard target = FindCardByRuntimeId(player.Graveyard, runtimeId);
            if (target == null || !IsBlueEyesMonster(target))
            {
                Log("White Stone failed: the selected Blue-Eyes monster is no longer in the GY.");
                return;
            }

            player.Graveyard.Remove(target);
            ResetCardForHiddenZone(target);
            player.Hand.Add(target);
            Log("The White Stone of Ancients adds " + target.Definition.Name + " from the GY to the hand.");
        }

        private void OpenWishesEquipTargetChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && IsTreatedAsBlueEyesWhiteDragon(monster))
                {
                    options.Add(new CleanChoiceOption(i, monster.Definition.Name, "Equip a Blue-Eyes Extra Deck monster", true, string.Empty));
                }
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            CleanCard extraMonster = State.ExtraMonsters[extraZone];
            if (extraMonster != null && !extraMonster.FaceDown && IsTreatedAsBlueEyesWhiteDragon(extraMonster))
            {
                options.Add(new CleanChoiceOption(
                    ExtraSpiritZoneOffset + extraZone,
                    extraMonster.Definition.Name,
                    "Equip a Blue-Eyes Extra Deck monster",
                    true,
                    string.Empty));
            }

            State.PendingChoice = new CleanChoicePrompt(
                WishesEquipTargetChoicePromptId,
                "Wishes for Eyes of Blue",
                "Choose 1 Blue-Eyes White Dragon you control.",
                options);
        }

        private void ResolveWishesEquipTargetChoice(CleanPlayerState player, int zone)
        {
            CleanCard target = MaterialCardAtZone(player, zone);
            if (target == null || target.FaceDown || !IsTreatedAsBlueEyesWhiteDragon(target))
            {
                Log("Wishes failed: Blue-Eyes White Dragon is no longer a legal target.");
                return;
            }

            pendingWishesEquipTargetZone = zone;
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < player.ExtraDeck.Count; i++)
            {
                CleanCard card = player.ExtraDeck[i];
                if (IsBlueEyesMonster(card))
                {
                    options.Add(new CleanChoiceOption(card.RuntimeId, card.Definition.Name, "Equip from Extra Deck", true, string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                WishesEquipMonsterChoicePromptId,
                "Wishes for Eyes of Blue",
                "Choose 1 Blue-Eyes monster from your Extra Deck to equip.",
                options);
        }

        private void ResolveWishesEquipMonsterChoice(CleanPlayerState player, int runtimeId)
        {
            CleanCard target = MaterialCardAtZone(player, pendingWishesEquipTargetZone);
            if (target == null || target.FaceDown || !IsTreatedAsBlueEyesWhiteDragon(target))
            {
                Log("Wishes failed: the equip target left the field.");
                pendingWishesEquipTargetZone = -1;
                return;
            }

            CleanCard equip = FindCardByRuntimeId(player.ExtraDeck, runtimeId);
            int spellZone = FirstEmpty(player.SpellsTraps);
            if (equip == null || !IsBlueEyesMonster(equip) || spellZone < 0)
            {
                Log("Wishes failed: the selected Extra Deck monster cannot be equipped.");
                pendingWishesEquipTargetZone = -1;
                return;
            }

            player.ExtraDeck.Remove(equip);
            equip.Controller = player.Id;
            equip.FaceDown = false;
            equip.EquippedToRuntimeId = target.RuntimeId;
            player.SpellsTraps[spellZone] = equip;
            target.EquipAttackBonus += 400;
            Log("Wishes equips " + equip.Definition.Name + " to " + target.Definition.Name + "; it gains 400 ATK.");
            pendingWishesEquipTargetZone = -1;
        }

        private bool AddSpecificCardFromDeckToHand(CleanPlayerState player, int cardId, string label)
        {
            CleanCard card;
            if (!TryTakeCard(player.Deck, cardId, out card))
            {
                Log(label + " is no longer in the Deck.");
                return false;
            }

            ResetCardForHiddenZone(card);
            player.Hand.Add(card);
            Log("Added " + card.Definition.Name + " from Deck to hand.");
            return true;
        }

        public bool CanActivateHandCard(int handIndex)
        {
            if (!CanUseLocalActions())
            {
                return false;
            }

            CleanCard card = CardInHand(State.Local, handIndex);
            if (card == null)
            {
                return false;
            }

            if (!IsMainPhase(State.Phase) && !IsQuickPlaySpell(card))
            {
                return false;
            }

            if (CanActivatePendulumFromHand(handIndex))
            {
                return true;
            }

            return card.Definition.Kind != CleanCardKind.Trap && CanActivateCleanCard(card, State.Local, CleanActivationSource.Hand);
        }

        public bool CanActivatePendulumFromHand(int handIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase))
            {
                return false;
            }

            CleanCard card = CardInHand(State.Local, handIndex);
            return card != null && IsPendulumMonster(card) && FirstAvailablePendulumZone(State.Local) >= 0;
        }

        public void ActivatePendulumFromHand(int handIndex)
        {
            if (!CanActivatePendulumFromHand(handIndex))
            {
                return;
            }

            CleanCard card = State.Local.Hand[handIndex];
            int zone = FirstAvailablePendulumZone(State.Local);
            State.Local.Hand.RemoveAt(handIndex);
            State.Local.SpellsTraps[zone] = card;
            card.Controller = State.Local.Id;
            card.FaceDown = false;
            card.FaceUpExtraDeck = false;
            Log(State.Local.Name + " activates " + card.Definition.Name + " as a Pendulum Spell in the " + (zone == 0 ? "left" : "right") + " Pendulum Zone.");
            NotifyChanged();
        }

        public bool CanPendulumSummon()
        {
            return CanUseLocalActions() && IsMainPhase(State.Phase) && CanPendulumSummon(State.Local);
        }

        public void PendulumSummon()
        {
            if (!CanPendulumSummon())
            {
                return;
            }

            pendingPendulumSummonCount = 0;
            OpenPendulumSummonChoice(State.Local);
            NotifyChanged();
        }

        public bool CanActivateMonsterZoneCard(int zoneIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            return card != null && !card.FaceDown && CanActivateCleanCard(card, State.Local, CleanActivationSource.MonsterZone);
        }
        public bool CanFlipSummonMonster(int zoneIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            return card != null && card.FaceDown && !card.SummonedThisTurn && !card.PositionChangedThisTurn && card.Definition.Kind == CleanCardKind.Monster;
        }

        public void FlipSummonMonster(int zoneIndex)
        {
            if (!CanFlipSummonMonster(zoneIndex))
            {
                return;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            card.FaceDown = false;
            card.Stance = CleanMonsterStance.Attack;
            card.SummonedThisTurn = true;
            card.PositionChangedThisTurn = true;
            Log("Flip Summoned " + card.Definition.Name + ".");
            ResolveFlipTrigger(State.Local, State.Rival, card, "Flip Summon");
            NotifyChanged();
        }

        public bool CanLinkSummonFromExtra(int extraDeckIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || extraDeckIndex < 0 || extraDeckIndex >= State.Local.ExtraDeck.Count)
            {
                return false;
            }

            List<int> materialZones;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            return TryPlanLinkSummon(State.Local, extraDeckIndex, out materialZones, out summonToExtraMonsterZone, out targetMainZone);
        }

        public void LinkSummonFromExtra(int extraDeckIndex)
        {
            if (!CanLinkSummonFromExtra(extraDeckIndex))
            {
                return;
            }

            if (!resolvingConfirmedSpecialSummon)
            {
                CleanCard summonCard = State.Local.ExtraDeck[extraDeckIndex];
                QueueSpecialSummonPrompt(
                    State.Local,
                    summonCard.Definition.Name,
                    "Confirm Link Summon from the Extra Deck.",
                    () => ResolveConfirmedSpecialSummonAction(() =>
                    {
                        int currentIndex = State.Local.ExtraDeck.IndexOf(summonCard);
                        if (currentIndex >= 0)
                        {
                            LinkSummonFromExtra(currentIndex);
                        }
                    }));
                NotifyChanged();
                return;
            }

            List<int> materialZones;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            if (!TryPlanLinkSummon(State.Local, extraDeckIndex, out materialZones, out summonToExtraMonsterZone, out targetMainZone))
            {
                return;
            }

            CleanCard linkMonster = State.Local.ExtraDeck[extraDeckIndex];
            State.Local.ExtraDeck.RemoveAt(extraDeckIndex);
            MoveLinkMaterialsToGrave(State.Local, materialZones);
            PrepareExtraDeckMonsterSummon(State.Local, linkMonster);
            if (summonToExtraMonsterZone)
            {
                State.ExtraMonsters[ExtraMonsterZoneIndexFor(State.Local)] = linkMonster;
                NotifySpecialSummon(State.Local);
                RequestSummonCinematics(State.Local, linkMonster, CleanSummonPresentationKind.Link);
                Log("Link Summoned " + linkMonster.Definition.Name + " to the Extra Monster Zone using " + materialZones.Count + " material(s).");
            }
            else
            {
                SummonToZone(State.Local, linkMonster, targetMainZone, false, CleanMonsterStance.Attack, false, CleanSummonPresentationKind.Link);
                Log("Link Summoned " + linkMonster.Definition.Name + " to a linked Main Monster Zone using " + materialZones.Count + " material(s).");
            }

            linkMonster.LinkSummoned = true;
            linkMonster.ProperlySummoned = true;
            ResolveLinkSummonTrigger(State.Local, linkMonster);
            NotifyChanged();
        }

        public bool CanSynchroSummonFromExtra(int extraDeckIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || extraDeckIndex < 0 || extraDeckIndex >= State.Local.ExtraDeck.Count)
            {
                return false;
            }

            List<int> materialZones;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            return TryPlanSynchroSummon(State.Local, extraDeckIndex, out materialZones, out summonToExtraMonsterZone, out targetMainZone);
        }

        public void SynchroSummonFromExtra(int extraDeckIndex)
        {
            if (!CanSynchroSummonFromExtra(extraDeckIndex))
            {
                return;
            }

            if (!resolvingConfirmedSpecialSummon)
            {
                CleanCard summonCard = State.Local.ExtraDeck[extraDeckIndex];
                QueueSpecialSummonPrompt(
                    State.Local,
                    summonCard.Definition.Name,
                    "Confirm Synchro Summon from the Extra Deck.",
                    () => ResolveConfirmedSpecialSummonAction(() =>
                    {
                        int currentIndex = State.Local.ExtraDeck.IndexOf(summonCard);
                        if (currentIndex >= 0)
                        {
                            SynchroSummonFromExtra(currentIndex);
                        }
                    }));
                NotifyChanged();
                return;
            }

            List<int> materialZones;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            if (!TryPlanSynchroSummon(State.Local, extraDeckIndex, out materialZones, out summonToExtraMonsterZone, out targetMainZone))
            {
                return;
            }

            CleanCard synchroMonster = State.Local.ExtraDeck[extraDeckIndex];
            State.Local.ExtraDeck.RemoveAt(extraDeckIndex);
            MoveSynchroMaterialsToGrave(State.Local, materialZones);
            PrepareExtraDeckMonsterSummon(State.Local, synchroMonster);
            synchroMonster.SynchroSummoned = true;
            synchroMonster.ProperlySummoned = true;
            if (summonToExtraMonsterZone)
            {
                State.ExtraMonsters[ExtraMonsterZoneIndexFor(State.Local)] = synchroMonster;
                NotifySpecialSummon(State.Local);
                RequestSummonCinematics(State.Local, synchroMonster, CleanSummonPresentationKind.Synchro);
                Log("Synchro Summoned " + synchroMonster.Definition.Name + " to the Extra Monster Zone using " + materialZones.Count + " material(s).");
            }
            else
            {
                SummonToZone(State.Local, synchroMonster, targetMainZone, false, CleanMonsterStance.Attack, false, CleanSummonPresentationKind.Synchro);
                Log("Synchro Summoned " + synchroMonster.Definition.Name + " to a linked Main Monster Zone using " + materialZones.Count + " material(s).");
            }

            NotifyChanged();
        }
        public bool OpenExtraDeckSummonSelection()
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || State.PendingChoice != null)
            {
                return false;
            }

            List<CleanChoiceOption> options = BuildValidExtraDeckSummonOptions();
            if (options.Count == 0)
            {
                return false;
            }

            State.PendingChoice = new CleanChoicePrompt(
                ExtraDeckSummonChoicePromptId,
                "Choose an Extra Deck Monster",
                "Only Synchro, Xyz, and Link Monsters whose material and zone requirements are currently satisfied are shown.",
                options,
                CleanPromptKind.SpecialSummon,
                true);
            Log("Extra Deck opened: choose a valid Synchro, Xyz, or Link Monster.");
            NotifyChanged();
            return true;
        }

        private List<CleanChoiceOption> BuildValidExtraDeckSummonOptions()
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < State.Local.ExtraDeck.Count; i++)
            {
                CleanCard card = State.Local.ExtraDeck[i];
                List<int> materials;
                bool toExtraZone;
                int targetMainZone;
                string detail;
                if (TryPlanSynchroSummon(State.Local, i, out materials, out toExtraZone, out targetMainZone))
                {
                    detail = "Synchro | Level " + card.Definition.Level + " | " + materials.Count + " materials | ATK " + card.CurrentAttack;
                }
                else if (TryPlanXyzSummon(State.Local, i, out materials, out toExtraZone, out targetMainZone))
                {
                    int rank = XyzRank(card);
                    detail = "Xyz | Rank " + rank + " | " + materials.Count + " Level " + rank + " materials | ATK " + card.CurrentAttack;
                }
                else if (TryPlanLinkSummon(State.Local, i, out materials, out toExtraZone, out targetMainZone))
                {
                    detail = "Link | Rating " + LinkRating(card) + " | " + materials.Count + " materials | ATK " + card.CurrentAttack;
                }
                else
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    card.RuntimeId,
                    card.Definition.Name,
                    detail,
                    true,
                    string.Empty,
                    card.Definition.Id));
            }

            return options;
        }

        private void ResolveExtraDeckSummonChoice(int runtimeId)
        {
            State.PendingChoice = null;
            CleanCard selected = FindCardByRuntimeId(State.Local.ExtraDeck, runtimeId);
            int extraDeckIndex = selected == null ? -1 : State.Local.ExtraDeck.IndexOf(selected);

            if (extraDeckIndex < 0)
            {
                Log("The selected Extra Deck Monster is no longer available.");
                return;
            }

            Log(State.Local.Name + " selects " + selected.Definition.Name + " from the Extra Deck.");
            if (CanSynchroSummonFromExtra(extraDeckIndex))
            {
                SynchroSummonFromExtra(extraDeckIndex);
            }
            else if (CanXyzSummonFromExtra(extraDeckIndex))
            {
                XyzSummonFromExtra(extraDeckIndex);
            }
            else if (CanLinkSummonFromExtra(extraDeckIndex))
            {
                LinkSummonFromExtra(extraDeckIndex);
            }
            else
            {
                Log("The selected Extra Deck Monster is no longer a valid summon.");
            }
        }
        public bool CanXyzSummonFromExtra(int extraDeckIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || extraDeckIndex < 0 || extraDeckIndex >= State.Local.ExtraDeck.Count)
            {
                return false;
            }

            List<int> materialZones;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            return TryPlanXyzSummon(State.Local, extraDeckIndex, out materialZones, out summonToExtraMonsterZone, out targetMainZone);
        }

        public void XyzSummonFromExtra(int extraDeckIndex)
        {
            if (!CanXyzSummonFromExtra(extraDeckIndex))
            {
                return;
            }

            if (!resolvingConfirmedSpecialSummon)
            {
                CleanCard summonCard = State.Local.ExtraDeck[extraDeckIndex];
                QueueSpecialSummonPrompt(
                    State.Local,
                    summonCard.Definition.Name,
                    "Confirm Xyz Summon from the Extra Deck.",
                    () => ResolveConfirmedSpecialSummonAction(() =>
                    {
                        int currentIndex = State.Local.ExtraDeck.IndexOf(summonCard);
                        if (currentIndex >= 0)
                        {
                            XyzSummonFromExtra(currentIndex);
                        }
                    }));
                NotifyChanged();
                return;
            }

            List<int> materialZones;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            if (!TryPlanXyzSummon(State.Local, extraDeckIndex, out materialZones, out summonToExtraMonsterZone, out targetMainZone))
            {
                return;
            }

            CleanCard xyzMonster = State.Local.ExtraDeck[extraDeckIndex];
            State.Local.ExtraDeck.RemoveAt(extraDeckIndex);
            MoveXyzMaterialsUnderMonster(State.Local, materialZones, xyzMonster);
            PrepareExtraDeckMonsterSummon(State.Local, xyzMonster);
            xyzMonster.XyzSummoned = true;
            xyzMonster.ProperlySummoned = true;
            if (summonToExtraMonsterZone)
            {
                State.ExtraMonsters[ExtraMonsterZoneIndexFor(State.Local)] = xyzMonster;
                NotifySpecialSummon(State.Local);
                RequestSummonCinematics(State.Local, xyzMonster, CleanSummonPresentationKind.Xyz);
                Log("Xyz Summoned " + xyzMonster.Definition.Name + " to the Extra Monster Zone using " + xyzMonster.XyzMaterials.Count + " material(s).");
            }
            else
            {
                SummonToZone(State.Local, xyzMonster, targetMainZone, false, CleanMonsterStance.Attack, false, CleanSummonPresentationKind.Xyz);
                Log("Xyz Summoned " + xyzMonster.Definition.Name + " to a linked Main Monster Zone using " + xyzMonster.XyzMaterials.Count + " material(s).");
            }

            NotifyChanged();
        }
        public bool CanActivateLocalExtraMonsterZoneCard(int extraMonsterZoneIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || extraMonsterZoneIndex != ExtraMonsterZoneIndexFor(State.Local))
            {
                return false;
            }

            CleanCard card = State.ExtraMonsters[extraMonsterZoneIndex];
            return card != null && !card.FaceDown && CanActivateCleanCard(card, State.Local, CleanActivationSource.MonsterZone);
        }

        public void RequestActivateLocalExtraMonsterZoneCard(int extraMonsterZoneIndex)
        {
            if (!CanActivateLocalExtraMonsterZoneCard(extraMonsterZoneIndex))
            {
                return;
            }

            CleanCard card = State.ExtraMonsters[extraMonsterZoneIndex];
            QueueManualEffectPrompt(
                card,
                CleanActivationSource.MonsterZone,
                "Activate this monster's effect from the Extra Monster Zone?",
                () =>
                {
                    if (object.ReferenceEquals(State.ExtraMonsters[extraMonsterZoneIndex], card))
                    {
                        ActivateLocalExtraMonsterZoneCard(extraMonsterZoneIndex);
                    }
                });
        }

        public void ActivateLocalExtraMonsterZoneCard(int extraMonsterZoneIndex)
        {
            if (!CanActivateLocalExtraMonsterZoneCard(extraMonsterZoneIndex))
            {
                return;
            }

            CleanCard card = State.ExtraMonsters[extraMonsterZoneIndex];
            ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.MonsterZone);
            NotifyChanged();
        }

        public bool IsLocalMainMonsterZoneLinkedForExtraDeckSummon(int zoneIndex)
        {
            return IsMainMonsterZoneLinkedForExtraDeckSummon(State.Local, zoneIndex);
        }

        public bool CanActivateSetCard(int zoneIndex)
        {
            return CanActivateSetCardInternal(zoneIndex, false);
        }

        public bool IsLocalQuickEffectWindowOpen()
        {
            return State.TurnPlayer == CleanPlayerId.Rival &&
                   State.PendingChoice != null &&
                   State.PendingChoice.SourceCardId == QuickEffectChoicePromptId;
        }

        public bool CanActivateLocalSpellTrapResponse(int zoneIndex)
        {
            return IsLocalQuickEffectWindowOpen() && CanActivateLocalSpellTrapResponseInternal(zoneIndex, true);
        }

        public void ActivateLocalSpellTrapResponse(int zoneIndex)
        {
            if (!CanActivateLocalSpellTrapResponse(zoneIndex))
            {
                return;
            }

            State.PendingChoice = null;
            pendingRivalTurnContinuationAfterResponse = true;
            if (!ResolveLocalSpellTrapResponse(zoneIndex, true))
            {
                pendingRivalTurnContinuationAfterResponse = false;
                Log("The selected Set card could not be activated in this response window.");
            }

            ContinueRivalTurnAfterResponseIfReady();
            NotifyChanged();
        }

        private bool CanActivateSetCardInternal(int zoneIndex, bool allowPending)
        {
            if (State.GameOver ||
                (!allowPending && State.PendingChoice != null) ||
                zoneIndex < 0 ||
                zoneIndex > FieldSpellZoneIndex)
            {
                return false;
            }

            bool localTurn = State.TurnPlayer == CleanPlayerId.Local;
            if (!localTurn && State.TurnPlayer != CleanPlayerId.Rival)
            {
                return false;
            }

            CleanCard card = zoneIndex == FieldSpellZoneIndex ? State.Local.FieldSpell : State.Local.SpellsTraps[zoneIndex];
            if (card == null)
            {
                return false;
            }

            bool spellSpeedTwo = IsQuickPlaySpell(card) || card.Definition.Kind == CleanCardKind.Trap;
            if (!localTurn && !spellSpeedTwo)
            {
                return false;
            }

            if (localTurn && !IsMainPhase(State.Phase) && !spellSpeedTwo)
            {
                return false;
            }

            if (card.FaceDown && card.SetThisTurn && spellSpeedTwo)
            {
                return false;
            }

            if (card.Definition.Id == TrueLightCardId)
            {
                return card.FaceDown || CanOpenTrueLightChoice(State.Local);
            }

            return CanActivateCleanCard(card, State.Local, CleanActivationSource.SpellTrapZone);
        }

        public bool CanActivateLocalQuickEffectHandCard(int handIndex)
        {
            return CanActivateLocalQuickEffectHandCardInternal(handIndex, false);
        }

        public bool CanActivateLocalQuickEffectMonsterZoneCard(int zoneIndex)
        {
            return CanActivateLocalQuickEffectMonsterZoneCardInternal(zoneIndex, false);
        }

        public void RequestActivateLocalQuickEffectHandCard(int handIndex)
        {
            if (!CanActivateLocalQuickEffectHandCard(handIndex))
            {
                return;
            }

            CleanCard card = State.Local.Hand[handIndex];
            QueueManualEffectPrompt(
                card,
                CleanActivationSource.Hand,
                "Activate this Quick Effect from your hand?",
                () =>
                {
                    int currentIndex = State.Local.Hand.IndexOf(card);
                    if (currentIndex >= 0)
                    {
                        ActivateLocalQuickEffectHandCard(currentIndex);
                    }
                });
        }

        public void ActivateLocalQuickEffectHandCard(int handIndex)
        {
            if (!ResolveLocalQuickEffectFromHand(handIndex, false))
            {
                return;
            }

            NotifyChanged();
        }

        public void RequestActivateLocalQuickEffectMonsterZoneCard(int zoneIndex)
        {
            if (!CanActivateLocalQuickEffectMonsterZoneCard(zoneIndex))
            {
                return;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            QueueManualEffectPrompt(
                card,
                CleanActivationSource.MonsterZone,
                "Activate this monster's Quick Effect?",
                () =>
                {
                    if (object.ReferenceEquals(State.Local.Monsters[zoneIndex], card))
                    {
                        ActivateLocalQuickEffectMonsterZoneCard(zoneIndex);
                    }
                });
        }

        public void ActivateLocalQuickEffectMonsterZoneCard(int zoneIndex)
        {
            if (!ResolveLocalQuickEffectFromMonsterZone(zoneIndex, false))
            {
                return;
            }

            NotifyChanged();
        }

        public CleanEffectTiming GetEffectTiming(CleanCard card)
        {
            if (card == null || string.IsNullOrEmpty(card.Definition.EffectKey) || IsNormalMonster(card))
            {
                return CleanEffectTiming.None;
            }

            if (IsQuickEffect(card))
            {
                return CleanEffectTiming.Quick;
            }

            if (IsFlipTriggerEffect(card) || IsTriggerOnlyEffect(card))
            {
                return CleanEffectTiming.Trigger;
            }

            if (HasContinuousEffect(card))
            {
                return CleanEffectTiming.Continuous;
            }

            return CleanEffectTiming.Ignition;
        }

        public int GetSpellSpeed(CleanCard card)
        {
            if (card == null || string.IsNullOrEmpty(card.Definition.EffectKey) || IsNormalMonster(card))
            {
                return 0;
            }

            if (IsQuickEffect(card) || IsQuickPlaySpell(card) || card.Definition.Kind == CleanCardKind.Trap)
            {
                return 2;
            }

            CleanEffectTiming timing = GetEffectTiming(card);
            return timing == CleanEffectTiming.Ignition || timing == CleanEffectTiming.Trigger ? 1 : 0;
        }
        public bool CanAttackWith(int monsterZone)
        {
            if (!CanUseLocalActions() || State.Phase != CleanDuelPhase.Battle || IsFirstTurnOfDuel() || monsterZone < 0 || monsterZone >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.Monsters[monsterZone];
            return card != null &&
                   !card.FaceDown &&
                   !card.CannotAttack &&
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
            bool usesMausoleumExtraNormalSummon = State.Local.NormalSummonUsed && CanUseMausoleumExtraNormalSummon(State.Local, card);
            int tributeCount = RequiredTributesForNormalPlay(card);
            if (!TributeForNormalPlay(State.Local, card))
            {
                return;
            }

            int zone = FirstEmpty(State.Local.Monsters);
            State.Local.Hand.RemoveAt(handIndex);
            SummonToZone(State.Local, card, zone, false, CleanMonsterStance.Attack, true);
            if (usesMausoleumExtraNormalSummon)
            {
                State.Local.UsedOncePerTurn.Add(MausoleumExtraNormalOnceKey);
                Log("Mausoleum of White grants the additional Normal Summon for " + card.Definition.Name + ".");
            }
            else
            {
                State.Local.NormalSummonUsed = true;
            }

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
            SummonToZone(State.Local, card, zone, true, CleanMonsterStance.Defense, true);
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
            State.Local.Hand.RemoveAt(handIndex);
            if (IsFieldSpellCard(card))
            {
                PlaceFieldSpell(State.Local, card, true);
                card.SetThisTurn = true;
                Log("Set Field Spell card in the Field Spell Zone.");
                NotifyChanged();
                return;
            }

            int zone = FirstEmpty(State.Local.SpellsTraps);
            State.Local.SpellsTraps[zone] = card;
            card.Controller = State.Local.Id;
            card.FaceDown = true;
            card.SetThisTurn = true;
            Log("Set " + card.Definition.Kind + " card.");
            NotifyChanged();
        }

        public void RequestActivateHandCard(int handIndex)
        {
            if (!CanActivateHandCard(handIndex))
            {
                return;
            }

            CleanCard card = State.Local.Hand[handIndex];
            string body = CanActivatePendulumFromHand(handIndex)
                ? "Activate this card as a Pendulum Spell?"
                : "Activate this card's effect from your hand?";
            QueueManualEffectPrompt(
                card,
                CleanActivationSource.Hand,
                body,
                () =>
                {
                    int currentIndex = State.Local.Hand.IndexOf(card);
                    if (currentIndex >= 0)
                    {
                        ActivateHandCard(currentIndex);
                    }
                });
        }

        public void ActivateHandCard(int handIndex)
        {
            if (!CanActivateHandCard(handIndex))
            {
                return;
            }

            if (CanActivatePendulumFromHand(handIndex))
            {
                ActivatePendulumFromHand(handIndex);
                return;
            }

            CleanCard card = State.Local.Hand[handIndex];
            if (!resolvingConfirmedSpecialSummon && RequiresSpecialSummonConfirmation(card, CleanActivationSource.Hand))
            {
                QueueSpecialSummonPrompt(
                    State.Local,
                    card.Definition.Name,
                    "Activate this card and perform its Special Summon?",
                    () => ResolveConfirmedSpecialSummonAction(() =>
                    {
                        int currentIndex = State.Local.Hand.IndexOf(card);
                        if (currentIndex >= 0)
                        {
                            ActivateHandCard(currentIndex);
                        }
                    }));
                NotifyChanged();
                return;
            }

            State.Local.Hand.RemoveAt(handIndex);
            if (ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.Hand) &&
                !object.ReferenceEquals(State.Local.FieldSpell, card))
            {
                AddToGraveyard(State.Local, card, true);
            }

            NotifyChanged();
        }

        public void RequestActivateMonsterZoneCard(int zoneIndex)
        {
            if (!CanActivateMonsterZoneCard(zoneIndex))
            {
                return;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            QueueManualEffectPrompt(
                card,
                CleanActivationSource.MonsterZone,
                "Activate this monster's effect?",
                () =>
                {
                    if (object.ReferenceEquals(State.Local.Monsters[zoneIndex], card))
                    {
                        ActivateMonsterZoneCard(zoneIndex);
                    }
                });
        }

        public void ActivateMonsterZoneCard(int zoneIndex)
        {
            if (!CanActivateMonsterZoneCard(zoneIndex))
            {
                return;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            if (!resolvingConfirmedSpecialSummon && RequiresSpecialSummonConfirmation(card, CleanActivationSource.MonsterZone))
            {
                QueueSpecialSummonPrompt(
                    State.Local,
                    card.Definition.Name,
                    "Activate this monster effect and perform its Special Summon?",
                    () => ResolveConfirmedSpecialSummonAction(() => ActivateMonsterZoneCard(zoneIndex)));
                NotifyChanged();
                return;
            }

            bool sendsSelf = ShouldSendSelfForActivatedMonsterEffect(card, State.Local, State.Rival);
            if (sendsSelf)
            {
                State.Local.Monsters[zoneIndex] = null;
                card.FaceDown = false;
                card.AttackBonus = 0;
                card.DefenseBonus = 0;
                card.HasBattled = false;
            }

            if (ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.MonsterZone) && sendsSelf)
            {
                SendFieldCardToGraveOrFaceUpExtra(State.Local, card, true);
            }

            NotifyChanged();
        }

        public void RequestActivateSetCard(int zoneIndex)
        {
            if (!CanActivateSetCard(zoneIndex))
            {
                return;
            }

            CleanCard card = zoneIndex == FieldSpellZoneIndex
                ? State.Local.FieldSpell
                : State.Local.SpellsTraps[zoneIndex];
            QueueManualEffectPrompt(
                card,
                CleanActivationSource.SpellTrapZone,
                card.FaceDown ? "Activate this Set card and its effect?" : "Activate this face-up card's effect?",
                () =>
                {
                    CleanCard current = zoneIndex == FieldSpellZoneIndex
                        ? State.Local.FieldSpell
                        : State.Local.SpellsTraps[zoneIndex];
                    if (object.ReferenceEquals(current, card))
                    {
                        ActivateSetCard(zoneIndex);
                    }
                });
        }

        public void ActivateSetCard(int zoneIndex)
        {
            if (!CanActivateSetCard(zoneIndex))
            {
                return;
            }

            if (zoneIndex == FieldSpellZoneIndex)
            {
                CleanCard fieldCard = State.Local.FieldSpell;
                if (fieldCard != null)
                {
                    fieldCard.SetThisTurn = false;
                    ResolveEffect(State.Local, State.Rival, fieldCard, CleanActivationSource.SpellTrapZone);
                    if (fieldCard.FaceDown)
                    {
                        fieldCard.FaceDown = false;
                    }
                }

                NotifyChanged();
                return;
            }

            CleanCard card = State.Local.SpellsTraps[zoneIndex];
            if (!resolvingConfirmedSpecialSummon && RequiresSpecialSummonConfirmation(card, CleanActivationSource.SpellTrapZone))
            {
                QueueSpecialSummonPrompt(
                    State.Local,
                    card.Definition.Name,
                    "Activate this Set card and perform its Special Summon?",
                    () => ResolveConfirmedSpecialSummonAction(() => ActivateSetCard(zoneIndex)));
                NotifyChanged();
                return;
            }

            if (card.Definition.Id == TrueLightCardId)
            {
                bool wasFaceDown = card.FaceDown;
                card.SetThisTurn = false;
                card.FaceDown = false;
                if (CanOpenTrueLightChoice(State.Local))
                {
                    OpenTrueLightChoice(State.Local);
                }
                else if (wasFaceDown)
                {
                    Log("True Light is activated. Its once-per-turn effect is not used.");
                }

                NotifyChanged();
                return;
            }

            if (card.Definition.Id == MausoleumOfWhiteCardId)
            {
                bool wasFaceDown = card.FaceDown;
                card.SetThisTurn = false;
                card.FaceDown = false;
                if (wasFaceDown)
                {
                    State.Local.SpellsTraps[zoneIndex] = null;
                    PlaceFieldSpell(State.Local, card);
                    Log("Mausoleum of White is activated in the Field Spell Zone.");
                }
                else
                {
                    ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.SpellTrapZone);
                }

                NotifyChanged();
                return;
            }

            State.Local.SpellsTraps[zoneIndex] = null;
            card.SetThisTurn = false;
            card.FaceDown = false;
            if (ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.SpellTrapZone))
            {
                SendFieldCardToGraveOrFaceUpExtra(State.Local, card, true);
            }

            NotifyChanged();
        }

        public void ChoosePendingOption(int optionIndex)
        {
            ChoosePendingOptionForPlayer(State.Local, optionIndex, true);
        }

        private void ChoosePendingOptionForPlayer(CleanPlayerState choicePlayer, int optionIndex, bool continueRivalTurn)
        {
            if (choicePlayer == null || State.PendingChoice == null || optionIndex < 0 || optionIndex >= State.PendingChoice.Options.Count)
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

            CleanChoicePrompt currentPrompt = State.PendingChoice;
            if (currentPrompt.SourceCardId == TriggerEffectPromptId ||
                currentPrompt.SourceCardId == SpecialSummonPromptId ||
                currentPrompt.SourceCardId == CardEffectPromptId)
            {
                ResolveQueuedUiPrompt(option.Id == PromptConfirmOptionId);
            }
            else if (currentPrompt.SourceCardId == TrueLightCardId)
            {
                ResolveTrueLightChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == TrueLightSetCardChoicePromptId)
            {
                if (ResolveTrueLightSetSpellTrap(choicePlayer, option.Id))
                {
                    choicePlayer.UsedOncePerTurn.Add(TrueLightOnceKey);
                }
            }
            else if (currentPrompt.SourceCardId == BattlePhaseChoicePromptId)
            {
                ResolveBattlePhaseChoice(option.Id);
            }
            else if (currentPrompt.SourceCardId == DictatorSendChoicePromptId)
            {
                ResolveDictatorSendChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == DictatorDiscardChoicePromptId)
            {
                ResolveDictatorDiscardChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == DictatorReviveChoicePromptId)
            {
                ResolveDictatorReviveChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == DictatorAttackTargetChoicePromptId)
            {
                ResolveDictatorAttackTargetChoice(option.Id);
            }
            else if (currentPrompt.SourceCardId == JetBounceChoicePromptId)
            {
                ResolveJetBounceChoice(option.Id);
            }
            else if (currentPrompt.SourceCardId == DragonSpiritBanishChoicePromptId)
            {
                ResolveDragonSpiritBanishChoice(choicePlayer, OpponentOf(choicePlayer.Id), option.Id);
            }
            else if (currentPrompt.SourceCardId == DragonShrineFirstChoicePromptId)
            {
                ResolveDragonShrineFirstChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == DragonShrineSecondChoicePromptId)
            {
                ResolveDragonShrineSecondChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == BingoChoicePromptId)
            {
                ResolveBingoChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == VeilerTargetChoicePromptId)
            {
                ResolveVeilerTargetChoice(choicePlayer, OpponentOf(choicePlayer.Id), option.Id);
            }
            else if (currentPrompt.SourceCardId == MagiaLeaveChoicePromptId)
            {
                ResolveMagiaLeaveChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == StoneRecoverChoicePromptId)
            {
                ResolveStoneRecoverChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == WishesEquipTargetChoicePromptId)
            {
                ResolveWishesEquipTargetChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == WishesEquipMonsterChoicePromptId)
            {
                ResolveWishesEquipMonsterChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == WishesDiscardChoicePromptId)
            {
                ResolveWishesDiscardChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == WishesTunerChoicePromptId)
            {
                ResolveWishesTunerChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == WishesSupportChoicePromptId)
            {
                ResolveWishesSupportChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == SageSearchChoicePromptId)
            {
                ResolveSageSearchChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == SageTargetChoicePromptId)
            {
                ResolveSageTargetChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == SageBlueEyesChoicePromptId)
            {
                ResolveSageBlueEyesChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == MajestySendChoicePromptId)
            {
                ResolveMajestySendChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == MajestyTargetChoicePromptId)
            {
                ResolveMajestyTargetChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == MausoleumTargetChoicePromptId)
            {
                ResolveMausoleumTargetChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == MausoleumNormalChoicePromptId)
            {
                ResolveMausoleumNormalChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == GatewaySearchChoicePromptId)
            {
                ResolveGatewaySearchChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == SpiritEffectChoicePromptId)
            {
                ResolveSpiritEffectChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == SpiritMausoleumDestinationChoicePromptId)
            {
                ResolveSpiritMausoleumDestinationChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == SpiritSummonChoicePromptId)
            {
                ResolveSpiritSummonChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == MaidenWhiteReviveChoicePromptId)
            {
                ResolveMaidenWhiteReviveChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == NeoKaiserLevelChoicePromptId)
            {
                ResolveNeoKaiserLevelChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == NeoKaiserSendChoicePromptId)
            {
                ResolveNeoKaiserSendChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == AlternativeDestroyChoicePromptId)
            {
                ResolveAlternativeDestroyChoice(choicePlayer, OpponentOf(choicePlayer.Id), option.Id);
            }
            else if (currentPrompt.SourceCardId == AbyssSearchChoicePromptId)
            {
                ResolveAbyssSearchChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == AbyssEndPhaseChoicePromptId)
            {
                ResolveAbyssEndPhaseChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == MonsterRebornChoicePromptId)
            {
                ResolveMonsterRebornChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == LuaDestroyTargetChoicePromptId)
            {
                ResolveLuaDestroyTargetChoice(option.Id);
            }
            else if (currentPrompt.SourceCardId == QuickEffectChoicePromptId)
            {
                ResolveQuickEffectChoice(option.Id);
            }
            else if (currentPrompt.SourceCardId == ExtraDeckSummonChoicePromptId)
            {
                ResolveExtraDeckSummonChoice(option.Id);
            }
            else if (currentPrompt.SourceCardId == FusionSummonChoicePromptId)
            {
                ResolveFusionSummonChoice(option.Id);
            }
            else if (currentPrompt.SourceCardId == RitualSummonChoicePromptId)
            {
                ResolveRitualSummonChoice(option.Id);
            }
            else if (currentPrompt.SourceCardId == PendulumSummonChoicePromptId)
            {
                ResolvePendulumSummonChoice(choicePlayer, option.Id);
            }
            else if (currentPrompt.SourceCardId == SurrenderPromptId)
            {
                State.PendingChoice = null;
                if (option.Id == PromptConfirmOptionId)
                {
                    CleanPlayerState loser = choicePlayer;
                    CleanPlayerState winner = OpponentOf(choicePlayer.Id);
                    EndDuel(
                        CleanDuelEndReason.Surrender,
                        winner,
                        loser,
                        loser.Name + " surrendered. " + winner.Name + " wins.");
                }
                else
                {
                    Log("Surrender cancelled.");
                }
            }

            if (object.ReferenceEquals(State.PendingChoice, currentPrompt))
            {
                State.PendingChoice = null;
            }

            ShowNextQueuedUiPrompt();
            ContinueEndTurnAfterPromptsIfReady();
            CheckWin();
            if (continueRivalTurn)
            {
                ContinueRivalTurnAfterResponseIfReady();
            }
            NotifyChanged();
        }

        public void CancelPendingChoice()
        {
            if (State.PendingChoice == null)
            {
                return;
            }

            if (State.PendingChoice.SourceCardId == TriggerEffectPromptId ||
                State.PendingChoice.SourceCardId == SpecialSummonPromptId ||
                State.PendingChoice.SourceCardId == CardEffectPromptId)
            {
                ResolveQueuedUiPrompt(false);
                CheckWin();
                NotifyChanged();
                return;
            }
            if (State.PendingChoice.SourceCardId == DictatorAttackTargetChoicePromptId)
            {
                Log("Dictator of D. requires an attack target choice.");
                NotifyChanged();
                return;
            }

            if (State.PendingChoice.SourceCardId == SurrenderPromptId)
            {
                State.PendingChoice = null;
                Log("Surrender cancelled.");
                NotifyChanged();
                return;
            }

            Log("Choice cancelled.");
            ClearPendingEffectChoices();
            State.PendingChoice = null;
            ShowNextQueuedUiPrompt();
            ContinueEndTurnAfterPromptsIfReady();
            ContinueRivalTurnAfterResponseIfReady();
            NotifyChanged();
        }

        private string TriggerPromptKey(CleanPlayerState player, string effectKey)
        {
            return player.Id + ":" + effectKey;
        }

        private bool TryReserveTriggerPrompt(CleanPlayerState player, string effectKey)
        {
            return pendingTriggerPromptKeys.Add(TriggerPromptKey(player, effectKey));
        }

        private void ReleaseTriggerPrompt(CleanPlayerState player, string effectKey)
        {
            pendingTriggerPromptKeys.Remove(TriggerPromptKey(player, effectKey));
        }
        private void QueueTriggerEffectPrompt(
            CleanPlayerState player,
            string sourceName,
            string body,
            bool optional,
            Action confirm,
            Action decline = null)
        {
            QueueUiPrompt(
                player,
                TriggerEffectPromptId,
                sourceName,
                body,
                "Resolve this Trigger Effect.",
                CleanPromptKind.TriggerEffect,
                optional,
                confirm,
                decline);
        }

        private void QueueSpecialSummonPrompt(CleanPlayerState player, string sourceName, string body, Action confirm)
        {
            QueueUiPrompt(
                player,
                SpecialSummonPromptId,
                sourceName,
                body,
                "Perform this Special Summon.",
                CleanPromptKind.SpecialSummon,
                true,
                confirm,
                null);
        }

        private void QueueCardEffectPrompt(CleanPlayerState player, string sourceName, string body, Action confirm)
        {
            QueueUiPrompt(
                player,
                CardEffectPromptId,
                sourceName,
                body,
                "Activate this card effect.",
                CleanPromptKind.CardEffect,
                true,
                confirm,
                null);
        }

        private void QueueManualEffectPrompt(
            CleanCard card,
            CleanActivationSource source,
            string body,
            Action activate)
        {
            if (card == null || activate == null)
            {
                return;
            }

            if (RequiresUiSpecialSummonPrompt(card, source))
            {
                QueueSpecialSummonPrompt(
                    State.Local,
                    card.Definition.Name,
                    body,
                    () => ResolveConfirmedSpecialSummonAction(activate));
            }
            else
            {
                QueueCardEffectPrompt(State.Local, card.Definition.Name, body, activate);
            }

            NotifyChanged();
        }

        private void QueueUiPrompt(
            CleanPlayerState player,
            int promptId,
            string title,
            string body,
            string confirmDetail,
            CleanPromptKind kind,
            bool optional,
            Action confirm,
            Action decline)
        {
            if (confirm == null)
            {
                return;
            }

            if (player == null || player.Id != CleanPlayerId.Local)
            {
                confirm();
                return;
            }

            pendingUiPrompts.Enqueue(new QueuedUiPrompt
            {
                PromptId = promptId,
                Title = title,
                Body = body,
                ConfirmDetail = confirmDetail,
                Kind = kind,
                Optional = optional,
                Confirm = confirm,
                Decline = decline
            });
            ShowNextQueuedUiPrompt();
        }

        private void ShowNextQueuedUiPrompt()
        {
            if (State.GameOver || State.PendingChoice != null || activeUiPrompt != null || pendingUiPrompts.Count == 0)
            {
                return;
            }

            activeUiPrompt = pendingUiPrompts.Dequeue();
            List<CleanChoiceOption> options = new List<CleanChoiceOption>
            {
                new CleanChoiceOption(
                    PromptConfirmOptionId,
                    activeUiPrompt.Kind == CleanPromptKind.SpecialSummon ? "Special Summon" : "Activate effect",
                    activeUiPrompt.ConfirmDetail,
                    true,
                    string.Empty)
            };

            if (activeUiPrompt.Optional)
            {
                options.Add(new CleanChoiceOption(
                    PromptDeclineOptionId,
                    activeUiPrompt.Kind == CleanPromptKind.SpecialSummon ? "Cancel" : "Do not activate",
                    activeUiPrompt.Kind == CleanPromptKind.SpecialSummon
                        ? "Do not perform this Special Summon."
                        : activeUiPrompt.Kind == CleanPromptKind.TriggerEffect
                            ? "Skip this optional Trigger Effect."
                            : "Do not activate this card effect.",
                    true,
                    string.Empty));
            }

            State.PendingChoice = new CleanChoicePrompt(
                activeUiPrompt.PromptId,
                activeUiPrompt.Title,
                activeUiPrompt.Body,
                options,
                activeUiPrompt.Kind,
                false);
            Log(PromptKindLabel(activeUiPrompt.Kind) + " prompt opened for " + activeUiPrompt.Title + ".");
        }

        private static string PromptKindLabel(CleanPromptKind kind)
        {
            switch (kind)
            {
                case CleanPromptKind.SpecialSummon:
                    return "Special Summon";
                case CleanPromptKind.TriggerEffect:
                    return "Trigger Effect";
                case CleanPromptKind.CardEffect:
                    return "Card Effect";
                default:
                    return "Choice";
            }
        }

        private void ResolveQueuedUiPrompt(bool confirmed)
        {
            QueuedUiPrompt prompt = activeUiPrompt;
            activeUiPrompt = null;
            State.PendingChoice = null;
            if (prompt == null)
            {
                ShowNextQueuedUiPrompt();
                ContinueRivalTurnAfterResponseIfReady();
                return;
            }

            Action action = confirmed ? prompt.Confirm : prompt.Decline;
            if (action != null)
            {
                action();
            }
            else if (!confirmed)
            {
                Log(prompt.Title + (prompt.Kind == CleanPromptKind.SpecialSummon
                    ? " Special Summon was cancelled."
                    : prompt.Kind == CleanPromptKind.TriggerEffect
                        ? " optional Trigger Effect was not activated."
                        : " card effect was not activated."));
            }

            ShowNextQueuedUiPrompt();
            ContinueEndTurnAfterPromptsIfReady();
            ContinueRivalTurnAfterResponseIfReady();
        }

        private void ResolveConfirmedSpecialSummonAction(Action action)
        {
            resolvingConfirmedSpecialSummon = true;
            try
            {
                action();
            }
            finally
            {
                resolvingConfirmedSpecialSummon = false;
            }
        }

        public bool CanToggleLocalMonsterStance(int zoneIndex)
        {
            if (!CanUseLocalActions() || !IsMainPhase(State.Phase) || zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            if (card == null || card.SummonedThisTurn || card.PositionChangedThisTurn || (State.Phase == CleanDuelPhase.Main2 && card.HasBattled))
            {
                return false;
            }

            return !card.FaceDown || CanFlipSummonMonster(zoneIndex);
        }

        public void ToggleLocalMonsterStance(int zoneIndex)
        {
            if (!CanToggleLocalMonsterStance(zoneIndex))
            {
                return;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            if (card.FaceDown)
            {
                FlipSummonMonster(zoneIndex);
                return;
            }

            card.Stance = card.Stance == CleanMonsterStance.Attack ? CleanMonsterStance.Defense : CleanMonsterStance.Attack;
            card.PositionChangedThisTurn = true;
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
                RequestSignatureFinalBlowIfLethal(State.Local, attacker, State.Rival);
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
            if (State.GameOver || State.PendingChoice != null)
            {
                return;
            }

            switch (State.Phase)
            {
                case CleanDuelPhase.Draw:
                    EnterStandbyPhase();
                    break;
                case CleanDuelPhase.Standby:
                    EnterMainPhase1();
                    break;
                case CleanDuelPhase.Main:
                    if (IsFirstTurnOfDuel())
                    {
                        Log("First turn cannot enter the Battle Phase; turn ends.");
                        EnterEndPhaseAndEndTurn();
                        return;
                    }

                    State.Phase = CleanDuelPhase.Battle;
                    Log("Phase: Battle.");
                    break;
                case CleanDuelPhase.Battle:
                    OpenBattlePhaseChoice();
                    NotifyChanged();
                    return;
                case CleanDuelPhase.Main2:
                case CleanDuelPhase.End:
                    EnterEndPhaseAndEndTurn();
                    return;
                default:
                    EnterMainPhase1();
                    break;
            }

            NotifyChanged();

            if (!State.GameOver && State.TurnPlayer == CleanPlayerId.Rival)
            {
                RunRivalTurn();
            }
        }

        public bool CanSelectPhase(CleanDuelPhase targetPhase)
        {
            if (!CanUseLocalActions() || targetPhase == State.Phase)
            {
                return false;
            }

            switch (targetPhase)
            {
                case CleanDuelPhase.Battle:
                    return State.Phase == CleanDuelPhase.Main && !IsFirstTurnOfDuel();
                case CleanDuelPhase.Main2:
                    return State.Phase == CleanDuelPhase.Battle;
                case CleanDuelPhase.End:
                    return State.Phase == CleanDuelPhase.Main ||
                           State.Phase == CleanDuelPhase.Battle ||
                           State.Phase == CleanDuelPhase.Main2;
                default:
                    return false;
            }
        }

        public void SelectPhase(CleanDuelPhase targetPhase)
        {
            if (!CanSelectPhase(targetPhase))
            {
                return;
            }

            switch (targetPhase)
            {
                case CleanDuelPhase.Battle:
                    State.Phase = CleanDuelPhase.Battle;
                    Log("Phase: Battle.");
                    NotifyChanged();
                    break;
                case CleanDuelPhase.Main2:
                    State.Phase = CleanDuelPhase.Main2;
                    Log("Phase: Main Phase 2.");
                    NotifyChanged();
                    break;
                case CleanDuelPhase.End:
                    EnterEndPhaseAndEndTurn();
                    break;
            }
        }

        private void EnterStandbyPhase()
        {
            State.Phase = CleanDuelPhase.Standby;
            Log("Phase: Standby.");
        }

        private void EnterMainPhase1()
        {
            State.Phase = CleanDuelPhase.Main;
            Log("Phase: Main Phase 1.");
        }

        private void OpenBattlePhaseChoice()
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>
            {
                new CleanChoiceOption(BattlePhaseMain2OptionId, "Main Phase 2", "Continue with Main Phase 2.", true, string.Empty),
                new CleanChoiceOption(BattlePhaseEndOptionId, "End Turn", "Go to End Phase and pass the turn.", true, string.Empty)
            };

            State.PendingChoice = new CleanChoicePrompt(
                BattlePhaseChoicePromptId,
                "Battle Phase",
                "Choose where to go after Battle Phase.",
                options);
            Log("Battle Phase awaits your next phase choice.");
        }

        private void ResolveBattlePhaseChoice(int optionId)
        {
            if (optionId == BattlePhaseMain2OptionId)
            {
                State.Phase = CleanDuelPhase.Main2;
                Log("Phase: Main Phase 2.");
                return;
            }

            if (optionId == BattlePhaseEndOptionId)
            {
                EnterEndPhaseAndEndTurn();
            }
        }

        private void EnterEndPhaseAndEndTurn()
        {
            State.Phase = CleanDuelPhase.End;
            Log("Phase: End. Turn ends automatically.");
            EndTurn();
        }

        public void EndTurn()
        {
            if (State.GameOver || pendingEndTurnContinuation)
            {
                return;
            }

            ResolveEndPhaseTriggers();
            if (State.PendingChoice != null || activeUiPrompt != null || pendingUiPrompts.Count > 0)
            {
                pendingEndTurnContinuation = true;
                NotifyChanged();
                return;
            }

            CompleteEndTurn();
        }

        private void ContinueEndTurnAfterPromptsIfReady()
        {
            if (!pendingEndTurnContinuation ||
                State.GameOver ||
                State.PendingChoice != null ||
                activeUiPrompt != null ||
                pendingUiPrompts.Count > 0)
            {
                return;
            }

            pendingEndTurnContinuation = false;
            CompleteEndTurn();
        }

        private void ContinueRivalTurnAfterResponseIfReady()
        {
            if (!pendingRivalTurnContinuationAfterResponse)
            {
                return;
            }

            if (State.GameOver || State.TurnPlayer != CleanPlayerId.Rival)
            {
                pendingRivalTurnContinuationAfterResponse = false;
                return;
            }

            if (State.PendingChoice != null || activeUiPrompt != null || pendingUiPrompts.Count > 0)
            {
                return;
            }

            pendingRivalTurnContinuationAfterResponse = false;
            ContinueRivalTurnAfterQuickWindow();
        }

        private void CompleteEndTurn()
        {
            pendingRivalTurnContinuationAfterResponse = false;
            rivalAiAttemptedEffectsThisTurn.Clear();
            ClearTurnBoundaryState(State.Local);
            ClearTurnBoundaryState(State.Rival);
            State.PendingChoice = null;
            ClearResponseContext();
            pendingRivalActivatedCard = null;
            State.TurnPlayer = State.TurnPlayer == CleanPlayerId.Local ? CleanPlayerId.Rival : CleanPlayerId.Local;
            localQuickWindowHandledForRivalTurn = State.TurnPlayer != CleanPlayerId.Rival;
            State.TurnNumber++;
            State.Phase = CleanDuelPhase.Draw;
            int drawn = Draw(CurrentPlayer, 1);
            if (State.GameOver)
            {
                NotifyChanged();
                return;
            }

            Log(CurrentPlayer.Name + " draws " + drawn + " card for turn.");
            EnterStandbyPhase();
            if (State.TurnPlayer == CleanPlayerId.Local)
            {
                EnterMainPhase1();
                RunRivalOpenStateResponses();
            }
            NotifyChanged();

            if (!State.GameOver && State.TurnPlayer == CleanPlayerId.Rival)
            {
                RunRivalTurn();
            }
        }

        private void ClearTurnBoundaryState(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                ClearEndOfTurnMonsterState(player.Monsters[i]);
                if (player.SpellsTraps[i] != null)
                {
                    player.SpellsTraps[i].SetThisTurn = false;
                }
            }

            CleanCard extraMonster = State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)];
            ClearEndOfTurnMonsterState(extraMonster);
            if (player.FieldSpell != null)
            {
                player.FieldSpell.SetThisTurn = false;
            }

            player.NormalSummonUsed = false;
            player.PendulumSummonUsed = false;
            player.MaxxCDrawActive = false;
            player.ExtraDeckDragonOnlyThisTurn = false;
            ClearSentToGraveFlags(player);
            player.UsedOncePerTurn.Clear();
        }

        private void ClearEndOfTurnMonsterState(CleanCard monster)
        {
            if (monster == null)
            {
                return;
            }

            monster.HasBattled = false;
            monster.SummonedThisTurn = false;
            monster.PositionChangedThisTurn = false;
            monster.AttackBonus = 0;
            monster.DefenseBonus = 0;
            monster.PiercingThisTurn = false;
            monster.DestroyDuringEndPhase = false;
            if (monster.CannotAttackUntilEndOfTurn)
            {
                monster.CannotAttack = false;
                monster.CannotAttackUntilEndOfTurn = false;
            }
            if (monster.EffectsNegatedUntilEndOfTurn)
            {
                monster.EffectsNegated = false;
                monster.EffectsNegatedUntilEndOfTurn = false;
            }
        }

        private void ResolveEndPhaseTriggers()
        {
            ResolveSpiritDragonEndPhaseDestruction(State.Local);
            ResolveSpiritDragonEndPhaseDestruction(State.Rival);
            ResolveAbyssEndPhaseTrigger(State.Local);
            ResolveAbyssEndPhaseTrigger(State.Rival);
            ResolveWhiteStoneEndPhaseTrigger(State.Local);
            ResolveWhiteStoneEndPhaseTrigger(State.Rival);
        }

        private void ResolveSpiritDragonEndPhaseDestruction(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.Monsters[i];
                if (card == null || !card.DestroyDuringEndPhase)
                {
                    continue;
                }

                string name = card.PublicName;
                MoveMonsterToGrave(player, i);
                TriggerJetSummonAfterFieldCardDestroyed(card);
                Log(name + " is destroyed during the End Phase by Blue-Eyes Spirit Dragon.");
                TriggerBlueEyesUltimateSpiritDestroyed(player, card);
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            CleanCard extraMonster = State.ExtraMonsters[extraZone];
            if (extraMonster == null || !extraMonster.DestroyDuringEndPhase)
            {
                return;
            }

            State.ExtraMonsters[extraZone] = null;
            string extraName = extraMonster.Definition.Name;
            ResetCardForHiddenZone(extraMonster);
            AddToGraveyard(player, extraMonster, true);
            TriggerJetSummonAfterFieldCardDestroyed(extraMonster);
            Log(extraName + " is destroyed during the End Phase by Blue-Eyes Spirit Dragon.");
            TriggerBlueEyesUltimateSpiritDestroyed(player, extraMonster);
        }

        private bool ResolveWhiteStoneEndPhaseTrigger(CleanPlayerState player)
        {
            if (player.UsedOncePerTurn.Contains(WhiteStoneEndPhaseOnceKey) ||
                pendingTriggerPromptKeys.Contains(TriggerPromptKey(player, WhiteStoneEndPhaseOnceKey)) ||
                FirstEmpty(player.Monsters) < 0)
            {
                return false;
            }

            CleanCard stone = player.Graveyard.Find(card =>
                card.Definition.Id == WhiteStoneOfAncientsCardId && card.SentToGraveThisTurn);
            int targetIndex = BestCardIndex(
                player.Deck,
                card => IsBlueEyesMonster(card) &&
                        CanSpecialSummonCard(player, card, CleanSummonOrigin.Deck, CleanSummonMethod.Effect));
            if (stone == null || targetIndex < 0)
            {
                return false;
            }

            CleanCard target = player.Deck[targetIndex];
            if (!TryReserveTriggerPrompt(player, WhiteStoneEndPhaseOnceKey))
            {
                return false;
            }

            QueueTriggerEffectPrompt(
                player,
                "The White Stone of Ancients",
                "Activate the End Phase Trigger Effect to Special Summon " + target.Definition.Name + " from the Deck?",
                true,
                () =>
                {
                    ReleaseTriggerPrompt(player, WhiteStoneEndPhaseOnceKey);
                    ResolveWhiteStoneEndPhaseSummon(player, stone, target);
                },
                () =>
                {
                    ReleaseTriggerPrompt(player, WhiteStoneEndPhaseOnceKey);
                    Log("The White Stone of Ancients End Phase Trigger Effect was not activated.");
                });
            return true;
        }

        private void ResolveWhiteStoneEndPhaseSummon(CleanPlayerState player, CleanCard stone, CleanCard target)
        {
            if (!player.Graveyard.Contains(stone) ||
                !stone.SentToGraveThisTurn ||
                !player.Deck.Contains(target) ||
                !IsBlueEyesMonster(target) ||
                !CanSpecialSummonCard(player, target, CleanSummonOrigin.Deck, CleanSummonMethod.Effect) ||
                FirstEmpty(player.Monsters) < 0)
            {
                Log("The White Stone of Ancients could not resolve its End Phase Trigger Effect.");
                return;
            }

            player.Deck.Remove(target);
            player.UsedOncePerTurn.Add(WhiteStoneEndPhaseOnceKey);
            SummonToZone(player, target, FirstEmpty(player.Monsters), false, CleanMonsterStance.Attack);
            Log("The White Stone of Ancients Special Summons " + target.Definition.Name + " from the Deck during the End Phase.");
        }

        private void ClearSentToGraveFlags(CleanPlayerState player)
        {
            ClearSentToGraveFlags(player.Deck);
            ClearSentToGraveFlags(player.ExtraDeck);
            ClearSentToGraveFlags(player.Hand);
            ClearSentToGraveFlags(player.Graveyard);
            ClearSentToGraveFlags(player.Banished);
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null)
                {
                    player.Monsters[i].SentToGraveThisTurn = false;
                }

                if (player.SpellsTraps[i] != null)
                {
                    player.SpellsTraps[i].SentToGraveThisTurn = false;
                }
            }

            if (player.FieldSpell != null)
            {
                player.FieldSpell.SentToGraveThisTurn = false;
            }
        }

        private void ClearSentToGraveFlags(List<CleanCard> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i].SentToGraveThisTurn = false;
            }
        }
        private void RunRivalTurn()
        {
            if (State.GameOver)
            {
                return;
            }

            EnterStandbyPhase();
            if (!localQuickWindowHandledForRivalTurn && OpenLocalQuickEffectWindowForRivalTurn())
            {
                localQuickWindowHandledForRivalTurn = true;
                NotifyChanged();
                return;
            }

            localQuickWindowHandledForRivalTurn = true;
            ContinueRivalTurnAfterQuickWindow();
        }

        private void ContinueRivalTurnAfterQuickWindow()
        {
            if (State.GameOver || State.TurnPlayer != CleanPlayerId.Rival)
            {
                return;
            }

            State.Phase = CleanDuelPhase.Main;
            if (pendingRivalActivatedCard != null)
            {
                ResolvePendingRivalEffect();
                if (!ResolveRivalAiChoicesOrPause())
                {
                    return;
                }
            }

            RivalPlayMain();
            if (State.GameOver || State.PendingChoice != null || activeUiPrompt != null)
            {
                return;
            }

            State.Phase = CleanDuelPhase.Battle;
            RivalBattle();
            if (State.GameOver || State.PendingChoice != null)
            {
                return;
            }

            EndTurn();
        }

        private void RivalPlayMain()
        {
            const int maximumActions = 20;
            for (int actionNumber = 0; actionNumber < maximumActions; actionNumber++)
            {
                if (State.GameOver || State.TurnPlayer != CleanPlayerId.Rival)
                {
                    return;
                }

                if (!ResolveRivalAiChoicesOrPause())
                {
                    return;
                }

                List<CleanDuelAiAction> legalActions = BuildRivalMainActions();
                CleanDuelAiAction action = rivalAiPlanner.SelectMainAction(
                    legalActions,
                    RivalAiDifficulty,
                    rivalAiRandom ?? (rivalAiRandom = CreateRivalAiRandom(State.RandomSeed)),
                    RivalRankedAiProfile);
                if (action == null)
                {
                    break;
                }

                Log("Opponent AI (" + RivalAiModeLabel + ") chooses " + action.Label + ".");
                ExecuteRivalMainAction(action);
                if (pendingRivalActivatedCard != null || State.PendingChoice != null || activeUiPrompt != null)
                {
                    if (pendingRivalActivatedCard == null && activeUiPrompt != null)
                    {
                        pendingRivalTurnContinuationAfterResponse = true;
                    }
                    return;
                }
            }
        }

        private void RivalNormalSummon()
        {
            CleanPlayerState rival = State.Rival;
            if (rival.NormalSummonUsed && !HasRivalMausoleumExtraNormalSummon())
            {
                return;
            }

            int bestMonster = BestMonsterInHand(rival);
            if (bestMonster < 0)
            {
                return;
            }

            CleanCard card = rival.Hand[bestMonster];
            int tributeCount = RequiredTributesForNormalPlay(card);
            if (!TributeForNormalPlay(rival, card))
            {
                return;
            }

            rival.Hand.RemoveAt(bestMonster);
            SummonToZone(rival, card, FirstEmpty(rival.Monsters), false, CleanMonsterStance.Attack, true);
            if (rival.NormalSummonUsed)
            {
                rival.UsedOncePerTurn.Add(MausoleumExtraNormalOnceKey);
            }
            else
            {
                rival.NormalSummonUsed = true;
            }
            Log((tributeCount > 0 ? "Rival tribute summons " : "Rival summons ") + card.Definition.Name + ".");
        }

        private List<CleanDuelAiAction> BuildRivalMainActions()
        {
            List<CleanDuelAiAction> actions = new List<CleanDuelAiAction>();
            CleanPlayerState rival = State.Rival;
            CleanPlayerState opponent = State.Local;

            for (int i = 0; i < rival.Hand.Count; i++)
            {
                CleanCard card = rival.Hand[i];
                if (card.Definition.Kind != CleanCardKind.Trap &&
                    CanActivateCleanCard(card, rival, CleanActivationSource.Hand))
                {
                    actions.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.ActivateHand,
                        i,
                        ScoreRivalActivation(card, opponent, CleanActivationSource.Hand),
                        "activate " + card.Definition.Name));
                }

                bool normalAvailable = !rival.NormalSummonUsed || CanUseMausoleumExtraNormalSummon(rival, card);
                if (normalAvailable && CanNormalPlayMonster(rival, card))
                {
                    int tributePenalty = RequiredTributesForNormalPlay(card) * 650;
                    actions.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.NormalSummon,
                        i,
                        1200 + card.CurrentAttack - tributePenalty,
                        "Normal Summon " + card.Definition.Name));
                    if (card.CurrentDefense > card.CurrentAttack + 400)
                    {
                        actions.Add(new CleanDuelAiAction(
                            CleanDuelAiActionKind.SetMonster,
                            i,
                            650 + card.CurrentDefense - tributePenalty,
                            "Set a monster"));
                    }
                }

                bool strategicallySettable = card.Definition.Kind == CleanCardKind.Trap ||
                                              IsQuickPlaySpell(card) ||
                                              IsFieldSpellCard(card) ||
                                              RivalAiDifficulty == CleanDuelAiDifficulty.Beginner;
                if (card.Definition.Kind != CleanCardKind.Monster &&
                    strategicallySettable &&
                    (IsFieldSpellCard(card) || FirstEmpty(rival.SpellsTraps) >= 0))
                {
                    int setScore = card.Definition.Kind == CleanCardKind.Trap ? 1050 : 420;
                    if (card.Definition.Id == TrueLightCardId)
                    {
                        setScore += 700;
                    }
                    actions.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.SetSpellTrap,
                        i,
                        setScore,
                        "Set " + card.Definition.Name));
                }
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = rival.Monsters[i];
                if (monster != null &&
                    monster.FaceDown &&
                    !monster.SummonedThisTurn &&
                    !monster.PositionChangedThisTurn)
                {
                    actions.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.FlipSummon,
                        i,
                        900 + monster.CurrentAttack,
                        "Flip Summon a monster"));
                }
                if (monster != null &&
                    !rivalAiAttemptedEffectsThisTurn.Contains(monster.RuntimeId) &&
                    CanActivateCleanCard(monster, rival, CleanActivationSource.MonsterZone))
                {
                    actions.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.ActivateMonster,
                        i,
                        ScoreRivalActivation(monster, opponent, CleanActivationSource.MonsterZone),
                        "activate " + monster.Definition.Name));
                }

                CleanCard spellTrap = rival.SpellsTraps[i];
                if (CanRivalActivateSetCard(i) &&
                    !rivalAiAttemptedEffectsThisTurn.Contains(spellTrap.RuntimeId))
                {
                    actions.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.ActivateSetCard,
                        i,
                        ScoreRivalActivation(spellTrap, opponent, CleanActivationSource.SpellTrapZone),
                        "activate " + spellTrap.Definition.Name));
                }
            }

            int rivalExtraZone = ExtraMonsterZoneIndexFor(rival);
            CleanCard extraMonster = State.ExtraMonsters[rivalExtraZone];
            if (extraMonster != null &&
                !rivalAiAttemptedEffectsThisTurn.Contains(extraMonster.RuntimeId) &&
                CanActivateCleanCard(extraMonster, rival, CleanActivationSource.MonsterZone))
            {
                actions.Add(new CleanDuelAiAction(
                    CleanDuelAiActionKind.ActivateExtraMonster,
                    rivalExtraZone,
                    ScoreRivalActivation(extraMonster, opponent, CleanActivationSource.MonsterZone),
                    "activate " + extraMonster.Definition.Name));
            }

            if (CanRivalActivateSetCard(FieldSpellZoneIndex) &&
                !rivalAiAttemptedEffectsThisTurn.Contains(rival.FieldSpell.RuntimeId))
            {
                actions.Add(new CleanDuelAiAction(
                    CleanDuelAiActionKind.ActivateSetCard,
                    FieldSpellZoneIndex,
                    ScoreRivalActivation(rival.FieldSpell, opponent, CleanActivationSource.SpellTrapZone),
                    "activate " + rival.FieldSpell.Definition.Name));
            }

            AddRivalExtraDeckActions(actions);
            return actions;
        }

        private void RunRivalOpenStateResponses()
        {
            if (State.GameOver ||
                State.TurnPlayer != CleanPlayerId.Local ||
                !IsMainPhase(State.Phase) ||
                State.PendingChoice != null)
            {
                return;
            }

            const int maximumResponses = 8;
            for (int responseNumber = 0; responseNumber < maximumResponses; responseNumber++)
            {
                List<CleanDuelAiAction> responses = BuildRivalOpenStateResponseActions();
                CleanDuelAiAction response = rivalAiPlanner.SelectMainAction(
                    responses,
                    RivalAiDifficulty,
                    rivalAiRandom ?? (rivalAiRandom = CreateRivalAiRandom(State.RandomSeed)),
                    RivalRankedAiProfile);
                if (response == null)
                {
                    return;
                }

                Log("Opponent AI (" + RivalAiModeLabel + ") responds with " + response.Label + ".");
                ExecuteRivalMainAction(response);
                if (!ResolveRivalAiChoicesOrPause() || State.PendingChoice != null)
                {
                    return;
                }
            }
        }

        private List<CleanDuelAiAction> BuildRivalOpenStateResponseActions()
        {
            List<CleanDuelAiAction> responses = new List<CleanDuelAiAction>();
            for (int i = 0; i < State.Rival.Hand.Count; i++)
            {
                CleanCard card = State.Rival.Hand[i];
                if (GetEffectTiming(card) == CleanEffectTiming.Quick &&
                    CanActivateCleanCard(card, State.Rival, CleanActivationSource.Hand))
                {
                    responses.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.ActivateHand,
                        i,
                        ScoreRivalActivation(card, State.Local, CleanActivationSource.Hand),
                        card.Definition.Name));
                }
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard setCard = State.Rival.SpellsTraps[i];
                if (setCard != null &&
                    (setCard.Definition.Kind == CleanCardKind.Trap || IsQuickPlaySpell(setCard)) &&
                    !rivalAiAttemptedEffectsThisTurn.Contains(setCard.RuntimeId) &&
                    CanRivalActivateSetCard(i))
                {
                    responses.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.ActivateSetCard,
                        i,
                        ScoreRivalActivation(setCard, State.Local, CleanActivationSource.SpellTrapZone),
                        setCard.Definition.Name));
                }

                CleanCard monster = State.Rival.Monsters[i];
                if (monster != null &&
                    GetEffectTiming(monster) == CleanEffectTiming.Quick &&
                    !rivalAiAttemptedEffectsThisTurn.Contains(monster.RuntimeId) &&
                    CanActivateCleanCard(monster, State.Rival, CleanActivationSource.MonsterZone))
                {
                    responses.Add(new CleanDuelAiAction(
                        CleanDuelAiActionKind.ActivateMonster,
                        i,
                        ScoreRivalActivation(monster, State.Local, CleanActivationSource.MonsterZone),
                        monster.Definition.Name));
                }
            }

            int extraZone = ExtraMonsterZoneIndexFor(State.Rival);
            CleanCard extraMonster = State.ExtraMonsters[extraZone];
            if (extraMonster != null &&
                GetEffectTiming(extraMonster) == CleanEffectTiming.Quick &&
                !rivalAiAttemptedEffectsThisTurn.Contains(extraMonster.RuntimeId) &&
                CanActivateCleanCard(extraMonster, State.Rival, CleanActivationSource.MonsterZone))
            {
                responses.Add(new CleanDuelAiAction(
                    CleanDuelAiActionKind.ActivateExtraMonster,
                    extraZone,
                    ScoreRivalActivation(extraMonster, State.Local, CleanActivationSource.MonsterZone),
                    extraMonster.Definition.Name));
            }

            return responses;
        }

        private void AddRivalExtraDeckActions(List<CleanDuelAiAction> actions)
        {
            CleanPlayerState rival = State.Rival;
            for (int i = 0; i < rival.ExtraDeck.Count; i++)
            {
                CleanCard card = rival.ExtraDeck[i];
                List<int> materials;
                bool toExtraZone;
                int targetMainZone;
                CleanDuelAiActionKind kind;
                if (TryPlanSynchroSummon(rival, i, out materials, out toExtraZone, out targetMainZone))
                {
                    kind = CleanDuelAiActionKind.SynchroSummon;
                }
                else if (TryPlanXyzSummon(rival, i, out materials, out toExtraZone, out targetMainZone))
                {
                    kind = CleanDuelAiActionKind.XyzSummon;
                }
                else if (TryPlanLinkSummon(rival, i, out materials, out toExtraZone, out targetMainZone))
                {
                    kind = CleanDuelAiActionKind.LinkSummon;
                }
                else
                {
                    continue;
                }

                int materialValue = 0;
                for (int m = 0; m < materials.Count; m++)
                {
                    CleanCard material = MaterialCardAtZone(rival, materials[m]);
                    materialValue += material != null ? Mathf.Max(material.CurrentAttack, material.CurrentDefense) : 0;
                }

                int score = 1750 + card.CurrentAttack - materialValue / 3;
                actions.Add(new CleanDuelAiAction(kind, i, score, kind + " " + card.Definition.Name));
            }
        }

        private int ScoreRivalActivation(CleanCard card, CleanPlayerState opponent, CleanActivationSource source)
        {
            int score = 1450;
            switch (card.Definition.EffectKey)
            {
                case "raigeki_clean":
                    return 5000 + MonsterCount(opponent) * 900;
                case "monster_reborn_clean":
                    return 4300;
                case "polymerization_clean":
                case "ultimate_fusion_clean":
                case "chaos_form_clean":
                    return 4000;
                case "alternative_clean":
                    return source == CleanActivationSource.Hand ? 3900 : 3600;
                case "dictator_clean":
                case "sage_clean":
                case "dragon_shrine_clean":
                case "bingo_clean":
                case "wishes_clean":
                    return 3100;
                case "true_light_clean":
                    return 2800;
                case "mausoleum_clean":
                case "gateway_chaos_clean":
                    return 2200;
                default:
                    return score + (card.Definition.Kind == CleanCardKind.Monster ? card.CurrentAttack / 3 : 0);
            }
        }

        private bool HasRivalMausoleumExtraNormalSummon()
        {
            for (int i = 0; i < State.Rival.Hand.Count; i++)
            {
                if (CanUseMausoleumExtraNormalSummon(State.Rival, State.Rival.Hand[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private void ExecuteRivalMainAction(CleanDuelAiAction action)
        {
            switch (action.Kind)
            {
                case CleanDuelAiActionKind.ActivateHand:
                    RivalActivateHandCard(action.Index);
                    break;
                case CleanDuelAiActionKind.ActivateMonster:
                    RivalActivateMonster(action.Index, false);
                    break;
                case CleanDuelAiActionKind.ActivateExtraMonster:
                    RivalActivateMonster(action.Index, true);
                    break;
                case CleanDuelAiActionKind.ActivateSetCard:
                    RivalActivateSetCard(action.Index);
                    break;
                case CleanDuelAiActionKind.NormalSummon:
                    RivalNormalPlayAt(action.Index, false);
                    break;
                case CleanDuelAiActionKind.SetMonster:
                    RivalNormalPlayAt(action.Index, true);
                    break;
                case CleanDuelAiActionKind.FlipSummon:
                    RivalFlipSummon(action.Index);
                    break;
                case CleanDuelAiActionKind.SetSpellTrap:
                    RivalSetSpellTrap(action.Index);
                    break;
                case CleanDuelAiActionKind.SynchroSummon:
                case CleanDuelAiActionKind.XyzSummon:
                case CleanDuelAiActionKind.LinkSummon:
                    RivalExtraDeckSummon(action.Index, action.Kind);
                    break;
            }
        }

        private void RivalActivateHandCard(int handIndex)
        {
            CleanCard card = CardInHand(State.Rival, handIndex);
            if (card == null ||
                card.Definition.Kind == CleanCardKind.Trap ||
                !CanActivateCleanCard(card, State.Rival, CleanActivationSource.Hand))
            {
                return;
            }

            State.Rival.Hand.RemoveAt(handIndex);
            pendingRivalActivatedCard = card;
            BeginResponseContext(
                State.Rival,
                card,
                CleanResponseKind.FieldEffect | (EffectMovesCardFromDeck(card) ? CleanResponseKind.DeckEffect : CleanResponseKind.None),
                false);

            if (OpenLocalQuickEffectWindowForRivalTurn())
            {
                return;
            }

            ResolvePendingRivalEffect();
        }

        private void RivalActivateMonster(int zoneIndex, bool extraMonsterZone)
        {
            CleanCard card = extraMonsterZone
                ? zoneIndex >= 0 && zoneIndex < State.ExtraMonsters.Length ? State.ExtraMonsters[zoneIndex] : null
                : zoneIndex >= 0 && zoneIndex < CleanPlayerState.ZoneCount ? State.Rival.Monsters[zoneIndex] : null;
            if (card == null || card.Controller != CleanPlayerId.Rival ||
                !CanActivateCleanCard(card, State.Rival, CleanActivationSource.MonsterZone))
            {
                return;
            }

            rivalAiAttemptedEffectsThisTurn.Add(card.RuntimeId);
            bool sendsSelf = ShouldSendSelfForActivatedMonsterEffect(card, State.Rival, State.Local);
            if (sendsSelf)
            {
                if (extraMonsterZone)
                {
                    State.ExtraMonsters[zoneIndex] = null;
                }
                else
                {
                    State.Rival.Monsters[zoneIndex] = null;
                }
                card.FaceDown = false;
                card.AttackBonus = 0;
                card.DefenseBonus = 0;
                card.HasBattled = false;
            }

            if (ResolveEffect(State.Rival, State.Local, card, CleanActivationSource.MonsterZone) && sendsSelf)
            {
                SendFieldCardToGraveOrFaceUpExtra(State.Rival, card, true);
            }
        }

        private bool CanRivalActivateSetCard(int zoneIndex)
        {
            if (State.GameOver ||
                State.PendingChoice != null ||
                !IsMainPhase(State.Phase) ||
                zoneIndex < 0 ||
                zoneIndex > FieldSpellZoneIndex)
            {
                return false;
            }

            CleanCard card = zoneIndex == FieldSpellZoneIndex
                ? State.Rival.FieldSpell
                : State.Rival.SpellsTraps[zoneIndex];
            if (card == null)
            {
                return false;
            }

            bool spellSpeedTwo = IsQuickPlaySpell(card) || card.Definition.Kind == CleanCardKind.Trap;
            if (card.FaceDown && card.SetThisTurn && spellSpeedTwo)
            {
                return false;
            }

            if (card.Definition.Id == TrueLightCardId)
            {
                return card.FaceDown || CanOpenTrueLightChoice(State.Rival);
            }

            return CanActivateCleanCard(card, State.Rival, CleanActivationSource.SpellTrapZone);
        }

        private void RivalActivateSetCard(int zoneIndex)
        {
            if (!CanRivalActivateSetCard(zoneIndex))
            {
                return;
            }

            CleanCard card = zoneIndex == FieldSpellZoneIndex
                ? State.Rival.FieldSpell
                : State.Rival.SpellsTraps[zoneIndex];
            rivalAiAttemptedEffectsThisTurn.Add(card.RuntimeId);

            if (zoneIndex == FieldSpellZoneIndex)
            {
                card.SetThisTurn = false;
                card.FaceDown = false;
                ResolveEffect(State.Rival, State.Local, card, CleanActivationSource.SpellTrapZone);
                return;
            }

            if (card.Definition.Id == TrueLightCardId)
            {
                bool wasFaceDown = card.FaceDown;
                card.SetThisTurn = false;
                card.FaceDown = false;
                if (CanOpenTrueLightChoice(State.Rival))
                {
                    OpenTrueLightChoice(State.Rival);
                }
                else if (wasFaceDown)
                {
                    Log("Rival activates True Light without using its once-per-turn effect.");
                }
                return;
            }

            if (card.Definition.Id == MausoleumOfWhiteCardId)
            {
                bool wasFaceDown = card.FaceDown;
                card.SetThisTurn = false;
                card.FaceDown = false;
                if (wasFaceDown)
                {
                    State.Rival.SpellsTraps[zoneIndex] = null;
                    PlaceFieldSpell(State.Rival, card);
                    Log("Rival activates Mausoleum of White in the Field Spell Zone.");
                }
                else
                {
                    ResolveEffect(State.Rival, State.Local, card, CleanActivationSource.SpellTrapZone);
                }
                return;
            }

            State.Rival.SpellsTraps[zoneIndex] = null;
            card.SetThisTurn = false;
            card.FaceDown = false;
            if (ResolveEffect(State.Rival, State.Local, card, CleanActivationSource.SpellTrapZone))
            {
                SendFieldCardToGraveOrFaceUpExtra(State.Rival, card, true);
            }
        }

        private void RivalNormalPlayAt(int handIndex, bool faceDown)
        {
            CleanCard card = CardInHand(State.Rival, handIndex);
            if (card == null ||
                !CanNormalPlayMonster(State.Rival, card) ||
                (State.Rival.NormalSummonUsed && !CanUseMausoleumExtraNormalSummon(State.Rival, card)))
            {
                return;
            }

            bool usesExtraNormal = State.Rival.NormalSummonUsed;
            int tributeCount = RequiredTributesForNormalPlay(card);
            if (!TributeForNormalPlay(State.Rival, card))
            {
                return;
            }

            State.Rival.Hand.RemoveAt(handIndex);
            SummonToZone(
                State.Rival,
                card,
                FirstEmpty(State.Rival.Monsters),
                faceDown,
                faceDown ? CleanMonsterStance.Defense : CleanMonsterStance.Attack,
                true);
            if (usesExtraNormal)
            {
                State.Rival.UsedOncePerTurn.Add(MausoleumExtraNormalOnceKey);
            }
            else
            {
                State.Rival.NormalSummonUsed = true;
            }

            Log(faceDown
                ? (tributeCount > 0 ? "Rival Tribute Sets a monster." : "Rival Sets a monster.")
                : (tributeCount > 0 ? "Rival Tribute Summons " : "Rival Normal Summons ") + card.Definition.Name + ".");
        }

        private void RivalFlipSummon(int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return;
            }

            CleanCard card = State.Rival.Monsters[zoneIndex];
            if (card == null || !card.FaceDown || card.SummonedThisTurn || card.PositionChangedThisTurn)
            {
                return;
            }

            card.FaceDown = false;
            card.Stance = CleanMonsterStance.Attack;
            card.SummonedThisTurn = true;
            card.PositionChangedThisTurn = true;
            Log("Rival Flip Summons " + card.Definition.Name + ".");
            ResolveFlipTrigger(State.Rival, State.Local, card, "Flip Summon");
        }

        private void RivalSetSpellTrap(int handIndex)
        {
            CleanCard card = CardInHand(State.Rival, handIndex);
            if (card == null || card.Definition.Kind == CleanCardKind.Monster)
            {
                return;
            }

            State.Rival.Hand.RemoveAt(handIndex);
            if (IsFieldSpellCard(card))
            {
                PlaceFieldSpell(State.Rival, card, true);
                card.SetThisTurn = true;
                Log("Rival Sets a Field Spell.");
                return;
            }

            int zone = FirstEmpty(State.Rival.SpellsTraps);
            if (zone < 0)
            {
                State.Rival.Hand.Add(card);
                return;
            }

            State.Rival.SpellsTraps[zone] = card;
            card.Controller = CleanPlayerId.Rival;
            card.FaceDown = true;
            card.SetThisTurn = true;
            Log("Rival Sets a " + card.Definition.Kind + " card.");
        }

        private void RivalExtraDeckSummon(int extraDeckIndex, CleanDuelAiActionKind kind)
        {
            if (extraDeckIndex < 0 || extraDeckIndex >= State.Rival.ExtraDeck.Count)
            {
                return;
            }

            List<int> materials;
            bool toExtraZone;
            int targetMainZone;
            bool valid = kind == CleanDuelAiActionKind.SynchroSummon
                ? TryPlanSynchroSummon(State.Rival, extraDeckIndex, out materials, out toExtraZone, out targetMainZone)
                : kind == CleanDuelAiActionKind.XyzSummon
                    ? TryPlanXyzSummon(State.Rival, extraDeckIndex, out materials, out toExtraZone, out targetMainZone)
                    : TryPlanLinkSummon(State.Rival, extraDeckIndex, out materials, out toExtraZone, out targetMainZone);
            if (!valid)
            {
                return;
            }

            CleanCard summoned = State.Rival.ExtraDeck[extraDeckIndex];
            State.Rival.ExtraDeck.RemoveAt(extraDeckIndex);
            CleanSummonPresentationKind presentation;
            if (kind == CleanDuelAiActionKind.SynchroSummon)
            {
                MoveSynchroMaterialsToGrave(State.Rival, materials);
                summoned.SynchroSummoned = true;
                presentation = CleanSummonPresentationKind.Synchro;
            }
            else if (kind == CleanDuelAiActionKind.XyzSummon)
            {
                MoveXyzMaterialsUnderMonster(State.Rival, materials, summoned);
                summoned.XyzSummoned = true;
                presentation = CleanSummonPresentationKind.Xyz;
            }
            else
            {
                MoveLinkMaterialsToGrave(State.Rival, materials);
                summoned.LinkSummoned = true;
                presentation = CleanSummonPresentationKind.Link;
            }

            PrepareExtraDeckMonsterSummon(State.Rival, summoned);
            summoned.ProperlySummoned = true;
            if (toExtraZone)
            {
                State.ExtraMonsters[ExtraMonsterZoneIndexFor(State.Rival)] = summoned;
                NotifySpecialSummon(State.Rival);
                RequestSummonCinematics(State.Rival, summoned, presentation);
            }
            else
            {
                SummonToZone(State.Rival, summoned, targetMainZone, false, CleanMonsterStance.Attack, false, presentation);
            }

            if (kind == CleanDuelAiActionKind.LinkSummon)
            {
                ResolveLinkSummonTrigger(State.Rival, summoned);
            }
            Log("Rival " + presentation + " Summons " + summoned.Definition.Name + " using " + materials.Count + " material(s).");
        }

        private bool ResolveRivalAiChoicesOrPause()
        {
            const int maximumChoices = 24;
            for (int i = 0; i < maximumChoices; i++)
            {
                if (State.GameOver)
                {
                    return false;
                }

                if (activeUiPrompt != null)
                {
                    pendingRivalTurnContinuationAfterResponse = true;
                    return false;
                }

                CleanChoicePrompt prompt = State.PendingChoice;
                if (prompt == null)
                {
                    return true;
                }

                if (IsLocalOnlyRivalTurnPrompt(prompt.SourceCardId))
                {
                    pendingRivalTurnContinuationAfterResponse = true;
                    return false;
                }

                int optionIndex = rivalAiPlanner.SelectChoiceIndex(
                    prompt,
                    RivalAiDifficulty,
                    rivalAiRandom ?? (rivalAiRandom = CreateRivalAiRandom(State.RandomSeed)),
                    RivalRankedAiProfile);
                if (optionIndex < 0)
                {
                    Log("Opponent AI has no legal option for " + prompt.Title + ".");
                    State.PendingChoice = null;
                    continue;
                }

                ChoosePendingOptionForPlayer(State.Rival, optionIndex, false);
            }

            Log("Opponent AI stopped a repeated choice sequence safely.");
            State.PendingChoice = null;
            return true;
        }

        private bool IsLocalOnlyRivalTurnPrompt(int sourceCardId)
        {
            return sourceCardId == QuickEffectChoicePromptId ||
                   sourceCardId == DictatorAttackTargetChoicePromptId ||
                   sourceCardId == JetBounceChoicePromptId ||
                   sourceCardId == BattlePhaseChoicePromptId ||
                   sourceCardId == SurrenderPromptId ||
                   sourceCardId == ExtraDeckSummonChoicePromptId ||
                   sourceCardId == FusionSummonChoicePromptId ||
                   sourceCardId == RitualSummonChoicePromptId ||
                   sourceCardId == PendulumSummonChoicePromptId;
        }

        private void BeginResponseContext(CleanPlayerState controller, CleanCard card, CleanResponseKind kind, bool sourceWasFaceUp)
        {
            pendingResponseController = controller;
            pendingResponseCard = card;
            pendingResponseKind = kind;
            pendingResponseSourceWasFaceUp = sourceWasFaceUp;
            pendingResponseNegated = false;
            pendingResponseDestroyed = false;
        }

        private void ClearResponseContext()
        {
            pendingResponseController = null;
            pendingResponseCard = null;
            pendingResponseKind = CleanResponseKind.None;
            pendingResponseSourceWasFaceUp = false;
            pendingResponseNegated = false;
            pendingResponseDestroyed = false;
        }

        private bool EffectMovesCardFromDeck(CleanCard card)
        {
            string text = card == null ? string.Empty : card.Definition.Text ?? string.Empty;
            if (text.IndexOf("Deck", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            return text.IndexOf("add", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("Special Summon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("send", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ResolvePendingRivalEffect()
        {
            CleanCard card = pendingRivalActivatedCard;
            CleanPlayerState controller = pendingResponseController ?? State.Rival;
            pendingRivalActivatedCard = null;
            if (card == null)
            {
                ClearResponseContext();
                return;
            }

            bool sendToGrave = true;
            if (pendingResponseNegated)
            {
                Log(card.Definition.Name + "'s activation/effect is negated.");
            }
            else
            {
                sendToGrave = ResolveEffect(controller, OpponentOf(controller.Id), card, CleanActivationSource.Hand);
            }

            if ((sendToGrave || pendingResponseNegated || pendingResponseDestroyed) &&
                !controller.Graveyard.Contains(card) &&
                !controller.Banished.Contains(card) &&
                !CardIsOnField(controller, card))
            {
                AddToGraveyard(controller, card);
            }

            ClearResponseContext();
        }

        private bool CardIsOnField(CleanPlayerState player, CleanCard card)
        {
            if (player == null || card == null)
            {
                return false;
            }

            if (object.ReferenceEquals(player.FieldSpell, card))
            {
                return true;
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (object.ReferenceEquals(player.Monsters[i], card) ||
                    object.ReferenceEquals(player.SpellsTraps[i], card))
                {
                    return true;
                }
            }

            for (int i = 0; i < State.ExtraMonsters.Length; i++)
            {
                if (object.ReferenceEquals(State.ExtraMonsters[i], card))
                {
                    return true;
                }
            }

            return false;
        }

        private void RivalBattle()
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard attacker = State.Rival.Monsters[i];
                if (attacker == null ||
                    attacker.FaceDown ||
                    attacker.HasBattled ||
                    attacker.CannotAttack ||
                    attacker.Stance != CleanMonsterStance.Attack)
                {
                    continue;
                }

                int target = SelectRivalBattleTarget(attacker);
                if (target >= 0)
                {
                    if (ShouldLocalChooseRivalAttackTarget())
                    {
                        OpenDictatorAttackTargetChoice(i);
                        return;
                    }

                    ResolveBattle(State.Rival, State.Local, i, target);
                }
                else if (MonsterCount(State.Local) > 0)
                {
                    Log("Opponent AI keeps " + attacker.Definition.Name + " from making an unfavorable attack.");
                    continue;
                }
                else
                {
                    State.Local.LifePoints -= attacker.CurrentAttack;
                    attacker.HasBattled = true;
                    Log("Rival attacks directly for " + attacker.CurrentAttack + ".");
                    RequestSignatureFinalBlowIfLethal(State.Rival, attacker, State.Local);
                }

                CheckWin();
                if (State.GameOver)
                {
                    return;
                }
            }
        }

        private int SelectRivalBattleTarget(CleanCard attacker)
        {
            if (RivalAiFollowsRank) return SelectRankedRivalBattleTarget(attacker);

            List<int> targets = new List<int>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (State.Local.Monsters[i] != null)
                {
                    targets.Add(i);
                }
            }

            if (targets.Count == 0)
            {
                return -1;
            }

            if (RivalAiDifficulty == CleanDuelAiDifficulty.Beginner)
            {
                return targets[rivalAiRandom.Next(targets.Count)];
            }

            int bestTarget = -1;
            int bestScore = int.MinValue;
            for (int i = 0; i < targets.Count; i++)
            {
                int zone = targets[i];
                CleanCard defender = State.Local.Monsters[zone];
                if (defender.FaceDown)
                {
                    // The opponent observes a hidden card, not its DEF value.
                    int hiddenScore = RivalAiDifficulty == CleanDuelAiDifficulty.Expert ? 100 : 20;
                    if (hiddenScore > bestScore)
                    {
                        bestScore = hiddenScore;
                        bestTarget = zone;
                    }
                    continue;
                }

                int battlePower = defender.Stance == CleanMonsterStance.Attack
                    ? defender.CurrentAttack
                    : defender.CurrentDefense;
                int margin = attacker.CurrentAttack - battlePower;
                if (margin < 0)
                {
                    continue;
                }

                int score = RivalAiDifficulty == CleanDuelAiDifficulty.Expert
                    ? defender.CurrentAttack + Mathf.Max(0, margin) + (defender.Stance == CleanMonsterStance.Attack ? margin : 0)
                    : -battlePower;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = zone;
                }
            }

            return bestTarget;
        }

        private int SelectRankedRivalBattleTarget(CleanCard attacker)
        {
            List<CleanDuelAiAction> targets = new List<CleanDuelAiAction>();
            targets.Add(new CleanDuelAiAction(CleanDuelAiActionKind.HoldAttack, -1, 0, "hold attack"));
            for (int zone = 0; zone < CleanPlayerState.ZoneCount; zone++)
            {
                CleanCard defender = State.Local.Monsters[zone];
                if (defender == null) continue;
                int score;
                if (defender.FaceDown)
                {
                    // A face-down defender has no visible identity, ATK or DEF for this decision.
                    score = 100;
                }
                else
                {
                    int battlePower = defender.Stance == CleanMonsterStance.Attack ? defender.CurrentAttack : defender.CurrentDefense;
                    int margin = attacker.CurrentAttack - battlePower;
                    score = margin < 0 ? -1000 + margin
                        : defender.CurrentAttack + Mathf.Max(0, margin) + (defender.Stance == CleanMonsterStance.Attack ? margin : 0);
                }
                targets.Add(new CleanDuelAiAction(CleanDuelAiActionKind.AttackTarget, zone, score, "choose visible target zone " + zone));
            }
            if (targets.Count == 1) return -1;
            CleanDuelAiAction action = rivalAiPlanner.SelectMainAction(targets, RivalAiDifficulty,
                rivalAiRandom ?? (rivalAiRandom = CreateRivalAiRandom(State.RandomSeed)), RivalRankedAiProfile);
            return action != null ? action.Index : -1;
        }

        private int StrongestFaceUpMonsterZone(CleanPlayerState player)
        {
            int strongestZone = -1;
            int strongestAttack = -1;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && monster.CurrentAttack > strongestAttack)
                {
                    strongestAttack = monster.CurrentAttack;
                    strongestZone = i;
                }
            }

            return strongestZone;
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

                if (IsProtectedFromCleanTargeting(player, monster))
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

        private bool IsProtectedFromCleanTargeting(CleanPlayerState player, CleanCard monster)
        {
            return IsProtectedByTrueLightFromTargeting(player, monster) || IsChaosRitualProtected(monster);
        }

        private bool IsProtectedByTrueLightFromTargeting(CleanPlayerState player, CleanCard monster)
        {
            return monster != null &&
                   !monster.FaceDown &&
                   IsTreatedAsBlueEyesWhiteDragon(monster) &&
                   HasFaceUpTrueLight(player);
        }

        private bool IsChaosRitualProtected(CleanCard monster)
        {
            return monster != null &&
                   !monster.FaceDown &&
                   (monster.Definition.Id == BlueEyesChaosMaxDragonCardId || monster.Definition.Id == BlueEyesChaosDragonCardId);
        }

        private bool ActivateJetFromHand(CleanPlayerState player, CleanCard jet)
        {
            if (!HasBlueEyesWhiteDragonOnFieldOrGrave(player) || FirstEmpty(player.Monsters) < 0)
            {
                Log("Blue-Eyes Jet Dragon failed: Blue-Eyes White Dragon is not on field/GY or no Monster Zone is open.");
                return false;
            }

            SummonToZone(player, jet, FirstEmpty(player.Monsters), false, CleanMonsterStance.Attack);
            player.UsedOncePerTurn.Add(JetSummonOnceKey);
            Log("Blue-Eyes Jet Dragon special summons itself from hand.");
            return true;
        }

        private void TriggerJetSummonAfterFieldCardDestroyed(CleanCard destroyedCard)
        {
            TryTriggerJetSummon(State.Local, destroyedCard);
            TryTriggerJetSummon(State.Rival, destroyedCard);
        }

        private bool TryTriggerJetSummon(CleanPlayerState player, CleanCard destroyedCard)
        {
            if (player.UsedOncePerTurn.Contains(JetSummonOnceKey) ||
                pendingTriggerPromptKeys.Contains(TriggerPromptKey(player, JetSummonOnceKey)) ||
                FirstEmpty(player.Monsters) < 0 ||
                !HasBlueEyesWhiteDragonOnFieldOrGrave(player))
            {
                return false;
            }

            CleanCard jet = FindEligibleJet(player.Graveyard, destroyedCard) ?? FindEligibleJet(player.Hand, destroyedCard);
            if (jet == null || !TryReserveTriggerPrompt(player, JetSummonOnceKey))
            {
                return false;
            }

            string source = player.Graveyard.Contains(jet) ? "GY" : "hand";
            QueueTriggerEffectPrompt(
                player,
                "Blue-Eyes Jet Dragon",
                "A card on the field was destroyed. Activate the Trigger Effect to Special Summon Blue-Eyes Jet Dragon from the " + source + "?",
                true,
                () =>
                {
                    ReleaseTriggerPrompt(player, JetSummonOnceKey);
                    ResolveJetSummonTrigger(player, jet);
                },
                () =>
                {
                    ReleaseTriggerPrompt(player, JetSummonOnceKey);
                    Log("Blue-Eyes Jet Dragon's destruction Trigger Effect was not activated.");
                });
            return true;
        }

        private CleanCard FindEligibleJet(List<CleanCard> cards, CleanCard destroyedCard)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCard candidate = cards[i];
                if (candidate.Definition.Id == BlueEyesJetDragonCardId && !object.ReferenceEquals(candidate, destroyedCard))
                {
                    return candidate;
                }
            }

            return null;
        }

        private void ResolveJetSummonTrigger(CleanPlayerState player, CleanCard jet)
        {
            List<CleanCard> source = player.Graveyard.Contains(jet)
                ? player.Graveyard
                : player.Hand.Contains(jet) ? player.Hand : null;
            if (source == null ||
                FirstEmpty(player.Monsters) < 0 ||
                !HasBlueEyesWhiteDragonOnFieldOrGrave(player))
            {
                Log("Blue-Eyes Jet Dragon could not resolve its destruction Trigger Effect.");
                return;
            }

            source.Remove(jet);
            SummonToZone(player, jet, FirstEmpty(player.Monsters), false, CleanMonsterStance.Attack);
            player.UsedOncePerTurn.Add(JetSummonOnceKey);
            Log("Blue-Eyes Jet Dragon Special Summons itself after a field card was destroyed.");
        }

        private bool OpenLocalJetBattleBounceChoice(
            CleanPlayerState attackingPlayer,
            CleanPlayerState defendingPlayer,
            int attackerZone,
            int defenderZone,
            CleanCard attacker,
            CleanCard defender)
        {
            CleanCard localJet = attackingPlayer.Id == CleanPlayerId.Local ? attacker : defendingPlayer.Id == CleanPlayerId.Local ? defender : null;
            if (localJet == null ||
                localJet.FaceDown ||
                localJet.EffectsNegated ||
                localJet.Definition.Id != BlueEyesJetDragonCardId ||
                State.Local.UsedOncePerTurn.Contains(JetBounceOnceKey) ||
                !HasBlueEyesWhiteDragonOnFieldOrGrave(State.Local))
            {
                return false;
            }

            List<CleanChoiceOption> options = BuildJetBounceOptions(State.Rival);
            if (options.Count == 0)
            {
                return false;
            }

            pendingJetAttackingPlayerId = attackingPlayer.Id;
            pendingJetAttackerZone = attackerZone;
            pendingJetDefenderZone = defenderZone;
            State.PendingChoice = new CleanChoicePrompt(
                JetBounceChoicePromptId,
                "Blue-Eyes Jet Dragon",
                "Choose 1 card your opponent controls to return to the hand.",
                options,
                CleanPromptKind.TriggerEffect,
                true);
            Log("Blue-Eyes Jet Dragon awaits a bounce target at the start of the Damage Step.");
            return true;
        }

        private List<CleanChoiceOption> BuildJetBounceOptions(CleanPlayerState opponent)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = opponent.Monsters[i];
                if (monster != null && !IsProtectedFromCleanTargeting(opponent, monster))
                {
                    options.Add(new CleanChoiceOption(i, monster.PublicName, "Return this monster", true, string.Empty));
                }

                CleanCard spellTrap = opponent.SpellsTraps[i];
                if (spellTrap != null)
                {
                    options.Add(new CleanChoiceOption(100 + i, spellTrap.PublicName, "Return this Spell/Trap", true, string.Empty));
                }
            }

            if (opponent.FieldSpell != null)
            {
                options.Add(new CleanChoiceOption(200, opponent.FieldSpell.PublicName, "Return the Field Spell", true, string.Empty));
            }

            int extraZone = ExtraMonsterZoneIndexFor(opponent);
            CleanCard extraMonster = State.ExtraMonsters[extraZone];
            if (extraMonster != null && !IsProtectedFromCleanTargeting(opponent, extraMonster))
            {
                options.Add(new CleanChoiceOption(300, extraMonster.PublicName, "Return the Extra Monster", true, string.Empty));
            }

            return options;
        }

        private void ResolveJetBounceChoice(int optionId)
        {
            int attackerZone = pendingJetAttackerZone;
            int defenderZone = pendingJetDefenderZone;
            CleanPlayerId attackingPlayerId = pendingJetAttackingPlayerId;
            pendingJetAttackerZone = -1;
            pendingJetDefenderZone = -1;
            State.PendingChoice = null;

            if (!ReturnChosenCardToHand(State.Rival, optionId))
            {
                Log("Blue-Eyes Jet Dragon's selected target is no longer legal.");
                return;
            }

            State.Local.UsedOncePerTurn.Add(JetBounceOnceKey);
            Log("Blue-Eyes Jet Dragon returns the chosen opponent card to the hand.");

            CleanPlayerState attackingPlayer = Player(attackingPlayerId);
            CleanPlayerState defendingPlayer = OpponentOf(attackingPlayerId);
            ResolveBattle(attackingPlayer, defendingPlayer, attackerZone, defenderZone);
            if (attackingPlayerId != CleanPlayerId.Rival || State.GameOver || State.PendingChoice != null)
            {
                return;
            }

            RivalBattle();
            if (!State.GameOver && State.PendingChoice == null && State.TurnPlayer == CleanPlayerId.Rival)
            {
                EndTurn();
            }
        }

        private bool ReturnChosenCardToHand(CleanPlayerState opponent, int optionId)
        {
            CleanCard card;
            if (optionId == 300)
            {
                int extraZone = ExtraMonsterZoneIndexFor(opponent);
                card = State.ExtraMonsters[extraZone];
                if (card == null || IsProtectedFromCleanTargeting(opponent, card))
                {
                    return false;
                }

                State.ExtraMonsters[extraZone] = null;
                SendEquipsForLeavingMonster(opponent, card);
                ResetCardForHiddenZone(card);
                ReturnFieldCardToPrivateZone(opponent, card);
                TriggerDragonMasterMagiaLeavesField(opponent, card);
                return true;
            }

            if (optionId == 200)
            {
                card = opponent.FieldSpell;
                if (card == null)
                {
                    return false;
                }

                opponent.FieldSpell = null;
                if (card.Definition.Id == GatewayToChaosCardId)
                {
                    opponent.GatewayToChaosCounters = 0;
                }

                ResetCardForHiddenZone(card);
                opponent.Hand.Add(card);
                return true;
            }

            if (optionId >= 100)
            {
                int spellZone = optionId - 100;
                if (spellZone < 0 || spellZone >= CleanPlayerState.ZoneCount || opponent.SpellsTraps[spellZone] == null)
                {
                    return false;
                }

                card = opponent.SpellsTraps[spellZone];
                DetachEquipBonus(opponent, card);
                opponent.SpellsTraps[spellZone] = null;
                ResetCardForHiddenZone(card);
                opponent.Hand.Add(card);
                return true;
            }

            if (optionId < 0 || optionId >= CleanPlayerState.ZoneCount || opponent.Monsters[optionId] == null || IsProtectedFromCleanTargeting(opponent, opponent.Monsters[optionId]))
            {
                return false;
            }

            card = opponent.Monsters[optionId];
            TriggerMaidenTargetedByCardEffect(opponent, optionId);
            opponent.Monsters[optionId] = null;
            SendEquipsForLeavingMonster(opponent, card);
            ResetCardForHiddenZone(card);
            ReturnFieldCardToPrivateZone(opponent, card);
            TriggerDragonMasterMagiaLeavesField(opponent, card);
            return true;
        }
        private void ApplyJetBattleBounce(CleanPlayerState jetPlayer, CleanPlayerState opponent, CleanCard jet)
        {
            if (jet == null || jet.FaceDown || jet.Definition.Id != BlueEyesJetDragonCardId || jetPlayer.Id == CleanPlayerId.Local || jetPlayer.UsedOncePerTurn.Contains(JetBounceOnceKey) || !HasBlueEyesWhiteDragonOnFieldOrGrave(jetPlayer))
            {
                return;
            }

            if (BounceBestOpponentCardToHand(opponent))
            {
                jetPlayer.UsedOncePerTurn.Add(JetBounceOnceKey);
                Log("Blue-Eyes Jet Dragon returns 1 opponent card to the hand at the start of the Damage Step.");
            }
        }

        private bool BounceBestOpponentCardToHand(CleanPlayerState opponent)
        {
            int monsterZone = StrongestMonsterZone(opponent);
            if (monsterZone >= 0)
            {
                CleanCard card = opponent.Monsters[monsterZone];
                opponent.Monsters[monsterZone] = null;
                ResetCardForHiddenZone(card);
                ReturnFieldCardToPrivateZone(opponent, card);
                return true;
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = opponent.SpellsTraps[i];
                if (card == null)
                {
                    continue;
                }

                opponent.SpellsTraps[i] = null;
                ResetCardForHiddenZone(card);
                opponent.Hand.Add(card);
                return true;
            }

            return false;
        }

        private void ReturnFieldCardToPrivateZone(CleanPlayerState player, CleanCard card)
        {
            SendXyzMaterialsToGrave(player, card);
            if (IsExtraDeckMonsterDefinition(card.Definition))
            {
                player.ExtraDeck.Add(card);
            }
            else
            {
                player.Hand.Add(card);
            }
        }

        private bool IsExtraDeckMonsterDefinition(CleanCardDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            string typeLine = definition.TypeLine ?? string.Empty;
            return IsFusionMonsterDefinition(definition) ||
                   IsSynchroMonsterDefinition(definition) ||
                   typeLine.IndexOf("Xyz", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   typeLine.IndexOf("Link", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int StrongestDestructibleEffectTargetZone(CleanPlayerState player)
        {
            int strongestZone = -1;
            int strongestAttack = -1;
            bool protectedByJet = false;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster == null)
                {
                    continue;
                }

                if (IsProtectedFromCleanTargeting(player, monster))
                {
                    continue;
                }

                if (IsProtectedByJetFromCardEffect(player, monster))
                {
                    protectedByJet = true;
                    continue;
                }

                if (monster.CurrentAttack > strongestAttack)
                {
                    strongestAttack = monster.CurrentAttack;
                    strongestZone = i;
                }
            }

            if (protectedByJet)
            {
                Log("Blue-Eyes Jet Dragon protects other controlled cards from card-effect destruction.");
            }

            return strongestZone;
        }

        private int FirstDestructibleSpellTrapZone(CleanPlayerState player)
        {
            bool protectedByJet = false;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.SpellsTraps[i];
                if (card == null || card.FaceDown)
                {
                    continue;
                }

                if (IsProtectedByJetFromCardEffect(player, card))
                {
                    protectedByJet = true;
                    continue;
                }

                return i;
            }

            if (protectedByJet)
            {
                Log("Blue-Eyes Jet Dragon protects other controlled cards from card-effect destruction.");
            }

            return -1;
        }

        private bool IsProtectedFromOpponentEffectDestruction(CleanPlayerState player, CleanCard card)
        {
            return IsChaosRitualProtected(card) || IsProtectedByJetFromCardEffect(player, card);
        }
        private bool IsProtectedByJetFromCardEffect(CleanPlayerState player, CleanCard card)
        {
            return card != null && card.Definition.Id != BlueEyesJetDragonCardId && HasFaceUpJetDragon(player);
        }

        private bool HasFaceUpJetDragon(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && !monster.EffectsNegated && monster.Definition.Id == BlueEyesJetDragonCardId)
                {
                    return true;
                }
            }

            return false;
        }
        private bool CanUseMausoleumExtraNormalSummon(CleanPlayerState player, CleanCard card)
        {
            return card != null &&
                   !player.UsedOncePerTurn.Contains(MausoleumExtraNormalOnceKey) &&
                   HasFaceUpMausoleum(player) &&
                   IsMausoleumExtraNormalTuner(card);
        }

        private bool IsMausoleumExtraNormalTuner(CleanCard card)
        {
            return card != null && card.Definition.Kind == CleanCardKind.Monster && IsLevelOneLightTuner(card);
        }

        private bool HasFaceUpMausoleum(CleanPlayerState player)
        {
            if (player.FieldSpell != null && player.FieldSpell.Definition.Id == MausoleumOfWhiteCardId && !player.FieldSpell.FaceDown && !player.FieldSpell.EffectsNegated)
            {
                return true;
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.SpellsTraps[i];
                if (card != null && card.Definition.Id == MausoleumOfWhiteCardId && !card.FaceDown && !card.EffectsNegated)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasFaceUpGatewayToChaos(CleanPlayerState player)
        {
            if (player.FieldSpell != null && player.FieldSpell.Definition.Id == GatewayToChaosCardId && !player.FieldSpell.FaceDown && !player.FieldSpell.EffectsNegated)
            {
                return true;
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.SpellsTraps[i];
                if (card != null && card.Definition.Id == GatewayToChaosCardId && !card.FaceDown && !card.EffectsNegated)
                {
                    return true;
                }
            }

            return false;
        }
        private bool HasFaceUpTrueLight(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.SpellsTraps[i];
                if (card != null && card.Definition.Id == TrueLightCardId && !card.FaceDown && !card.EffectsNegated)
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
            ApplyChaosDragonAttackDeclaration(attacker, defendingPlayer);
            defender = defendingPlayer.Monsters[defenderZone];
            if (defender == null)
            {
                return;
            }

            if (!suppressAttackTargetTriggerOnce && ResolveAttackTargetTrigger(attackingPlayer, defendingPlayer, attackerZone, defenderZone, attacker, defender))
            {
                return;
            }

            if (OpenLocalJetBattleBounceChoice(attackingPlayer, defendingPlayer, attackerZone, defenderZone, attacker, defender))
            {
                return;
            }

            ApplyJetBattleBounce(attackingPlayer, defendingPlayer, attacker);
            ApplyJetBattleBounce(defendingPlayer, attackingPlayer, defender);
            attacker = attackingPlayer.Monsters[attackerZone];
            defender = defendingPlayer.Monsters[defenderZone];
            if (attacker == null || defender == null)
            {
                return;
            }

            bool defenderWasFaceDown = defender.FaceDown;
            defender.FaceDown = false;
            if (defenderWasFaceDown)
            {
                if (ResolveFlipTrigger(
                    defendingPlayer,
                    attackingPlayer,
                    defender,
                    "attacked while face-down",
                    () => ResumeBattleAfterTriggerPrompt(attackingPlayer.Id, attackerZone, defenderZone)))
                {
                    return;
                }

                attacker = attackingPlayer.Monsters[attackerZone];
                defender = defendingPlayer.Monsters[defenderZone];
                if (attacker == null || defender == null)
                {
                    return;
                }
            }

            RequestRivalryClimaxIfEligible(attackingPlayer, defendingPlayer, attacker, defender);
            int defenderPower = defender.Stance == CleanMonsterStance.Attack ? defender.CurrentAttack : defender.CurrentDefense;
            int difference = attacker.CurrentAttack - defenderPower;

            if (defender.Stance == CleanMonsterStance.Attack)
            {
                if (difference > 0)
                {
                    defendingPlayer.LifePoints -= difference;
                    CleanCard destroyed = defendingPlayer.Monsters[defenderZone];
                    MoveMonsterToGrave(defendingPlayer, defenderZone);
                    TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                    Log(attacker.Definition.Name + " destroys " + defender.Definition.Name + " for " + difference + " damage.");
                    TriggerBlueEyesUltimateSpiritDestroyed(defendingPlayer, destroyed);
                    TriggerDragonMasterMagiaLeavesField(defendingPlayer, destroyed);
                }
                else if (difference < 0)
                {
                    attackingPlayer.LifePoints += difference;
                    CleanCard destroyed = attackingPlayer.Monsters[attackerZone];
                    MoveMonsterToGrave(attackingPlayer, attackerZone);
                    TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                    Log(attacker.Definition.Name + " is destroyed. " + attackingPlayer.Name + " takes " + (-difference) + " damage.");
                    TriggerBlueEyesUltimateSpiritDestroyed(attackingPlayer, destroyed);
                    TriggerDragonMasterMagiaLeavesField(attackingPlayer, destroyed);
                }
                else
                {
                    CleanCard destroyedAttacker = attackingPlayer.Monsters[attackerZone];
                    CleanCard destroyedDefender = defendingPlayer.Monsters[defenderZone];
                    MoveMonsterToGrave(attackingPlayer, attackerZone);
                    MoveMonsterToGrave(defendingPlayer, defenderZone);
                    TriggerJetSummonAfterFieldCardDestroyed(destroyedAttacker);
                    TriggerJetSummonAfterFieldCardDestroyed(destroyedDefender);
                    Log("Both monsters are destroyed.");
                    TriggerBlueEyesUltimateSpiritDestroyed(attackingPlayer, destroyedAttacker);
                    TriggerDragonMasterMagiaLeavesField(attackingPlayer, destroyedAttacker);
                    TriggerBlueEyesUltimateSpiritDestroyed(defendingPlayer, destroyedDefender);
                    TriggerDragonMasterMagiaLeavesField(defendingPlayer, destroyedDefender);
                }
            }
            else if (difference > 0)
            {
                int piercingDamage = PiercingBattleDamage(attacker, difference);
                if (piercingDamage > 0)
                {
                    defendingPlayer.LifePoints -= piercingDamage;
                }

                CleanCard destroyed = defendingPlayer.Monsters[defenderZone];
                MoveMonsterToGrave(defendingPlayer, defenderZone);
                TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                Log(piercingDamage > 0
                    ? attacker.Definition.Name + " destroys a defense monster and deals " + piercingDamage + " piercing damage."
                    : attacker.Definition.Name + " destroys a defense monster.");
                TriggerBlueEyesUltimateSpiritDestroyed(defendingPlayer, destroyed);
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

        private bool TriggerMaidenTargetedByCardEffect(CleanPlayerState player, int zone, Action continuation = null)
        {
            if (player == null || zone < 0 || zone >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard maiden = player.Monsters[zone];
            if (maiden == null || maiden.FaceDown || maiden.EffectsNegated)
            {
                return false;
            }

            string onceKey;
            string body;
            if (maiden.Definition.Id == MaidenWithEyesOfBlueCardId &&
                !player.UsedOncePerTurn.Contains(MaidenWithEyesOnceKey) &&
                FirstEmpty(player.Monsters) >= 0 &&
                HasBlueEyesWhiteDragonInHandDeckOrGrave(player))
            {
                onceKey = MaidenWithEyesOnceKey;
                body = "This card was targeted by a card effect. Activate its Trigger Effect to change its battle position and Special Summon Blue-Eyes White Dragon?";
            }
            else if (maiden.Definition.Id == MaidenOfWhiteCardId &&
                     !player.UsedOncePerTurn.Contains(MaidenWhiteTargetOnceKey) &&
                     FirstEmpty(player.Monsters) >= 0 &&
                     HasMatchingCard(player.Graveyard, IsMaidenOfWhiteAttackReviveTarget))
            {
                onceKey = MaidenWhiteTargetOnceKey;
                body = "This card was targeted by a card effect. Activate its Trigger Effect to Special Summon from the GY?";
            }
            else
            {
                return false;
            }

            if (player.Id == CleanPlayerId.Local)
            {
                if (!TryReserveTriggerPrompt(player, onceKey))
                {
                    return false;
                }

                QueueTriggerEffectPrompt(
                    player,
                    maiden.Definition.Name,
                    body,
                    true,
                    () =>
                    {
                        ReleaseTriggerPrompt(player, onceKey);
                        ResolveMaidenTargetedEffect(player, maiden, onceKey);
                        if (continuation != null)
                        {
                            continuation();
                        }
                    },
                    () =>
                    {
                        ReleaseTriggerPrompt(player, onceKey);
                        Log(maiden.Definition.Name + "'s targeted Trigger Effect was not activated.");
                        if (continuation != null)
                        {
                            continuation();
                        }
                    });
                return true;
            }

            ResolveMaidenTargetedEffect(player, maiden, onceKey);
            return false;
        }

        private void ResolveMaidenTargetedEffect(CleanPlayerState player, CleanCard maiden, string onceKey)
        {
            int zone = IndexOfMonsterZone(player, maiden);
            if (zone < 0 || maiden.FaceDown || maiden.EffectsNegated)
            {
                Log(maiden.Definition.Name + " could not resolve its targeted Trigger Effect.");
                return;
            }

            player.UsedOncePerTurn.Add(onceKey);
            if (maiden.Definition.Id == MaidenWithEyesOfBlueCardId)
            {
                maiden.Stance = maiden.Stance == CleanMonsterStance.Attack
                    ? CleanMonsterStance.Defense
                    : CleanMonsterStance.Attack;
                bool summoned = FirstEmpty(player.Monsters) >= 0 &&
                                HasBlueEyesWhiteDragonInHandDeckOrGrave(player) &&
                                LuaSpecialSummonBlueEyesWhiteDragon(player, CleanMonsterStance.Attack);
                Log("Maiden with Eyes of Blue resolves its targeted Trigger Effect" +
                    (summoned ? " and Special Summons Blue-Eyes White Dragon." : "."));
                return;
            }

            SpecialSummonBestFromGraveyard(player, IsMaidenOfWhiteAttackReviveTarget, CleanMonsterStance.Attack, "Maiden of White");
            Log("Maiden of White resolves its targeted Trigger Effect.");
        }

        private bool ResolveAttackTargetTrigger(
            CleanPlayerState attackingPlayer,
            CleanPlayerState defendingPlayer,
            int attackerZone,
            int defenderZone,
            CleanCard attacker,
            CleanCard defender)
        {
            if (attacker == null || defender == null || defender.FaceDown)
            {
                return false;
            }

            if (defender.Definition.Id == MaidenWithEyesOfBlueCardId &&
                !defendingPlayer.UsedOncePerTurn.Contains(MaidenWithEyesOnceKey))
            {
                if (defendingPlayer.Id == CleanPlayerId.Local)
                {
                    if (!TryReserveTriggerPrompt(defendingPlayer, MaidenWithEyesOnceKey))
                    {
                        return false;
                    }

                    QueueTriggerEffectPrompt(
                        defendingPlayer,
                        defender.Definition.Name,
                        "This monster was targeted for an attack. Activate its Trigger Effect to negate the attack, change its battle position, and Special Summon Blue-Eyes White Dragon?",
                        true,
                        () =>
                        {
                            ReleaseTriggerPrompt(defendingPlayer, MaidenWithEyesOnceKey);
                            ResolveMaidenAttackTrigger(defendingPlayer, defender);
                        },
                        () =>
                        {
                            ReleaseTriggerPrompt(defendingPlayer, MaidenWithEyesOnceKey);
                            Log("Maiden with Eyes of Blue's attack Trigger Effect was not activated.");
                            ResumeBattleAfterTriggerPrompt(attackingPlayer.Id, attackerZone, defenderZone);
                        });
                    return true;
                }

                ResolveMaidenAttackTrigger(defendingPlayer, defender);
                return true;
            }

            if (defender.Definition.Id == MaidenOfWhiteCardId &&
                !defendingPlayer.UsedOncePerTurn.Contains(MaidenWhiteTargetOnceKey) &&
                FirstEmpty(defendingPlayer.Monsters) >= 0 &&
                HasMatchingCard(defendingPlayer.Graveyard, IsMaidenOfWhiteAttackReviveTarget))
            {
                if (defendingPlayer.Id == CleanPlayerId.Local)
                {
                    if (!TryReserveTriggerPrompt(defendingPlayer, MaidenWhiteTargetOnceKey))
                    {
                        return false;
                    }

                    QueueTriggerEffectPrompt(
                        defendingPlayer,
                        defender.Definition.Name,
                        "This monster was targeted for an attack. Activate its Trigger Effect to Special Summon from the GY before the attack continues?",
                        true,
                        () =>
                        {
                            ReleaseTriggerPrompt(defendingPlayer, MaidenWhiteTargetOnceKey);
                            ResolveMaidenAttackTrigger(defendingPlayer, defender);
                            ResumeBattleAfterTriggerPrompt(attackingPlayer.Id, attackerZone, defenderZone);
                        },
                        () =>
                        {
                            ReleaseTriggerPrompt(defendingPlayer, MaidenWhiteTargetOnceKey);
                            Log("Maiden of White's attack Trigger Effect was not activated.");
                            ResumeBattleAfterTriggerPrompt(attackingPlayer.Id, attackerZone, defenderZone);
                        });
                    return true;
                }

                ResolveMaidenAttackTrigger(defendingPlayer, defender);
            }

            return false;
        }

        private void ResolveMaidenAttackTrigger(CleanPlayerState player, CleanCard maiden)
        {
            int zone = IndexOfMonsterZone(player, maiden);
            if (zone < 0 || maiden.FaceDown || maiden.EffectsNegated)
            {
                Log(maiden.Definition.Name + " could not resolve its attack Trigger Effect.");
                return;
            }

            if (maiden.Definition.Id == MaidenWithEyesOfBlueCardId)
            {
                player.UsedOncePerTurn.Add(MaidenWithEyesOnceKey);
                maiden.Stance = maiden.Stance == CleanMonsterStance.Attack
                    ? CleanMonsterStance.Defense
                    : CleanMonsterStance.Attack;
                bool summoned = FirstEmpty(player.Monsters) >= 0 &&
                                HasBlueEyesWhiteDragonInHandDeckOrGrave(player) &&
                                LuaSpecialSummonBlueEyesWhiteDragon(player, CleanMonsterStance.Attack);
                Log("Maiden with Eyes of Blue negates the attack and changes its battle position" +
                    (summoned ? ", then Special Summons Blue-Eyes White Dragon." : "."));
                return;
            }

            player.UsedOncePerTurn.Add(MaidenWhiteTargetOnceKey);
            SpecialSummonBestFromGraveyard(player, IsMaidenOfWhiteAttackReviveTarget, CleanMonsterStance.Attack, "Maiden of White");
            Log("Maiden of White resolves its attack-targeted Trigger Effect; the attack continues.");
        }

        private void ResumeBattleAfterTriggerPrompt(CleanPlayerId attackingPlayerId, int attackerZone, int defenderZone)
        {
            CleanPlayerState attackingPlayer = Player(attackingPlayerId);
            CleanPlayerState defendingPlayer = OpponentOf(attackingPlayerId);
            suppressAttackTargetTriggerOnce = true;
            try
            {
                ResolveBattle(attackingPlayer, defendingPlayer, attackerZone, defenderZone);
            }
            finally
            {
                suppressAttackTargetTriggerOnce = false;
            }

            if (attackingPlayerId != CleanPlayerId.Rival || State.GameOver || State.PendingChoice != null)
            {
                return;
            }

            RivalBattle();
            if (!State.GameOver && State.PendingChoice == null && State.TurnPlayer == CleanPlayerId.Rival)
            {
                EndTurn();
            }
        }

        private bool IsMaidenOfWhiteAttackReviveTarget(CleanCard card)
        {
            return IsBlueEyesWhiteDragonCard(card) || IsLevelOneLightTuner(card);
        }

        private bool SpecialSummonBestFromGraveyard(CleanPlayerState player, Predicate<CleanCard> predicate, CleanMonsterStance stance, string sourceName)
        {
            CleanCard card;
            if (!TryTakeBestMonster(player.Graveyard, predicate, out card))
            {
                return false;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                player.Graveyard.Add(card);
                return false;
            }

            SummonToZone(player, card, zone, false, stance);
            Log(sourceName + " special summons " + card.Definition.Name + " from the GY.");
            return true;
        }

        private void ApplyChaosDragonAttackDeclaration(CleanCard attacker, CleanPlayerState defendingPlayer)
        {
            if (attacker == null || attacker.Definition.Id != BlueEyesChaosDragonCardId || !attacker.RitualSummonedUsingBlueEyesWhiteDragon)
            {
                return;
            }

            int changed = 0;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = defendingPlayer.Monsters[i];
                if (monster == null || IsLinkMonsterDefinition(monster.Definition))
                {
                    continue;
                }

                monster.FaceDown = false;
                monster.Stance = monster.Stance == CleanMonsterStance.Attack ? CleanMonsterStance.Defense : CleanMonsterStance.Attack;
                monster.AttackOverride = 0;
                monster.DefenseOverride = 0;
                changed++;
            }

            if (changed > 0)
            {
                attacker.PiercingThisTurn = true;
                Log("Blue-Eyes Chaos Dragon changes " + changed + " rival monster(s) to 0 ATK/DEF.");
            }
        }

        private int PiercingBattleDamage(CleanCard attacker, int difference)
        {
            if (attacker == null || difference <= 0)
            {
                return 0;
            }

            if (attacker.Definition.Id == BlueEyesChaosMaxDragonCardId)
            {
                return difference * 2;
            }

            return attacker.PiercingThisTurn ? difference : 0;
        }
        internal void LuaLog(string message)
        {
            Log(message);
        }

        internal void LuaDraw(CleanPlayerState player, int count)
        {
            int safeCount = Mathf.Clamp(count, 1, 5);
            int drawn = Draw(player, safeCount);
            if (!State.GameOver)
            {
                Log(player.Name + " draws " + drawn + " card(s) by Lua.");
            }
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
            return LuaDestroyTargets(player, 1, "Lua");
        }

        private int LuaDestroyTargets(CleanPlayerState player, int requestedCount, string sourceName)
        {
            bool faceUpCards = string.Equals(sourceName, "Ultimate Fusion", StringComparison.OrdinalIgnoreCase);
            int availableTargets = CountDestructibleEffectTargets(player, faceUpCards);
            if (availableTargets == 0 || requestedCount <= 0)
            {
                Log(sourceName + " found no valid card to destroy.");
                return 0;
            }

            int destroyCount = Mathf.Min(requestedCount, availableTargets);
            if (ShouldOpenLuaDestroyChoice(player))
            {
                OpenLuaDestroyTargetChoice(player, destroyCount, sourceName, faceUpCards, faceUpCards);
                return destroyCount;
            }

            int destroyedCount = 0;
            for (int i = 0; i < destroyCount; i++)
            {
                int choiceId = FirstLuaDestroyChoiceId(player, faceUpCards);
                if (choiceId < 0 || !DestroyLuaTargetAtChoice(player, choiceId, sourceName, faceUpCards))
                {
                    break;
                }

                destroyedCount++;
            }

            return destroyedCount;
        }

        private bool ShouldOpenLuaDestroyChoice(CleanPlayerState targetPlayer)
        {
            return targetPlayer.Id == CleanPlayerId.Rival &&
                   State.TurnPlayer == CleanPlayerId.Local &&
                   State.PendingChoice == null &&
                   !State.GameOver;
        }

        private void OpenLuaDestroyTargetChoice(CleanPlayerState targetPlayer, int remainingCount, string sourceName, bool faceUpCards, bool mayStop)
        {
            pendingLuaDestroyTargetPlayerId = targetPlayer.Id;
            pendingLuaDestroyRemaining = Mathf.Max(1, remainingCount);
            pendingLuaDestroySource = string.IsNullOrEmpty(sourceName) ? "Lua" : sourceName;
            pendingLuaDestroyFaceUpCards = faceUpCards;
            pendingLuaDestroyMayStop = mayStop;
            State.PendingChoice = new CleanChoicePrompt(
                LuaDestroyTargetChoicePromptId,
                pendingLuaDestroySource,
                mayStop
                    ? "Choose a face-up card to destroy, or finish. Up to " + pendingLuaDestroyRemaining + " choice(s) remain."
                    : pendingLuaDestroyRemaining > 1
                        ? "Choose a monster to destroy. " + pendingLuaDestroyRemaining + " destroy choices remain."
                        : "Choose a monster to destroy.",
                BuildLuaDestroyTargetOptions(targetPlayer, faceUpCards, mayStop));
            Log(pendingLuaDestroySource + " awaits your destroy choice.");
        }

        private List<CleanChoiceOption> BuildLuaDestroyTargetOptions(CleanPlayerState targetPlayer, bool faceUpCards, bool mayStop)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = targetPlayer.Monsters[i];
                if (IsDestructibleEffectMonster(targetPlayer, monster, faceUpCards))
                {
                    options.Add(new CleanChoiceOption(i, monster.PublicName, "Destroy this monster.", true, string.Empty));
                }

                CleanCard spellTrap = targetPlayer.SpellsTraps[i];
                if (faceUpCards && IsDestructibleFaceUpSpellTrap(targetPlayer, spellTrap))
                {
                    options.Add(new CleanChoiceOption(100 + i, spellTrap.Definition.Name, "Destroy this face-up Spell/Trap.", true, string.Empty));
                }
            }

            if (faceUpCards && IsDestructibleFaceUpSpellTrap(targetPlayer, targetPlayer.FieldSpell))
            {
                options.Add(new CleanChoiceOption(200, targetPlayer.FieldSpell.Definition.Name, "Destroy this Field Spell.", true, string.Empty));
            }

            if (mayStop)
            {
                options.Add(new CleanChoiceOption(-1, "Finish", "Destroy no more cards.", true, string.Empty));
            }

            return options;
        }

        private void ResolveLuaDestroyTargetChoice(int choiceId)
        {
            if (choiceId == -1 && pendingLuaDestroyMayStop)
            {
                Log(pendingLuaDestroySource + " finishes without destroying more cards.");
                ClearPendingEffectChoices();
                return;
            }

            CleanPlayerState targetPlayer = Player(pendingLuaDestroyTargetPlayerId);
            if (!DestroyLuaTargetAtChoice(targetPlayer, choiceId, pendingLuaDestroySource, pendingLuaDestroyFaceUpCards))
            {
                ClearPendingEffectChoices();
                return;
            }

            pendingLuaDestroyRemaining--;
            if (pendingLuaDestroyRemaining > 0 &&
                CountDestructibleEffectTargets(targetPlayer, pendingLuaDestroyFaceUpCards) > 0)
            {
                OpenLuaDestroyTargetChoice(
                    targetPlayer,
                    pendingLuaDestroyRemaining,
                    pendingLuaDestroySource,
                    pendingLuaDestroyFaceUpCards,
                    pendingLuaDestroyMayStop);
                return;
            }

            ClearPendingEffectChoices();
        }

        private bool DestroyLuaTargetAtChoice(CleanPlayerState player, int choiceId, string sourceName, bool faceUpCards)
        {
            CleanCard destroyed;
            if (choiceId >= 200)
            {
                destroyed = player.FieldSpell;
                if (!faceUpCards || !IsDestructibleFaceUpSpellTrap(player, destroyed))
                {
                    return false;
                }

                string name = destroyed.Definition.Name;
                MoveFieldSpellToGrave(player);
                TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                Log(sourceName + " destroys " + name + ".");
                return true;
            }

            if (choiceId >= 100)
            {
                int spellZone = choiceId - 100;
                if (spellZone < 0 || spellZone >= CleanPlayerState.ZoneCount)
                {
                    return false;
                }

                destroyed = player.SpellsTraps[spellZone];
                if (!faceUpCards || !IsDestructibleFaceUpSpellTrap(player, destroyed))
                {
                    return false;
                }

                string name = destroyed.Definition.Name;
                MoveSpellTrapToGrave(player, spellZone);
                TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                Log(sourceName + " destroys " + name + ".");
                return true;
            }

            if (choiceId < 0 ||
                choiceId >= CleanPlayerState.ZoneCount ||
                !IsDestructibleEffectMonster(player, player.Monsters[choiceId], faceUpCards))
            {
                Log(sourceName + " failed: selected card is no longer valid.");
                return false;
            }

            destroyed = player.Monsters[choiceId];
            string monsterName = destroyed.PublicName;
            TriggerMaidenTargetedByCardEffect(player, choiceId);
            MoveMonsterToGrave(player, choiceId);
            TriggerJetSummonAfterFieldCardDestroyed(destroyed);
            Log(sourceName + " destroys " + monsterName + ".");
            TriggerBlueEyesUltimateSpiritDestroyed(player, destroyed);
            TriggerDragonMasterMagiaLeavesField(player, destroyed);
            return true;
        }

        private int FirstLuaDestroyChoiceId(CleanPlayerState player, bool faceUpCards)
        {
            List<CleanChoiceOption> options = BuildLuaDestroyTargetOptions(player, faceUpCards, false);
            return options.Count == 0 ? -1 : options[0].Id;
        }

        private int CountDestructibleEffectTargets(CleanPlayerState player, bool faceUpCards)
        {
            return BuildLuaDestroyTargetOptions(player, faceUpCards, false).Count;
        }

        private bool IsDestructibleEffectMonster(CleanPlayerState player, CleanCard monster, bool nonTargetingFaceUpEffect)
        {
            if (monster == null || (nonTargetingFaceUpEffect && monster.FaceDown))
            {
                return false;
            }

            if (nonTargetingFaceUpEffect)
            {
                return !IsProtectedFromOpponentEffectDestruction(player, monster);
            }

            return !IsProtectedFromCleanTargeting(player, monster) &&
                   !IsProtectedByJetFromCardEffect(player, monster);
        }

        private bool IsDestructibleFaceUpSpellTrap(CleanPlayerState player, CleanCard card)
        {
            return card != null &&
                   !card.FaceDown &&
                   !IsProtectedByJetFromCardEffect(player, card);
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

            int bestIndex = BestCardIndex(player.Graveyard, card => card.Definition.Kind == CleanCardKind.Monster && CanSpecialSummonUnderSpiritRestriction(player, card));
            if (bestIndex < 0)
            {
                Log("Lua revive failed: no monster in graveyard.");
                return false;
            }

            if (TryNegateRivalGraveyardEffect(player, "a graveyard Special Summon effect"))
            {
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

            int handIndex = BestCardIndex(player.Hand, card => card.Definition.Kind == CleanCardKind.Monster && CanSpecialSummonUnderSpiritRestriction(player, card));
            if (handIndex >= 0)
            {
                CleanCard card = player.Hand[handIndex];
                player.Hand.RemoveAt(handIndex);
                SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
                Log("Lua special summons " + card.Definition.Name + " from the hand.");
                return true;
            }

            int graveIndex = BestCardIndex(player.Graveyard, card => card.Definition.Kind == CleanCardKind.Monster && CanSpecialSummonUnderSpiritRestriction(player, card));
            if (graveIndex >= 0)
            {
                if (TryNegateRivalGraveyardEffect(player, "a graveyard Special Summon effect"))
                {
                    return false;
                }

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
            AddToGraveyard(player, card);
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
            trueLight.DefenseBonus = 0;
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

            if (IndexOfCard(player.Hand, BlueEyesWhiteDragonCardId) < 0 &&
                !HasMatchingCard(
                    player.Graveyard,
                    card => IsTreatedAsBlueEyesWhiteDragon(card) &&
                            CanSpecialSummonCard(player, card, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect)))
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
                if (ResolveTrueLightSummonBlueEyes(player))
                {
                    player.UsedOncePerTurn.Add(TrueLightOnceKey);
                }
            }
            else if (optionId == TrueLightSetOptionId)
            {
                OpenTrueLightSetCardChoice(player);
            }
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
            if (!TryTakeTreatedBlueEyesWhiteDragonFromGrave(player, out card) && !TryTakeCard(player.Hand, BlueEyesWhiteDragonCardId, out card))
            {
                Log("True Light cannot find Blue-Eyes White Dragon.");
                return false;
            }

            SummonToZone(player, card, FirstEmpty(player.Monsters), false, CleanMonsterStance.Attack);
            Log("True Light special summons Blue-Eyes White Dragon.");
            return true;
        }

        private bool TryTakeTreatedBlueEyesWhiteDragonFromGrave(CleanPlayerState player, out CleanCard card)
        {
            for (int i = 0; i < player.Graveyard.Count; i++)
            {
                if (!IsTreatedAsBlueEyesWhiteDragon(player.Graveyard[i]) ||
                    !CanSpecialSummonCard(player, player.Graveyard[i], CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect))
                {
                    continue;
                }

                card = player.Graveyard[i];
                player.Graveyard.RemoveAt(i);
                return true;
            }

            card = null;
            return false;
        }

        private void OpenTrueLightSetCardChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            HashSet<int> seenIds = new HashSet<int>();
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard card = player.Deck[i];
                if (!IsTrueLightSetCandidate(card) || NameExistsOnFieldOrGraveyard(player, card.Definition.Name) || seenIds.Contains(card.Definition.Id))
                {
                    continue;
                }

                seenIds.Add(card.Definition.Id);
                options.Add(new CleanChoiceOption(
                    card.Definition.Id,
                    card.Definition.Name,
                    card.Definition.Kind + " from Deck",
                    true,
                    string.Empty));
            }

            if (options.Count == 0)
            {
                Log("True Light cannot set a Spell/Trap: no valid listed Spell/Trap in deck.");
                return;
            }

            State.PendingChoice = new CleanChoicePrompt(
                TrueLightSetCardChoicePromptId,
                "True Light",
                "Choose the Spell/Trap to Set from your Deck.",
                options);
            Log("True Light awaits a Spell/Trap choice.");
        }

        private bool ResolveTrueLightSetSpellTrap(CleanPlayerState player, int selectedCardId)
        {
            string reason;
            if (!ValidateTrueLightSetSpellTrap(player, out reason))
            {
                Log("True Light cannot set a Spell/Trap: " + reason + ".");
                return false;
            }

            int deckIndex = IndexOfTrueLightSetCandidate(player, selectedCardId);
            if (deckIndex < 0)
            {
                Log("True Light cannot set that Spell/Trap anymore.");
                return false;
            }

            int zone = FirstEmpty(player.SpellsTraps);
            CleanCard card = player.Deck[deckIndex];
            player.Deck.RemoveAt(deckIndex);
            card.Controller = player.Id;
            card.FaceDown = true;
            player.SpellsTraps[zone] = card;
            Log("True Light sets " + card.Definition.Name + " from the deck.");
            return true;
        }

        private int IndexOfTrueLightSetCandidate(CleanPlayerState player, int selectedCardId)
        {
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard card = player.Deck[i];
                if (card.Definition.Id == selectedCardId && IsTrueLightSetCandidate(card) && !NameExistsOnFieldOrGraveyard(player, card.Definition.Name))
                {
                    return i;
                }
            }

            return -1;
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
                case 24382602:
                case 93437091:
                case 80326401:
                case 17725109:
                case 2783661:
                case 21082832:
                case 41620959:
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
                case 2783661:
                    return 75;
                case 93437091:
                    return 70;
                case 24382602:
                    return 65;
                case 21082832:
                    return 60;
                case 41620959:
                    return 55;
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

            if (player.FieldSpell != null && string.Equals(player.FieldSpell.Definition.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
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
        private bool RequiresUiSpecialSummonPrompt(CleanCard card, CleanActivationSource source)
        {
            if (card == null)
            {
                return false;
            }

            if (RequiresSpecialSummonConfirmation(card, source))
            {
                return true;
            }

            switch (card.Definition.EffectKey)
            {
                case "sage_clean":
                    return source == CleanActivationSource.Hand;
                case "dictator_clean":
                    return source == CleanActivationSource.Hand || source == CleanActivationSource.MonsterZone;
                case "neo_kaiser_clean":
                case "alternative_clean":
                    return source == CleanActivationSource.Hand;
                default:
                    return false;
            }
        }

        private bool RequiresSpecialSummonConfirmation(CleanCard card, CleanActivationSource source)
        {
            if (card == null)
            {
                return false;
            }

            switch (card.Definition.EffectKey)
            {
                case "roar_clean":
                case "chaos_form_clean":
                case "ultimate_fusion_clean":
                    return true;
                case "neo_kaiser_clean":
                case "alternative_clean":
                    return source == CleanActivationSource.Hand;
                case "dragon_spirit_white_clean":
                    return source == CleanActivationSource.MonsterZone;
                case "spirit_dragon_clean":
                    return source == CleanActivationSource.MonsterZone &&
                           !CanUseSpiritDragonGraveNegate(State.Local, card);
                default:
                    return false;
            }
        }
        private bool CanActivateCleanCard(CleanCard card, CleanPlayerState player, CleanActivationSource source)
        {
            if (card == null || IsNormalMonster(card) || string.IsNullOrEmpty(card.Definition.EffectKey))
            {
                return false;
            }

            if (source == CleanActivationSource.MonsterZone && (card.FaceDown || card.EffectsNegated))
            {
                return false;
            }

            if (IsRitualMonster(card) || IsFlipTriggerEffect(card) || IsTriggerOnlyEffect(card))
            {
                return false;
            }

            switch (card.Definition.EffectKey)
            {
                case "neo_kaiser_clean":
                    return source == CleanActivationSource.Hand
                        ? !player.UsedOncePerTurn.Contains(NeoKaiserSummonOnceKey) &&
                          FirstEmpty(player.Monsters) >= 0 &&
                          HasFaceUpBlueEyesWhiteDragon(player) &&
                          CanSpecialSummonUnderSpiritRestriction(player, card)
                        : source == CleanActivationSource.MonsterZone &&
                          !player.UsedOncePerTurn.Contains(NeoKaiserLevelOnceKey) &&
                          HasLightTunerOnField(player);
                case "maiden_white":
                    return source != CleanActivationSource.SpellTrapZone &&
                           !player.UsedOncePerTurn.Contains(MaidenWhitePlaceOnceKey) &&
                           CanPlaceTrueLight(player);
                case "maiden_with_eyes_clean":
                    return false;
                case "sage_clean":
                    return source == CleanActivationSource.Hand &&
                           !player.UsedOncePerTurn.Contains(SageHandOnceKey) &&
                           HasOwnEffectMonsterOnField(player) &&
                           HasMatchingCard(
                               player.Deck,
                               candidate => IsBlueEyesMonster(candidate) &&
                                            CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Deck, CleanSummonMethod.Effect));
                case "stone_ancients_clean":
                    return false;
                case "maxx_c_clean":
                    return source == CleanActivationSource.Hand &&
                           !player.UsedOncePerTurn.Contains(MaxxCOnceKey);
                case "veiler_clean":
                    return source == CleanActivationSource.Hand &&
                           State.TurnPlayer != player.Id &&
                           IsMainPhase(State.Phase) &&
                           StrongestFaceUpEffectMonsterZone(OpponentOf(player.Id)) >= 0;
                case "ash_clean":
                    return source == CleanActivationSource.Hand &&
                           !player.UsedOncePerTurn.Contains(AshBlossomOnceKey) &&
                           CanRespondToOpponentEffect(player, CleanResponseKind.DeckEffect);
                case "ghost_ogre_clean":
                    return (source == CleanActivationSource.Hand || source == CleanActivationSource.MonsterZone) &&
                           !player.UsedOncePerTurn.Contains(GhostOgreOnceKey) &&
                           pendingResponseSourceWasFaceUp &&
                           CanRespondToOpponentEffect(player, CleanResponseKind.FieldEffect);
                case "dragon_spirit_white_clean":
                    return source == CleanActivationSource.MonsterZone &&
                           CanDragonSpiritTributeForBlueEyes(player, OpponentOf(player.Id));
                case "dictator_clean":
                    return source == CleanActivationSource.Hand
                        ? !player.UsedOncePerTurn.Contains(DictatorSummonOnceKey) &&
                          FirstEmpty(player.Monsters) >= 0 &&
                          CanSpecialSummonUnderSpiritRestriction(player, card) &&
                          HasBlueEyesWhiteDragonInHandOrDeck(player)
                        : !player.UsedOncePerTurn.Contains(DictatorReviveOnceKey) &&
                          FirstEmpty(player.Monsters) >= 0 &&
                          HasBlueEyesDiscardCost(player) &&
                          HasMatchingCard(
                              player.Graveyard,
                              candidate => IsBlueEyesMonster(candidate) &&
                                           CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect));
                case "blue_eyes_clean":
                    return false;
                case "jet_clean":
                    return false;
                case "alternative_clean":
                    return source == CleanActivationSource.Hand
                        ? !player.UsedOncePerTurn.Contains(AlternativeSummonOnceKey) &&
                          FirstEmpty(player.Monsters) >= 0 &&
                          IndexOfCard(player.Hand, BlueEyesWhiteDragonCardId) >= 0 &&
                          CanSpecialSummonCard(player, card, CleanSummonOrigin.Hand, CleanSummonMethod.AlternativeProcedure)
                        : source == CleanActivationSource.MonsterZone &&
                          !player.UsedOncePerTurn.Contains(AlternativeDestroyOnceKey) &&
                          HasTargetableMonster(OpponentOf(player.Id));
                case "abyss_clean":
                    return false;
                case "mausoleum_clean":
                    return source == CleanActivationSource.Hand
                        ? true
                        : card.FaceDown || CanActivateMausoleumBoost(player);
                case "bingo_clean":
                    return !player.UsedOncePerTurn.Contains(BingoActivationOnceKey) &&
                           CountMatchingCards(player.Deck, IsBingoSearchCandidate) >= 3;
                case "monster_reborn_clean":
                    return FirstEmpty(player.Monsters) >= 0 && HasMonsterRebornTarget(player);
                case "wishes_clean":
                    return !player.UsedOncePerTurn.Contains(WishesSearchOnceKey) &&
                           CanActivateWishesForEyesOfBlue(player, source);
                case "roar_clean":
                    return !player.UsedOncePerTurn.Contains(RoarSharedOnceKey) &&
                           CanRoarSummonBlueEyes(player);
                case "raigeki_clean":
                    return MonsterCount(OpponentOf(player.Id)) > 0;
                case "polymerization_clean":
                    return CanFusionSummonFromHandOrField(player);
                case "gateway_chaos_clean":
                    return source == CleanActivationSource.Hand || card.FaceDown
                        ? !player.UsedOncePerTurn.Contains(GatewayActivationOnceKey) && HasMatchingCard(player.Deck, IsGatewayToChaosSearchCandidate)
                        : CanUseGatewayToChaosRitualSearch(player);
                case "majesty_blue_clean":
                    return !player.UsedOncePerTurn.Contains(MajestyActivationOnceKey) &&
                           (HasMatchingCard(player.Deck, IsBlueEyesMonster) || HasMatchingCard(player.Hand, IsBlueEyesMonster)) &&
                           (StrongestFaceUpMonsterZone(player) >= 0 || StrongestEffectTargetZone(OpponentOf(player.Id)) >= 0);
                case "chaos_form_clean":
                    return CanRitualSummonWithChaosForm(player);
                case "dragon_shrine_clean":
                    return !player.UsedOncePerTurn.Contains(DragonShrineActivationOnceKey) &&
                           HasMatchingCard(player.Deck, IsDragonMonster);
                case "ultimate_fusion_clean":
                    return IsMainPhase(State.Phase) &&
                           !player.UsedOncePerTurn.Contains(UltimateFusionActivationOnceKey) &&
                           CanFusionSummonFromExtra(player, DragonMasterMagiaCardId);
                case "azure_eyes_clean":
                    return false;
                case "magia_clean":
                    return source == CleanActivationSource.MonsterZone &&
                           CanRespondWithDragonMasterMagia(player, card);
                case "ultimate_spirit_clean":
                    return source == CleanActivationSource.MonsterZone &&
                           CanRespondToOpponentEffect(player, CleanResponseKind.FieldEffect) &&
                           CanUseUltimateSpiritNegate(player, card);
                case "spirit_dragon_clean":
                    return source == CleanActivationSource.MonsterZone &&
                           (CanActivateBlueEyesSpiritDragon(player, card) ||
                            CanUseSpiritDragonGraveNegate(player, card));
                case "spirit_with_eyes_clean":
                    return source == CleanActivationSource.MonsterZone && CanActivateSpiritWithEyesOfBlue(player, card);
                default:
                    return false;
            }
        }

        private bool CanRespondToOpponentEffect(CleanPlayerState player, CleanResponseKind responseKind)
        {
            return pendingResponseController != null &&
                   pendingResponseController.Id != player.Id &&
                   (pendingResponseKind & responseKind) != 0;
        }

        private bool CanUseUltimateSpiritNegate(CleanPlayerState player, CleanCard spirit)
        {
            return spirit != null &&
                   spirit.Definition.Id == BlueEyesUltimateSpiritDragonCardId &&
                   !spirit.FaceDown &&
                   !spirit.EffectsNegated &&
                   !player.UsedOncePerTurn.Contains(UltimateSpiritNegateOnceKey);
        }

        private bool CanUseSpiritDragonGraveNegate(CleanPlayerState player, CleanCard spirit)
        {
            return spirit != null &&
                   spirit.Definition.Id == BlueEyesSpiritDragonCardId &&
                   !spirit.FaceDown &&
                   !spirit.EffectsNegated &&
                   !player.UsedOncePerTurn.Contains(BlueEyesSpiritGraveNegateOnceKey) &&
                   CanRespondToOpponentEffect(player, CleanResponseKind.GraveyardEffect);
        }

        private bool CanRespondWithDragonMasterMagia(CleanPlayerState player, CleanCard magia)
        {
            if (magia == null ||
                magia.Definition.Id != DragonMasterMagiaCardId ||
                magia.FaceDown ||
                magia.EffectsNegated ||
                !CanRespondToOpponentEffect(
                    player,
                    CleanResponseKind.DeckEffect | CleanResponseKind.FieldEffect | CleanResponseKind.GraveyardEffect))
            {
                return false;
            }

            return !player.UsedOncePerTurn.Contains(MagiaOnceKeyFor(pendingResponseCard));
        }

        private string MagiaOnceKeyFor(CleanCard responseCard)
        {
            if (responseCard != null && responseCard.Definition.Kind == CleanCardKind.Trap)
            {
                return MagiaTrapNegateOnceKey;
            }

            if (responseCard != null && responseCard.Definition.Kind == CleanCardKind.Spell)
            {
                return MagiaSpellNegateOnceKey;
            }

            return MagiaMonsterNegateOnceKey;
        }

        private int CountMatchingCards(List<CleanCard> cards, Predicate<CleanCard> predicate)
        {
            int count = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (predicate(cards[i]))
                {
                    count++;
                }
            }

            return count;
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
                case "neo_kaiser_clean":
                    if (source == CleanActivationSource.Hand)
                    {
                        controller.UsedOncePerTurn.Add(NeoKaiserSummonOnceKey);
                        LuaSpecialSummonSpecific(controller, card);
                        return false;
                    }

                    controller.UsedOncePerTurn.Add(NeoKaiserLevelOnceKey);
                    OpenNeoKaiserLevelChoice(controller);
                    return false;
                case "alternative_clean":
                    if (source == CleanActivationSource.Hand)
                    {
                        controller.UsedOncePerTurn.Add(AlternativeSummonOnceKey);
                        card.ProperlySummoned = true;
                        LuaSpecialSummonSpecific(controller, card);
                        return false;
                    }

                    controller.UsedOncePerTurn.Add(AlternativeDestroyOnceKey);
                    card.CannotAttack = true;
                    card.CannotAttackUntilEndOfTurn = true;
                    ActivateAlternativeDestroy(controller, rival);
                    return false;
                case "abyss_clean":
                    return false;
                case "blue_eyes_clean":
                    return false;
                case "maiden_white":
                    controller.UsedOncePerTurn.Add(MaidenWhitePlaceOnceKey);
                    LuaPlaceTrueLight(controller);
                    return true;
                case "maiden_with_eyes_clean":
                    return false;
                case "sage_clean":
                    if (source == CleanActivationSource.Hand)
                    {
                        controller.UsedOncePerTurn.Add(SageHandOnceKey);
                        ActivateSageHandEffect(controller, card);
                        return false;
                    }

                    return false;
                case "stone_ancients_clean":
                    return false;
                case "maxx_c_clean":
                    controller.UsedOncePerTurn.Add(MaxxCOnceKey);
                    controller.MaxxCDrawActive = true;
                    Log("Maxx \"C\" is sent to the GY. Each opposing Special Summon event this turn will draw 1 card.");
                    return true;
                case "veiler_clean":
                    if (controller.Id == CleanPlayerId.Local)
                    {
                        OpenVeilerTargetChoice(rival);
                    }
                    else
                    {
                        NegateStrongestRivalMonsterUntilEndOfTurn(rival);
                    }

                    return true;
                case "ash_clean":
                    controller.UsedOncePerTurn.Add(AshBlossomOnceKey);
                    pendingResponseNegated = true;
                    Log("Ash Blossom negates the opposing effect that would move a card from the Deck.");
                    return true;
                case "ghost_ogre_clean":
                    controller.UsedOncePerTurn.Add(GhostOgreOnceKey);
                    DestroyPendingResponseSource();
                    return true;
                case "dictator_clean":
                    if (source == CleanActivationSource.Hand)
                    {
                        controller.UsedOncePerTurn.Add(DictatorSummonOnceKey);
                        if (controller.Id == CleanPlayerId.Local)
                        {
                            OpenDictatorSendChoice(controller, card);
                            return false;
                        }

                        SendBlueEyesWhiteDragonToGraveFromHandOrDeck(controller);
                        LuaSpecialSummonSpecific(controller, card);
                        return false;
                    }

                    controller.UsedOncePerTurn.Add(DictatorReviveOnceKey);
                    if (controller.Id == CleanPlayerId.Local)
                    {
                        OpenDictatorDiscardChoice(controller);
                    }
                    else
                    {
                        ActivateDictatorRevive(controller);
                    }

                    return false;
                case "jet_clean":
                    return false;
                case "dragon_spirit_white_clean":
                    return ActivateDragonSpiritTributeEffect(controller, card);
                case "bingo_clean":
                    controller.UsedOncePerTurn.Add(BingoActivationOnceKey);
                    ActivateBingoMachine(controller);
                    return true;
                case "monster_reborn_clean":
                    ActivateMonsterReborn(controller, rival);
                    return true;
                case "wishes_clean":
                    controller.UsedOncePerTurn.Add(WishesSearchOnceKey);
                    ActivateWishesForEyesOfBlue(controller, card);
                    return true;
                case "roar_clean":
                    controller.UsedOncePerTurn.Add(RoarSharedOnceKey);
                    LuaRoarSummonBlueEyes(controller);
                    controller.ExtraDeckDragonOnlyThisTurn = true;
                    return true;
                case "raigeki_clean":
                    DestroyAllMonsters(rival);
                    Log("Raigeki destroys all monsters the opponent controls.");
                    return true;
                case "polymerization_clean":
                    if (!TryOpenFusionSummonChoice(controller, 0, false, false, false, "Polymerization"))
                    {
                        int ignoredFieldMaterials;
                        LuaFusionSummonBestAvailable(controller, 0, false, out ignoredFieldMaterials);
                    }
                    return true;
                case "gateway_chaos_clean":
                    ActivateGatewayToChaos(controller, card, source);
                    return false;
                case "majesty_blue_clean":
                    controller.UsedOncePerTurn.Add(MajestyActivationOnceKey);
                    ActivateMajestyWithEyesOfBlue(controller, rival);
                    return true;
                case "mausoleum_clean":
                    ActivateMausoleumOfWhite(controller, card, source);
                    return false;
                case "chaos_form_clean":
                    LuaRitualSummonWithChaosForm(controller);
                    return true;
                case "dragon_shrine_clean":
                    controller.UsedOncePerTurn.Add(DragonShrineActivationOnceKey);
                    return ActivateDragonShrine(controller);
                case "ultimate_fusion_clean":
                    controller.UsedOncePerTurn.Add(UltimateFusionActivationOnceKey);
                    if (TryOpenFusionSummonChoice(controller, DragonMasterMagiaCardId, true, true, true, "Ultimate Fusion"))
                    {
                        return true;
                    }

                    int fieldBlueEyesMaterialCount;
                    if (!LuaFusionSummonBestAvailable(controller, DragonMasterMagiaCardId, true, out fieldBlueEyesMaterialCount))
                    {
                        return true;
                    }

                    LuaDestroyTargets(rival, fieldBlueEyesMaterialCount, "Ultimate Fusion");
                    return true;
                case "true_light_clean":
                    resolved = false;
                    return true;
                case "ultimate_dragon_clean":
                    return false;
                case "magia_clean":
                    ResolveDragonMasterMagiaResponse(controller, card);
                    return false;
                case "ultimate_spirit_clean":
                    ResolveUltimateSpiritResponse(controller, card);
                    return false;
                case "spirit_dragon_clean":
                    if (CanUseSpiritDragonGraveNegate(controller, card))
                    {
                        controller.UsedOncePerTurn.Add(BlueEyesSpiritGraveNegateOnceKey);
                        pendingResponseNegated = true;
                        Log("Blue-Eyes Spirit Dragon negates the activation of the opposing GY effect.");
                    }
                    else
                    {
                        ActivateBlueEyesSpiritDragon(controller, card);
                    }

                    return false;
                case "spirit_with_eyes_clean":
                    ActivateSpiritWithEyesOfBlue(controller, card);
                    return false;
                default:
                    resolved = false;
                    return true;
            }
        }

        private void ResolveUltimateSpiritResponse(CleanPlayerState player, CleanCard spirit)
        {
            if (!CanUseUltimateSpiritNegate(player, spirit) ||
                !CanRespondToOpponentEffect(player, CleanResponseKind.FieldEffect))
            {
                return;
            }

            pendingResponseNegated = true;
            spirit.AttackBonus += 1000;
            player.UsedOncePerTurn.Add(UltimateSpiritNegateOnceKey);
            Log("Blue-Eyes Ultimate Spirit Dragon negates the activation and gains 1000 ATK until the end of this turn.");
        }

        private void ResolveDragonMasterMagiaResponse(CleanPlayerState player, CleanCard magia)
        {
            if (!CanRespondWithDragonMasterMagia(player, magia))
            {
                return;
            }

            string onceKey = MagiaOnceKeyFor(pendingResponseCard);
            player.UsedOncePerTurn.Add(onceKey);
            pendingResponseNegated = true;
            DestroyPendingResponseSource();
            Log("Dragon Master Magia negates the activation and destroys the activated card.");
        }

        private void DestroyPendingResponseSource()
        {
            pendingResponseDestroyed = pendingResponseCard != null;
            if (!pendingResponseDestroyed)
            {
                return;
            }

            CleanPlayerState controller = pendingResponseController;
            CleanCard source = pendingResponseCard;
            if (controller == null)
            {
                return;
            }

            if (source.Definition.Kind == CleanCardKind.Monster && IsProtectedFromOpponentEffectDestruction(controller, source))
            {
                pendingResponseDestroyed = false;
                Log(source.Definition.Name + " is protected from card-effect destruction.");
                return;
            }

            if (source.Definition.Kind != CleanCardKind.Monster && IsProtectedByJetFromCardEffect(controller, source))
            {
                pendingResponseDestroyed = false;
                Log("Blue-Eyes Jet Dragon protects " + source.Definition.Name + " from card-effect destruction.");
                return;
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (object.ReferenceEquals(controller.Monsters[i], source))
                {
                    MoveMonsterToGrave(controller, i);
                    TriggerJetSummonAfterFieldCardDestroyed(source);
                    TriggerBlueEyesUltimateSpiritDestroyed(controller, source);
                    TriggerDragonMasterMagiaLeavesField(controller, source);
                    Log(source.Definition.Name + " is destroyed by the chained effect.");
                    return;
                }

                if (object.ReferenceEquals(controller.SpellsTraps[i], source))
                {
                    MoveSpellTrapToGrave(controller, i);
                    TriggerJetSummonAfterFieldCardDestroyed(source);
                    Log(source.Definition.Name + " is destroyed by the chained effect.");
                    return;
                }
            }

            if (object.ReferenceEquals(controller.FieldSpell, source))
            {
                MoveFieldSpellToGrave(controller);
                TriggerJetSummonAfterFieldCardDestroyed(source);
                Log(source.Definition.Name + " is destroyed by the chained effect.");
                return;
            }

            for (int i = 0; i < State.ExtraMonsters.Length; i++)
            {
                if (!object.ReferenceEquals(State.ExtraMonsters[i], source))
                {
                    continue;
                }

                State.ExtraMonsters[i] = null;
                SendFieldCardToGraveOrFaceUpExtra(controller, source, true);
                TriggerJetSummonAfterFieldCardDestroyed(source);
                TriggerBlueEyesUltimateSpiritDestroyed(controller, source);
                TriggerDragonMasterMagiaLeavesField(controller, source);
                Log(source.Definition.Name + " is destroyed by the chained effect.");
                return;
            }

            Log(source.Definition.Name + " will be destroyed when its activation finishes resolving.");
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

        private void ActivateSageHandEffect(CleanPlayerState player, CleanCard sage)
        {
            ResetCardForHiddenZone(sage);
            AddToGraveyard(player, sage, true);

            if (player.Id == CleanPlayerId.Local)
            {
                OpenSageTargetChoice(player);
                return;
            }

            int targetZone = FirstOwnEffectMonsterZone(player, sage.Definition.Id);
            if (targetZone >= 0)
            {
                string targetName = player.Monsters[targetZone].Definition.Name;
                MoveMonsterToGrave(player, targetZone);
                Log("Sage sends " + targetName + " to the GY.");
                LuaSpecialSummonBestBlueEyesFromDeck(player);
            }
        }

        private void OpenSageTargetChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && !IsNormalMonster(monster))
                {
                    options.Add(new CleanChoiceOption(i, monster.Definition.Name, "Send this Effect Monster to the GY.", true, string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                SageTargetChoicePromptId,
                "Sage with Eyes of Blue",
                "Choose 1 Effect Monster you control to send to the GY.",
                options);
            Log("Sage awaits an Effect Monster target.");
        }

        private void ResolveSageTargetChoice(CleanPlayerState player, int zone)
        {
            if (zone < 0 || zone >= CleanPlayerState.ZoneCount || player.Monsters[zone] == null || player.Monsters[zone].FaceDown || IsNormalMonster(player.Monsters[zone]))
            {
                Log("Sage failed: selected target is no longer a face-up Effect Monster.");
                return;
            }

            CleanCard target = player.Monsters[zone];
            string targetName = target.Definition.Name;
            Action continueSageEffect = () =>
            {
                int currentZone = IndexOfMonsterZone(player, target);
                if (currentZone < 0)
                {
                    Log("Sage failed: its targeted monster left the field.");
                    return;
                }

                MoveMonsterToGrave(player, currentZone);
                Log("Sage sends " + targetName + " to the GY.");
                OpenSageBlueEyesChoice(player);
            };

            if (TriggerMaidenTargetedByCardEffect(player, zone, continueSageEffect))
            {
                return;
            }

            continueSageEffect();
        }

        private void OpenSageBlueEyesChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = BuildUniqueDeckChoiceOptions(
                player,
                card => IsBlueEyesMonster(card) &&
                        CanSpecialSummonCard(player, card, CleanSummonOrigin.Deck, CleanSummonMethod.Effect),
                "Special Summon from Deck");
            if (options.Count == 0)
            {
                Log("Sage failed: no Blue-Eyes monster in Deck.");
                return;
            }

            State.PendingChoice = new CleanChoicePrompt(
                SageBlueEyesChoicePromptId,
                "Sage with Eyes of Blue",
                "Choose 1 Blue-Eyes monster from your Deck to Special Summon.",
                options,
                CleanPromptKind.SpecialSummon,
                true);
            Log("Sage awaits a Blue-Eyes summon choice.");
        }

        private void ResolveSageBlueEyesChoice(CleanPlayerState player, int selectedCardId)
        {
            CleanCard card;
            if (!TryTakeDeckChoice(
                    player,
                    selectedCardId,
                    candidate => IsBlueEyesMonster(candidate) &&
                                 CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Deck, CleanSummonMethod.Effect),
                    out card))
            {
                Log("Sage failed: chosen Blue-Eyes monster is no longer in Deck.");
                return;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                player.Deck.Add(card);
                Log("Sage failed: no open Monster Zone.");
                return;
            }

            SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
            Log("Sage special summons " + card.Definition.Name + " from Deck.");
        }

        private void OpenSageNormalSummonSearchChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = BuildUniqueDeckChoiceOptions(player, card => card.Definition.Id != SageWithEyesOfBlueCardId && IsLevelOneLightTuner(card), "Add to hand");
            if (options.Count == 0)
            {
                Log("Sage search failed: no Level 1 LIGHT Tuner in Deck.");
                return;
            }

            State.PendingChoice = new CleanChoicePrompt(
                SageSearchChoicePromptId,
                "Sage with Eyes of Blue",
                "Choose 1 Level 1 LIGHT Tuner from your Deck to add to your hand, except Sage.",
                options,
                CleanPromptKind.TriggerEffect,
                true);
            Log("Sage awaits a Level 1 LIGHT Tuner choice.");
        }

        private void ResolveSageSearchChoice(CleanPlayerState player, int selectedCardId)
        {
            CleanCard card;
            if (!TryTakeDeckChoice(player, selectedCardId, candidate => candidate.Definition.Id != SageWithEyesOfBlueCardId && IsLevelOneLightTuner(candidate), out card))
            {
                Log("Sage search failed: chosen Tuner is no longer in Deck.");
                return;
            }

            ResetCardForHiddenZone(card);
            player.Hand.Add(card);
            Log("Sage adds " + card.Definition.Name + " from Deck to hand.");
        }

        private bool CanActivateWishesForEyesOfBlue(CleanPlayerState player, CleanActivationSource source)
        {
            int discardableCards = player.Hand.Count - (source == CleanActivationSource.Hand ? 1 : 0);
            return discardableCards > 0 &&
                   HasMatchingCard(player.Deck, IsWishesLevelOneLightTunerTarget) &&
                   HasMatchingCard(player.Deck, IsWishesSpellTrapTarget);
        }

        private bool ActivateWishesForEyesOfBlue(CleanPlayerState player, CleanCard source)
        {
            if (player.Id == CleanPlayerId.Local)
            {
                OpenWishesDiscardChoice(player);
                return true;
            }

            CleanCard discarded;
            if (!TryTakeFirstDiscard(player, out discarded))
            {
                Log("Wishes for Eyes of Blue failed: no card to discard.");
                return false;
            }

            ResetCardForHiddenZone(discarded);
            AddToGraveyard(player, discarded, true);
            Log("Discarded " + discarded.Definition.Name + " for Wishes.");
            AddFromDeckToHand(player, IsWishesLevelOneLightTunerTarget, "Level 1 LIGHT Tuner");
            AddFromDeckToHand(player, IsWishesSpellTrapTarget, "Spell/Trap that mentions Blue-Eyes White Dragon");
            return true;
        }

        private void OpenWishesDiscardChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            HashSet<int> seenIds = new HashSet<int>();
            for (int i = 0; i < player.Hand.Count; i++)
            {
                CleanCard card = player.Hand[i];
                if (seenIds.Contains(card.Definition.Id))
                {
                    continue;
                }

                seenIds.Add(card.Definition.Id);
                options.Add(new CleanChoiceOption(card.Definition.Id, card.Definition.Name, "Discard for cost", true, string.Empty));
            }

            State.PendingChoice = new CleanChoicePrompt(
                WishesDiscardChoicePromptId,
                "Wishes for Eyes of Blue",
                "Choose 1 card to discard.",
                options);
            Log("Wishes awaits a discard choice.");
        }

        private void ResolveWishesDiscardChoice(CleanPlayerState player, int selectedCardId)
        {
            CleanCard discarded;
            if (!TryTakeCard(player.Hand, selectedCardId, out discarded))
            {
                Log("Wishes failed: chosen discard is no longer in hand.");
                return;
            }

            ResetCardForHiddenZone(discarded);
            AddToGraveyard(player, discarded, true);
            Log("Discarded " + discarded.Definition.Name + " for Wishes.");
            OpenWishesTunerChoice(player);
        }

        private void OpenWishesTunerChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = BuildUniqueDeckChoiceOptions(player, IsWishesLevelOneLightTunerTarget, "Add to hand");
            State.PendingChoice = new CleanChoicePrompt(
                WishesTunerChoicePromptId,
                "Wishes for Eyes of Blue",
                "Choose 1 Level 1 LIGHT Tuner from your Deck.",
                options);
            Log("Wishes awaits a Tuner choice.");
        }

        private void ResolveWishesTunerChoice(CleanPlayerState player, int selectedCardId)
        {
            CleanCard card;
            if (!TryTakeDeckChoice(player, selectedCardId, IsWishesLevelOneLightTunerTarget, out card))
            {
                Log("Wishes failed: chosen Tuner is no longer in Deck.");
                return;
            }

            ResetCardForHiddenZone(card);
            player.Hand.Add(card);
            Log("Wishes adds " + card.Definition.Name + " from Deck to hand.");
            OpenWishesSupportChoice(player);
        }

        private void OpenWishesSupportChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = BuildUniqueDeckChoiceOptions(player, IsWishesSpellTrapTarget, "Add to hand");
            State.PendingChoice = new CleanChoicePrompt(
                WishesSupportChoicePromptId,
                "Wishes for Eyes of Blue",
                "Choose 1 Spell/Trap that mentions Blue-Eyes White Dragon, except Wishes.",
                options);
            Log("Wishes awaits a Spell/Trap choice.");
        }

        private void ResolveWishesSupportChoice(CleanPlayerState player, int selectedCardId)
        {
            CleanCard card;
            if (!TryTakeDeckChoice(player, selectedCardId, IsWishesSpellTrapTarget, out card))
            {
                Log("Wishes failed: chosen Spell/Trap is no longer in Deck.");
                return;
            }

            ResetCardForHiddenZone(card);
            player.Hand.Add(card);
            Log("Wishes adds " + card.Definition.Name + " from Deck to hand.");
        }

        private bool ActivateMausoleumOfWhite(CleanPlayerState player, CleanCard source, CleanActivationSource activationSource)
        {
            if (activationSource == CleanActivationSource.Hand)
            {
                source.FaceDown = false;
                source.Controller = player.Id;
                PlaceFieldSpell(player, source);
                Log("Mausoleum of White is activated in the Field Spell Zone.");
                return true;
            }

            if (source.FaceDown)
            {
                source.FaceDown = false;
                Log("Mausoleum of White is activated face-up.");
            }

            if (!CanActivateMausoleumBoost(player))
            {
                Log("Mausoleum failed: no valid target and Normal Monster to send.");
                return false;
            }

            if (player.Id == CleanPlayerId.Local)
            {
                OpenMausoleumTargetChoice(player);
                return true;
            }

            int targetZone = StrongestFaceUpMonsterZone(player);
            CleanCard sent;
            bool sentFromHand = false;
            if (!TryTakeBestMonster(player.Deck, IsNormalMonsterCard, out sent))
            {
                if (!TryTakeBestMonster(player.Hand, IsNormalMonsterCard, out sent))
                {
                    return false;
                }

                sentFromHand = true;
            }

            ResolveMausoleumBoost(player, targetZone, sent, sentFromHand);
            return true;
        }

        private bool CanActivateMausoleumBoost(CleanPlayerState player)
        {
            return !player.UsedOncePerTurn.Contains(MausoleumBoostOnceKey) &&
                   StrongestFaceUpMonsterZone(player) >= 0 &&
                   (HasMatchingCard(player.Deck, IsNormalMonsterCard) || HasMatchingCard(player.Hand, IsNormalMonsterCard));
        }

        private void OpenMausoleumTargetChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown)
                {
                    options.Add(new CleanChoiceOption(i, monster.Definition.Name, "Target this face-up monster.", true, string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                MausoleumTargetChoicePromptId,
                "Mausoleum of White",
                "Choose 1 face-up monster you control.",
                options);
            Log("Mausoleum awaits a monster target.");
        }

        private void ResolveMausoleumTargetChoice(CleanPlayerState player, int zone)
        {
            if (zone < 0 || zone >= CleanPlayerState.ZoneCount || player.Monsters[zone] == null || player.Monsters[zone].FaceDown)
            {
                Log("Mausoleum failed: target is no longer face-up.");
                return;
            }

            CleanCard target = player.Monsters[zone];
            Action continueMausoleumEffect = () =>
            {
                int currentZone = IndexOfMonsterZone(player, target);
                if (currentZone < 0)
                {
                    Log("Mausoleum failed: its targeted monster left the field.");
                    return;
                }

                pendingMausoleumTargetZone = currentZone;
                OpenMausoleumNormalChoice(player);
            };

            if (TriggerMaidenTargetedByCardEffect(player, zone, continueMausoleumEffect))
            {
                return;
            }

            continueMausoleumEffect();
        }

        private void OpenMausoleumNormalChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            AddMausoleumNormalOptions(options, player.Hand, ChoiceSourceHand, "Send from hand to GY");
            AddMausoleumNormalOptions(options, player.Deck, ChoiceSourceDeck, "Send from Deck to GY");
            State.PendingChoice = new CleanChoicePrompt(
                MausoleumNormalChoicePromptId,
                "Mausoleum of White",
                "Choose 1 Normal Monster from your hand or Deck to send to the GY.",
                options);
            Log("Mausoleum awaits a Normal Monster choice.");
        }

        private void ResolveMausoleumNormalChoice(CleanPlayerState player, int encodedChoice)
        {
            if (pendingMausoleumTargetZone < 0 || pendingMausoleumTargetZone >= CleanPlayerState.ZoneCount || player.Monsters[pendingMausoleumTargetZone] == null)
            {
                Log("Mausoleum failed: target is gone.");
                ClearPendingEffectChoices();
                return;
            }

            CleanCard sent;
            if (!TryTakeMausoleumNormalChoice(player, encodedChoice, out sent))
            {
                Log("Mausoleum failed: chosen Normal Monster is no longer available.");
                ClearPendingEffectChoices();
                return;
            }

            ResolveMausoleumBoost(player, pendingMausoleumTargetZone, sent, encodedChoice < 0);
            ClearPendingEffectChoices();
        }

        private void ResolveMausoleumBoost(CleanPlayerState player, int targetZone, CleanCard sent, bool sentFromHand)
        {
            ResetCardForHiddenZone(sent);
            AddToGraveyard(player, sent, sentFromHand);
            int boost = Mathf.Max(0, sent.Definition.Level) * 100;
            player.Monsters[targetZone].AttackBonus += boost;
            player.Monsters[targetZone].DefenseBonus += boost;
            player.UsedOncePerTurn.Add(MausoleumBoostOnceKey);
            Log("Mausoleum sends " + sent.Definition.Name + " and boosts " + player.Monsters[targetZone].Definition.Name + " by " + boost + " ATK/DEF until end of turn.");
        }
        private List<CleanChoiceOption> BuildUniqueDeckChoiceOptions(CleanPlayerState player, Predicate<CleanCard> predicate, string detail)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            HashSet<int> seenIds = new HashSet<int>();
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard card = player.Deck[i];
                if (!predicate(card) || seenIds.Contains(card.Definition.Id))
                {
                    continue;
                }

                seenIds.Add(card.Definition.Id);
                options.Add(new CleanChoiceOption(card.Definition.Id, card.Definition.Name, detail, true, string.Empty));
            }

            return options;
        }

        private bool TryTakeDeckChoice(CleanPlayerState player, int selectedCardId, Predicate<CleanCard> predicate, out CleanCard card)
        {
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard candidate = player.Deck[i];
                if (candidate.Definition.Id == selectedCardId && predicate(candidate))
                {
                    player.Deck.RemoveAt(i);
                    card = candidate;
                    return true;
                }
            }

            card = null;
            return false;
        }

        private bool TryTakeFirstDiscard(CleanPlayerState player, out CleanCard card)
        {
            if (player.Hand.Count == 0)
            {
                card = null;
                return false;
            }

            card = player.Hand[0];
            player.Hand.RemoveAt(0);
            return true;
        }

        private bool LuaSpecialSummonBestBlueEyesFromDeck(CleanPlayerState player)
        {
            CleanCard card;
            if (!TryTakeBestMonster(
                    player.Deck,
                    candidate => IsBlueEyesMonster(candidate) &&
                                 CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Deck, CleanSummonMethod.Effect),
                    out card))
            {
                Log("Clean Blue-Eyes summon failed: no Blue-Eyes monster in Deck.");
                return false;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                player.Deck.Add(card);
                Log("Clean Blue-Eyes summon failed: no open Monster Zone.");
                return false;
            }

            SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
            Log("Clean special summons " + card.Definition.Name + " from Deck.");
            return true;
        }

        private bool IsWishesLevelOneLightTunerTarget(CleanCard card)
        {
            return card != null && IsLevelOneLightTuner(card);
        }

        private bool IsWishesSpellTrapTarget(CleanCard card)
        {
            if (card == null || card.Definition.Kind == CleanCardKind.Monster || card.Definition.Id == WishesForEyesOfBlueCardId)
            {
                return false;
            }

            string text = card.Definition.Text ?? string.Empty;
            return text.IndexOf("Blue-Eyes White Dragon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   card.Definition.Id == 17725109 ||
                   card.Definition.Id == 2783661 ||
                   card.Definition.Id == 71143015 ||
                   card.Definition.Id == 62089826;
        }

        private void AddMausoleumNormalOptions(List<CleanChoiceOption> options, List<CleanCard> cards, int source, string detail)
        {
            HashSet<int> seenIds = new HashSet<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCard card = cards[i];
                if (!IsNormalMonsterCard(card) || seenIds.Contains(card.Definition.Id))
                {
                    continue;
                }

                seenIds.Add(card.Definition.Id);
                options.Add(new CleanChoiceOption(EncodeCardChoice(source, card.Definition.Id), card.Definition.Name, detail, true, string.Empty));
            }
        }

        private bool TryTakeMausoleumNormalChoice(CleanPlayerState player, int encodedChoice, out CleanCard card)
        {
            int source;
            int cardId;
            DecodeCardChoice(encodedChoice, out source, out cardId);
            List<CleanCard> list = source == ChoiceSourceHand ? player.Hand : player.Deck;
            for (int i = 0; i < list.Count; i++)
            {
                CleanCard candidate = list[i];
                if (candidate.Definition.Id == cardId && IsNormalMonsterCard(candidate))
                {
                    list.RemoveAt(i);
                    card = candidate;
                    return true;
                }
            }

            card = null;
            return false;
        }

        private int EncodeCardChoice(int source, int cardId)
        {
            return source == ChoiceSourceHand ? -cardId : cardId;
        }

        private void DecodeCardChoice(int encodedChoice, out int source, out int cardId)
        {
            if (encodedChoice < 0)
            {
                source = ChoiceSourceHand;
                cardId = -encodedChoice;
                return;
            }

            source = ChoiceSourceDeck;
            cardId = encodedChoice;
        }

        private void ClearPendingEffectChoices()
        {
            pendingMausoleumTargetZone = -1;
            pendingSpiritZone = -1;
            pendingLuaDestroyTargetPlayerId = CleanPlayerId.Rival;
            pendingLuaDestroyRemaining = 0;
            pendingLuaDestroySource = string.Empty;
            pendingLuaDestroyFaceUpCards = false;
            pendingLuaDestroyMayStop = false;
            pendingBingoRuntimeIds.Clear();
            pendingWishesEquipTargetZone = -1;
            pendingDictatorCard = null;
            pendingPendulumSummonCount = 0;
            ClearPendingFusionSummonChoice();
            pendingRivalAttackerZone = -1;
            pendingJetAttackerZone = -1;
            pendingJetDefenderZone = -1;
        }
        private bool OpenLocalQuickEffectWindowForRivalTurn()
        {
            List<CleanChoiceOption> options = BuildLocalQuickEffectOptions();
            if (options.Count == 0)
            {
                return false;
            }

            options.Add(new CleanChoiceOption(
                QuickEffectPassOptionId,
                "Do not activate",
                "Pass this response window without activating a card or effect.",
                true,
                string.Empty));

            State.PendingChoice = new CleanChoicePrompt(
                QuickEffectChoicePromptId,
                "Activate a card or effect?",
                "During the opponent's turn, choose a legal Trap, Quick-Play Spell, or Quick Effect to activate.",
                options,
                CleanPromptKind.CardEffect,
                false);
            Log("Card/effect response window opened for the opponent's turn.");
            return true;
        }

        private List<CleanChoiceOption> BuildLocalQuickEffectOptions()
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < State.Local.Hand.Count; i++)
            {
                CleanCard card = State.Local.Hand[i];
                if (!CanActivateLocalQuickEffectHandCardInternal(i, true))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    EncodeQuickEffectChoice(QuickEffectChoiceHandSource, i),
                    card.Definition.Name,
                    "Activate from hand. Spell Speed 2.",
                    true,
                    string.Empty));
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = State.Local.Monsters[i];
                if (!CanActivateLocalQuickEffectMonsterZoneCardInternal(i, true))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    EncodeQuickEffectChoice(QuickEffectChoiceMonsterSource, i),
                    card.Definition.Name,
                    "Activate from field. Spell Speed 2.",
                    true,
                    string.Empty));
            }

            int extraZone = ExtraMonsterZoneIndexFor(State.Local);
            if (CanActivateLocalQuickEffectExtraMonsterZoneInternal(extraZone, true))
            {
                CleanCard card = State.ExtraMonsters[extraZone];
                options.Add(new CleanChoiceOption(
                    EncodeQuickEffectChoice(QuickEffectChoiceExtraMonsterSource, extraZone),
                    card.Definition.Name,
                    "Activate from the Extra Monster Zone. Spell Speed 2.",
                    true,
                    string.Empty));
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (!CanActivateLocalSpellTrapResponseInternal(i, true))
                {
                    continue;
                }

                CleanCard card = State.Local.SpellsTraps[i];
                options.Add(new CleanChoiceOption(
                    EncodeQuickEffectChoice(QuickEffectChoiceSpellTrapSource, i),
                    "Activate " + card.Definition.Name,
                    card.FaceDown ? "Flip and activate this Set card." : "Activate this face-up card's effect.",
                    true,
                    string.Empty));
            }

            return options;
        }

        private void ResolveQuickEffectChoice(int optionId)
        {
            State.PendingChoice = null;
            if (optionId == QuickEffectPassOptionId)
            {
                pendingRivalTurnContinuationAfterResponse = false;
                Log("Card/effect response passed.");
                ContinueRivalTurnAfterQuickWindow();
                return;
            }

            pendingRivalTurnContinuationAfterResponse = true;
            int source;
            int index;
            DecodeQuickEffectChoice(optionId, out source, out index);
            bool resolved;
            if (source == QuickEffectChoiceHandSource)
            {
                resolved = ResolveLocalQuickEffectFromHand(index, true);
            }
            else if (source == QuickEffectChoiceExtraMonsterSource)
            {
                resolved = ResolveLocalQuickEffectFromExtraMonsterZone(index, true);
            }
            else if (source == QuickEffectChoiceSpellTrapSource)
            {
                resolved = ResolveLocalSpellTrapResponse(index, true);
            }
            else
            {
                resolved = ResolveLocalQuickEffectFromMonsterZone(index, true);
            }
            if (!resolved)
            {
                Log("Quick Effect could not resolve from the selected card.");
            }

            ContinueRivalTurnAfterResponseIfReady();
        }

        private int EncodeQuickEffectChoice(int source, int index)
        {
            return source * 100 + index;
        }

        private void DecodeQuickEffectChoice(int encodedChoice, out int source, out int index)
        {
            source = encodedChoice / 100;
            index = encodedChoice % 100;
        }

        private bool CanActivateLocalSpellTrapResponseInternal(int zoneIndex, bool allowPending)
        {
            if (State.TurnPlayer != CleanPlayerId.Rival ||
                zoneIndex < 0 ||
                zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.SpellsTraps[zoneIndex];
            return card != null &&
                   (card.Definition.Kind == CleanCardKind.Trap || IsQuickPlaySpell(card)) &&
                   CanActivateSetCardInternal(zoneIndex, allowPending);
        }

        private bool ResolveLocalSpellTrapResponse(int zoneIndex, bool allowPending)
        {
            if (!CanActivateLocalSpellTrapResponseInternal(zoneIndex, allowPending))
            {
                return false;
            }

            CleanCard card = State.Local.SpellsTraps[zoneIndex];
            ActivateSetCard(zoneIndex);
            Log(card.Definition.Name + " was activated during the opponent's turn.");
            return true;
        }

        private bool CanActivateLocalQuickEffectHandCardInternal(int handIndex, bool allowPending)
        {
            if (State.GameOver || (!allowPending && State.PendingChoice != null) || handIndex < 0 || handIndex >= State.Local.Hand.Count)
            {
                return false;
            }

            CleanCard card = State.Local.Hand[handIndex];
            return IsQuickEffect(card) && CanActivateCleanCard(card, State.Local, CleanActivationSource.Hand);
        }

        private bool CanActivateLocalQuickEffectMonsterZoneCardInternal(int zoneIndex, bool allowPending)
        {
            if (State.GameOver || (!allowPending && State.PendingChoice != null) || zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            return card != null && !card.FaceDown && IsQuickEffect(card) && CanActivateCleanCard(card, State.Local, CleanActivationSource.MonsterZone);
        }

        private bool ResolveLocalQuickEffectFromHand(int handIndex, bool allowPending)
        {
            if (!CanActivateLocalQuickEffectHandCardInternal(handIndex, allowPending))
            {
                return false;
            }

            CleanCard card = State.Local.Hand[handIndex];
            State.Local.Hand.RemoveAt(handIndex);
            if (ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.Hand) && !object.ReferenceEquals(State.Local.FieldSpell, card))
            {
                AddToGraveyard(State.Local, card, true);
            }

            CheckWin();
            return true;
        }

        private bool ResolveLocalQuickEffectFromMonsterZone(int zoneIndex, bool allowPending)
        {
            if (!CanActivateLocalQuickEffectMonsterZoneCardInternal(zoneIndex, allowPending))
            {
                return false;
            }

            CleanCard card = State.Local.Monsters[zoneIndex];
            bool sendsSelf = ShouldSendSelfForActivatedMonsterEffect(card, State.Local, State.Rival);
            if (sendsSelf)
            {
                State.Local.Monsters[zoneIndex] = null;
                card.FaceDown = false;
                card.AttackBonus = 0;
                card.DefenseBonus = 0;
                card.HasBattled = false;
            }

            if (ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.MonsterZone) && sendsSelf)
            {
                AddToGraveyard(State.Local, card, true);
            }

            CheckWin();
            return true;
        }

        private bool CanActivateLocalQuickEffectExtraMonsterZoneInternal(int extraZone, bool allowPending)
        {
            if (State.GameOver ||
                (!allowPending && State.PendingChoice != null) ||
                extraZone != ExtraMonsterZoneIndexFor(State.Local))
            {
                return false;
            }

            CleanCard card = State.ExtraMonsters[extraZone];
            return card != null &&
                   !card.FaceDown &&
                   IsQuickEffect(card) &&
                   CanActivateCleanCard(card, State.Local, CleanActivationSource.MonsterZone);
        }

        private bool ResolveLocalQuickEffectFromExtraMonsterZone(int extraZone, bool allowPending)
        {
            if (!CanActivateLocalQuickEffectExtraMonsterZoneInternal(extraZone, allowPending))
            {
                return false;
            }

            CleanCard card = State.ExtraMonsters[extraZone];
            ResolveEffect(State.Local, State.Rival, card, CleanActivationSource.MonsterZone);
            CheckWin();
            return true;
        }
        private bool ShouldSendSelfForActivatedMonsterEffect(CleanCard card, CleanPlayerState player, CleanPlayerState rival)
        {
            return card != null &&
                   (card.Definition.Id == MaidenOfWhiteCardId ||
                    card.Definition.Id == GhostOgreSnowRabbitCardId ||
                    (card.Definition.Id == DragonSpiritOfWhiteCardId && CanDragonSpiritTributeForBlueEyes(player, rival)));
        }

        public bool IsQuickEffect(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster || string.IsNullOrEmpty(card.Definition.EffectKey) || IsNormalMonster(card))
            {
                return false;
            }

            switch (card.Definition.EffectKey)
            {
                case "maxx_c_clean":
                case "veiler_clean":
                case "ash_clean":
                case "ghost_ogre_clean":
                case "dragon_spirit_white_clean":
                case "magia_clean":
                case "ultimate_spirit_clean":
                case "spirit_dragon_clean":
                    return true;
                default:
                    return false;
            }
        }

        public bool HasContinuousEffect(CleanCard card)
        {
            if (card == null || string.IsNullOrEmpty(card.Definition.EffectKey) || IsNormalMonster(card))
            {
                return false;
            }

            switch (card.Definition.EffectKey)
            {
                case "jet_clean":
                case "true_light_clean":
                case "mausoleum_clean":
                case "gateway_chaos_clean":
                case "spirit_with_eyes_clean":
                    return true;
                default:
                    return false;
            }
        }

        private bool IsTriggerOnlyEffect(CleanCard card)
        {
            if (card == null || string.IsNullOrEmpty(card.Definition.EffectKey))
            {
                return false;
            }

            switch (card.Definition.EffectKey)
            {
                case "maiden_with_eyes_clean":
                case "stone_ancients_clean":
                    return true;
                default:
                    return false;
            }
        }

        private bool IsQuickPlaySpell(CleanCard card)
        {
            return card != null &&
                   card.Definition.Kind == CleanCardKind.Spell &&
                   (card.Definition.TypeLine ?? string.Empty).IndexOf("Quick-Play", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool ResolveFlipTrigger(
            CleanPlayerState player,
            CleanPlayerState rival,
            CleanCard card,
            string reason,
            Action continuation = null)
        {
            if (!IsFlipTriggerEffect(card) || card.EffectsNegated)
            {
                return false;
            }

            if (player.Id != CleanPlayerId.Local)
            {
                ResolveFlipTriggerNow(player, rival, card, reason);
                return false;
            }

            string pendingKey = "flip_trigger:" + card.RuntimeId;
            if (!TryReserveTriggerPrompt(player, pendingKey))
            {
                return false;
            }

            string text = card.Definition.Text ?? string.Empty;
            bool optional = text.IndexOf("you can", StringComparison.OrdinalIgnoreCase) >= 0;
            QueueTriggerEffectPrompt(
                player,
                card.Definition.Name,
                card.Definition.Name + " was " + reason + ". Resolve its FLIP Trigger Effect?",
                optional,
                () =>
                {
                    ReleaseTriggerPrompt(player, pendingKey);
                    ResolveFlipTriggerNow(player, rival, card, reason);
                    if (continuation != null)
                    {
                        continuation();
                    }
                },
                () =>
                {
                    ReleaseTriggerPrompt(player, pendingKey);
                    Log(card.Definition.Name + "'s optional FLIP Trigger Effect was not activated.");
                    if (continuation != null)
                    {
                        continuation();
                    }
                });
            return true;
        }

        private void ResolveFlipTriggerNow(CleanPlayerState player, CleanPlayerState rival, CleanCard card, string reason)
        {
            Log(card.Definition.Name + " activates its FLIP Trigger Effect after it was " + reason + ".");
            string text = card.Definition.Text ?? string.Empty;
            if (string.Equals(card.Definition.EffectKey, "flip_draw_clean", StringComparison.OrdinalIgnoreCase) ||
                text.IndexOf("draw", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                int before = player.Hand.Count;
                Draw(player, 1);
                Log(card.Definition.Name + " resolves its FLIP effect and draws " + (player.Hand.Count - before) + " card.");
                return;
            }

            if (text.IndexOf("destroy", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                DestroyStrongestMonster(rival);
                Log(card.Definition.Name + " resolves its FLIP effect and destroys a monster.");
            }
        }

        private void ResolveLinkSummonTrigger(CleanPlayerState player, CleanCard linkMonster)
        {
            if (linkMonster == null || linkMonster.Definition.Id != SpiritWithEyesOfBlueCardId || !CanUseSpiritMausoleumEffect(player, linkMonster))
            {
                return;
            }

            pendingSpiritZone = FindSpiritZone(player, linkMonster);
            OpenSpiritMausoleumDestinationChoice(CleanPromptKind.TriggerEffect);
            Log("Spirit with Eyes of Blue triggers after being Link Summoned.");
        }

        private bool TryPlanLinkSummon(CleanPlayerState player, int extraDeckIndex, out List<int> materialZones, out bool summonToExtraMonsterZone, out int targetMainZone)
        {
            materialZones = null;
            summonToExtraMonsterZone = false;
            targetMainZone = -1;
            if (extraDeckIndex < 0 || extraDeckIndex >= player.ExtraDeck.Count || !IsLinkMonsterDefinition(player.ExtraDeck[extraDeckIndex].Definition))
            {
                return false;
            }

            CleanCard linkMonster = player.ExtraDeck[extraDeckIndex];
            if (!CanSpecialSummonCard(player, linkMonster, CleanSummonOrigin.ExtraDeck, CleanSummonMethod.Link))
            {
                return false;
            }

            if (!TrySelectLinkMaterials(player, linkMonster, out materialZones))
            {
                return false;
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            if (State.ExtraMonsters[extraZone] == null || MaterialZonesClearExtraMonsterZone(player, materialZones))
            {
                summonToExtraMonsterZone = true;
                return true;
            }

            targetMainZone = FirstLinkedExtraDeckMainMonsterZone(player, materialZones);
            return targetMainZone >= 0;
        }

        private bool TrySelectLinkMaterials(CleanPlayerState player, CleanCard linkMonster, out List<int> materialZones)
        {
            materialZones = new List<int>();
            int requiredMaterials = LinkRating(linkMonster);
            if (requiredMaterials <= 0)
            {
                return false;
            }

            if (linkMonster.Definition.Id == SpiritWithEyesOfBlueCardId)
            {
                List<int> zones = MaterialCandidateZones(player);
                for (int i = 0; i < zones.Count; i++)
                {
                    CleanCard material = MaterialCardAtZone(player, zones[i]);
                    if (IsSpiritWithEyesLinkMaterial(material))
                    {
                        materialZones.Add(zones[i]);
                        return true;
                    }
                }

                return false;
            }

            List<int> candidateZones = MaterialCandidateZones(player);
            for (int i = 0; i < candidateZones.Count && materialZones.Count < requiredMaterials; i++)
            {
                CleanCard material = MaterialCardAtZone(player, candidateZones[i]);
                if (material != null && !material.FaceDown && material.Definition.Kind == CleanCardKind.Monster)
                {
                    materialZones.Add(candidateZones[i]);
                }
            }

            return materialZones.Count == requiredMaterials;
        }

        private bool IsSpiritWithEyesLinkMaterial(CleanCard material)
        {
            if (material == null ||
                material.FaceDown ||
                material.Definition.Kind != CleanCardKind.Monster ||
                material.Definition.Level <= 0 ||
                material.Definition.Level > 4)
            {
                return false;
            }

            string typeLine = material.Definition.TypeLine ?? string.Empty;
            return IsDragonMonster(material) || typeLine.IndexOf("Spellcaster", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int LinkRating(CleanCard linkMonster)
        {
            if (!IsLinkMonsterDefinition(linkMonster != null ? linkMonster.Definition : null))
            {
                return 0;
            }

            if (linkMonster.Definition.Id == SpiritWithEyesOfBlueCardId)
            {
                return 1;
            }

            return Mathf.Max(1, linkMonster.Definition.LinkRating);
        }

        private void MoveLinkMaterialsToGrave(CleanPlayerState player, List<int> materialZones)
        {
            for (int i = 0; i < materialZones.Count; i++)
            {
                MoveMaterialZoneToGrave(player, materialZones[i]);
            }
        }

        private void PrepareExtraDeckMonsterSummon(CleanPlayerState player, CleanCard card)
        {
            card.Controller = player.Id;
            card.FaceDown = false;
            card.Stance = CleanMonsterStance.Attack;
            card.HasBattled = false;
            card.SummonedThisTurn = true;
            card.PositionChangedThisTurn = false;
        }

        private int ExtraMonsterZoneIndexFor(CleanPlayerState player)
        {
            return player.Id == CleanPlayerId.Local ? 0 : 1;
        }

        private List<int> MaterialCandidateZones(CleanPlayerState player)
        {
            List<int> zones = new List<int>(CleanPlayerState.ZoneCount + 1);
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                zones.Add(i);
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            if (State.ExtraMonsters[extraZone] != null)
            {
                zones.Add(ExtraSpiritZoneOffset + extraZone);
            }

            return zones;
        }

        private CleanCard MaterialCardAtZone(CleanPlayerState player, int encodedZone)
        {
            if (encodedZone >= 0 && encodedZone < CleanPlayerState.ZoneCount)
            {
                return player.Monsters[encodedZone];
            }

            int extraZone = encodedZone - ExtraSpiritZoneOffset;
            return extraZone == ExtraMonsterZoneIndexFor(player) ? State.ExtraMonsters[extraZone] : null;
        }

        private bool MaterialZonesClearExtraMonsterZone(CleanPlayerState player, List<int> encodedZones)
        {
            return encodedZones != null && encodedZones.Contains(ExtraSpiritZoneOffset + ExtraMonsterZoneIndexFor(player));
        }

        private void MoveMaterialZoneToGrave(CleanPlayerState player, int encodedZone)
        {
            if (encodedZone >= 0 && encodedZone < CleanPlayerState.ZoneCount)
            {
                MoveMonsterToGrave(player, encodedZone);
                return;
            }

            int extraZone = encodedZone - ExtraSpiritZoneOffset;
            if (extraZone != ExtraMonsterZoneIndexFor(player) || State.ExtraMonsters[extraZone] == null)
            {
                return;
            }

            CleanCard material = State.ExtraMonsters[extraZone];
            State.ExtraMonsters[extraZone] = null;
            SendEquipsForLeavingMonster(player, material);
            ResetCardForHiddenZone(material);
            SendFieldCardToGraveOrFaceUpExtra(player, material, true);
        }

        private int FirstLinkedExtraDeckMainMonsterZone(CleanPlayerState player)
        {
            return FirstLinkedExtraDeckMainMonsterZone(player, null);
        }

        private int FirstLinkedExtraDeckMainMonsterZone(CleanPlayerState player, List<int> zonesClearedBySummon)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if ((player.Monsters[i] == null || ContainsZone(zonesClearedBySummon, i)) && IsMainMonsterZoneLinkedForExtraDeckSummon(player, i))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool ContainsZone(List<int> zones, int zone)
        {
            return zones != null && zones.Contains(zone);
        }

        private bool IsMainMonsterZoneLinkedForExtraDeckSummon(CleanPlayerState player, int zoneIndex)
        {
            if (zoneIndex < 0 || zoneIndex >= CleanPlayerState.ZoneCount)
            {
                return false;
            }

            CleanCard extraLink = State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)];
            if (extraLink == null || extraLink.FaceDown || extraLink.EffectsNegated || !IsLinkMonsterDefinition(extraLink.Definition))
            {
                return false;
            }

            return zoneIndex == 2;
        }

        private bool TryPlanXyzSummon(CleanPlayerState player, int extraDeckIndex, out List<int> materialZones, out bool summonToExtraMonsterZone, out int targetMainZone)
        {
            materialZones = null;
            summonToExtraMonsterZone = false;
            targetMainZone = -1;
            if (extraDeckIndex < 0 || extraDeckIndex >= player.ExtraDeck.Count || !IsXyzMonsterDefinition(player.ExtraDeck[extraDeckIndex].Definition))
            {
                return false;
            }

            CleanCard xyzMonster = player.ExtraDeck[extraDeckIndex];
            if (!CanSpecialSummonCard(player, xyzMonster, CleanSummonOrigin.ExtraDeck, CleanSummonMethod.Xyz))
            {
                return false;
            }

            if (!TrySelectXyzMaterials(player, xyzMonster, out materialZones))
            {
                return false;
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            if (State.ExtraMonsters[extraZone] == null || MaterialZonesClearExtraMonsterZone(player, materialZones))
            {
                summonToExtraMonsterZone = true;
                return true;
            }

            targetMainZone = FirstLinkedExtraDeckMainMonsterZone(player, materialZones);
            return targetMainZone >= 0;
        }

        private bool TrySelectXyzMaterials(CleanPlayerState player, CleanCard xyzMonster, out List<int> materialZones)
        {
            materialZones = new List<int>();
            int rank = XyzRank(xyzMonster);
            int requiredMaterials = XyzMaterialCount(xyzMonster);
            if (rank <= 0 || requiredMaterials <= 0)
            {
                return false;
            }

            List<int> candidateZones = MaterialCandidateZones(player);
            for (int i = 0; i < candidateZones.Count && materialZones.Count < requiredMaterials; i++)
            {
                CleanCard material = MaterialCardAtZone(player, candidateZones[i]);
                if (IsXyzMaterialCandidate(material, rank))
                {
                    materialZones.Add(candidateZones[i]);
                }
            }

            return materialZones.Count == requiredMaterials;
        }

        private bool IsXyzMaterialCandidate(CleanCard material, int requiredLevel)
        {
            return material != null &&
                   !material.FaceDown &&
                   material.Definition.Kind == CleanCardKind.Monster &&
                   !IsLinkMonsterDefinition(material.Definition) &&
                   !IsXyzMonsterDefinition(material.Definition) &&
                   material.CurrentLevel == requiredLevel;
        }

        private int XyzRank(CleanCard xyzMonster)
        {
            if (!IsXyzMonsterDefinition(xyzMonster != null ? xyzMonster.Definition : null))
            {
                return 0;
            }

            return Mathf.Max(0, xyzMonster.Definition.Rank);
        }

        private int XyzMaterialCount(CleanCard xyzMonster)
        {
            if (xyzMonster == null)
            {
                return 0;
            }

            int parsed = ParseLeadingMaterialCount(xyzMonster.Definition.Text);
            if (parsed > 0)
            {
                return parsed;
            }

            parsed = ParseLeadingMaterialCount(xyzMonster.Definition.TypeLine);
            return parsed > 0 ? parsed : 2;
        }

        private int ParseLeadingMaterialCount(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int levelIndex = text.IndexOf("Level", StringComparison.OrdinalIgnoreCase);
            if (levelIndex < 0)
            {
                return 0;
            }

            int value = 0;
            bool found = false;
            for (int i = Mathf.Max(0, levelIndex - 4); i < levelIndex; i++)
            {
                if (!char.IsDigit(text[i]))
                {
                    if (found)
                    {
                        break;
                    }

                    continue;
                }

                found = true;
                value = value * 10 + (text[i] - '0');
            }

            return found ? value : 0;
        }

        private void MoveXyzMaterialsUnderMonster(CleanPlayerState player, List<int> materialZones, CleanCard xyzMonster)
        {
            xyzMonster.XyzMaterials.Clear();
            for (int i = 0; i < materialZones.Count; i++)
            {
                int zone = materialZones[i];
                CleanCard material = MaterialCardAtZone(player, zone);
                if (material == null)
                {
                    continue;
                }

                if (zone >= 0 && zone < CleanPlayerState.ZoneCount)
                {
                    player.Monsters[zone] = null;
                }
                else
                {
                    int extraZone = zone - ExtraSpiritZoneOffset;
                    State.ExtraMonsters[extraZone] = null;
                    SendEquipsForLeavingMonster(player, material);
                }

                SendXyzMaterialsToGrave(player, material);
                ResetCardForHiddenZone(material);
                material.Controller = player.Id;
                material.FaceDown = false;
                xyzMonster.XyzMaterials.Add(material);
            }
        }

        private bool DetachXyzMaterial(CleanPlayerState player, CleanCard xyzMonster)
        {
            if (player == null || xyzMonster == null || !IsXyzMonsterDefinition(xyzMonster.Definition) || xyzMonster.XyzMaterials.Count == 0)
            {
                return false;
            }

            CleanCard material = xyzMonster.XyzMaterials[0];
            xyzMonster.XyzMaterials.RemoveAt(0);
            ResetCardForHiddenZone(material);
            AddToGraveyard(player, material, false);
            Log(xyzMonster.Definition.Name + " detaches " + material.Definition.Name + " as Xyz Material.");
            return true;
        }

        private void SendXyzMaterialsToGrave(CleanPlayerState player, CleanCard xyzMonster)
        {
            if (player == null || xyzMonster == null || xyzMonster.XyzMaterials.Count == 0)
            {
                return;
            }

            int count = xyzMonster.XyzMaterials.Count;
            while (xyzMonster.XyzMaterials.Count > 0)
            {
                CleanCard material = xyzMonster.XyzMaterials[0];
                xyzMonster.XyzMaterials.RemoveAt(0);
                ResetCardForHiddenZone(material);
                AddToGraveyard(player, material, false);
            }

            Log(count + " Xyz Material(s) from " + xyzMonster.Definition.Name + " are sent to the GY.");
        }

        private bool IsXyzMonsterDefinition(CleanCardDefinition definition)
        {
            return definition != null && (definition.TypeLine ?? string.Empty).IndexOf("Xyz", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private bool IsLinkMonsterDefinition(CleanCardDefinition definition)
        {
            return definition != null && (definition.TypeLine ?? string.Empty).IndexOf("Link", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsFlipTriggerEffect(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            string text = card.Definition.Text ?? string.Empty;
            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return text.TrimStart().StartsWith("FLIP:", StringComparison.OrdinalIgnoreCase) ||
                   typeLine.IndexOf("Flip", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   string.Equals(card.Definition.EffectKey, "flip_draw_clean", StringComparison.OrdinalIgnoreCase);
        }
        private bool CanPendulumSummon(CleanPlayerState player)
        {
            return player != null &&
                   !player.PendulumSummonUsed &&
                   HasPendulumScales(player) &&
                   HasPendulumSummonTarget(player);
        }

        private bool HasPendulumScales(CleanPlayerState player)
        {
            int left;
            int right;
            return TryGetPendulumScales(player, out left, out right);
        }

        private bool TryGetPendulumScales(CleanPlayerState player, out int leftScale, out int rightScale)
        {
            leftScale = -1;
            rightScale = -1;
            if (player == null || !IsPendulumMonster(player.SpellsTraps[0]) || !IsPendulumMonster(player.SpellsTraps[CleanPlayerState.ZoneCount - 1]))
            {
                return false;
            }

            leftScale = PendulumScale(player.SpellsTraps[0]);
            rightScale = PendulumScale(player.SpellsTraps[CleanPlayerState.ZoneCount - 1]);
            return leftScale >= 0 && rightScale >= 0 && leftScale != rightScale;
        }

        private int FirstAvailablePendulumZone(CleanPlayerState player)
        {
            if (player == null)
            {
                return -1;
            }

            if (player.SpellsTraps[0] == null)
            {
                return 0;
            }

            int right = CleanPlayerState.ZoneCount - 1;
            return player.SpellsTraps[right] == null ? right : -1;
        }

        private bool HasPendulumSummonTarget(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = BuildPendulumSummonOptions(player);
            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].Id != PendulumFinishOptionId && options[i].IsAvailable)
                {
                    return true;
                }
            }

            return false;
        }

        private void OpenPendulumSummonChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = BuildPendulumSummonOptions(player);
            options.Add(new CleanChoiceOption(PendulumFinishOptionId, "Finish", "Stop choosing Pendulum Summon targets.", true, string.Empty));
            State.PendingChoice = new CleanChoicePrompt(
                PendulumSummonChoicePromptId,
                "Pendulum Summon",
                "Choose monsters with Levels between your Pendulum Scales.",
                options);
            Log(player.Name + " declares a Pendulum Summon.");
        }

        private List<CleanChoiceOption> BuildPendulumSummonOptions(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            int leftScale;
            int rightScale;
            if (!TryGetPendulumScales(player, out leftScale, out rightScale))
            {
                return options;
            }

            bool blockedBySpiritDragon = pendingPendulumSummonCount >= 1 && IsSimultaneousSpecialSummonLimitedBySpiritDragon();
            int mainZone = FirstEmpty(player.Monsters);
            for (int i = 0; i < player.Hand.Count; i++)
            {
                CleanCard card = player.Hand[i];
                if (blockedBySpiritDragon || mainZone < 0 || !CanPendulumSummonFromHand(player, card, leftScale, rightScale))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    card.RuntimeId,
                    card.Definition.Name,
                    "Pendulum | Level " + card.Definition.Level + " | from hand | ATK " + card.CurrentAttack,
                    true,
                    string.Empty,
                    card.Definition.Id));
            }

            int extraTargetZone = FirstPendulumExtraDeckTargetZone(player);
            for (int i = 0; i < player.ExtraDeck.Count; i++)
            {
                CleanCard card = player.ExtraDeck[i];
                if (!card.FaceUpExtraDeck || !IsPendulumMonster(card))
                {
                    continue;
                }

                if (blockedBySpiritDragon || extraTargetZone < 0 || !CanPendulumSummonFromFaceUpExtraDeck(player, card, leftScale, rightScale))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    card.RuntimeId,
                    card.Definition.Name,
                    "Pendulum | Level " + card.Definition.Level + " | face-up Extra Deck | ATK " + card.CurrentAttack,
                    true,
                    string.Empty,
                    card.Definition.Id));
            }

            return options;
        }

        private void ResolvePendulumSummonChoice(CleanPlayerState player, int optionId)
        {
            if (optionId == PendulumFinishOptionId)
            {
                State.PendingChoice = null;
                Log(pendingPendulumSummonCount > 0 ? "Pendulum Summon selection finished." : "Pendulum Summon selection cancelled.");
                return;
            }

            int leftScale;
            int rightScale;
            if (!TryGetPendulumScales(player, out leftScale, out rightScale))
            {
                State.PendingChoice = null;
                return;
            }

            if (FindCardByRuntimeId(player.Hand, optionId) != null)
            {
                ResolvePendulumSummonFromHand(player, optionId, leftScale, rightScale);
            }
            else if (FindCardByRuntimeId(player.ExtraDeck, optionId) != null)
            {
                ResolvePendulumSummonFromExtraDeck(player, optionId, leftScale, rightScale);
            }

            if (HasPendulumSummonTarget(player))
            {
                OpenPendulumSummonChoice(player);
            }
            else
            {
                State.PendingChoice = null;
            }
        }

        private void ResolvePendulumSummonFromHand(CleanPlayerState player, int runtimeId, int leftScale, int rightScale)
        {
            CleanCard card = FindCardByRuntimeId(player.Hand, runtimeId);
            if (card == null)
            {
                return;
            }

            if (!CanPendulumSummonFromHand(player, card, leftScale, rightScale))
            {
                return;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                return;
            }

            player.Hand.Remove(card);
            bool firstPendulumMonster = pendingPendulumSummonCount == 0;
            pendingPendulumSummonCount++;
            player.PendulumSummonUsed = true;
            SummonToZone(
                player,
                card,
                zone,
                false,
                CleanMonsterStance.Attack,
                false,
                firstPendulumMonster ? CleanSummonPresentationKind.Pendulum : CleanSummonPresentationKind.None);
            Log(player.Name + " Pendulum Summons " + card.Definition.Name + " from the hand.");
        }

        private void ResolvePendulumSummonFromExtraDeck(CleanPlayerState player, int runtimeId, int leftScale, int rightScale)
        {
            CleanCard card = FindCardByRuntimeId(player.ExtraDeck, runtimeId);
            if (card == null)
            {
                return;
            }

            if (!CanPendulumSummonFromFaceUpExtraDeck(player, card, leftScale, rightScale))
            {
                return;
            }

            int targetZone = FirstPendulumExtraDeckTargetZone(player);
            if (targetZone < 0)
            {
                return;
            }

            player.ExtraDeck.Remove(card);
            bool firstPendulumMonster = pendingPendulumSummonCount == 0;
            pendingPendulumSummonCount++;
            player.PendulumSummonUsed = true;
            PrepareExtraDeckMonsterSummon(player, card);
            card.FaceUpExtraDeck = false;
            int extraMonsterZone = ExtraMonsterZoneIndexFor(player);
            if (targetZone == ExtraSpiritZoneOffset + extraMonsterZone)
            {
                State.ExtraMonsters[extraMonsterZone] = card;
                NotifySpecialSummonForCurrentEvent(player);
                RequestSummonCinematics(
                    player,
                    card,
                    firstPendulumMonster ? CleanSummonPresentationKind.Pendulum : CleanSummonPresentationKind.None);
                Log(player.Name + " Pendulum Summons " + card.Definition.Name + " face-up from the Extra Deck to the Extra Monster Zone.");
            }
            else
            {
                SummonToZone(
                    player,
                    card,
                    targetZone,
                    false,
                    CleanMonsterStance.Attack,
                    false,
                    firstPendulumMonster ? CleanSummonPresentationKind.Pendulum : CleanSummonPresentationKind.None);
                Log(player.Name + " Pendulum Summons " + card.Definition.Name + " face-up from the Extra Deck to a linked Main Monster Zone.");
            }
        }

        private bool IsSimultaneousSpecialSummonLimitedBySpiritDragon()
        {
            return FirstFaceUpBlueEyesSpiritDragonZone(State.Local) >= 0 ||
                   FirstFaceUpBlueEyesSpiritDragonZone(State.Rival) >= 0;
        }
        private bool CanPendulumSummonFromHand(CleanPlayerState player, CleanCard card, int leftScale, int rightScale)
        {
            return card != null &&
                   card.Definition.Kind == CleanCardKind.Monster &&
                   CardLevelWithinPendulumScales(card, leftScale, rightScale) &&
                   !IsLinkMonsterDefinition(card.Definition) &&
                   CanSpecialSummonUnderSpiritRestriction(player, card);
        }

        private bool CanPendulumSummonFromFaceUpExtraDeck(CleanPlayerState player, CleanCard card, int leftScale, int rightScale)
        {
            return card != null &&
                   card.FaceUpExtraDeck &&
                   IsPendulumMonster(card) &&
                   CardLevelWithinPendulumScales(card, leftScale, rightScale) &&
                   CanSpecialSummonFromExtraUnderTurnRestriction(player, card) &&
                   CanSpecialSummonUnderSpiritRestriction(player, card);
        }

        private int FirstPendulumExtraDeckTargetZone(CleanPlayerState player)
        {
            int extraZone = ExtraMonsterZoneIndexFor(player);
            if (State.ExtraMonsters[extraZone] == null)
            {
                return ExtraSpiritZoneOffset + extraZone;
            }

            return FirstLinkedExtraDeckMainMonsterZone(player);
        }

        private bool CardLevelWithinPendulumScales(CleanCard card, int leftScale, int rightScale)
        {
            int low = Mathf.Min(leftScale, rightScale);
            int high = Mathf.Max(leftScale, rightScale);
            return card != null && card.Definition.Level > low && card.Definition.Level < high;
        }

        private bool IsPendulumMonster(CleanCard card)
        {
            return card != null && IsPendulumMonsterDefinition(card.Definition);
        }

        private bool IsPendulumMonsterDefinition(CleanCardDefinition definition)
        {
            return definition != null && (definition.TypeLine ?? string.Empty).IndexOf("Pendulum", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int PendulumScale(CleanCard card)
        {
            if (card == null)
            {
                return -1;
            }

            int scale = ParsePendulumScale(card.Definition.TypeLine);
            return scale >= 0 ? scale : ParsePendulumScale(card.Definition.Text);
        }

        private int ParsePendulumScale(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return -1;
            }

            int index = text.IndexOf("Scale", StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return -1;
            }

            for (int i = index + 5; i < text.Length; i++)
            {
                if (!char.IsDigit(text[i]))
                {
                    continue;
                }

                int value = 0;
                while (i < text.Length && char.IsDigit(text[i]))
                {
                    value = value * 10 + (text[i] - '0');
                    i++;
                }

                return value;
            }

            return -1;
        }
        private bool ActivateBingoMachine(CleanPlayerState player)
        {
            if (CountMatchingCards(player.Deck, IsBingoSearchCandidate) < 3)
            {
                Log("Bingo Machine, Go!!! needs 3 valid cards in the Deck.");
                return false;
            }

            if (player.Id == CleanPlayerId.Local)
            {
                pendingBingoRuntimeIds.Clear();
                OpenBingoChoice(player);
                return true;
            }

            List<CleanCard> revealed = new List<CleanCard>();
            for (int i = 0; i < player.Deck.Count && revealed.Count < 3; i++)
            {
                if (IsBingoSearchCandidate(player.Deck[i]))
                {
                    revealed.Add(player.Deck[i]);
                }
            }

            return ResolveBingoReveal(player, revealed);
        }

        private void OpenBingoChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard card = player.Deck[i];
                if (!IsBingoSearchCandidate(card) || pendingBingoRuntimeIds.Contains(card.RuntimeId))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    card.RuntimeId,
                    card.Definition.Name,
                    "Reveal card " + (pendingBingoRuntimeIds.Count + 1) + " of 3",
                    true,
                    string.Empty));
            }

            State.PendingChoice = new CleanChoicePrompt(
                BingoChoicePromptId,
                "Bingo Machine, Go!!!",
                "Choose 3 valid cards to reveal. Duplicate copies may be chosen.",
                options);
            Log("Bingo Machine awaits reveal choice " + (pendingBingoRuntimeIds.Count + 1) + " of 3.");
        }

        private void ResolveBingoChoice(CleanPlayerState player, int runtimeId)
        {
            CleanCard selected = FindCardByRuntimeId(player.Deck, runtimeId);
            if (selected == null || !IsBingoSearchCandidate(selected) || pendingBingoRuntimeIds.Contains(runtimeId))
            {
                Log("Bingo Machine failed: that reveal card is no longer valid.");
                pendingBingoRuntimeIds.Clear();
                return;
            }

            pendingBingoRuntimeIds.Add(runtimeId);
            if (pendingBingoRuntimeIds.Count < 3)
            {
                OpenBingoChoice(player);
                return;
            }

            List<CleanCard> revealed = new List<CleanCard>();
            for (int i = 0; i < pendingBingoRuntimeIds.Count; i++)
            {
                CleanCard card = FindCardByRuntimeId(player.Deck, pendingBingoRuntimeIds[i]);
                if (card != null)
                {
                    revealed.Add(card);
                }
            }

            ResolveBingoReveal(player, revealed);
            pendingBingoRuntimeIds.Clear();
        }

        private bool ResolveBingoReveal(CleanPlayerState player, List<CleanCard> revealed)
        {
            if (revealed == null || revealed.Count != 3)
            {
                Log("Bingo Machine failed: 3 valid revealed cards are required.");
                return false;
            }

            CleanCard chosen = revealed[random.Next(revealed.Count)];
            player.Deck.Remove(chosen);
            ResetCardForHiddenZone(chosen);
            player.Hand.Add(chosen);
            Shuffle(player.Deck);
            Log("The opponent randomly chooses " + chosen.Definition.Name + "; it is added to the hand and the Deck is shuffled.");
            return true;
        }

        private CleanCard FindCardByRuntimeId(List<CleanCard> cards, int runtimeId)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].RuntimeId == runtimeId)
                {
                    return cards[i];
                }
            }

            return null;
        }

        private bool CanUseGatewayToChaosRitualSearch(CleanPlayerState player)
        {
            return !player.UsedOncePerTurn.Contains(GatewayRitualSearchOnceKey) &&
                   player.GatewayToChaosCounters >= 3 &&
                   HasMatchingCard(player.Deck, IsRitualSpellCard);
        }

        private bool ActivateGatewayToChaos(CleanPlayerState player, CleanCard source, CleanActivationSource activationSource)
        {
            bool alreadyFieldSpell = object.ReferenceEquals(player.FieldSpell, source);
            if (activationSource == CleanActivationSource.Hand || !alreadyFieldSpell || source.FaceDown)
            {
                if (player.UsedOncePerTurn.Contains(GatewayActivationOnceKey) || !HasMatchingCard(player.Deck, IsGatewayToChaosSearchCandidate))
                {
                    Log("Gateway to Chaos cannot be activated without a Black Luster Soldier Ritual Monster or Gaia the Fierce Knight monster to search.");
                    return false;
                }

                source.FaceDown = false;
                source.Controller = player.Id;
                PlaceFieldSpell(player, source);
                player.UsedOncePerTurn.Add(GatewayActivationOnceKey);
                Log("Gateway to Chaos is activated in the Field Spell Zone.");
                if (player.Id == CleanPlayerId.Local)
                {
                    OpenGatewaySearchChoice(player);
                    return true;
                }

                return AddFromDeckToHand(player, IsGatewayToChaosSearchCandidate, "Black Luster Soldier Ritual Monster or Gaia the Fierce Knight monster");
            }

            if (!CanUseGatewayToChaosRitualSearch(player))
            {
                Log("Gateway to Chaos needs 3 Spell Counters and an available Ritual Spell, and this effect is once per turn.");
                return false;
            }

            player.GatewayToChaosCounters -= 3;
            player.UsedOncePerTurn.Add(GatewayRitualSearchOnceKey);
            Log("Gateway to Chaos removes 3 Spell Counters.");
            return AddFromDeckToHand(player, IsRitualSpellCard, "Ritual Spell");
        }

        private void OpenGatewaySearchChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = BuildUniqueDeckChoiceOptions(player, IsGatewayToChaosSearchCandidate, "Add to hand");
            State.PendingChoice = new CleanChoicePrompt(
                GatewaySearchChoicePromptId,
                "Gateway to Chaos",
                "Choose 1 \"Black Luster Soldier\" Ritual Monster or \"Gaia The Fierce Knight\" monster from your Deck.",
                options);
            Log("Gateway to Chaos awaits a search choice.");
        }

        private void ResolveGatewaySearchChoice(CleanPlayerState player, int selectedCardId)
        {
            CleanCard card;
            if (!TryTakeDeckChoice(player, selectedCardId, IsGatewayToChaosSearchCandidate, out card))
            {
                Log("Gateway to Chaos failed: chosen monster is no longer in Deck.");
                return;
            }

            ResetCardForHiddenZone(card);
            player.Hand.Add(card);
            Log("Gateway to Chaos adds " + card.Definition.Name + " from Deck to hand.");
        }

        private bool ActivateMajestyWithEyesOfBlue(CleanPlayerState player, CleanPlayerState rival)
        {
            if (player.Id == CleanPlayerId.Local)
            {
                OpenMajestySendChoice(player);
                return true;
            }

            CleanCard sent;
            bool sentFromHand = false;
            if (!TryTakeBestMonster(player.Deck, IsBlueEyesMonster, out sent))
            {
                if (!TryTakeBestMonster(player.Hand, IsBlueEyesMonster, out sent))
                {
                    Log("Majesty failed: no Blue-Eyes monster in hand or Deck to send.");
                    return false;
                }

                sentFromHand = true;
            }

            ResetCardForHiddenZone(sent);
            AddToGraveyard(player, sent, sentFromHand);
            CleanCard target = BestMajestyTarget(player, rival);
            if (target == null)
            {
                Log("Majesty sends " + sent.Definition.Name + ", but no legal face-up monster remains to target.");
                return true;
            }

            target.CannotAttack = true;
            Log("Majesty sends " + sent.Definition.Name + " to the GY and prevents " + target.Definition.Name + " from attacking while face-up.");
            return true;
        }

        private void OpenMajestySendChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            AddMajestySendOptions(options, player.Deck, false);
            AddMajestySendOptions(options, player.Hand, true);
            State.PendingChoice = new CleanChoicePrompt(
                MajestySendChoicePromptId,
                "Majesty with Eyes of Blue",
                "Choose 1 Blue-Eyes monster from your hand or Deck to send to the GY.",
                options);
            Log("Majesty awaits a Blue-Eyes monster choice.");
        }

        private void AddMajestySendOptions(List<CleanChoiceOption> options, List<CleanCard> cards, bool fromHand)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCard card = cards[i];
                if (!IsBlueEyesMonster(card))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    fromHand ? -card.RuntimeId : card.RuntimeId,
                    card.Definition.Name,
                    fromHand ? "Send from hand to GY" : "Send from Deck to GY",
                    true,
                    string.Empty));
            }
        }

        private void ResolveMajestySendChoice(CleanPlayerState player, int encodedRuntimeId)
        {
            bool fromHand = encodedRuntimeId < 0;
            int runtimeId = Math.Abs(encodedRuntimeId);
            List<CleanCard> source = fromHand ? player.Hand : player.Deck;
            CleanCard sent = FindCardByRuntimeId(source, runtimeId);
            if (sent == null || !IsBlueEyesMonster(sent))
            {
                Log("Majesty failed: the selected Blue-Eyes monster is no longer available.");
                return;
            }

            source.Remove(sent);
            ResetCardForHiddenZone(sent);
            AddToGraveyard(player, sent, fromHand);
            Log("Majesty sends " + sent.Definition.Name + " to the GY.");
            OpenMajestyTargetChoice(player);
        }

        private void OpenMajestyTargetChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            AddMajestyTargetOptions(options, player, false);
            AddMajestyTargetOptions(options, OpponentOf(player.Id), true);
            State.PendingChoice = new CleanChoicePrompt(
                MajestyTargetChoicePromptId,
                "Majesty with Eyes of Blue",
                "Choose 1 face-up monster on the field. It cannot attack while face-up.",
                options);
            Log("Majesty awaits a face-up monster target.");
        }

        private void AddMajestyTargetOptions(List<CleanChoiceOption> options, CleanPlayerState targetPlayer, bool opposing)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = targetPlayer.Monsters[i];
                if (monster == null || monster.FaceDown || (opposing && IsProtectedFromCleanTargeting(targetPlayer, monster)))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    opposing ? 100 + i : i,
                    monster.Definition.Name,
                    opposing ? "Opponent's monster" : "Your monster",
                    true,
                    string.Empty));
            }
        }

        private void ResolveMajestyTargetChoice(CleanPlayerState player, int encodedZone)
        {
            bool opposing = encodedZone >= 100;
            int zone = opposing ? encodedZone - 100 : encodedZone;
            CleanPlayerState targetPlayer = opposing ? OpponentOf(player.Id) : player;
            if (zone < 0 || zone >= CleanPlayerState.ZoneCount)
            {
                return;
            }

            CleanCard target = targetPlayer.Monsters[zone];
            if (target == null || target.FaceDown || (opposing && IsProtectedFromCleanTargeting(targetPlayer, target)))
            {
                Log("Majesty failed: the selected monster is no longer a legal target.");
                return;
            }

            Action continueMajestyEffect = () =>
            {
                int currentZone = IndexOfMonsterZone(targetPlayer, target);
                if (currentZone < 0)
                {
                    Log("Majesty failed: its targeted monster left the field.");
                    return;
                }

                target.CannotAttack = true;
                Log("Majesty prevents " + target.Definition.Name + " from attacking while it remains face-up.");
            };

            if (TriggerMaidenTargetedByCardEffect(targetPlayer, zone, continueMajestyEffect))
            {
                return;
            }

            continueMajestyEffect();
        }

        private CleanCard BestMajestyTarget(CleanPlayerState player, CleanPlayerState rival)
        {
            int zone = StrongestEffectTargetZone(rival);
            if (zone >= 0)
            {
                return rival.Monsters[zone];
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null && !player.Monsters[i].FaceDown)
                {
                    return player.Monsters[i];
                }
            }

            return null;
        }

        private bool ActivateDragonShrine(CleanPlayerState player)
        {
            if (!HasMatchingCard(player.Deck, IsDragonMonster))
            {
                Log("Dragon Shrine failed: no Dragon monster in deck.");
                return false;
            }

            if (player.Id == CleanPlayerId.Local)
            {
                OpenDragonShrineFirstChoice(player);
                return true;
            }

            CleanCard first;
            if (!TryTakeBestMonster(player.Deck, IsDragonMonster, out first))
            {
                return false;
            }

            bool firstWasNormalDragon = IsNormalMonsterCard(first);
            SendDragonShrineCardToGrave(player, first, "Dragon Shrine sends " + first.Definition.Name + " from Deck to the GY.");
            if (firstWasNormalDragon)
            {
                CleanCard second;
                if (TryTakeBestMonster(player.Deck, IsDragonMonster, out second))
                {
                    SendDragonShrineCardToGrave(player, second, "Dragon Shrine sends an extra Dragon, " + second.Definition.Name + ", to the GY.");
                }
            }

            return true;
        }

        private void OpenDragonShrineFirstChoice(CleanPlayerState player)
        {
            State.PendingChoice = new CleanChoicePrompt(
                DragonShrineFirstChoicePromptId,
                "Dragon Shrine",
                "Choose 1 Dragon monster from your Deck to send to the GY.",
                BuildDragonShrineOptions(player, false));
            Log("Dragon Shrine awaits your Dragon choice.");
        }

        private void OpenDragonShrineSecondChoice(CleanPlayerState player)
        {
            State.PendingChoice = new CleanChoicePrompt(
                DragonShrineSecondChoicePromptId,
                "Dragon Shrine",
                "You sent a Normal Dragon. Choose 1 more Dragon to send, or skip.",
                BuildDragonShrineOptions(player, true));
            Log("Dragon Shrine can send one extra Dragon.");
        }

        private List<CleanChoiceOption> BuildDragonShrineOptions(CleanPlayerState player, bool includeSkip)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            HashSet<int> seenIds = new HashSet<int>();
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard card = player.Deck[i];
                if (!IsDragonMonster(card) || seenIds.Contains(card.Definition.Id))
                {
                    continue;
                }

                seenIds.Add(card.Definition.Id);
                options.Add(new CleanChoiceOption(
                    card.Definition.Id,
                    card.Definition.Name,
                    "Send from Deck to GY",
                    true,
                    string.Empty));
            }

            if (includeSkip)
            {
                options.Add(new CleanChoiceOption(
                    DragonShrineSkipSecondOptionId,
                    "No extra Dragon",
                    "Resolve without sending another Dragon.",
                    true,
                    string.Empty));
            }

            return options;
        }

        private void ResolveDragonShrineFirstChoice(CleanPlayerState player, int selectedCardId)
        {
            CleanCard first;
            if (!TryTakeDragonShrineChoice(player, selectedCardId, out first))
            {
                Log("Dragon Shrine failed: selected Dragon is no longer in the deck.");
                return;
            }

            bool firstWasNormalDragon = IsNormalMonsterCard(first) && IsDragonMonster(first);
            SendDragonShrineCardToGrave(player, first, "Dragon Shrine sends " + first.Definition.Name + " from deck to the GY.");
            if (firstWasNormalDragon && HasMatchingCard(player.Deck, IsDragonMonster))
            {
                OpenDragonShrineSecondChoice(player);
            }
        }

        private void ResolveDragonShrineSecondChoice(CleanPlayerState player, int selectedCardId)
        {
            if (selectedCardId == DragonShrineSkipSecondOptionId)
            {
                Log("Dragon Shrine resolves without sending an extra Dragon.");
                return;
            }

            CleanCard second;
            if (!TryTakeDragonShrineChoice(player, selectedCardId, out second))
            {
                Log("Dragon Shrine failed: selected extra Dragon is no longer in the deck.");
                return;
            }

            SendDragonShrineCardToGrave(player, second, "Dragon Shrine sends an extra Dragon, " + second.Definition.Name + ", to the GY.");
        }

        private bool TryTakeDragonShrineChoice(CleanPlayerState player, int selectedCardId, out CleanCard card)
        {
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard candidate = player.Deck[i];
                if (candidate.Definition.Id == selectedCardId && IsDragonMonster(candidate))
                {
                    player.Deck.RemoveAt(i);
                    card = candidate;
                    return true;
                }
            }

            card = null;
            return false;
        }

        private void SendDragonShrineCardToGrave(CleanPlayerState player, CleanCard card, string log)
        {
            ResetCardForHiddenZone(card);
            AddToGraveyard(player, card);
            Log(log);
        }

        private void OpenDictatorSendChoice(CleanPlayerState player, CleanCard dictator)
        {
            pendingDictatorCard = dictator;
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            AddSpecificCardRuntimeOptions(options, player.Hand, BlueEyesWhiteDragonCardId, true, "Send from hand to GY");
            AddSpecificCardRuntimeOptions(options, player.Deck, BlueEyesWhiteDragonCardId, false, "Send from Deck to GY");
            State.PendingChoice = new CleanChoicePrompt(
                DictatorSendChoicePromptId,
                "Dictator of D.",
                "Choose 1 Blue-Eyes White Dragon from your hand or Deck to send to the GY.",
                options);
        }

        private void ResolveDictatorSendChoice(CleanPlayerState player, int encodedRuntimeId)
        {
            bool fromHand = encodedRuntimeId < 0;
            int runtimeId = Math.Abs(encodedRuntimeId);
            List<CleanCard> source = fromHand ? player.Hand : player.Deck;
            CleanCard cost = FindCardByRuntimeId(source, runtimeId);
            if (cost == null || cost.Definition.Id != BlueEyesWhiteDragonCardId || pendingDictatorCard == null)
            {
                Log("Dictator failed: the selected Blue-Eyes White Dragon is no longer available.");
                pendingDictatorCard = null;
                return;
            }

            source.Remove(cost);
            ResetCardForHiddenZone(cost);
            AddToGraveyard(player, cost, fromHand);
            CleanCard dictator = pendingDictatorCard;
            pendingDictatorCard = null;
            LuaSpecialSummonSpecific(player, dictator);
            Log("Dictator of D. sends Blue-Eyes White Dragon to the GY and Special Summons itself.");
        }

        private void OpenDictatorDiscardChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < player.Hand.Count; i++)
            {
                CleanCard card = player.Hand[i];
                if (IsBlueEyesWhiteDragonOrMention(card))
                {
                    options.Add(new CleanChoiceOption(card.RuntimeId, card.Definition.Name, "Discard for cost", true, string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                DictatorDiscardChoicePromptId,
                "Dictator of D.",
                "Choose 1 Blue-Eyes White Dragon or a card that mentions it to discard.",
                options);
        }

        private void ResolveDictatorDiscardChoice(CleanPlayerState player, int runtimeId)
        {
            CleanCard cost = FindCardByRuntimeId(player.Hand, runtimeId);
            if (cost == null || !IsBlueEyesWhiteDragonOrMention(cost))
            {
                Log("Dictator failed: the selected discard is no longer available.");
                return;
            }

            player.Hand.Remove(cost);
            ResetCardForHiddenZone(cost);
            AddToGraveyard(player, cost, true);
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < player.Graveyard.Count; i++)
            {
                CleanCard card = player.Graveyard[i];
                if (IsBlueEyesMonster(card))
                {
                    options.Add(new CleanChoiceOption(card.RuntimeId, card.Definition.Name, "Special Summon from GY", true, string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                DictatorReviveChoicePromptId,
                "Dictator of D.",
                "Choose 1 Blue-Eyes monster in your GY to Special Summon.",
                options,
                CleanPromptKind.SpecialSummon,
                true);
        }

        private void ResolveDictatorReviveChoice(CleanPlayerState player, int runtimeId)
        {
            CleanCard target = FindCardByRuntimeId(player.Graveyard, runtimeId);
            int zone = FirstEmpty(player.Monsters);
            if (target == null || !IsBlueEyesMonster(target) || zone < 0)
            {
                Log("Dictator failed: the selected Blue-Eyes monster cannot be Special Summoned.");
                return;
            }

            player.Graveyard.Remove(target);
            SummonToZone(player, target, zone, false, CleanMonsterStance.Attack);
            Log("Dictator of D. Special Summons " + target.Definition.Name + " from the GY.");
        }

        private void AddSpecificCardRuntimeOptions(List<CleanChoiceOption> options, List<CleanCard> cards, int cardId, bool negative, string detail)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Definition.Id == cardId)
                {
                    options.Add(new CleanChoiceOption(
                        negative ? -cards[i].RuntimeId : cards[i].RuntimeId,
                        cards[i].Definition.Name,
                        detail,
                        true,
                        string.Empty));
                }
            }
        }
        private bool ActivateDictatorRevive(CleanPlayerState player)
        {
            CleanCard cost;
            if (!TryTakeDiscardCost(player.Hand, IsBlueEyesWhiteDragonOrMention, out cost))
            {
                Log("Dictator of D. failed: no Blue-Eyes White Dragon or card that mentions it to discard.");
                return false;
            }

            ResetCardForHiddenZone(cost);
            AddToGraveyard(player, cost, true);
            CleanCard target;
            if (!TryTakeBestMonster(
                    player.Graveyard,
                    candidate => IsBlueEyesMonster(candidate) &&
                                 CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect),
                    out target))
            {
                Log("Dictator of D. failed: no Blue-Eyes monster in GY to summon.");
                return false;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                player.Graveyard.Add(target);
                Log("Dictator of D. failed: no open Monster Zone.");
                return false;
            }

            SummonToZone(player, target, zone, false, CleanMonsterStance.Attack);
            Log("Dictator of D. special summons " + target.Definition.Name + " from the GY.");
            return true;
        }

        private void ResolveDragonSpiritSummonTrigger(CleanPlayerState player, CleanPlayerState rival)
        {
            List<CleanChoiceOption> options = BuildDragonSpiritBanishOptions(rival);
            if (options.Count == 0)
            {
                return;
            }

            if (player.Id == CleanPlayerId.Local)
            {
                State.PendingChoice = new CleanChoicePrompt(
                    DragonSpiritBanishChoicePromptId,
                    "Dragon Spirit of White",
                    "Choose 1 Spell/Trap your opponent controls to banish.",
                    options,
                CleanPromptKind.TriggerEffect,
                true);
                Log("Dragon Spirit of White awaits a Spell/Trap target.");
                return;
            }

            ResolveDragonSpiritBanishChoice(player, rival, options[0].Id);
        }

        private List<CleanChoiceOption> BuildDragonSpiritBanishOptions(CleanPlayerState targetPlayer)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = targetPlayer.SpellsTraps[i];
                if (card == null || IsProtectedByJetFromCardEffect(targetPlayer, card))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(i, card.PublicName, "Banish this Spell/Trap", true, string.Empty));
            }

            if (targetPlayer.FieldSpell != null && !IsProtectedByJetFromCardEffect(targetPlayer, targetPlayer.FieldSpell))
            {
                options.Add(new CleanChoiceOption(
                    DragonSpiritFieldSpellChoiceId,
                    targetPlayer.FieldSpell.PublicName,
                    "Banish the Field Spell",
                    true,
                    string.Empty));
            }

            return options;
        }

        private void ResolveDragonSpiritBanishChoice(CleanPlayerState player, CleanPlayerState rival, int optionId)
        {
            CleanCard target;
            if (optionId == DragonSpiritFieldSpellChoiceId)
            {
                target = rival.FieldSpell;
                if (target == null || IsProtectedByJetFromCardEffect(rival, target))
                {
                    return;
                }

                rival.FieldSpell = null;
                if (target.Definition.Id == GatewayToChaosCardId)
                {
                    rival.GatewayToChaosCounters = 0;
                }
            }
            else
            {
                if (optionId < 0 || optionId >= CleanPlayerState.ZoneCount)
                {
                    return;
                }

                target = rival.SpellsTraps[optionId];
                if (target == null || IsProtectedByJetFromCardEffect(rival, target))
                {
                    return;
                }

                DetachEquipBonus(rival, target);
                rival.SpellsTraps[optionId] = null;
            }

            string targetName = target.PublicName;
            ResetCardForHiddenZone(target);
            rival.Banished.Add(target);
            Log("Dragon Spirit of White banishes " + targetName + ".");
        }
        private bool ActivateDragonSpiritTributeEffect(CleanPlayerState player, CleanCard spirit)
        {
            CleanCard blueEyes;
            if (spirit == null ||
                !CanDragonSpiritTributeForBlueEyes(player, OpponentOf(player.Id)) ||
                !TryTakeCard(player.Hand, BlueEyesWhiteDragonCardId, out blueEyes))
            {
                Log("Dragon Spirit of White cannot Tribute itself for Blue-Eyes White Dragon.");
                return false;
            }

            SummonToZone(player, blueEyes, FirstEmpty(player.Monsters), false, CleanMonsterStance.Attack);
            Log("Dragon Spirit of White Tributes itself to Special Summon Blue-Eyes White Dragon from the hand.");
            return true;
        }

        private bool CanActivateBlueEyesUltimateSpiritDragon(CleanPlayerState player, CleanCard spirit, CleanPlayerState rival)
        {
            return spirit != null &&
                   spirit.Definition.Id == BlueEyesUltimateSpiritDragonCardId &&
                   !spirit.FaceDown &&
                   !spirit.EffectsNegated &&
                   !player.UsedOncePerTurn.Contains(UltimateSpiritNegateOnceKey) &&
                   HasNegatableFaceUpFieldEffect(rival);
        }

        private bool ActivateBlueEyesUltimateSpiritDragon(CleanPlayerState player, CleanPlayerState rival, CleanCard spirit)
        {
            if (!CanActivateBlueEyesUltimateSpiritDragon(player, spirit, rival))
            {
                Log("Blue-Eyes Ultimate Spirit Dragon cannot negate a field effect right now.");
                return false;
            }

            CleanCard negated;
            if (!TryNegateStrongestFaceUpFieldEffect(rival, out negated))
            {
                Log("Blue-Eyes Ultimate Spirit Dragon found no face-up field effect to negate.");
                return false;
            }

            spirit.AttackBonus += 1000;
            player.UsedOncePerTurn.Add(UltimateSpiritNegateOnceKey);
            Log("Blue-Eyes Ultimate Spirit Dragon negates " + negated.Definition.Name + " and gains 1000 ATK until the End Phase.");
            return true;
        }

        private bool HasNegatableFaceUpFieldEffect(CleanPlayerState player)
        {
            CleanCard ignored;
            return TryFindStrongestFaceUpFieldEffect(player, out ignored);
        }

        private bool TryNegateStrongestFaceUpFieldEffect(CleanPlayerState player, out CleanCard target)
        {
            if (!TryFindStrongestFaceUpFieldEffect(player, out target))
            {
                return false;
            }

            target.EffectsNegated = true;
            target.AttackBonus = 0;
            return true;
        }

        private bool TryFindStrongestFaceUpFieldEffect(CleanPlayerState player, out CleanCard target)
        {
            target = null;
            int bestScore = -1;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && !monster.EffectsNegated && monster.Definition.Kind == CleanCardKind.Monster && !IsNormalMonster(monster))
                {
                    int score = Mathf.Max(monster.CurrentAttack, monster.CurrentDefense);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        target = monster;
                    }
                }

                CleanCard spellTrap = player.SpellsTraps[i];
                if (spellTrap != null && !spellTrap.FaceDown && !spellTrap.EffectsNegated)
                {
                    int score = spellTrap.Definition.Kind == CleanCardKind.Trap ? 800 : 700;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        target = spellTrap;
                    }
                }
            }

            if (player.FieldSpell != null && !player.FieldSpell.FaceDown && !player.FieldSpell.EffectsNegated && 900 > bestScore)
            {
                target = player.FieldSpell;
            }

            return target != null;
        }

        private bool TriggerDragonMasterMagiaLeavesField(CleanPlayerState player, CleanCard destroyed)
        {
            if (destroyed == null || destroyed.Definition.Id != DragonMasterMagiaCardId)
            {
                return false;
            }

            List<CleanChoiceOption> options = BuildMagiaLeaveOptions(player);
            if (options.Count == 0)
            {
                Log("Dragon Master Magia found no legal Blue-Eyes or Chaos/Black Luster Soldier Ritual monster to Special Summon.");
                return false;
            }

            if (player.Id == CleanPlayerId.Local)
            {
                State.PendingChoice = new CleanChoicePrompt(
                    MagiaLeaveChoicePromptId,
                    "Dragon Master Magia",
                    "Choose 1 Blue-Eyes monster or Chaos/Black Luster Soldier Ritual Monster from your Extra Deck or GY.",
                    options,
                CleanPromptKind.TriggerEffect,
                true);
                Log("Dragon Master Magia's leave-field Trigger Effect awaits a summon choice.");
                return true;
            }

            ResolveMagiaLeaveChoice(player, options[0].Id);
            return true;
        }

        private List<CleanChoiceOption> BuildMagiaLeaveOptions(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            bool graveyardSpace = FirstEmpty(player.Monsters) >= 0;
            if (graveyardSpace)
            {
                for (int i = 0; i < player.Graveyard.Count; i++)
                {
                    CleanCard card = player.Graveyard[i];
                    if (IsMagiaLeaveTarget(card) &&
                        CanSpecialSummonCard(player, card, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect))
                    {
                        options.Add(new CleanChoiceOption(card.RuntimeId, card.Definition.Name, "Special Summon from GY", true, string.Empty));
                    }
                }
            }

            if (CanPlaceExtraDeckEffectSummon(player))
            {
                for (int i = 0; i < player.ExtraDeck.Count; i++)
                {
                    CleanCard card = player.ExtraDeck[i];
                    if (IsMagiaLeaveTarget(card) &&
                        CanSpecialSummonCard(player, card, CleanSummonOrigin.ExtraDeck, CleanSummonMethod.Effect))
                    {
                        options.Add(new CleanChoiceOption(-card.RuntimeId, card.Definition.Name, "Special Summon from Extra Deck", true, string.Empty));
                    }
                }
            }

            return options;
        }

        private bool IsMagiaLeaveTarget(CleanCard card)
        {
            return card != null && (IsBlueEyesMonster(card) || IsChaosRitualMaterial(card));
        }

        private bool CanPlaceExtraDeckEffectSummon(CleanPlayerState player)
        {
            return State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)] == null ||
                   FirstLinkedExtraDeckMainMonsterZone(player) >= 0;
        }

        private void ResolveMagiaLeaveChoice(CleanPlayerState player, int encodedRuntimeId)
        {
            bool fromExtra = encodedRuntimeId < 0;
            int runtimeId = Math.Abs(encodedRuntimeId);
            List<CleanCard> source = fromExtra ? player.ExtraDeck : player.Graveyard;
            CleanCard target = FindCardByRuntimeId(source, runtimeId);
            if (target == null ||
                !IsMagiaLeaveTarget(target) ||
                !CanSpecialSummonCard(
                    player,
                    target,
                    fromExtra ? CleanSummonOrigin.ExtraDeck : CleanSummonOrigin.Graveyard,
                    CleanSummonMethod.Effect))
            {
                Log("Dragon Master Magia failed: the selected monster is no longer legal.");
                return;
            }

            source.Remove(target);
            if (fromExtra)
            {
                PrepareExtraDeckMonsterSummon(player, target);
                int extraZone = ExtraMonsterZoneIndexFor(player);
                if (State.ExtraMonsters[extraZone] == null)
                {
                    State.ExtraMonsters[extraZone] = target;
                    NotifySpecialSummon(player);
                }
                else
                {
                    int zone = FirstLinkedExtraDeckMainMonsterZone(player);
                    if (zone < 0)
                    {
                        source.Add(target);
                        return;
                    }

                    SummonToZone(player, target, zone, false, CleanMonsterStance.Attack);
                }
            }
            else
            {
                int zone = FirstEmpty(player.Monsters);
                if (zone < 0)
                {
                    source.Add(target);
                    return;
                }

                SummonToZone(player, target, zone, false, CleanMonsterStance.Attack);
            }

            Log("Dragon Master Magia Special Summons " + target.Definition.Name + " from the " + (fromExtra ? "Extra Deck" : "GY") + ".");
        }
        private bool TriggerBlueEyesUltimateSpiritDestroyed(CleanPlayerState player, CleanCard destroyed)
        {
            if (destroyed == null ||
                destroyed.Definition.Id != BlueEyesUltimateSpiritDragonCardId ||
                player.UsedOncePerTurn.Contains(UltimateSpiritDestroyedOnceKey) ||
                pendingTriggerPromptKeys.Contains(TriggerPromptKey(player, UltimateSpiritDestroyedOnceKey)) ||
                FirstEmpty(player.Monsters) < 0)
            {
                return false;
            }

            int targetIndex = BestCardIndex(
                player.Graveyard,
                card => IsUltimateSpiritReviveTarget(card) &&
                        CanSpecialSummonCard(player, card, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect));
            if (targetIndex < 0 || !TryReserveTriggerPrompt(player, UltimateSpiritDestroyedOnceKey))
            {
                return false;
            }

            CleanCard target = player.Graveyard[targetIndex];
            QueueTriggerEffectPrompt(
                player,
                "Blue-Eyes Ultimate Spirit Dragon",
                "This Synchro Monster was destroyed. Activate its Trigger Effect to Special Summon " + target.Definition.Name + " from the GY?",
                true,
                () =>
                {
                    ReleaseTriggerPrompt(player, UltimateSpiritDestroyedOnceKey);
                    ResolveUltimateSpiritDestroyedSummon(player, target);
                },
                () =>
                {
                    ReleaseTriggerPrompt(player, UltimateSpiritDestroyedOnceKey);
                    Log("Blue-Eyes Ultimate Spirit Dragon's destroyed Trigger Effect was not activated.");
                });
            return true;
        }

        private void ResolveUltimateSpiritDestroyedSummon(CleanPlayerState player, CleanCard target)
        {
            if (!player.Graveyard.Contains(target) ||
                !IsUltimateSpiritReviveTarget(target) ||
                !CanSpecialSummonCard(player, target, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect) ||
                FirstEmpty(player.Monsters) < 0)
            {
                Log("Blue-Eyes Ultimate Spirit Dragon could not resolve its destroyed Trigger Effect.");
                return;
            }

            player.Graveyard.Remove(target);
            SummonToZone(player, target, FirstEmpty(player.Monsters), false, CleanMonsterStance.Attack);
            player.UsedOncePerTurn.Add(UltimateSpiritDestroyedOnceKey);
            Log("Blue-Eyes Ultimate Spirit Dragon Special Summons " + target.Definition.Name + " from the GY after being destroyed.");
        }

        private bool IsUltimateSpiritReviveTarget(CleanCard card)
        {
            return card != null &&
                   card.Definition.Id != BlueEyesUltimateSpiritDragonCardId &&
                   card.Definition.Kind == CleanCardKind.Monster &&
                   IsDragonMonster(card) &&
                   IsLightMonster(card);
        }

        private bool CanActivateBlueEyesSpiritDragon(CleanPlayerState player, CleanCard spirit)
        {
            return spirit != null &&
                   spirit.Definition.Id == BlueEyesSpiritDragonCardId &&
                   !spirit.FaceDown &&
                   !spirit.EffectsNegated &&
                   spirit.SynchroSummoned &&
                   FindBlueEyesSpiritZone(player, spirit) >= 0 &&
                   HasMatchingCard(player.ExtraDeck, IsBlueEyesSpiritDragonTagOutTarget);
        }

        private bool ActivateBlueEyesSpiritDragon(CleanPlayerState player, CleanCard spirit)
        {
            int encodedZone = FindBlueEyesSpiritZone(player, spirit);
            if (encodedZone < 0 || !CanActivateBlueEyesSpiritDragon(player, spirit))
            {
                Log("Blue-Eyes Spirit Dragon cannot activate its tag-out effect right now.");
                return false;
            }

            CleanCard target;
            if (!TryTakeBestMonster(player.ExtraDeck, IsBlueEyesSpiritDragonTagOutTarget, out target))
            {
                Log("Blue-Eyes Spirit Dragon found no LIGHT Dragon Synchro monster in the Extra Deck.");
                return false;
            }

            bool fromExtraMonsterZone = encodedZone >= ExtraSpiritZoneOffset;
            if (fromExtraMonsterZone)
            {
                State.ExtraMonsters[encodedZone - ExtraSpiritZoneOffset] = null;
            }
            else
            {
                player.Monsters[encodedZone] = null;
            }

            ResetCardForHiddenZone(spirit);
            AddToGraveyard(player, spirit, true);
            if (fromExtraMonsterZone)
            {
                PrepareExtraDeckMonsterSummon(player, target);
                target.Stance = CleanMonsterStance.Defense;
                State.ExtraMonsters[encodedZone - ExtraSpiritZoneOffset] = target;
                NotifySpecialSummon(player);
            }
            else
            {
                SummonToZone(player, target, encodedZone, false, CleanMonsterStance.Defense);
            }

            target.DestroyDuringEndPhase = true;
            Log("Blue-Eyes Spirit Dragon Tributes itself to Special Summon " + target.Definition.Name + " from the Extra Deck in Defense Position.");
            return true;
        }

        private int FindBlueEyesSpiritZone(CleanPlayerState player, CleanCard spirit)
        {
            int mainZone = IndexOfMonsterZone(player, spirit);
            if (mainZone >= 0)
            {
                return mainZone;
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            return object.ReferenceEquals(State.ExtraMonsters[extraZone], spirit)
                ? ExtraSpiritZoneOffset + extraZone
                : -1;
        }

        private int IndexOfMonsterZone(CleanPlayerState player, CleanCard target)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (object.ReferenceEquals(player.Monsters[i], target))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool IsBlueEyesSpiritDragonTagOutTarget(CleanCard card)
        {
            return card != null &&
                   card.Definition.Id != BlueEyesSpiritDragonCardId &&
                   card.Definition.Kind == CleanCardKind.Monster &&
                   IsSynchroMonsterDefinition(card.Definition) &&
                   IsDragonMonster(card) &&
                   IsLightMonster(card);
        }

        private bool IsLightMonster(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("LIGHT", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   card.Definition.Id == AzureEyesSilverDragonCardId ||
                   card.Definition.Id == BlueEyesSpiritDragonCardId ||
                   card.Definition.Id == WhiteStoneOfAncientsCardId;
        }

        private bool TryNegateRivalGraveyardEffect(CleanPlayerState effectPlayer, string effectName)
        {
            CleanPlayerState opponent = OpponentOf(effectPlayer.Id);
            int spiritZone = FirstFaceUpBlueEyesSpiritDragonZone(opponent);
            if (spiritZone < 0 || opponent.UsedOncePerTurn.Contains(BlueEyesSpiritGraveNegateOnceKey))
            {
                return false;
            }

            opponent.UsedOncePerTurn.Add(BlueEyesSpiritGraveNegateOnceKey);
            Log("Blue-Eyes Spirit Dragon negates " + effectName + " from the GY.");
            return true;
        }

        private int FirstFaceUpBlueEyesSpiritDragonZone(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.Monsters[i];
                if (card != null && card.Definition.Id == BlueEyesSpiritDragonCardId && !card.FaceDown && !card.EffectsNegated)
                {
                    return i;
                }
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            CleanCard extraMonster = State.ExtraMonsters[extraZone];
            return extraMonster != null &&
                   extraMonster.Definition.Id == BlueEyesSpiritDragonCardId &&
                   !extraMonster.FaceDown &&
                   !extraMonster.EffectsNegated
                ? ExtraSpiritZoneOffset + extraZone
                : -1;
        }

        private bool HasFaceUpBlueEyesUltimateSpiritDragon(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.Monsters[i];
                if (card != null && card.Definition.Id == BlueEyesUltimateSpiritDragonCardId && !card.FaceDown && !card.EffectsNegated)
                {
                    return true;
                }
            }

            CleanCard extraMonster = State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)];
            return extraMonster != null &&
                   extraMonster.Definition.Id == BlueEyesUltimateSpiritDragonCardId &&
                   !extraMonster.FaceDown &&
                   !extraMonster.EffectsNegated;
        }
        private bool CanActivateSpiritWithEyesOfBlue(CleanPlayerState player, CleanCard spirit)
        {
            return CanUseSpiritMausoleumEffect(player, spirit) || CanUseSpiritTributeEffect(player, spirit);
        }

        private bool CanUseSpiritMausoleumEffect(CleanPlayerState player, CleanCard spirit)
        {
            return spirit != null && spirit.LinkSummoned && !spirit.FaceDown && !spirit.EffectsNegated &&
                   !player.UsedOncePerTurn.Contains(SpiritMausoleumOnceKey) &&
                   HasMatchingCard(player.Deck, IsMausoleumOfWhite);
        }

        private bool CanUseSpiritTributeEffect(CleanPlayerState player, CleanCard spirit)
        {
            return spirit != null && !spirit.FaceDown && !spirit.EffectsNegated &&
                   !player.UsedOncePerTurn.Contains(SpiritTributeOnceKey) &&
                   (HasMatchingCard(
                        player.Hand,
                        card => IsBlueEyesMonster(card) &&
                                CanSpecialSummonCard(player, card, CleanSummonOrigin.Hand, CleanSummonMethod.Effect)) ||
                    HasMatchingCard(
                        player.Graveyard,
                        card => IsBlueEyesMonster(card) &&
                                CanSpecialSummonCard(player, card, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect)));
        }

        private bool ActivateSpiritWithEyesOfBlue(CleanPlayerState player, CleanCard spirit)
        {
            pendingSpiritZone = FindSpiritZone(player, spirit);
            if (pendingSpiritZone < 0)
            {
                return false;
            }

            bool canUseMausoleum = CanUseSpiritMausoleumEffect(player, spirit);
            bool canUseTribute = CanUseSpiritTributeEffect(player, spirit);
            if (canUseMausoleum && canUseTribute)
            {
                State.PendingChoice = new CleanChoicePrompt(
                    SpiritEffectChoicePromptId,
                    "Spirit with Eyes of Blue",
                    "Choose which once-per-turn effect to activate.",
                    new List<CleanChoiceOption>
                    {
                        new CleanChoiceOption(SpiritMausoleumEffectOptionId, "Mausoleum of White", "Take Mausoleum from the Deck.", true, string.Empty),
                        new CleanChoiceOption(SpiritTributeEffectOptionId, "Tribute and Summon", "Tribute Spirit to Special Summon a Blue-Eyes monster.", true, string.Empty)
                    });
                Log("Spirit with Eyes of Blue awaits an effect choice.");
                return false;
            }

            if (canUseMausoleum)
            {
                OpenSpiritMausoleumDestinationChoice();
                return false;
            }

            if (canUseTribute)
            {
                OpenSpiritSummonChoice(player);
                return false;
            }

            pendingSpiritZone = -1;
            return false;
        }

        private void ResolveSpiritEffectChoice(CleanPlayerState player, int optionId)
        {
            if (optionId == SpiritMausoleumEffectOptionId)
            {
                OpenSpiritMausoleumDestinationChoice();
            }
            else if (optionId == SpiritTributeEffectOptionId)
            {
                OpenSpiritSummonChoice(player);
            }
        }

        private void OpenSpiritMausoleumDestinationChoice(CleanPromptKind kind = CleanPromptKind.Choice)
        {
            State.PendingChoice = new CleanChoicePrompt(
                SpiritMausoleumDestinationChoicePromptId,
                "Spirit with Eyes of Blue",
                "Choose where to place Mausoleum of White.",
                new List<CleanChoiceOption>
                {
                    new CleanChoiceOption(SpiritMausoleumToHandOptionId, "Add to hand", "Add Mausoleum of White to your hand.", true, string.Empty),
                    new CleanChoiceOption(SpiritMausoleumToGraveOptionId, "Send to GY", "Send Mausoleum of White to the GY.", true, string.Empty)
                },
                kind,
                true);
            Log("Choose whether Mausoleum of White goes to the hand or GY.");
        }

        private void ResolveSpiritMausoleumDestinationChoice(CleanPlayerState player, int optionId)
        {
            CleanCard mausoleum;
            if (!TryTakeCard(player.Deck, MausoleumOfWhiteCardId, out mausoleum))
            {
                pendingSpiritZone = -1;
                return;
            }

            player.UsedOncePerTurn.Add(SpiritMausoleumOnceKey);
            if (optionId == SpiritMausoleumToGraveOptionId)
            {
                AddToGraveyard(player, mausoleum);
                Log("Spirit with Eyes of Blue sends Mausoleum of White from the Deck to the GY.");
            }
            else
            {
                ResetCardForHiddenZone(mausoleum);
                player.Hand.Add(mausoleum);
                Log("Spirit with Eyes of Blue adds Mausoleum of White from the Deck to the hand.");
            }

            pendingSpiritZone = -1;
        }

        private void OpenSpiritSummonChoice(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            AddSpiritSummonOptions(player, options, player.Hand, true);
            AddSpiritSummonOptions(player, options, player.Graveyard, false);
            State.PendingChoice = new CleanChoicePrompt(
                SpiritSummonChoicePromptId,
                "Spirit with Eyes of Blue",
                "Choose 1 Blue-Eyes monster from your hand or GY to Special Summon.",
                options,
                CleanPromptKind.SpecialSummon,
                true);
            Log("Spirit with Eyes of Blue awaits a Blue-Eyes summon choice.");
        }

        private void AddSpiritSummonOptions(CleanPlayerState player, List<CleanChoiceOption> options, List<CleanCard> cards, bool fromHand)
        {
            HashSet<int> seenIds = new HashSet<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCard card = cards[i];
                CleanSummonOrigin origin = fromHand ? CleanSummonOrigin.Hand : CleanSummonOrigin.Graveyard;
                if (!IsBlueEyesMonster(card) ||
                    !CanSpecialSummonCard(player, card, origin, CleanSummonMethod.Effect) ||
                    seenIds.Contains(card.Definition.Id))
                {
                    continue;
                }

                seenIds.Add(card.Definition.Id);
                int optionId = fromHand ? -card.Definition.Id : card.Definition.Id;
                string detail = fromHand ? "Special Summon from hand" : "Special Summon from GY";
                options.Add(new CleanChoiceOption(optionId, card.Definition.Name, detail, true, string.Empty));
            }
        }

        private void ResolveSpiritSummonChoice(CleanPlayerState player, int optionId)
        {
            bool fromHand = optionId < 0;
            int cardId = Math.Abs(optionId);
            List<CleanCard> source = fromHand ? player.Hand : player.Graveyard;
            int sourceIndex = IndexOfCard(source, cardId);
            CleanCard target = sourceIndex >= 0 ? source[sourceIndex] : null;
            CleanSummonOrigin origin = fromHand ? CleanSummonOrigin.Hand : CleanSummonOrigin.Graveyard;
            if (!PendingSpiritIsValid(player) ||
                target == null ||
                !IsBlueEyesMonster(target) ||
                !CanSpecialSummonCard(player, target, origin, CleanSummonMethod.Effect))
            {
                pendingSpiritZone = -1;
                return;
            }

            source.RemoveAt(sourceIndex);

            MovePendingSpiritToGrave(player);
            player.UsedOncePerTurn.Add(SpiritTributeOnceKey);
            int zone = FirstEmpty(player.Monsters);
            SummonToZone(player, target, zone, false, CleanMonsterStance.Attack);
            if (!fromHand && !IsNormalMonsterCard(target))
            {
                target.EffectsNegated = true;
                target.CannotAttack = true;
                Log("Spirit with Eyes of Blue Special Summons " + target.Definition.Name + " from the GY with its effects negated and it cannot attack.");
            }
            else
            {
                Log("Spirit with Eyes of Blue Special Summons " + target.Definition.Name + " from " + (fromHand ? "the hand" : "the GY") + ".");
            }

            pendingSpiritZone = -1;
        }

        private int FindMonsterZone(CleanPlayerState player, CleanCard target)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (object.ReferenceEquals(player.Monsters[i], target))
                {
                    return i;
                }
            }

            return -1;
        }

        private int FindSpiritZone(CleanPlayerState player, CleanCard target)
        {
            int mainZone = FindMonsterZone(player, target);
            if (mainZone >= 0)
            {
                return mainZone;
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            return object.ReferenceEquals(State.ExtraMonsters[extraZone], target) ? ExtraSpiritZoneOffset + extraZone : -1;
        }

        private bool PendingSpiritIsValid(CleanPlayerState player)
        {
            if (pendingSpiritZone >= 0 && pendingSpiritZone < CleanPlayerState.ZoneCount)
            {
                CleanCard spirit = player.Monsters[pendingSpiritZone];
                return spirit != null && spirit.Definition.Id == SpiritWithEyesOfBlueCardId;
            }

            int extraZone = pendingSpiritZone - ExtraSpiritZoneOffset;
            return extraZone == ExtraMonsterZoneIndexFor(player) &&
                   State.ExtraMonsters[extraZone] != null &&
                   State.ExtraMonsters[extraZone].Definition.Id == SpiritWithEyesOfBlueCardId;
        }

        private void MovePendingSpiritToGrave(CleanPlayerState player)
        {
            if (pendingSpiritZone >= 0 && pendingSpiritZone < CleanPlayerState.ZoneCount)
            {
                MoveMonsterToGrave(player, pendingSpiritZone);
                return;
            }

            int extraZone = pendingSpiritZone - ExtraSpiritZoneOffset;
            if (extraZone == ExtraMonsterZoneIndexFor(player) && State.ExtraMonsters[extraZone] != null)
            {
                CleanCard spirit = State.ExtraMonsters[extraZone];
                State.ExtraMonsters[extraZone] = null;
                ResetCardForHiddenZone(spirit);
                AddToGraveyard(player, spirit, true);
            }
        }

        private bool CanRoarSummonBlueEyes(CleanPlayerState player)
        {
            Predicate<CleanCard> predicate = HasFaceUpBlueEyesWhiteDragon(player) ? new Predicate<CleanCard>(IsBlueEyesMonster) : new Predicate<CleanCard>(IsBlueEyesWhiteDragonCard);
            return FirstEmpty(player.Monsters) >= 0 &&
                   (HasMatchingCard(player.Deck, card => predicate(card) && CanSpecialSummonCard(player, card, CleanSummonOrigin.Deck, CleanSummonMethod.Effect)) ||
                    HasMatchingCard(player.Graveyard, card => predicate(card) && CanSpecialSummonCard(player, card, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect)) ||
                    HasMatchingCard(player.Banished, card => predicate(card) && CanSpecialSummonCard(player, card, CleanSummonOrigin.Banished, CleanSummonMethod.Effect)));
        }

        private bool LuaRoarSummonBlueEyes(CleanPlayerState player)
        {
            Predicate<CleanCard> predicate = HasFaceUpBlueEyesWhiteDragon(player) ? new Predicate<CleanCard>(IsBlueEyesMonster) : new Predicate<CleanCard>(IsBlueEyesWhiteDragonCard);
            CleanCard card;
            string source;
            if (!TryTakeBestSpecialSummonTarget(player, predicate, out card, out source, true, false))
            {
                Log("Roar failed: no valid Blue-Eyes summon target.");
                return false;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                ReturnCardToSource(player, card, source);
                Log("Roar failed: no open Monster Zone.");
                return false;
            }

            SummonToZone(player, card, zone, false, CleanMonsterStance.Defense);
            Log("Roar special summons " + card.Definition.Name + " from " + source + " in Defense Position.");
            return true;
        }

        private bool LuaSpecialSummonBlueEyesWhiteDragon(CleanPlayerState player, CleanMonsterStance stance)
        {
            CleanCard card;
            string source;
            if (!TryTakeBestMonsterFromZones(player, IsBlueEyesWhiteDragonCard, out card, out source))
            {
                Log("Blue-Eyes White Dragon summon failed: no copy found in hand, deck, or GY.");
                return false;
            }

            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                ReturnCardToSource(player, card, source);
                Log("Blue-Eyes White Dragon summon failed: no open Monster Zone.");
                return false;
            }

            SummonToZone(player, card, zone, false, stance);
            Log("Clean special summons Blue-Eyes White Dragon from " + source + ".");
            return true;
        }

        private bool LuaSearchLevelOneLightTuner(CleanPlayerState player, int excludeId)
        {
            int index = BestCardInDeckForSearch(player, card => card.Definition.Id != excludeId && IsLevelOneLightTuner(card));
            if (index < 0)
            {
                Log("Sage search failed: no Level 1 LIGHT Tuner in deck.");
                return false;
            }

            CleanCard card = player.Deck[index];
            player.Deck.RemoveAt(index);
            ResetCardForHiddenZone(card);
            player.Hand.Add(card);
            Log("Sage adds " + card.Definition.Name + " from deck to hand.");
            return true;
        }

        private bool RecoverBlueEyesFromGraveyard(CleanPlayerState player, int selfId)
        {
            int selfIndex = IndexOfCard(player.Graveyard, selfId);
            int targetIndex = BestCardIndex(player.Graveyard, card => card.Definition.Id != selfId && IsBlueEyesCard(card));
            if (selfIndex < 0 || targetIndex < 0)
            {
                return false;
            }

            if (TryNegateRivalGraveyardEffect(player, "a graveyard recovery effect"))
            {
                return false;
            }

            CleanCard self = player.Graveyard[selfIndex];
            player.Graveyard.RemoveAt(selfIndex);
            if (targetIndex > selfIndex)
            {
                targetIndex--;
            }

            CleanCard target = player.Graveyard[targetIndex];
            player.Graveyard.RemoveAt(targetIndex);
            ResetCardForHiddenZone(self);
            player.Banished.Add(self);
            ResetCardForHiddenZone(target);
            player.Hand.Add(target);
            Log(self.Definition.Name + " is banished from the GY to recover " + target.Definition.Name + ".");
            return true;
        }

        private bool SendBlueEyesWhiteDragonToGraveFromHandOrDeck(CleanPlayerState player)
        {
            CleanCard card;
            bool fromHand = false;
            if (!TryTakeCard(player.Deck, BlueEyesWhiteDragonCardId, out card))
            {
                if (!TryTakeCard(player.Hand, BlueEyesWhiteDragonCardId, out card))
                {
                    Log("Could not send Blue-Eyes White Dragon for cost.");
                    return false;
                }

                fromHand = true;
            }

            ResetCardForHiddenZone(card);
            AddToGraveyard(player, card, fromHand);
            Log("Sent Blue-Eyes White Dragon to the GY for cost.");
            return true;
        }

        private bool DiscardFirstCard(CleanPlayerState player, CleanCard source)
        {
            for (int i = 0; i < player.Hand.Count; i++)
            {
                CleanCard candidate = player.Hand[i];
                if (object.ReferenceEquals(candidate, source))
                {
                    continue;
                }

                player.Hand.RemoveAt(i);
                ResetCardForHiddenZone(candidate);
                AddToGraveyard(player, candidate, true);
                Log("Discarded " + candidate.Definition.Name + " for cost.");
                return true;
            }

            return false;
        }

        private bool AddFromDeckToHand(CleanPlayerState player, Predicate<CleanCard> predicate, string label)
        {
            int index = BestCardInDeckForSearch(player, predicate);
            if (index < 0)
            {
                Log("Search failed: no " + label + " in deck.");
                return false;
            }

            CleanCard card = player.Deck[index];
            player.Deck.RemoveAt(index);
            ResetCardForHiddenZone(card);
            player.Hand.Add(card);
            Log("Added " + card.Definition.Name + " from deck to hand.");
            return true;
        }

        private void OpenVeilerTargetChoice(CleanPlayerState rival)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = rival.Monsters[i];
                if (monster != null &&
                    !monster.FaceDown &&
                    !monster.EffectsNegated &&
                    !IsNormalMonster(monster) &&
                    !IsProtectedFromCleanTargeting(rival, monster))
                {
                    options.Add(new CleanChoiceOption(i, monster.Definition.Name, "Negate until the end of this turn", true, string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                VeilerTargetChoicePromptId,
                "Effect Veiler",
                "Choose 1 face-up Effect Monster your opponent controls.",
                options);
        }

        private void ResolveVeilerTargetChoice(CleanPlayerState player, CleanPlayerState rival, int zone)
        {
            if (zone < 0 || zone >= CleanPlayerState.ZoneCount)
            {
                return;
            }

            CleanCard target = rival.Monsters[zone];
            if (target == null ||
                target.FaceDown ||
                target.EffectsNegated ||
                IsNormalMonster(target) ||
                IsProtectedFromCleanTargeting(rival, target))
            {
                Log("Effect Veiler failed: the selected monster is no longer a legal target.");
                return;
            }

            target.EffectsNegated = true;
            target.EffectsNegatedUntilEndOfTurn = true;
            Log("Effect Veiler negates " + target.Definition.Name + " until the end of this turn.");
        }
        private int StrongestFaceUpEffectMonsterZone(CleanPlayerState player)
        {
            int strongestZone = -1;
            int strongestAttack = -1;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster == null ||
                    monster.FaceDown ||
                    monster.EffectsNegated ||
                    IsNormalMonster(monster) ||
                    IsProtectedFromCleanTargeting(player, monster))
                {
                    continue;
                }

                if (monster.CurrentAttack > strongestAttack)
                {
                    strongestAttack = monster.CurrentAttack;
                    strongestZone = i;
                }
            }

            return strongestZone;
        }

        private bool NegateStrongestRivalMonsterUntilEndOfTurn(CleanPlayerState rival)
        {
            int zone = StrongestFaceUpEffectMonsterZone(rival);
            if (zone < 0)
            {
                Log("Effect Veiler found no valid face-up Effect Monster to negate.");
                return false;
            }

            CleanCard target = rival.Monsters[zone];
            TriggerMaidenTargetedByCardEffect(rival, zone);
            target.EffectsNegated = true;
            target.EffectsNegatedUntilEndOfTurn = true;
            Log("Effect Veiler negates " + target.Definition.Name + " until the end of this turn.");
            return true;
        }

        private bool DestroyStrongestFaceUpCard(CleanPlayerState player)
        {
            int spellTrapZone = FirstDestructibleSpellTrapZone(player);
            int monsterZone = StrongestDestructibleEffectTargetZone(player);
            if (spellTrapZone >= 0)
            {
                CleanCard destroyed = player.SpellsTraps[spellTrapZone];
                string name = destroyed.Definition.Name;
                MoveSpellTrapToGrave(player, spellTrapZone);
                TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                Log("Ghost Ogre destroys " + name + ".");
                TriggerBlueEyesUltimateSpiritDestroyed(player, destroyed);
                return true;
            }

            if (player.FieldSpell != null && !player.FieldSpell.FaceDown && !IsProtectedByJetFromCardEffect(player, player.FieldSpell))
            {
                CleanCard destroyed = player.FieldSpell;
                string name = destroyed.Definition.Name;
                MoveFieldSpellToGrave(player);
                TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                Log("Ghost Ogre destroys " + name + ".");
                TriggerBlueEyesUltimateSpiritDestroyed(player, destroyed);
                return true;
            }

            if (monsterZone >= 0)
            {
                CleanCard destroyed = player.Monsters[monsterZone];
                string name = destroyed.Definition.Name;
                MoveMonsterToGrave(player, monsterZone);
                TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                Log("Ghost Ogre destroys " + name + ".");
                TriggerBlueEyesUltimateSpiritDestroyed(player, destroyed);
                return true;
            }

            Log("Ghost Ogre found no face-up card to destroy.");
            return false;
        }

        private bool BanishBestSpellTrap(CleanPlayerState player)
        {
            int zone = StrongestSpellTrapZone(player);
            if (zone >= 0)
            {
                CleanCard card = player.SpellsTraps[zone];
                string name = card.Definition.Name;
                player.SpellsTraps[zone] = null;
                ResetCardForHiddenZone(card);
                player.Banished.Add(card);
                Log("Dragon Spirit of White banishes " + name + ".");
                return true;
            }

            if (player.FieldSpell != null)
            {
                CleanCard card = player.FieldSpell;
                string name = card.Definition.Name;
                player.FieldSpell = null;
                ResetCardForHiddenZone(card);
                player.Banished.Add(card);
                Log("Dragon Spirit of White banishes " + name + ".");
                return true;
            }

            return false;
        }
        private int StrongestSpellTrapZone(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.SpellsTraps[i];
                if (card != null && !card.FaceDown)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool CanDragonSpiritTributeForBlueEyes(CleanPlayerState player, CleanPlayerState rival)
        {
            return rival != null && MonsterCount(rival) > 0 && IndexOfCard(player.Hand, BlueEyesWhiteDragonCardId) >= 0;
        }

        private bool HasFaceUpCard(CleanPlayerState player)
        {
            return StrongestEffectTargetZone(player) >= 0 || StrongestSpellTrapZone(player) >= 0 || HasFaceUpFieldSpell(player);
        }

        private bool HasSpellTrap(CleanPlayerState player)
        {
            return StrongestSpellTrapZone(player) >= 0 || player.FieldSpell != null;
        }

        private bool HasFaceUpFieldSpell(CleanPlayerState player)
        {
            return player.FieldSpell != null && !player.FieldSpell.FaceDown;
        }
        private bool HasOwnEffectMonsterOnField(CleanPlayerState player)
        {
            return FirstOwnEffectMonsterZone(player, -1) >= 0;
        }

        private int FirstOwnEffectMonsterZone(CleanPlayerState player, int excludeId)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && monster.Definition.Id != excludeId && !IsNormalMonster(monster))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool HasBlueEyesWhiteDragonInHandDeckOrGrave(CleanPlayerState player)
        {
            return IndexOfCard(player.Hand, BlueEyesWhiteDragonCardId) >= 0 || IndexOfCard(player.Deck, BlueEyesWhiteDragonCardId) >= 0 || IndexOfCard(player.Graveyard, BlueEyesWhiteDragonCardId) >= 0;
        }

        private bool HasBlueEyesWhiteDragonInHandOrDeck(CleanPlayerState player)
        {
            return IndexOfCard(player.Hand, BlueEyesWhiteDragonCardId) >= 0 || IndexOfCard(player.Deck, BlueEyesWhiteDragonCardId) >= 0;
        }

        private bool HasBlueEyesWhiteDragonOnFieldOrGrave(CleanPlayerState player)
        {
            return HasFaceUpBlueEyesWhiteDragon(player) || HasMatchingCard(player.Graveyard, IsTreatedAsBlueEyesWhiteDragon);
        }

        private bool HasFaceUpBlueEyesWhiteDragon(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && IsTreatedAsBlueEyesWhiteDragon(monster))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasBlueEyesDiscardCost(CleanPlayerState player)
        {
            return HasMatchingCard(player.Hand, IsBlueEyesWhiteDragonOrMention);
        }

        private bool TryTakeDiscardCost(List<CleanCard> cards, Predicate<CleanCard> predicate, out CleanCard card)
        {
            int index = BestCardIndex(cards, predicate);
            if (index < 0)
            {
                card = null;
                return false;
            }

            card = cards[index];
            cards.RemoveAt(index);
            return true;
        }

        private int BestCardIndex(List<CleanCard> cards, Predicate<CleanCard> predicate)
        {
            int bestIndex = -1;
            int bestScore = -1;
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCard card = cards[i];
                if (!predicate(card))
                {
                    continue;
                }

                int score = card.Definition.Attack + card.Definition.Level * 100;
                if (IsBlueEyesCard(card))
                {
                    score += 100000;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private bool HasLightTunerOnField(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (IsLightTuner(player.Monsters[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsLightTuner(CleanCard card)
        {
            if (card == null || card.FaceDown || !IsLightMonster(card))
            {
                return false;
            }

            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Tuner", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OpenNeoKaiserLevelChoice(CleanPlayerState player)
        {
            if (player.Id != CleanPlayerId.Local)
            {
                for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
                {
                    CleanCard tuner = player.Monsters[i];
                    if (IsLightTuner(tuner))
                    {
                        tuner.LevelModifier++;
                        Log("Neo Kaiser Sea Horse increases " + tuner.Definition.Name + "'s Level by 1.");
                        return;
                    }
                }

                return;
            }

            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard tuner = player.Monsters[i];
                if (!IsLightTuner(tuner))
                {
                    continue;
                }

                options.Add(new CleanChoiceOption(
                    tuner.RuntimeId * 10 + 1,
                    tuner.Definition.Name + ": increase",
                    "Change Level " + tuner.CurrentLevel + " to " + (tuner.CurrentLevel + 1) + ".",
                    true,
                    string.Empty,
                    tuner.Definition.Id));
                if (tuner.CurrentLevel > 1)
                {
                    options.Add(new CleanChoiceOption(
                        tuner.RuntimeId * 10 + 2,
                        tuner.Definition.Name + ": decrease",
                        "Change Level " + tuner.CurrentLevel + " to " + (tuner.CurrentLevel - 1) + ".",
                        true,
                        string.Empty,
                        tuner.Definition.Id));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                NeoKaiserLevelChoicePromptId,
                "Neo Kaiser Sea Horse",
                "Choose a LIGHT Tuner you control, then increase or decrease its Level by 1.",
                options,
                CleanPromptKind.CardEffect,
                false);
        }

        private void ResolveNeoKaiserLevelChoice(CleanPlayerState player, int encodedChoice)
        {
            int mode = encodedChoice % 10;
            int runtimeId = encodedChoice / 10;
            CleanCard target = FindMonsterByRuntimeId(player, runtimeId);
            int delta = mode == 1 ? 1 : mode == 2 ? -1 : 0;
            if (!IsLightTuner(target) || delta == 0 || target.CurrentLevel + delta < 1)
            {
                Log("Neo Kaiser Sea Horse could not change the selected Tuner's Level.");
                return;
            }

            target.LevelModifier += delta;
            Log("Neo Kaiser Sea Horse changes " + target.Definition.Name + " to Level " + target.CurrentLevel + ".");
        }

        private void ResolveNeoKaiserSentFromFieldTrigger(CleanPlayerState player, CleanCard neoKaiser)
        {
            if (neoKaiser == null ||
                player.UsedOncePerTurn.Contains(NeoKaiserSendOnceKey) ||
                !HasMatchingCard(player.Deck, IsNeoKaiserSendTarget))
            {
                return;
            }

            if (player.Id != CleanPlayerId.Local)
            {
                int best = BestCardIndex(player.Deck, IsNeoKaiserSendTarget);
                if (best >= 0)
                {
                    ResolveNeoKaiserSendChoice(player, player.Deck[best].Definition.Id);
                }

                return;
            }

            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard target = player.Deck[i];
                if (IsNeoKaiserSendTarget(target) && seen.Add(target.Definition.Id))
                {
                    options.Add(new CleanChoiceOption(
                        target.Definition.Id,
                        target.Definition.Name,
                        "Send this card from the Deck to the GY.",
                        true,
                        string.Empty,
                        target.Definition.Id));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                NeoKaiserSendChoicePromptId,
                "Neo Kaiser Sea Horse",
                "Neo Kaiser Sea Horse was sent from the field to the GY. Choose a valid monster to send from the Deck.",
                options,
                CleanPromptKind.TriggerEffect,
                true);
            Log("Neo Kaiser Sea Horse's sent-to-GY Trigger Effect awaits a Deck choice.");
        }

        private bool IsNeoKaiserSendTarget(CleanCard card)
        {
            if (card == null ||
                card.Definition.Kind != CleanCardKind.Monster ||
                card.Definition.Id == NeoKaiserSeaHorseCardId)
            {
                return false;
            }

            string text = card.Definition.Text ?? string.Empty;
            return IsBlueEyesCard(card) ||
                   text.IndexOf("Blue-Eyes White Dragon", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ResolveNeoKaiserSendChoice(CleanPlayerState player, int cardId)
        {
            int index = -1;
            for (int i = 0; i < player.Deck.Count; i++)
            {
                if (player.Deck[i].Definition.Id == cardId && IsNeoKaiserSendTarget(player.Deck[i]))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                Log("Neo Kaiser Sea Horse could not send the selected monster from the Deck.");
                return;
            }

            CleanCard target = player.Deck[index];
            player.Deck.RemoveAt(index);
            ResetCardForHiddenZone(target);
            AddToGraveyard(player, target);
            player.UsedOncePerTurn.Add(NeoKaiserSendOnceKey);
            Log("Neo Kaiser Sea Horse sends " + target.Definition.Name + " from the Deck to the GY.");
        }

        private bool HasTargetableMonster(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !IsProtectedFromCleanTargeting(player, monster))
                {
                    return true;
                }
            }

            CleanCard extra = State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)];
            return extra != null && !IsProtectedFromCleanTargeting(player, extra);
        }

        private void ActivateAlternativeDestroy(CleanPlayerState controller, CleanPlayerState rival)
        {
            if (controller.Id != CleanPlayerId.Local)
            {
                int zone = StrongestEffectTargetZone(rival);
                if (zone >= 0)
                {
                    ResolveAlternativeDestroyChoice(controller, rival, zone);
                }

                return;
            }

            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = rival.Monsters[i];
                if (monster != null && !IsProtectedFromCleanTargeting(rival, monster))
                {
                    options.Add(new CleanChoiceOption(
                        i,
                        monster.PublicName,
                        "Target and destroy this monster.",
                        true,
                        string.Empty,
                        monster.Definition.Id));
                }
            }

            int extraZone = ExtraMonsterZoneIndexFor(rival);
            CleanCard extra = State.ExtraMonsters[extraZone];
            if (extra != null && !IsProtectedFromCleanTargeting(rival, extra))
            {
                options.Add(new CleanChoiceOption(
                    ExtraSpiritZoneOffset + extraZone,
                    extra.PublicName,
                    "Target and destroy this Extra Monster Zone monster.",
                    true,
                    string.Empty,
                    extra.Definition.Id));
            }

            State.PendingChoice = new CleanChoicePrompt(
                AlternativeDestroyChoicePromptId,
                "Blue-Eyes Alternative White Dragon",
                "Choose 1 monster your opponent controls to destroy. Alternative cannot attack this turn.",
                options,
                CleanPromptKind.CardEffect,
                false);
        }

        private void ResolveAlternativeDestroyChoice(CleanPlayerState controller, CleanPlayerState rival, int encodedZone)
        {
            CleanCard target;
            bool extraZone = encodedZone >= ExtraSpiritZoneOffset;
            int zone = extraZone ? encodedZone - ExtraSpiritZoneOffset : encodedZone;
            if (extraZone)
            {
                target = zone == ExtraMonsterZoneIndexFor(rival) ? State.ExtraMonsters[zone] : null;
            }
            else
            {
                target = zone >= 0 && zone < CleanPlayerState.ZoneCount ? rival.Monsters[zone] : null;
            }

            if (target == null || IsProtectedFromCleanTargeting(rival, target))
            {
                Log("Blue-Eyes Alternative White Dragon lost its target.");
                return;
            }

            if (IsProtectedFromOpponentEffectDestruction(rival, target))
            {
                Log(target.Definition.Name + " is protected from Blue-Eyes Alternative White Dragon's destruction effect.");
                return;
            }

            if (extraZone)
            {
                State.ExtraMonsters[zone] = null;
                SendEquipsForLeavingMonster(rival, target);
                ResetCardForHiddenZone(target);
                SendFieldCardToGraveOrFaceUpExtra(rival, target, true);
            }
            else
            {
                MoveMonsterToGrave(rival, zone);
            }

            TriggerJetSummonAfterFieldCardDestroyed(target);
            TriggerBlueEyesUltimateSpiritDestroyed(rival, target);
            TriggerDragonMasterMagiaLeavesField(rival, target);
            Log("Blue-Eyes Alternative White Dragon destroys " + target.Definition.Name + ".");
        }

        private bool HasMonsterRebornTarget(CleanPlayerState controller)
        {
            return HasMatchingCard(controller.Graveyard, card => IsMonsterRebornTarget(controller, card)) ||
                   HasMatchingCard(OpponentOf(controller.Id).Graveyard, card => IsMonsterRebornTarget(controller, card));
        }

        private bool IsMonsterRebornTarget(CleanPlayerState controller, CleanCard card)
        {
            return card != null &&
                   card.Definition.Kind == CleanCardKind.Monster &&
                   CanSpecialSummonCard(controller, card, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect);
        }

        private void ActivateMonsterReborn(CleanPlayerState controller, CleanPlayerState rival)
        {
            if (controller.Id != CleanPlayerId.Local)
            {
                CleanCard best = null;
                List<CleanCard> source = null;
                FindBestMonsterRebornTarget(controller, controller.Graveyard, ref best, ref source);
                FindBestMonsterRebornTarget(controller, rival.Graveyard, ref best, ref source);
                if (best != null)
                {
                    ResolveMonsterRebornChoice(controller, best.RuntimeId);
                }

                return;
            }

            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            AddMonsterRebornOptions(controller, controller.Graveyard, "your GY", options);
            AddMonsterRebornOptions(controller, rival.Graveyard, "opponent's GY", options);
            State.PendingChoice = new CleanChoicePrompt(
                MonsterRebornChoicePromptId,
                "Monster Reborn",
                "Choose 1 legally Summonable monster in either GY.",
                options,
                CleanPromptKind.SpecialSummon,
                false);
        }

        private void AddMonsterRebornOptions(CleanPlayerState controller, List<CleanCard> graveyard, string source, List<CleanChoiceOption> options)
        {
            for (int i = 0; i < graveyard.Count; i++)
            {
                CleanCard card = graveyard[i];
                if (IsMonsterRebornTarget(controller, card))
                {
                    options.Add(new CleanChoiceOption(
                        card.RuntimeId,
                        card.Definition.Name,
                        "Special Summon from " + source + ".",
                        true,
                        string.Empty,
                        card.Definition.Id));
                }
            }
        }

        private void FindBestMonsterRebornTarget(CleanPlayerState controller, List<CleanCard> graveyard, ref CleanCard best, ref List<CleanCard> source)
        {
            for (int i = 0; i < graveyard.Count; i++)
            {
                CleanCard card = graveyard[i];
                if (IsMonsterRebornTarget(controller, card) &&
                    (best == null || card.CurrentAttack > best.CurrentAttack))
                {
                    best = card;
                    source = graveyard;
                }
            }
        }

        private void ResolveMonsterRebornChoice(CleanPlayerState controller, int runtimeId)
        {
            CleanPlayerState rival = OpponentOf(controller.Id);
            CleanCard target = FindCardByRuntimeId(controller.Graveyard, runtimeId);
            List<CleanCard> source = controller.Graveyard;
            if (target == null)
            {
                target = FindCardByRuntimeId(rival.Graveyard, runtimeId);
                source = rival.Graveyard;
            }

            if (target == null ||
                FirstEmpty(controller.Monsters) < 0 ||
                !IsMonsterRebornTarget(controller, target))
            {
                Log("Monster Reborn could not Special Summon the selected monster.");
                return;
            }

            source.Remove(target);
            ResetCardForHiddenZone(target);
            SummonToZone(controller, target, FirstEmpty(controller.Monsters), false, CleanMonsterStance.Attack);
            Log("Monster Reborn Special Summons " + target.Definition.Name + " from the GY.");
        }

        private bool IsAbyssSpellSearchTarget(CleanCard card)
        {
            return card != null &&
                   card.Definition.Kind == CleanCardKind.Spell &&
                   (card.Definition.Id == PolymerizationCardId || IsRitualSpellCard(card));
        }

        private void ResolveAbyssSpecialSummonTrigger(CleanPlayerState player)
        {
            if (player.UsedOncePerTurn.Contains(AbyssSummonSearchOnceKey) ||
                !HasBlueEyesWhiteDragonOnFieldOrGrave(player) ||
                !HasMatchingCard(player.Deck, IsAbyssSpellSearchTarget))
            {
                return;
            }

            if (player.Id != CleanPlayerId.Local)
            {
                int index = BestCardIndex(player.Deck, IsAbyssSpellSearchTarget);
                if (index >= 0)
                {
                    ResolveAbyssSearchChoice(player, player.Deck[index].Definition.Id);
                }

                return;
            }

            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard target = player.Deck[i];
                if (IsAbyssSpellSearchTarget(target) && seen.Add(target.Definition.Id))
                {
                    options.Add(new CleanChoiceOption(
                        target.Definition.Id,
                        target.Definition.Name,
                        "Add this Ritual Spell or Polymerization from Deck to hand.",
                        true,
                        string.Empty,
                        target.Definition.Id));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                AbyssSearchChoicePromptId,
                "Blue-Eyes Abyss Dragon",
                "Blue-Eyes Abyss Dragon was Special Summoned. Choose a Ritual Spell or Polymerization to add.",
                options,
                CleanPromptKind.TriggerEffect,
                true);
        }

        private void ResolveAbyssSearchChoice(CleanPlayerState player, int cardId)
        {
            int index = -1;
            for (int i = 0; i < player.Deck.Count; i++)
            {
                if (player.Deck[i].Definition.Id == cardId && IsAbyssSpellSearchTarget(player.Deck[i]))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0 || !HasBlueEyesWhiteDragonOnFieldOrGrave(player))
            {
                Log("Blue-Eyes Abyss Dragon could not add the selected Spell.");
                return;
            }

            CleanCard target = player.Deck[index];
            player.Deck.RemoveAt(index);
            ResetCardForHiddenZone(target);
            player.Hand.Add(target);
            player.UsedOncePerTurn.Add(AbyssSummonSearchOnceKey);
            Log("Blue-Eyes Abyss Dragon adds " + target.Definition.Name + " from the Deck to the hand.");
        }

        private void ResolveAbyssEndPhaseTrigger(CleanPlayerState player)
        {
            if (player.UsedOncePerTurn.Contains(AbyssEndPhaseSearchOnceKey) ||
                !HasBlueEyesWhiteDragonOnFieldOrGrave(player) ||
                !HasFaceUpAbyssDragon(player) ||
                !HasMatchingCard(player.Deck, IsLevelEightOrHigherDragon))
            {
                return;
            }

            if (player.Id != CleanPlayerId.Local)
            {
                int index = BestCardIndex(player.Deck, IsLevelEightOrHigherDragon);
                if (index >= 0)
                {
                    ResolveAbyssEndPhaseChoice(player, player.Deck[index].Definition.Id);
                }

                return;
            }

            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < player.Deck.Count; i++)
            {
                CleanCard target = player.Deck[i];
                if (IsLevelEightOrHigherDragon(target) && seen.Add(target.Definition.Id))
                {
                    options.Add(new CleanChoiceOption(
                        target.Definition.Id,
                        target.Definition.Name,
                        "Add this Level 8 or higher Dragon from Deck to hand.",
                        true,
                        string.Empty,
                        target.Definition.Id));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                AbyssEndPhaseChoicePromptId,
                "Blue-Eyes Abyss Dragon",
                "During your End Phase, choose a Level 8 or higher Dragon monster to add from the Deck.",
                options,
                CleanPromptKind.TriggerEffect,
                true);
        }

        private bool HasFaceUpAbyssDragon(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null &&
                    !monster.FaceDown &&
                    !monster.EffectsNegated &&
                    monster.Definition.Id == BlueEyesAbyssDragonCardId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsLevelEightOrHigherDragon(CleanCard card)
        {
            return card != null &&
                   card.Definition.Kind == CleanCardKind.Monster &&
                   card.CurrentLevel >= 8 &&
                   IsDragonMonster(card);
        }

        private bool HasLevelEightOrHigherDragonOnField(CleanPlayerState player)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && IsLevelEightOrHigherDragon(monster))
                {
                    return true;
                }
            }

            CleanCard extra = State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)];
            return extra != null && !extra.FaceDown && IsLevelEightOrHigherDragon(extra);
        }

        private void ResolveAbyssEndPhaseChoice(CleanPlayerState player, int cardId)
        {
            int index = -1;
            for (int i = 0; i < player.Deck.Count; i++)
            {
                if (player.Deck[i].Definition.Id == cardId && IsLevelEightOrHigherDragon(player.Deck[i]))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0 ||
                !HasFaceUpAbyssDragon(player) ||
                !HasBlueEyesWhiteDragonOnFieldOrGrave(player))
            {
                Log("Blue-Eyes Abyss Dragon could not resolve its End Phase search.");
                return;
            }

            CleanCard target = player.Deck[index];
            player.Deck.RemoveAt(index);
            ResetCardForHiddenZone(target);
            player.Hand.Add(target);
            player.UsedOncePerTurn.Add(AbyssEndPhaseSearchOnceKey);
            Log("Blue-Eyes Abyss Dragon adds " + target.Definition.Name + " from the Deck to the hand during the End Phase.");
        }

        private void BoostLevelEightOrHigherDragons(CleanPlayerState player, int amount)
        {
            int boosted = 0;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && !monster.FaceDown && IsLevelEightOrHigherDragon(monster))
                {
                    monster.AttackBonus += amount;
                    boosted++;
                }
            }

            CleanCard extra = State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)];
            if (extra != null && !extra.FaceDown && IsLevelEightOrHigherDragon(extra))
            {
                extra.AttackBonus += amount;
                boosted++;
            }

            Log("Blue-Eyes Abyss Dragon gives " + boosted + " Level 8 or higher Dragon monster(s) " + amount + " ATK.");
        }

        private void ResolveSummonTrigger(CleanPlayerState player, CleanPlayerState rival, CleanCard card, bool normalSummon)
        {
            if (card == null || card.FaceDown)
            {
                return;
            }

            if (normalSummon && card.Definition.Id == SageWithEyesOfBlueCardId)
            {
                OpenSageNormalSummonSearchChoice(player);
            }

            if (card.Definition.Id == DragonSpiritOfWhiteCardId)
            {
                ResolveDragonSpiritSummonTrigger(player, rival);
            }

            if (!normalSummon && card.Definition.Id == BlueEyesAbyssDragonCardId)
            {
                ResolveAbyssSpecialSummonTrigger(player);
            }

            if (!normalSummon && IsTreatedAsBlueEyesWhiteDragon(card))
            {
                if (player.Id == CleanPlayerId.Local)
                {
                    OpenMaidenWhiteReviveChoice(player);
                }
                else
                {
                    ReviveMaidenOfWhiteFromGrave(player, 0);
                }
            }
        }

        private bool CanReviveMaidenOfWhiteFromGrave(CleanPlayerState player)
        {
            if (player == null ||
                player.UsedOncePerTurn.Contains(MaidenWhiteReviveOnceKey) ||
                FirstEmpty(player.Monsters) < 0)
            {
                return false;
            }

            for (int i = 0; i < player.Graveyard.Count; i++)
            {
                CleanCard card = player.Graveyard[i];
                if (card.Definition.Id == MaidenOfWhiteCardId && CanSpecialSummonUnderSpiritRestriction(player, card))
                {
                    return true;
                }
            }

            return false;
        }

        private void OpenMaidenWhiteReviveChoice(CleanPlayerState player)
        {
            if (!CanReviveMaidenOfWhiteFromGrave(player))
            {
                return;
            }

            List<CleanChoiceOption> options = new List<CleanChoiceOption>
            {
                new CleanChoiceOption(
                    MaidenWhiteReviveSkipOptionId,
                    "Do not activate",
                    "Leave Maiden of White in the GY.",
                    true,
                    string.Empty)
            };

            for (int i = 0; i < player.Graveyard.Count; i++)
            {
                CleanCard card = player.Graveyard[i];
                if (card.Definition.Id == MaidenOfWhiteCardId && CanSpecialSummonUnderSpiritRestriction(player, card))
                {
                    options.Add(new CleanChoiceOption(
                        card.RuntimeId,
                        "Activate Maiden of White",
                        "Special Summon this Maiden of White from the GY.",
                        true,
                        string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                MaidenWhiteReviveChoicePromptId,
                "Maiden of White",
                "Blue-Eyes White Dragon was Special Summoned. Activate Maiden of White's separate optional Trigger Effect?",
                options,
                CleanPromptKind.TriggerEffect,
                false);
            Log("Maiden of White's optional GY Trigger Effect awaits a choice.");
        }

        private void ResolveMaidenWhiteReviveChoice(CleanPlayerState player, int runtimeId)
        {
            if (runtimeId == MaidenWhiteReviveSkipOptionId)
            {
                Log("Maiden of White's optional GY Trigger Effect was not activated.");
                return;
            }

            ReviveMaidenOfWhiteFromGrave(player, runtimeId);
        }

        private bool ReviveMaidenOfWhiteFromGrave(CleanPlayerState player, int runtimeId)
        {
            if (!CanReviveMaidenOfWhiteFromGrave(player))
            {
                return false;
            }

            CleanCard maiden = runtimeId == 0
                ? player.Graveyard.Find(card => card.Definition.Id == MaidenOfWhiteCardId && CanSpecialSummonUnderSpiritRestriction(player, card))
                : FindCardByRuntimeId(player.Graveyard, runtimeId);
            if (maiden == null || maiden.Definition.Id != MaidenOfWhiteCardId || !CanSpecialSummonUnderSpiritRestriction(player, maiden))
            {
                return false;
            }

            int zone = FirstEmpty(player.Monsters);
            player.Graveyard.Remove(maiden);
            SummonToZone(player, maiden, zone, false, CleanMonsterStance.Attack);
            player.UsedOncePerTurn.Add(MaidenWhiteReviveOnceKey);
            Log("Maiden of White special summons itself from the GY after Blue-Eyes White Dragon was Special Summoned.");
            return true;
        }

        private bool IsBlueEyesWhiteDragonCard(CleanCard card)
        {
            return card != null && card.Definition.Id == BlueEyesWhiteDragonCardId;
        }

        private bool IsTreatedAsBlueEyesWhiteDragon(CleanCard card)
        {
            return card != null &&
                   (card.Definition.Id == BlueEyesWhiteDragonCardId ||
                    card.Definition.Id == BlueEyesAlternativeWhiteDragonCardId);
        }

        private bool IsBlueEyesWhiteDragonOrMention(CleanCard card)
        {
            if (card == null)
            {
                return false;
            }

            string text = card.Definition.Text ?? string.Empty;
            return card.Definition.Id == BlueEyesWhiteDragonCardId ||
                   text.IndexOf("Blue-Eyes White Dragon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   IsBlueEyesSpellTrapSupport(card);
        }

        private bool IsLevelOneLightTuner(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster || card.Definition.Level != 1)
            {
                return false;
            }

            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Tuner", StringComparison.OrdinalIgnoreCase) >= 0 || card.Definition.Name.IndexOf("with Eyes of Blue", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsBlueEyesSpellTrapSupport(CleanCard card)
        {
            if (card == null || card.Definition.Kind == CleanCardKind.Monster || card.Definition.Id == BingoMachineCardId)
            {
                return false;
            }

            string text = card.Definition.Text ?? string.Empty;
            return text.IndexOf("Blue-Eyes White Dragon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   text.IndexOf("Blue-Eyes Ultimate Dragon", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsBingoSearchCandidate(CleanCard card)
        {
            return card != null && (IsBlueEyesMonster(card) || IsBlueEyesSpellTrapSupport(card));
        }

        private bool IsGatewayToChaosSearchCandidate(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            string name = card.Definition.Name ?? string.Empty;
            bool blackLusterSoldierRitual = IsRitualMonster(card) && name.IndexOf("Black Luster Soldier", StringComparison.OrdinalIgnoreCase) >= 0;
            bool gaiaTheFierceKnight = name.IndexOf("Gaia The Fierce Knight", StringComparison.OrdinalIgnoreCase) >= 0;
            return blackLusterSoldierRitual || gaiaTheFierceKnight;
        }

        private bool IsRitualSpellCard(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Spell)
            {
                return false;
            }

            return card.Definition.Id == ChaosFormCardId || (card.Definition.TypeLine ?? string.Empty).IndexOf("Ritual", StringComparison.OrdinalIgnoreCase) >= 0 || (card.Definition.Name ?? string.Empty).IndexOf("Ritual", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsMausoleumOfWhite(CleanCard card)
        {
            return card != null && card.Definition.Id == 24382602;
        }

        private bool IsDragonMonster(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Dragon", StringComparison.OrdinalIgnoreCase) >= 0 || IsBlueEyesCard(card);
        }

        private bool CanSpecialSummonFromExtraUnderTurnRestriction(CleanPlayerState player, CleanCard card)
        {
            return !player.ExtraDeckDragonOnlyThisTurn || IsDragonMonster(card);
        }

        private bool CanSpecialSummonCard(
            CleanPlayerState player,
            CleanCard card,
            CleanSummonOrigin origin,
            CleanSummonMethod method)
        {
            if (player == null || card == null || card.Definition.Kind != CleanCardKind.Monster ||
                !CanSpecialSummonUnderSpiritRestriction(player, card))
            {
                return false;
            }

            if (origin == CleanSummonOrigin.ExtraDeck && !CanSpecialSummonFromExtraUnderTurnRestriction(player, card))
            {
                return false;
            }

            if (card.Definition.Id == BlueEyesAlternativeWhiteDragonCardId)
            {
                if (origin == CleanSummonOrigin.Hand)
                {
                    return method == CleanSummonMethod.AlternativeProcedure;
                }

                return (origin == CleanSummonOrigin.Graveyard || origin == CleanSummonOrigin.Banished) &&
                       card.ProperlySummoned;
            }

            if (IsStrictRitualSummonOnly(card))
            {
                return method == CleanSummonMethod.Ritual;
            }

            if (IsStrictFusionSummonOnly(card))
            {
                return method == CleanSummonMethod.Fusion;
            }

            if (IsRitualMonster(card))
            {
                if (method == CleanSummonMethod.Ritual)
                {
                    return true;
                }

                return (origin == CleanSummonOrigin.Graveyard || origin == CleanSummonOrigin.Banished) &&
                       card.ProperlySummoned;
            }

            if (IsExtraDeckMonsterDefinition(card.Definition) &&
                (origin == CleanSummonOrigin.Graveyard || origin == CleanSummonOrigin.Banished) &&
                method == CleanSummonMethod.Effect)
            {
                return card.ProperlySummoned;
            }

            return true;
        }

        private bool IsStrictRitualSummonOnly(CleanCard card)
        {
            if (card == null)
            {
                return false;
            }

            if (card.Definition.Id == BlueEyesChaosMaxDragonCardId ||
                card.Definition.Id == BlueEyesChaosDragonCardId ||
                card.Definition.Id == BlackLusterSoldierLegendarySwordsmanCardId)
            {
                return true;
            }

            string text = card.Definition.Text ?? string.Empty;
            return text.IndexOf("Must be Ritual Summoned", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsStrictFusionSummonOnly(CleanCard card)
        {
            if (card == null)
            {
                return false;
            }

            if (card.Definition.Id == DragonMasterMagiaCardId)
            {
                return true;
            }

            string text = card.Definition.Text ?? string.Empty;
            return text.IndexOf("Must be Fusion Summoned", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool CanSpecialSummonUnderSpiritRestriction(CleanPlayerState player, CleanCard card)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster != null && monster.Definition.Id == SpiritWithEyesOfBlueCardId && !monster.FaceDown && !monster.EffectsNegated)
                {
                    return IsDragonMonster(card);
                }
            }

            CleanCard extraMonster = State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)];
            if (extraMonster != null && extraMonster.Definition.Id == SpiritWithEyesOfBlueCardId && !extraMonster.FaceDown && !extraMonster.EffectsNegated)
            {
                return IsDragonMonster(card);
            }

            return true;
        }

        private bool IsNormalMonsterCard(CleanCard card)
        {
            return card != null && IsNormalMonsterDefinition(card.Definition);
        }
        internal bool LuaSpecialSummonSpecific(CleanPlayerState player, CleanCard card)
        {
            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                Log("Clean summon failed: no open monster zone.");
                return false;
            }

            if (!CanSpecialSummonUnderSpiritRestriction(player, card))
            {
                Log("Spirit with Eyes of Blue prevents the Special Summon of non-Dragon monsters.");
                return false;
            }

            SummonToZone(player, card, zone, false, CleanMonsterStance.Attack);
            Log("Clean special summons " + card.Definition.Name + ".");
            return true;
        }

        internal bool LuaSpecialSummonBestBlueEyes(CleanPlayerState player)
        {
            return LuaSpecialSummonBestBlueEyes(player, CleanMonsterStance.Attack);
        }

        internal bool LuaSpecialSummonBestBlueEyes(CleanPlayerState player, CleanMonsterStance stance)
        {
            CleanCard card;
            string source;
            if (!TryTakeBestSpecialSummonTarget(player, IsBlueEyesMonster, out card, out source))
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

            SummonToZone(player, card, zone, false, stance);
            Log("Clean special summons " + card.Definition.Name + " from " + source + ".");
            return true;
        }


        private sealed class RitualMaterialCandidate
        {
            public CleanCard Card;
            public List<CleanCard> List;
            public int ZoneIndex = -1;
            public bool BanishesFromGraveyard;
            public bool IsExtraMonsterZone;

            public bool IsMonsterZone
            {
                get { return ZoneIndex >= 0 && !IsExtraMonsterZone; }
            }

            public bool IsFieldMonster
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
        private bool TryOpenRitualSummonChoice(CleanPlayerState player)
        {
            if (player == null || player.Id != CleanPlayerId.Local)
            {
                return false;
            }

            List<CleanChoiceOption> options = BuildValidRitualSummonOptions(player);
            if (options.Count <= 1)
            {
                return false;
            }

            State.PendingChoice = new CleanChoicePrompt(
                RitualSummonChoicePromptId,
                "Choose a Ritual Monster",
                "Only Ritual Monsters in your hand with legal tribute materials are shown.",
                options,
                CleanPromptKind.SpecialSummon,
                false);
            Log("Chaos Form resolves: choose a valid Ritual Monster.");
            return true;
        }

        private List<CleanChoiceOption> BuildValidRitualSummonOptions(CleanPlayerState player)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < player.Hand.Count; i++)
            {
                List<RitualMaterialCandidate> materials;
                if (!TryPlanChaosFormRitualSummon(player, i, out materials))
                {
                    continue;
                }

                CleanCard card = player.Hand[i];
                options.Add(new CleanChoiceOption(
                    card.RuntimeId,
                    card.Definition.Name,
                    "Ritual | Level " + card.Definition.Level + " | " + materials.Count + " tribute(s) | ATK " + card.CurrentAttack,
                    true,
                    string.Empty,
                    card.Definition.Id));
            }

            return options;
        }

        private void ResolveRitualSummonChoice(int runtimeId)
        {
            State.PendingChoice = null;
            CleanCard selected = FindCardByRuntimeId(State.Local.Hand, runtimeId);
            int ritualIndex = selected == null ? -1 : State.Local.Hand.IndexOf(selected);

            List<RitualMaterialCandidate> materials;
            if (ritualIndex < 0 || !TryPlanChaosFormRitualSummon(State.Local, ritualIndex, out materials))
            {
                Log("The selected Ritual Monster is no longer a valid summon.");
                return;
            }

            PerformChaosFormRitualSummon(State.Local, ritualIndex, materials);
        }

        private bool TryPlanChaosFormRitualSummon(CleanPlayerState player, int ritualIndex, out List<RitualMaterialCandidate> materials)
        {
            materials = null;
            if (player == null || ritualIndex < 0 || ritualIndex >= player.Hand.Count)
            {
                return false;
            }

            CleanCard ritualMonster = player.Hand[ritualIndex];
            if (!IsChaosFormRitualTarget(ritualMonster) ||
                !CanSpecialSummonCard(player, ritualMonster, CleanSummonOrigin.Hand, CleanSummonMethod.Ritual))
            {
                return false;
            }

            List<RitualMaterialCandidate> pool = BuildRitualMaterialPool(player, ritualMonster);
            return TrySelectExactRitualMaterials(pool, ritualMonster.Definition.Level, out materials) && HasRitualSummonSpace(player, materials);
        }

        private bool PerformChaosFormRitualSummon(CleanPlayerState player, int ritualIndex, List<RitualMaterialCandidate> materials)
        {
            CleanCard ritualMonster = player.Hand[ritualIndex];
            player.Hand.RemoveAt(ritualIndex);
            bool usedBlueEyesWhiteDragon = RitualMaterialsIncludeBlueEyesWhiteDragon(materials);
            MoveRitualMaterialsToGrave(player, materials);
            ritualMonster.RitualSummonedUsingBlueEyesWhiteDragon = usedBlueEyesWhiteDragon;
            ritualMonster.ProperlySummoned = true;
            int zone = FirstEmpty(player.Monsters);
            if (zone < 0)
            {
                player.Hand.Add(ritualMonster);
                Log("Chaos Form failed: no open Monster Zone after tributes.");
                return false;
            }

            SummonToZone(
                player,
                ritualMonster,
                zone,
                false,
                CleanMonsterStance.Attack,
                false,
                CleanSummonPresentationKind.Ritual);
            Log("Chaos Form Ritual Summons " + ritualMonster.Definition.Name + " using " + materials.Count + " tribute(s).");
            return true;
        }

        internal bool LuaRitualSummonWithChaosForm(CleanPlayerState player)
        {
            if (TryOpenRitualSummonChoice(player))
            {
                return true;
            }

            int ritualIndex;
            List<RitualMaterialCandidate> materials;
            if (!TryFindChaosFormRitualSummon(player, out ritualIndex, out materials))
            {
                Log("Chaos Form failed: no Ritual Monster, exact-level tribute, or open Monster Zone.");
                return false;
            }

            return PerformChaosFormRitualSummon(player, ritualIndex, materials);
        }

        private bool TryFindChaosFormRitualSummon(CleanPlayerState player, out int ritualIndex, out List<RitualMaterialCandidate> materials)
        {
            ritualIndex = -1;
            materials = null;
            int bestAttack = -1;
            for (int i = 0; i < player.Hand.Count; i++)
            {
                CleanCard ritualMonster = player.Hand[i];
                if (!IsChaosFormRitualTarget(ritualMonster) ||
                    !CanSpecialSummonCard(player, ritualMonster, CleanSummonOrigin.Hand, CleanSummonMethod.Ritual))
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

            int extraZone = ExtraMonsterZoneIndexFor(player);
            CleanCard extraMonster = State.ExtraMonsters[extraZone];
            if (IsRitualTributeMaterial(extraMonster))
            {
                pool.Add(new RitualMaterialCandidate
                {
                    Card = extraMonster,
                    ZoneIndex = extraZone,
                    IsExtraMonsterZone = true
                });
            }


            for (int i = 0; i < player.Graveyard.Count; i++)
            {
                CleanCard card = player.Graveyard[i];
                if (IsChaosFormGraveyardMaterial(card))
                {
                    pool.Add(new RitualMaterialCandidate { Card = card, List = player.Graveyard, BanishesFromGraveyard = true });
                }
            }

            // Ritual Monsters are legal Tributes, but prefer ordinary monsters when
            // automatically planning for the rival. This preserves the player's
            // unchosen Ritual target whenever an equivalent ordinary Tribute exists.
            List<RitualMaterialCandidate> prioritized = new List<RitualMaterialCandidate>(pool.Count);
            for (int i = 0; i < pool.Count; i++)
            {
                if (!IsRitualMonster(pool[i].Card))
                {
                    prioritized.Add(pool[i]);
                }
            }

            for (int i = 0; i < pool.Count; i++)
            {
                if (IsRitualMonster(pool[i].Card))
                {
                    prioritized.Add(pool[i]);
                }
            }

            return prioritized;
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
                int level = Mathf.Max(0, pool[i].Card.CurrentLevel);
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
                if (material.IsExtraMonsterZone)
                {
                    State.ExtraMonsters[material.ZoneIndex] = null;
                    SendEquipsForLeavingMonster(player, card);
                    ResetCardForHiddenZone(card);
                    SendFieldCardToGraveOrFaceUpExtra(player, card, true);
                    continue;
                }

                if (material.IsMonsterZone)
                {
                    MoveMonsterToGrave(player, material.ZoneIndex);
                    continue;
                }
                else if (material.List != null)
                {
                    material.List.Remove(card);
                }

                ResetCardForHiddenZone(card);
                if (!material.BanishesFromGraveyard)
                {
                    AddToGraveyard(player, card, material.IsMonsterZone || object.ReferenceEquals(material.List, player.Hand));
                }
                else
                {
                    player.Banished.Add(card);
                    Log("Chaos Form banishes " + card.Definition.Name + " from the GY as ritual material.");
                }
            }
        }

        private bool RitualMaterialsIncludeBlueEyesWhiteDragon(List<RitualMaterialCandidate> materials)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].Card != null && IsTreatedAsBlueEyesWhiteDragon(materials[i].Card))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsChaosFormGraveyardMaterial(CleanCard card)
        {
            return card != null && (IsTreatedAsBlueEyesWhiteDragon(card) || string.Equals(card.Definition.Name, "Dark Magician", StringComparison.OrdinalIgnoreCase));
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
            return card != null && card.Definition.Kind == CleanCardKind.Monster;
        }
        private bool CanSynchroSummonFromExtra(CleanPlayerState player, int preferredId)
        {
            int extraIndex;
            List<int> materialZones;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            return IsMainPhase(State.Phase) && TryFindSynchroSummon(player, preferredId, out extraIndex, out materialZones, out summonToExtraMonsterZone, out targetMainZone);
        }

        internal bool LuaCanSynchroSummonBestAvailable(CleanPlayerState player, int preferredId)
        {
            return CanSynchroSummonFromExtra(player, preferredId);
        }

        internal bool LuaSynchroSummonBestAvailable(CleanPlayerState player, int preferredId)
        {
            int extraIndex;
            List<int> materialZones;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            if (!IsMainPhase(State.Phase) || !TryFindSynchroSummon(player, preferredId, out extraIndex, out materialZones, out summonToExtraMonsterZone, out targetMainZone))
            {
                Log("Synchro summon failed: needs legal Extra Deck zone plus face-up Tuner and non-Tuner materials with matching Levels.");
                return false;
            }

            CleanCard synchroMonster = player.ExtraDeck[extraIndex];
            player.ExtraDeck.RemoveAt(extraIndex);
            MoveSynchroMaterialsToGrave(player, materialZones);
            PrepareExtraDeckMonsterSummon(player, synchroMonster);
            synchroMonster.SynchroSummoned = true;
            synchroMonster.ProperlySummoned = true;

            if (summonToExtraMonsterZone)
            {
                State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)] = synchroMonster;
                NotifySpecialSummon(player);
                RequestSummonCinematics(player, synchroMonster, CleanSummonPresentationKind.Synchro);
                Log("Clean Synchro Summons " + synchroMonster.Definition.Name + " to the Extra Monster Zone using " + materialZones.Count + " material(s).");
            }
            else
            {
                SummonToZone(player, synchroMonster, targetMainZone, false, CleanMonsterStance.Attack, false, CleanSummonPresentationKind.Synchro);
                Log("Clean Synchro Summons " + synchroMonster.Definition.Name + " to a linked Main Monster Zone using " + materialZones.Count + " material(s).");
            }

            return true;
        }

        private bool TryFindSynchroSummon(CleanPlayerState player, int preferredId, out int extraIndex, out List<int> materialZones, out bool summonToExtraMonsterZone, out int targetMainZone)
        {
            extraIndex = -1;
            materialZones = null;
            summonToExtraMonsterZone = false;
            targetMainZone = -1;

            if (preferredId > 0)
            {
                int preferredIndex = IndexOfCard(player.ExtraDeck, preferredId);
                List<int> preferredMaterials;
                bool preferredToExtraMonsterZone;
                int preferredTargetMainZone;
                if (preferredIndex >= 0 && TryPlanSynchroSummon(player, preferredIndex, out preferredMaterials, out preferredToExtraMonsterZone, out preferredTargetMainZone))
                {
                    extraIndex = preferredIndex;
                    materialZones = preferredMaterials;
                    summonToExtraMonsterZone = preferredToExtraMonsterZone;
                    targetMainZone = preferredTargetMainZone;
                    return true;
                }
            }

            int bestAttack = -1;
            List<int> bestMaterials = null;
            bool bestToExtraMonsterZone = false;
            int bestTargetMainZone = -1;
            for (int i = 0; i < player.ExtraDeck.Count; i++)
            {
                CleanCard candidate = player.ExtraDeck[i];
                List<int> candidateMaterials;
                bool candidateToExtraMonsterZone;
                int candidateTargetMainZone;
                if (!TryPlanSynchroSummon(player, i, out candidateMaterials, out candidateToExtraMonsterZone, out candidateTargetMainZone))
                {
                    continue;
                }

                if (candidate.Definition.Attack > bestAttack)
                {
                    bestAttack = candidate.Definition.Attack;
                    extraIndex = i;
                    bestMaterials = candidateMaterials;
                    bestToExtraMonsterZone = candidateToExtraMonsterZone;
                    bestTargetMainZone = candidateTargetMainZone;
                }
            }

            materialZones = bestMaterials;
            summonToExtraMonsterZone = bestToExtraMonsterZone;
            targetMainZone = bestTargetMainZone;
            return extraIndex >= 0;
        }

        private bool TryPlanSynchroSummon(CleanPlayerState player, int extraDeckIndex, out List<int> materialZones, out bool summonToExtraMonsterZone, out int targetMainZone)
        {
            materialZones = null;
            summonToExtraMonsterZone = false;
            targetMainZone = -1;
            if (extraDeckIndex < 0 || extraDeckIndex >= player.ExtraDeck.Count || !IsSynchroSummonCandidate(player.ExtraDeck[extraDeckIndex]))
            {
                return false;
            }

            CleanCard synchroMonster = player.ExtraDeck[extraDeckIndex];
            if (!CanSpecialSummonCard(player, synchroMonster, CleanSummonOrigin.ExtraDeck, CleanSummonMethod.Synchro))
            {
                return false;
            }

            if (!TrySelectSynchroMaterials(player, synchroMonster, out materialZones))
            {
                return false;
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            if (State.ExtraMonsters[extraZone] == null || MaterialZonesClearExtraMonsterZone(player, materialZones))
            {
                summonToExtraMonsterZone = true;
                return true;
            }

            targetMainZone = FirstLinkedExtraDeckMainMonsterZone(player, materialZones);
            return targetMainZone >= 0;
        }

        private bool TrySelectSynchroMaterials(CleanPlayerState player, CleanCard synchroMonster, out List<int> materialZones)
        {
            materialZones = new List<int>();
            int targetLevel = Mathf.Max(0, synchroMonster.Definition.Level);
            if (targetLevel <= 0)
            {
                return false;
            }

            List<int> candidateZones = MaterialCandidateZones(player);
            return TrySelectSynchroMaterials(player, synchroMonster, candidateZones, targetLevel, 0, 0, materialZones);
        }

        private bool TrySelectSynchroMaterials(
            CleanPlayerState player,
            CleanCard synchroMonster,
            List<int> candidateZones,
            int targetLevel,
            int startIndex,
            int currentLevel,
            List<int> selectedZones)
        {
            if (currentLevel == targetLevel)
            {
                return SynchroMaterialsAreValid(player, synchroMonster, selectedZones);
            }

            if (currentLevel > targetLevel)
            {
                return false;
            }

            for (int i = startIndex; i < candidateZones.Count; i++)
            {
                int encodedZone = candidateZones[i];
                CleanCard material = MaterialCardAtZone(player, encodedZone);
                if (!IsSynchroMaterialCandidate(material) || !SynchroMaterialAllowedFor(synchroMonster, material))
                {
                    continue;
                }

                selectedZones.Add(encodedZone);
                if (TrySelectSynchroMaterials(player, synchroMonster, candidateZones, targetLevel, i + 1, currentLevel + material.CurrentLevel, selectedZones))
                {
                    return true;
                }

                selectedZones.RemoveAt(selectedZones.Count - 1);
            }

            return false;
        }

        private bool SynchroMaterialsAreValid(CleanPlayerState player, CleanCard synchroMonster, List<int> selectedZones)
        {
            int tunerCount = 0;
            int nonTunerCount = 0;
            for (int i = 0; i < selectedZones.Count; i++)
            {
                CleanCard material = MaterialCardAtZone(player, selectedZones[i]);
                if (material == null)
                {
                    return false;
                }

                if (IsTunerMonster(material))
                {
                    tunerCount++;
                }
                else
                {
                    nonTunerCount++;
                    if ((synchroMonster.Definition.Id == BlueEyesSpiritDragonCardId || synchroMonster.Definition.Id == BlueEyesUltimateSpiritDragonCardId) && !IsBlueEyesMonster(material))
                    {
                        return false;
                    }
                }
            }

            if (synchroMonster.Definition.Id == BlueEyesUltimateSpiritDragonCardId)
            {
                return tunerCount >= 2 && nonTunerCount == 1;
            }

            return tunerCount == 1 && nonTunerCount >= 1;
        }

        private bool SynchroMaterialAllowedFor(CleanCard synchroMonster, CleanCard material)
        {
            if ((synchroMonster.Definition.Id == BlueEyesSpiritDragonCardId || synchroMonster.Definition.Id == BlueEyesUltimateSpiritDragonCardId) &&
                !IsTunerMonster(material) &&
                !IsBlueEyesMonster(material))
            {
                return false;
            }

            return true;
        }

        private bool IsSynchroMaterialCandidate(CleanCard card)
        {
            return card != null && card.Definition.Kind == CleanCardKind.Monster && !card.FaceDown && card.Definition.Level > 0;
        }

        private void MoveSynchroMaterialsToGrave(CleanPlayerState player, List<int> materialZones)
        {
            for (int i = 0; i < materialZones.Count; i++)
            {
                MoveMaterialZoneToGrave(player, materialZones[i]);
            }
        }

        private bool IsSynchroSummonCandidate(CleanCard card)
        {
            return card != null && card.Definition.Kind == CleanCardKind.Monster && IsSynchroMonsterDefinition(card.Definition);
        }

        private bool IsSynchroMonsterDefinition(CleanCardDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            string typeLine = definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Synchro", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsTunerMonster(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Tuner", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private sealed class FusionMaterialCandidate
        {
            public CleanCard Card;
            public List<CleanCard> List;
            public int ZoneIndex = -1;
            public string Source;
            public bool IsExtraMonsterZone;

            public bool IsMonsterZone
            {
                get { return ZoneIndex >= 0 && !IsExtraMonsterZone; }
            }

            public bool IsFieldMonster
            {
                get { return ZoneIndex >= 0; }
            }
        }

        private bool TryOpenFusionSummonChoice(
            CleanPlayerState player,
            int preferredId,
            bool includeGraveyard,
            bool materialsToDeck,
            bool destroyTargets,
            string sourceName)
        {
            if (player == null || player.Id != CleanPlayerId.Local)
            {
                return false;
            }

            List<CleanChoiceOption> options = BuildValidFusionSummonOptions(player, preferredId, includeGraveyard);
            if (options.Count <= 1)
            {
                return false;
            }

            pendingFusionPreferredId = preferredId;
            pendingFusionIncludeGraveyard = includeGraveyard;
            pendingFusionMaterialsToDeck = materialsToDeck;
            pendingFusionDestroyTargets = destroyTargets;
            pendingFusionSourceName = sourceName ?? "Fusion";
            State.PendingChoice = new CleanChoicePrompt(
                FusionSummonChoicePromptId,
                "Choose a Fusion Monster",
                "Only Fusion Monsters with legal materials and an available Extra Deck zone are shown.",
                options,
                CleanPromptKind.SpecialSummon,
                false);
            Log(pendingFusionSourceName + " resolves: choose a valid Fusion Monster.");
            return true;
        }

        private List<CleanChoiceOption> BuildValidFusionSummonOptions(CleanPlayerState player, int preferredId, bool includeGraveyard)
        {
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            bool ultimateFusionOnly = preferredId == DragonMasterMagiaCardId;
            for (int i = 0; i < player.ExtraDeck.Count; i++)
            {
                List<FusionMaterialCandidate> materials;
                bool toExtraZone;
                int targetMainZone;
                if (!TryPlanFusionSummon(player, i, ultimateFusionOnly, includeGraveyard, out materials, out toExtraZone, out targetMainZone))
                {
                    continue;
                }

                CleanCard card = player.ExtraDeck[i];
                options.Add(new CleanChoiceOption(
                    card.RuntimeId,
                    card.Definition.Name,
                    "Fusion | Level " + card.Definition.Level + " | " + materials.Count + " materials | ATK " + card.CurrentAttack,
                    true,
                    string.Empty,
                    card.Definition.Id));
            }

            return options;
        }

        private void ResolveFusionSummonChoice(int runtimeId)
        {
            State.PendingChoice = null;
            int preferredId = pendingFusionPreferredId;
            bool includeGraveyard = pendingFusionIncludeGraveyard;
            bool materialsToDeck = pendingFusionMaterialsToDeck;
            bool destroyTargets = pendingFusionDestroyTargets;
            string sourceName = pendingFusionSourceName;
            ClearPendingFusionSummonChoice();

            CleanCard selected = FindCardByRuntimeId(State.Local.ExtraDeck, runtimeId);
            int extraDeckIndex = selected == null ? -1 : State.Local.ExtraDeck.IndexOf(selected);

            List<FusionMaterialCandidate> materials;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            bool ultimateFusionOnly = preferredId == DragonMasterMagiaCardId;
            if (extraDeckIndex < 0 || !TryPlanFusionSummon(State.Local, extraDeckIndex, ultimateFusionOnly, includeGraveyard, out materials, out summonToExtraMonsterZone, out targetMainZone))
            {
                Log("The selected Fusion Monster is no longer a valid summon.");
                return;
            }

            int fieldBlueEyesMaterialCount;
            if (!PerformFusionSummon(State.Local, extraDeckIndex, materials, summonToExtraMonsterZone, targetMainZone, materialsToDeck, sourceName, out fieldBlueEyesMaterialCount))
            {
                Log("The selected Fusion Monster could not be Summoned.");
                return;
            }

            if (destroyTargets)
            {
                LuaDestroyTargets(State.Rival, fieldBlueEyesMaterialCount, sourceName);
            }
        }

        private void ClearPendingFusionSummonChoice()
        {
            pendingFusionPreferredId = 0;
            pendingFusionIncludeGraveyard = false;
            pendingFusionMaterialsToDeck = false;
            pendingFusionDestroyTargets = false;
            pendingFusionSourceName = string.Empty;
        }
        private bool CanFusionSummonFromExtra(CleanPlayerState player, int preferredId)
        {
            int extraIndex;
            List<FusionMaterialCandidate> materials;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            return TryFindFusionSummon(player, preferredId, true, out extraIndex, out materials, out summonToExtraMonsterZone, out targetMainZone);
        }

        private bool CanFusionSummonFromHandOrField(CleanPlayerState player)
        {
            int extraIndex;
            List<FusionMaterialCandidate> materials;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            return TryFindFusionSummon(player, 0, false, out extraIndex, out materials, out summonToExtraMonsterZone, out targetMainZone);
        }

        internal bool LuaFusionSummonBestAvailable(CleanPlayerState player, int preferredId, bool materialsToDeck)
        {
            int ignored;
            return LuaFusionSummonBestAvailable(player, preferredId, materialsToDeck, out ignored);
        }

        internal bool LuaCanFusionSummonBestAvailable(CleanPlayerState player, int preferredId)
        {
            return CanFusionSummonFromExtra(player, preferredId);
        }

        private bool PerformFusionSummon(
            CleanPlayerState player,
            int extraDeckIndex,
            List<FusionMaterialCandidate> materials,
            bool summonToExtraMonsterZone,
            int targetMainZone,
            bool materialsToDeck,
            string sourceName,
            out int fieldBlueEyesMaterialCount)
        {
            fieldBlueEyesMaterialCount = 0;
            if (player == null || extraDeckIndex < 0 || extraDeckIndex >= player.ExtraDeck.Count || materials == null)
            {
                return false;
            }

            CleanCard fusionMonster = player.ExtraDeck[extraDeckIndex];
            player.ExtraDeck.RemoveAt(extraDeckIndex);
            fieldBlueEyesMaterialCount = CountFieldBlueEyesMaterials(materials);
            MoveFusionMaterials(player, materials, materialsToDeck);
            PrepareExtraDeckMonsterSummon(player, fusionMonster);
            fusionMonster.FusionSummoned = true;
            fusionMonster.ProperlySummoned = true;
            string summonLabel = string.IsNullOrEmpty(sourceName) ? "Fusion" : sourceName;

            if (summonToExtraMonsterZone)
            {
                State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)] = fusionMonster;
                NotifySpecialSummon(player);
                RequestSummonCinematics(player, fusionMonster, CleanSummonPresentationKind.Fusion);
                Log(summonLabel + " Summons " + fusionMonster.Definition.Name + " to the Extra Monster Zone using " + materials.Count + " material(s).");
            }
            else
            {
                SummonToZone(player, fusionMonster, targetMainZone, false, CleanMonsterStance.Attack, false, CleanSummonPresentationKind.Fusion);
                Log(summonLabel + " Summons " + fusionMonster.Definition.Name + " to a linked Main Monster Zone using " + materials.Count + " material(s).");
            }

            return true;
        }
        internal bool LuaFusionSummonBestAvailable(CleanPlayerState player, int preferredId, bool materialsToDeck, out int fieldBlueEyesMaterialCount)
        {
            fieldBlueEyesMaterialCount = 0;
            int extraIndex;
            List<FusionMaterialCandidate> materials;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            if (!TryFindFusionSummon(player, preferredId, true, out extraIndex, out materials, out summonToExtraMonsterZone, out targetMainZone))
            {
                Log("Fusion summon failed: no valid Fusion Monster, materials, or legal Extra Deck zone.");
                return false;
            }

            return PerformFusionSummon(player, extraIndex, materials, summonToExtraMonsterZone, targetMainZone, materialsToDeck, "Clean Fusion", out fieldBlueEyesMaterialCount);
        }

        private bool FusionSummonFromHandOrField(CleanPlayerState player)
        {
            if (TryOpenFusionSummonChoice(player, 0, false, false, false, "Roar"))
            {
                return true;
            }

            int extraIndex;
            List<FusionMaterialCandidate> materials;
            bool summonToExtraMonsterZone;
            int targetMainZone;
            if (!TryFindFusionSummon(player, 0, false, out extraIndex, out materials, out summonToExtraMonsterZone, out targetMainZone))
            {
                Log("Roar failed: no valid Fusion Monster, hand/field materials including Blue-Eyes, or legal Extra Deck zone.");
                return false;
            }

            int ignoredFieldBlueEyesMaterialCount;
            return PerformFusionSummon(player, extraIndex, materials, summonToExtraMonsterZone, targetMainZone, false, "Roar", out ignoredFieldBlueEyesMaterialCount);
        }
        private bool TryFindFusionSummon(CleanPlayerState player, int preferredId, bool includeGraveyard, out int extraIndex, out List<FusionMaterialCandidate> materials, out bool summonToExtraMonsterZone, out int targetMainZone)
        {
            extraIndex = -1;
            materials = null;
            summonToExtraMonsterZone = false;
            targetMainZone = -1;
            bool ultimateFusionOnly = preferredId == DragonMasterMagiaCardId;

            if (preferredId > 0)
            {
                int preferredIndex = IndexOfCard(player.ExtraDeck, preferredId);
                bool preferredToExtraMonsterZone;
                int preferredTargetMainZone;
                if (preferredIndex >= 0 && TryPlanFusionSummon(player, preferredIndex, ultimateFusionOnly, includeGraveyard, out materials, out preferredToExtraMonsterZone, out preferredTargetMainZone))
                {
                    extraIndex = preferredIndex;
                    summonToExtraMonsterZone = preferredToExtraMonsterZone;
                    targetMainZone = preferredTargetMainZone;
                    return true;
                }
            }

            int bestAttack = -1;
            List<FusionMaterialCandidate> bestMaterials = null;
            bool bestToExtraMonsterZone = false;
            int bestTargetMainZone = -1;
            for (int i = 0; i < player.ExtraDeck.Count; i++)
            {
                CleanCard candidate = player.ExtraDeck[i];
                List<FusionMaterialCandidate> candidateMaterials;
                bool candidateToExtraMonsterZone;
                int candidateTargetMainZone;
                if (!TryPlanFusionSummon(player, i, ultimateFusionOnly, includeGraveyard, out candidateMaterials, out candidateToExtraMonsterZone, out candidateTargetMainZone))
                {
                    continue;
                }

                if (candidate.Definition.Attack > bestAttack)
                {
                    bestAttack = candidate.Definition.Attack;
                    extraIndex = i;
                    bestMaterials = candidateMaterials;
                    bestToExtraMonsterZone = candidateToExtraMonsterZone;
                    bestTargetMainZone = candidateTargetMainZone;
                }
            }

            materials = bestMaterials;
            summonToExtraMonsterZone = bestToExtraMonsterZone;
            targetMainZone = bestTargetMainZone;
            return extraIndex >= 0;
        }

        private bool TryPlanFusionSummon(CleanPlayerState player, int extraDeckIndex, bool ultimateFusionOnly, bool includeGraveyard, out List<FusionMaterialCandidate> materials, out bool summonToExtraMonsterZone, out int targetMainZone)
        {
            materials = null;
            summonToExtraMonsterZone = false;
            targetMainZone = -1;
            if (extraDeckIndex < 0 || extraDeckIndex >= player.ExtraDeck.Count)
            {
                return false;
            }

            CleanCard fusionMonster = player.ExtraDeck[extraDeckIndex];
            bool validCandidate = ultimateFusionOnly
                ? IsUltimateFusionSummonCandidate(fusionMonster)
                : IsFusionSummonCandidate(fusionMonster);
            if (!validCandidate ||
                !CanSpecialSummonCard(player, fusionMonster, CleanSummonOrigin.ExtraDeck, CleanSummonMethod.Fusion) ||
                !TrySelectFusionMaterials(player, fusionMonster, includeGraveyard, out materials))
            {
                return false;
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            if (State.ExtraMonsters[extraZone] == null || FusionMaterialsClearExtraMonsterZone(materials))
            {
                summonToExtraMonsterZone = true;
                return true;
            }

            targetMainZone = FirstLinkedExtraDeckMainMonsterZone(player, FusionMaterialMainZones(materials));
            return targetMainZone >= 0;
        }

        private List<int> FusionMaterialMainZones(List<FusionMaterialCandidate> materials)
        {
            List<int> zones = new List<int>();
            if (materials == null)
            {
                return zones;
            }

            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].IsMonsterZone)
                {
                    zones.Add(materials[i].ZoneIndex);
                }
            }

            return zones;
        }

        private bool FusionMaterialsClearExtraMonsterZone(List<FusionMaterialCandidate> materials)
        {
            if (materials == null)
            {
                return false;
            }

            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i].IsExtraMonsterZone)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TrySelectFusionMaterials(CleanPlayerState player, CleanCard fusionMonster, bool includeGraveyard, out List<FusionMaterialCandidate> materials)
        {
            List<FusionMaterialCandidate> pool = BuildFusionMaterialPool(player, includeGraveyard);
            materials = new List<FusionMaterialCandidate>();

            switch (fusionMonster.Definition.Id)
            {
                case BlueEyesUltimateDragonCardId:
                    return TryAddBlueEyesWhiteDragonMaterials(pool, materials, 3);
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

        private List<FusionMaterialCandidate> BuildFusionMaterialPool(CleanPlayerState player, bool includeGraveyard)
        {
            List<FusionMaterialCandidate> pool = new List<FusionMaterialCandidate>();
            AddFusionMaterialsFromList(pool, player.Hand, "hand");
            if (includeGraveyard)
            {
                AddFusionMaterialsFromList(pool, player.Graveyard, "graveyard");
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard card = player.Monsters[i];
                if (IsFusionMaterialMonster(card))
                {
                    pool.Add(new FusionMaterialCandidate { Card = card, ZoneIndex = i, Source = "field" });
                }
            }

            int extraZone = ExtraMonsterZoneIndexFor(player);
            CleanCard extraMonster = State.ExtraMonsters[extraZone];
            if (IsFusionMaterialMonster(extraMonster))
            {
                pool.Add(new FusionMaterialCandidate
                {
                    Card = extraMonster,
                    ZoneIndex = extraZone,
                    Source = "field",
                    IsExtraMonsterZone = true
                });
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

        private bool TryAddBlueEyesWhiteDragonMaterials(List<FusionMaterialCandidate> pool, List<FusionMaterialCandidate> selected, int count)
        {
            for (int i = 0; i < pool.Count && selected.Count < count; i++)
            {
                FusionMaterialCandidate candidate = pool[i];
                bool legalName = candidate.Card.Definition.Id == BlueEyesWhiteDragonCardId ||
                                 (candidate.Card.Definition.Id == BlueEyesAlternativeWhiteDragonCardId &&
                                  !string.Equals(candidate.Source, "hand", StringComparison.OrdinalIgnoreCase));
                if (legalName && !ContainsMaterial(selected, candidate.Card))
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
                if (material.IsExtraMonsterZone)
                {
                    State.ExtraMonsters[material.ZoneIndex] = null;
                    SendEquipsForLeavingMonster(player, card);
                }
                else if (material.IsMonsterZone)
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
                    if (material.IsFieldMonster)
                    {
                        SendFieldCardToGraveOrFaceUpExtra(player, card, true);
                    }
                    else
                    {
                        AddToGraveyard(player, card, string.Equals(material.Source, "hand", StringComparison.OrdinalIgnoreCase));
                    }
                }
            }
        }

        private void SendFieldCardToGraveOrFaceUpExtra(CleanPlayerState player, CleanCard card, bool countGatewayCounter)
        {
            if (card == null)
            {
                return;
            }

            if (IsPendulumMonster(card))
            {
                ResetCardForHiddenZone(card);
                card.FaceUpExtraDeck = true;
                player.ExtraDeck.Add(card);
                Log(card.Definition.Name + " is placed face-up on the Extra Deck instead of going to the GY.");
                return;
            }

            AddToGraveyard(player, card, countGatewayCounter);
            if (card.Definition.Id == NeoKaiserSeaHorseCardId)
            {
                ResolveNeoKaiserSentFromFieldTrigger(player, card);
            }
        }

        private void AddToGraveyard(CleanPlayerState player, CleanCard card, bool countGatewayCounter)
        {
            if (card == null)
            {
                return;
            }

            card.SentToGraveThisTurn = true;
            player.Graveyard.Add(card);
            if (countGatewayCounter && card.Definition.Kind == CleanCardKind.Monster)
            {
                AddGatewayToChaosCounter(State.Local);
                AddGatewayToChaosCounter(State.Rival);
            }
        }

        private void AddToGraveyard(CleanPlayerState player, CleanCard card)
        {
            AddToGraveyard(player, card, false);
        }

        private void AddGatewayToChaosCounter(CleanPlayerState player)
        {
            if (!HasFaceUpGatewayToChaos(player))
            {
                return;
            }

            player.GatewayToChaosCounters = Mathf.Min(6, player.GatewayToChaosCounters + 1);
            Log("Gateway to Chaos gains 1 Spell Counter (" + player.GatewayToChaosCounters + "/6).");
        }
        private void ResetCardForHiddenZone(CleanCard card)
        {
            card.FaceDown = false;
            card.SetThisTurn = false;
            card.AttackBonus = 0;
            card.LevelModifier = 0;
            card.DefenseBonus = 0;
            card.AttackOverride = -1;
            card.DefenseOverride = -1;
            card.CannotAttack = false;
            card.CannotAttackUntilEndOfTurn = false;
            card.EffectsNegated = false;
            card.PiercingThisTurn = false;
            card.RitualSummonedUsingBlueEyesWhiteDragon = false;
            card.SentToGraveThisTurn = false;
            card.PositionChangedThisTurn = false;
            card.FusionSummoned = false;
            card.SynchroSummoned = false;
            card.LinkSummoned = false;
            card.XyzSummoned = false;
            card.FaceUpExtraDeck = false;
            card.DestroyDuringEndPhase = false;
            card.HasBattled = false;
            card.SummonedThisTurn = false;
            card.PositionChangedThisTurn = false;
        }

        private int CountFieldBlueEyesMaterials(List<FusionMaterialCandidate> materials)
        {
            int count = 0;
            for (int i = 0; i < materials.Count; i++)
            {
                CleanCard card = materials[i].Card;
                if (materials[i].IsFieldMonster && (IsTreatedAsBlueEyesWhiteDragon(card) || card.Definition.Id == BlueEyesUltimateDragonCardId))
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

        private bool IsUltimateFusionSummonCandidate(CleanCard card)
        {
            if (!IsFusionSummonCandidate(card))
            {
                return false;
            }

            return card.Definition.Id == DragonMasterMagiaCardId
                || card.Definition.Name.StartsWith("Blue-Eyes", StringComparison.OrdinalIgnoreCase);
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

        private bool TryTakeBestMonsterFromZones(CleanPlayerState player, Predicate<CleanCard> predicate, out CleanCard card, out string source, bool includeBanished = false, bool includeHand = true)
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

            if (includeBanished && TryTakeBestMonster(player.Banished, predicate, out card))
            {
                source = "banished";
                return true;
            }

            if (includeHand && TryTakeBestMonster(player.Hand, predicate, out card))
            {
                source = "hand";
                return true;
            }

            source = string.Empty;
            return false;
        }

        private bool TryTakeBestSpecialSummonTarget(
            CleanPlayerState player,
            Predicate<CleanCard> predicate,
            out CleanCard card,
            out string source,
            bool includeBanished = false,
            bool includeHand = true)
        {
            if (TryTakeBestMonster(
                    player.Graveyard,
                    candidate => predicate(candidate) &&
                                 CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Graveyard, CleanSummonMethod.Effect),
                    out card))
            {
                source = "graveyard";
                return true;
            }

            if (TryTakeBestMonster(
                    player.Deck,
                    candidate => predicate(candidate) &&
                                 CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Deck, CleanSummonMethod.Effect),
                    out card))
            {
                source = "deck";
                return true;
            }

            if (includeBanished && TryTakeBestMonster(
                    player.Banished,
                    candidate => predicate(candidate) &&
                                 CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Banished, CleanSummonMethod.Effect),
                    out card))
            {
                source = "banished";
                return true;
            }

            if (includeHand && TryTakeBestMonster(
                    player.Hand,
                    candidate => predicate(candidate) &&
                                 CanSpecialSummonCard(player, candidate, CleanSummonOrigin.Hand, CleanSummonMethod.Effect),
                    out card))
            {
                source = "hand";
                return true;
            }

            card = null;
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
            else if (string.Equals(source, "banished", StringComparison.OrdinalIgnoreCase))
            {
                player.Banished.Add(card);
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
                   card.Definition.Id == DragonSpiritOfWhiteCardId ||
                   card.Definition.Name.IndexOf("Blue-Eyes", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsBlueEyesMonster(CleanCard card)
        {
            return card != null && card.Definition.Kind == CleanCardKind.Monster && IsBlueEyesCard(card);
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
            RequestEffectAnimation(controller, card);

            bool cleanResolved;
            bool sendSourceCardToGrave = ResolveCleanEffect(controller, rival, card, source, out cleanResolved);
            if (cleanResolved)
            {
                CheckWin();
                return sendSourceCardToGrave;
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
        private int Draw(CleanPlayerState player, int count)
        {
            int drawn = 0;
            for (int i = 0; i < count; i++)
            {
                if (player.Deck.Count == 0)
                {
                    player.LifePoints = 0;
                    Log(player.Name + " cannot draw because the Deck is empty.");
                    CleanPlayerState winner = OpponentOf(player.Id);
                    EndDuel(
                        CleanDuelEndReason.DeckOut,
                        winner,
                        player,
                        winner.Name + " wins by deck-out.");
                    return drawn;
                }

                CleanCard top = player.Deck[0];
                player.Deck.RemoveAt(0);
                top.FaceDown = false;
                player.Hand.Add(top);
                drawn++;
            }

            return drawn;
        }

        private void FillDeck(CleanPlayerState player)
        {
            List<CleanCard> deck = CleanCardLibrary.BuildDeck(player.Id);
            List<CleanCard> extraDeck = CleanCardLibrary.BuildExtraDeck(player.Id);
            if (unavailableCardIds.Count > 0)
            {
                deck.RemoveAll(card => unavailableCardIds.Contains(card.Definition.Id));
                extraDeck.RemoveAll(card => unavailableCardIds.Contains(card.Definition.Id));
            }

            player.Deck.AddRange(deck);
            player.ExtraDeck.AddRange(extraDeck);
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
            player.LifePoints = CleanPlayerState.StartingLifePoints;
            player.NormalSummonUsed = false;
            player.PendulumSummonUsed = false;
            player.GatewayToChaosCounters = 0;
            player.ExtraDeckDragonOnlyThisTurn = false;
            player.MaxxCDrawActive = false;
            player.Deck.Clear();
            player.ExtraDeck.Clear();
            player.Hand.Clear();
            player.Graveyard.Clear();
            player.Banished.Clear();
            player.FieldSpell = null;
            player.UsedOncePerTurn.Clear();
            Array.Clear(player.Monsters, 0, player.Monsters.Length);
            Array.Clear(player.SpellsTraps, 0, player.SpellsTraps.Length);
        }

        private void SummonToZone(
            CleanPlayerState player,
            CleanCard card,
            int zone,
            bool faceDown,
            CleanMonsterStance stance,
            bool normalSummon = false,
            CleanSummonPresentationKind summonPresentation = CleanSummonPresentationKind.None)
        {
            player.Monsters[zone] = card;
            card.Controller = player.Id;
            card.FaceDown = faceDown;
            card.Stance = stance;
            card.HasBattled = false;
            card.SummonedThisTurn = true;
            card.PositionChangedThisTurn = false;
            if (!faceDown)
            {
                RequestSummonCinematics(player, card, summonPresentation);
                ResolveSummonTrigger(player, OpponentOf(player.Id), card, normalSummon);
            }

            if (!normalSummon)
            {
                NotifySpecialSummonForCurrentEvent(player);
            }
        }

        private void NotifySpecialSummonForCurrentEvent(CleanPlayerState summoningPlayer)
        {
            bool pendulumSelection = State.PendingChoice != null && State.PendingChoice.SourceCardId == PendulumSummonChoicePromptId;
            if (pendulumSelection && pendingPendulumSummonCount > 1)
            {
                return;
            }

            NotifySpecialSummon(summoningPlayer);
        }

        private void NotifySpecialSummon(CleanPlayerState summoningPlayer)
        {
            CleanPlayerState opponent = OpponentOf(summoningPlayer.Id);
            if (!opponent.MaxxCDrawActive || State.GameOver)
            {
                return;
            }

            int drawn = Draw(opponent, 1);
            if (!State.GameOver)
            {
                Log("Maxx \"C\" draws " + drawn + " card after " + summoningPlayer.Name + " Special Summons.");
            }
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
            if (card == null ||
                card.Definition.Kind != CleanCardKind.Monster ||
                IsRitualMonster(card) ||
                card.Definition.Id == BlueEyesAlternativeWhiteDragonCardId)
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
                CleanCard card = State.Rival.Hand[i];
                if (card.Definition.Kind == CleanCardKind.Spell && CanActivateCleanCard(card, State.Rival, CleanActivationSource.Hand))
                {
                    return i;
                }
            }

            return -1;
        }

        private bool ShouldLocalChooseRivalAttackTarget()
        {
            bool hasDictator = false;
            bool hasBlueEyes = false;
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = State.Local.Monsters[i];
                if (monster == null || monster.FaceDown || monster.EffectsNegated)
                {
                    continue;
                }

                hasDictator |= monster.Definition.Id == 66961194;
                hasBlueEyes |= IsBlueEyesMonster(monster);
            }

            return hasDictator && hasBlueEyes;
        }

        private void OpenDictatorAttackTargetChoice(int rivalAttackerZone)
        {
            pendingRivalAttackerZone = rivalAttackerZone;
            List<CleanChoiceOption> options = new List<CleanChoiceOption>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = State.Local.Monsters[i];
                if (monster != null)
                {
                    options.Add(new CleanChoiceOption(i, monster.PublicName, "Make this monster the attack target", true, string.Empty));
                }
            }

            State.PendingChoice = new CleanChoicePrompt(
                DictatorAttackTargetChoicePromptId,
                "Dictator of D.",
                "Choose the target for your opponent's attack.",
                options);
            Log("Dictator of D. lets you choose the opponent's attack target.");
        }

        private void ResolveDictatorAttackTargetChoice(int localTargetZone)
        {
            int attackerZone = pendingRivalAttackerZone;
            pendingRivalAttackerZone = -1;
            State.PendingChoice = null;
            if (attackerZone < 0 ||
                attackerZone >= CleanPlayerState.ZoneCount ||
                State.Rival.Monsters[attackerZone] == null ||
                localTargetZone < 0 ||
                localTargetZone >= CleanPlayerState.ZoneCount ||
                State.Local.Monsters[localTargetZone] == null)
            {
                Log("Dictator of D. could not resolve the selected attack target.");
                return;
            }

            ResolveBattle(State.Rival, State.Local, attackerZone, localTargetZone);
            if (State.GameOver || State.PendingChoice != null)
            {
                return;
            }

            RivalBattle();
            if (!State.GameOver && State.PendingChoice == null && State.TurnPlayer == CleanPlayerId.Rival)
            {
                EndTurn();
            }
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
            int strongestZone = StrongestDestructibleEffectTargetZone(player);
            if (strongestZone >= 0)
            {
                CleanCard destroyed = player.Monsters[strongestZone];
                string name = destroyed.PublicName;
                MoveMonsterToGrave(player, strongestZone);
                TriggerJetSummonAfterFieldCardDestroyed(destroyed);
                Log(name + " is destroyed.");
                TriggerBlueEyesUltimateSpiritDestroyed(player, destroyed);
                TriggerDragonMasterMagiaLeavesField(player, destroyed);
            }
            else
            {
                Log("No valid monster to target.");
            }
        }
        private void DestroyAllMonsters(CleanPlayerState player, bool opponentCardEffect = true)
        {
            List<CleanCard> destroyedCards = new List<CleanCard>();
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard monster = player.Monsters[i];
                if (monster == null)
                {
                    continue;
                }

                if (opponentCardEffect && IsProtectedFromOpponentEffectDestruction(player, monster))
                {
                    Log("Blue-Eyes Jet Dragon protects " + monster.Definition.Name + " from card-effect destruction.");
                    continue;
                }

                destroyedCards.Add(monster);
                MoveMonsterToGrave(player, i);
            }
            int extraZone = ExtraMonsterZoneIndexFor(player);
            CleanCard extraMonster = State.ExtraMonsters[extraZone];
            if (extraMonster != null)
            {
                if (opponentCardEffect && IsProtectedFromOpponentEffectDestruction(player, extraMonster))
                {
                    Log("Blue-Eyes Jet Dragon protects " + extraMonster.Definition.Name + " from card-effect destruction.");
                }
                else
                {
                    State.ExtraMonsters[extraZone] = null;
                    SendEquipsForLeavingMonster(player, extraMonster);
                    ResetCardForHiddenZone(extraMonster);
                    SendFieldCardToGraveOrFaceUpExtra(player, extraMonster, true);
                    destroyedCards.Add(extraMonster);
                }
            }

            for (int i = 0; i < destroyedCards.Count; i++)
            {
                TriggerJetSummonAfterFieldCardDestroyed(destroyedCards[i]);
                if (opponentCardEffect)
                {
                    TriggerBlueEyesUltimateSpiritDestroyed(player, destroyedCards[i]);
                    TriggerDragonMasterMagiaLeavesField(player, destroyedCards[i]);
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
                if (card.Definition.Kind == CleanCardKind.Monster && CanSpecialSummonUnderSpiritRestriction(player, card) && card.Definition.Attack > bestAttack)
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

        private bool IsFieldSpellCard(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Spell)
            {
                return false;
            }

            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return card.Definition.Id == MausoleumOfWhiteCardId ||
                   card.Definition.Id == GatewayToChaosCardId ||
                   typeLine.IndexOf("Field", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private void PlaceFieldSpell(CleanPlayerState player, CleanCard card, bool faceDown = false)
        {
            if (player.FieldSpell != null && !object.ReferenceEquals(player.FieldSpell, card))
            {
                CleanCard oldField = player.FieldSpell;
                player.FieldSpell = null;
                if (oldField.Definition.Id == GatewayToChaosCardId)
                {
                    player.GatewayToChaosCounters = 0;
                }

                ResetCardForHiddenZone(oldField);
                AddToGraveyard(player, oldField);
                Log(oldField.Definition.Name + " is sent to the GY from the Field Spell Zone.");
            }

            player.FieldSpell = card;
            card.Controller = player.Id;
            card.FaceDown = faceDown;
            card.SetThisTurn = faceDown;
            if (card.Definition.Id == GatewayToChaosCardId)
            {
                player.GatewayToChaosCounters = 0;
            }
        }

        private void MoveFieldSpellToGrave(CleanPlayerState player)
        {
            CleanCard card = player.FieldSpell;
            if (card == null)
            {
                return;
            }

            player.FieldSpell = null;
            if (card.Definition.Id == GatewayToChaosCardId)
            {
                player.GatewayToChaosCounters = 0;
            }

            ResetCardForHiddenZone(card);
            SendFieldCardToGraveOrFaceUpExtra(player, card, true);
        }

        private void MoveSpellTrapToGrave(CleanPlayerState player, int zone)
        {
            CleanCard card = player.SpellsTraps[zone];
            if (card == null)
            {
                return;
            }

            bool trueLightLeftFaceUp = card.Definition.Id == TrueLightCardId && !card.FaceDown;
            player.SpellsTraps[zone] = null;
            ResetCardForHiddenZone(card);
            SendFieldCardToGraveOrFaceUpExtra(player, card, true);

            if (trueLightLeftFaceUp)
            {
                DestroyAllMonsters(player, false);
                Log("True Light left the field face-up, so all of " + player.Name + "'s monsters are destroyed.");
            }
        }

        private void DetachEquipBonus(CleanPlayerState player, CleanCard equip)
        {
            if (equip == null || equip.EquippedToRuntimeId == 0)
            {
                return;
            }

            CleanCard target = FindMonsterByRuntimeId(player, equip.EquippedToRuntimeId);
            if (target != null)
            {
                target.EquipAttackBonus = Mathf.Max(0, target.EquipAttackBonus - 400);
            }

            equip.EquippedToRuntimeId = 0;
        }

        private void SendEquipsForLeavingMonster(CleanPlayerState player, CleanCard monster)
        {
            if (monster == null)
            {
                return;
            }

            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                CleanCard equip = player.SpellsTraps[i];
                if (equip == null || equip.EquippedToRuntimeId != monster.RuntimeId)
                {
                    continue;
                }

                string equipName = equip.Definition.Name;
                MoveSpellTrapToGrave(player, i);
                Log(equipName + " is sent to the GY because its equipped monster left the field.");
            }

            monster.EquipAttackBonus = 0;
        }

        private CleanCard FindMonsterByRuntimeId(CleanPlayerState player, int runtimeId)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null && player.Monsters[i].RuntimeId == runtimeId)
                {
                    return player.Monsters[i];
                }
            }

            CleanCard extra = State.ExtraMonsters[ExtraMonsterZoneIndexFor(player)];
            return extra != null && extra.RuntimeId == runtimeId ? extra : null;
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
            card.LevelModifier = 0;
            card.DefenseBonus = 0;
            card.AttackOverride = -1;
            card.DefenseOverride = -1;
            card.CannotAttack = false;
            card.CannotAttackUntilEndOfTurn = false;
            card.EffectsNegated = false;
            card.PiercingThisTurn = false;
            card.RitualSummonedUsingBlueEyesWhiteDragon = false;
            card.SentToGraveThisTurn = false;
            card.PositionChangedThisTurn = false;
            card.FusionSummoned = false;
            card.SynchroSummoned = false;
            card.LinkSummoned = false;
            card.XyzSummoned = false;
            card.FaceUpExtraDeck = false;
            card.DestroyDuringEndPhase = false;
            card.HasBattled = false;
            player.Monsters[zone] = null;
            SendXyzMaterialsToGrave(player, card);
            SendFieldCardToGraveOrFaceUpExtra(player, card, true);
        }

        internal void WinByCardEffect(CleanPlayerState winner, string sourceName)
        {
            if (State.GameOver || winner == null)
            {
                return;
            }

            string effectName = string.IsNullOrEmpty(sourceName) ? "card effect" : sourceName;
            EndDuel(
                CleanDuelEndReason.CardEffect,
                winner,
                OpponentOf(winner.Id),
                winner.Name + " wins by " + effectName + ".");
        }

        private void EndDuel(
            CleanDuelEndReason reason,
            CleanPlayerState winner,
            CleanPlayerState loser,
            string summary)
        {
            if (State.GameOver)
            {
                return;
            }

            State.GameOver = true;
            State.PendingChoice = null;
            State.Result = new CleanDuelResult(
                reason,
                winner != null,
                winner == null ? CleanPlayerId.Local : winner.Id,
                loser == null ? CleanPlayerId.Local : loser.Id,
                State.TurnNumber,
                summary);
            State.WinnerText = summary ?? string.Empty;
            Log(State.WinnerText);
        }

        private void EndDuelAsDraw(string summary)
        {
            if (State.GameOver)
            {
                return;
            }

            State.GameOver = true;
            State.PendingChoice = null;
            State.Result = new CleanDuelResult(
                CleanDuelEndReason.Draw,
                false,
                CleanPlayerId.Local,
                CleanPlayerId.Local,
                State.TurnNumber,
                summary);
            State.WinnerText = summary ?? string.Empty;
            Log(State.WinnerText);
        }

        private void CheckWin()
        {
            if (State.GameOver)
            {
                return;
            }

            bool localDefeated = State.Local.LifePoints <= 0;
            bool rivalDefeated = State.Rival.LifePoints <= 0;
            if (localDefeated && rivalDefeated)
            {
                State.Local.LifePoints = 0;
                State.Rival.LifePoints = 0;
                EndDuelAsDraw("Duel ends in a draw.");
                return;
            }

            if (localDefeated)
            {
                State.Local.LifePoints = 0;
                EndDuel(
                    CleanDuelEndReason.LifePoints,
                    State.Rival,
                    State.Local,
                    State.Rival.Name + " wins by reducing the opponent's LP to 0.");
            }
            else if (rivalDefeated)
            {
                State.Rival.LifePoints = 0;
                EndDuel(
                    CleanDuelEndReason.LifePoints,
                    State.Local,
                    State.Rival,
                    State.Local.Name + " wins by reducing the opponent's LP to 0.");
            }
        }

        private void RequestRivalryClimaxIfEligible(
            CleanPlayerState attackingPlayer,
            CleanPlayerState defendingPlayer,
            CleanCard attacker,
            CleanCard defender)
        {
            if (State.RivalryClimaxShown ||
                attacker == null ||
                defender == null ||
                !IsBlueEyesDarkMagicianPair(attacker, defender) ||
                (attackingPlayer.LifePoints > SignatureClimaxLifePointThreshold &&
                 defendingPlayer.LifePoints > SignatureClimaxLifePointThreshold))
            {
                return;
            }

            State.RivalryClimaxShown = true;
            EmitCinematic(
                CleanDuelCinematicKind.RivalryClimax,
                attackingPlayer.Id,
                attacker,
                defender,
                SignatureClimaxLifePointThreshold);
        }

        private void RequestSignatureFinalBlowIfLethal(
            CleanPlayerState attackingPlayer,
            CleanCard attacker,
            CleanPlayerState defendingPlayer)
        {
            if (attacker == null ||
                defendingPlayer.LifePoints > 0 ||
                !CleanDuelAnimationCatalog.HasFinalBlowAnimation(attacker.Definition.Id))
            {
                return;
            }

            EmitCinematic(
                CleanDuelCinematicKind.SignatureFinalBlow,
                attackingPlayer.Id,
                attacker,
                null,
                attacker.CurrentAttack);
        }

        private void RequestSummonCinematics(
            CleanPlayerState summoningPlayer,
            CleanCard card,
            CleanSummonPresentationKind summonKind)
        {
            if (summoningPlayer == null || card == null || card.FaceDown)
            {
                return;
            }

            if (CleanDuelAnimationCatalog.HasGenericSummonAnimation(summonKind))
            {
                EmitCinematic(
                    CleanDuelCinematicKind.GenericSummon,
                    summoningPlayer.Id,
                    card,
                    null,
                    0,
                    summonKind);
            }

            if (CleanDuelAnimationCatalog.HasOwnSummonAnimation(card.Definition.Id))
            {
                EmitCinematic(
                    CleanDuelCinematicKind.SignatureSummon,
                    summoningPlayer.Id,
                    card,
                    null,
                    0);
            }
        }

        private void RequestEffectAnimation(CleanPlayerState controller, CleanCard card)
        {
            if (controller == null || card == null || !CleanDuelAnimationCatalog.HasEffectAnimation(card.Definition.Id))
            {
                return;
            }

            EmitCinematic(
                CleanDuelCinematicKind.EffectActivation,
                controller.Id,
                card,
                null,
                0);
        }

        private void EmitCinematic(
            CleanDuelCinematicKind kind,
            CleanPlayerId attackingPlayer,
            CleanCard attacker,
            CleanCard defender,
            int value,
            CleanSummonPresentationKind summonKind = CleanSummonPresentationKind.None)
        {
            CleanDuelCinematicEvent cinematicEvent = new CleanDuelCinematicEvent(
                ++State.CinematicSequence,
                kind,
                attackingPlayer,
                attacker.Definition.Id,
                attacker.Definition.Name,
                defender != null ? defender.Definition.Id : 0,
                defender != null ? defender.Definition.Name : string.Empty,
                value,
                summonKind);
            if (CinematicRequested != null)
            {
                CinematicRequested(cinematicEvent);
            }
        }

        private static bool IsBlueEyesDarkMagicianPair(CleanCard first, CleanCard second)
        {
            return first != null &&
                   second != null &&
                   ((first.Definition.Id == BlueEyesWhiteDragonCardId && second.Definition.Id == DarkMagicianCardId) ||
                    (first.Definition.Id == DarkMagicianCardId && second.Definition.Id == BlueEyesWhiteDragonCardId));
        }

        private void Log(string message)
        {
            State.Log.Add(message);
            State.StateSequence++;
            State.EventStream.Add(new CleanDuelEventRecord(State.StateSequence, message));
            while (State.Log.Count > 10)
            {
                State.Log.RemoveAt(0);
            }
        }

        private void NotifyChanged()
        {
            State.DeterministicStateHash = CleanDuelDeterminism.ComputeStateHash(State);
            if (Changed != null)
            {
                Changed();
            }
        }
    }
}
