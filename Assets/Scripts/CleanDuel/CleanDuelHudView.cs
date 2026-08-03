using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelHudView
    {
        private readonly CleanDuelBootstrap runtime;
        private readonly CleanDuelArtCache artCache;
        private Font font;
        private RectTransform inspectorPanel;
        private Text inspectorTitle;
        private Text inspectorStats;
        private Text inspectorBody;
        private Text localLpText;
        private Text rivalLpText;
        private Text localNameText;
        private Text rivalNameText;
        private Text phaseOrbText;
        private Button phaseOrbButton;
        private Image phaseOrbImage;
        private RectTransform phaseMicroMenu;
        private Image phaseMenuBackground;
        private Text phaseMenuTitle;
        private Sprite phaseCircleSprite;
        private bool phaseMenuOpen;
        private readonly List<Image> phaseStepImages = new List<Image>();
        private Text logText;
        private RectTransform canvasRect;
        private RectTransform handLayer;
        private RectTransform rivalHandLayer;
        private RectTransform actionLayer;
        private RectTransform choicePanel;
        private Text choiceKind;
        private Text choiceTitle;
        private Text choiceBody;
        private RectTransform gameOverOverlay;
        private Image gameOverScrim;
        private Image gameOverBanner;
        private Image gameOverAccent;
        private Text gameOverTitle;
        private Text gameOverSubtitle;
        private Text gameOverHint;
        private readonly List<Button> choiceButtons = new List<Button>();
        private readonly List<Text> choiceButtonLabels = new List<Text>();
        private readonly List<Image> choiceButtonArtwork = new List<Image>();
        private readonly List<Text> phaseStepLabels = new List<Text>();
        private Button choiceCancelButton;
        private Button primaryActionButton;
        private Button secondaryActionButton;
        private Button pendulumButton;
        private Button nextPhaseButton;

        public CleanDuelHudView(CleanDuelBootstrap runtime, CleanDuelArtCache artCache)
        {
            this.runtime = runtime;
            this.artCache = artCache;
        }

        public void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            GameObject canvasObject = new GameObject("Master Duel HUD Rebuild", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasRect = canvasObject.GetComponent<RectTransform>();
            Stretch(canvasRect);

            BuildTopChrome(canvasRect);
            BuildPlayerPanel(canvasRect);
            BuildPhaseOrb(canvasRect);
            BuildInspector(canvasRect);
            BuildRivalHand(canvasRect);
            BuildHand(canvasRect);
            BuildActions(canvasRect);
            BuildQuickButtons(canvasRect);
            BuildLog(canvasRect);
            BuildChoicePrompt(canvasRect);
            BuildGameOverOverlay(canvasRect);
        }

        public void Render(
            CleanDuelGame game,
            CleanCard selectedCard,
            bool revealSelectedFaceDownCard,
            int selectedHand,
            int selectedMonster,
            int selectedSpellTrap,
            int selectedExtraDeck,
            int selectedExtraMonster,
            int selectedGraveyard,
            bool selectingAttackTarget,
            bool hasBoardActionAnchor,
            Vector2 boardActionAnchor)
        {
            CleanDuelState state = game.State;
            localNameText.text = state.Local.Name;
            localLpText.text = "LP  " + state.Local.LifePoints;
            rivalNameText.text = state.Rival.Name;
            rivalLpText.text = "LP  " + state.Rival.LifePoints;
            RenderPhaseControl(state);

            RenderInspector(selectedCard, revealSelectedFaceDownCard, state, game);
            RenderRivalHand(game);
            RenderHand(game, selectedHand);
            RenderLog(state);
            RenderChoicePrompt(state.GameOver ? null : state.PendingChoice);
            RenderGameOverOverlay(state);
            bool allowActions = !state.GameOver && state.PendingChoice == null;
            bool selectedFaceDownMonster = selectedMonster >= 0 &&
                                           selectedMonster < CleanPlayerState.ZoneCount &&
                                           state.Local.Monsters[selectedMonster] != null &&
                                           state.Local.Monsters[selectedMonster].FaceDown;
            bool canFlipSummon = allowActions && selectedFaceDownMonster && game.CanFlipSummonMonster(selectedMonster);
            bool canActivateSelectedCard = allowActions &&
                                           ((selectedHand >= 0 && (game.CanActivateHandCard(selectedHand) || game.CanActivateLocalQuickEffectHandCard(selectedHand))) ||
                                            (selectedExtraDeck >= 0 && (game.CanSynchroSummonFromExtra(selectedExtraDeck) || game.CanLinkSummonFromExtra(selectedExtraDeck) || game.CanXyzSummonFromExtra(selectedExtraDeck))) ||
                                            (selectedExtraMonster >= 0 && game.CanActivateLocalExtraMonsterZoneCard(selectedExtraMonster)) ||
                                            (selectedMonster >= 0 && (game.CanActivateMonsterZoneCard(selectedMonster) || game.CanActivateLocalQuickEffectMonsterZoneCard(selectedMonster))) ||
                                            (selectedGraveyard >= 0 && game.CanActivateGraveyardCard(selectedGraveyard)));
            bool canNormalSummon = allowActions && selectedHand >= 0 && game.CanNormalSummon(selectedHand);
            bool canSetMonster = allowActions && selectedHand >= 0 && game.CanSetMonster(selectedHand);
            bool canSetSpellTrap = allowActions && selectedHand >= 0 && game.CanSetSpellTrap(selectedHand);
            bool canActivateSpellTrapZone = allowActions && selectedSpellTrap >= 0 && game.CanActivateSetCard(selectedSpellTrap);
            bool canChangeStance = allowActions &&
                                   selectedMonster >= 0 &&
                                   !selectedFaceDownMonster &&
                                   game.CanToggleLocalMonsterStance(selectedMonster);
            bool canAttack = allowActions && selectedMonster >= 0 && game.CanAttackWith(selectedMonster);
            bool canPendulumSummon = allowActions && game.CanPendulumSummon();

            int actionCount = 0;
            HideContextActions();
            if (allowActions && selectedHand >= 0 && selectedCard != null)
            {
                if (selectedCard.Definition.Kind == CleanCardKind.Monster)
                {
                    if (canActivateSelectedCard)
                    {
                        AddContextAction(ref actionCount, game.CanActivatePendulumFromHand(selectedHand) ? "Pendulum" : "Activate", runtime.ActivateOrSetSelected, new Color(0.95f, 0.72f, 0.08f, 0.97f));
                    }

                    if (canNormalSummon)
                    {
                        AddContextAction(ref actionCount, "Summon", runtime.NormalSummonSelected, new Color(0.08f, 0.52f, 0.95f, 0.97f));
                    }

                    if (canSetMonster)
                    {
                        AddContextAction(ref actionCount, "Set", runtime.SetMonsterSelected, new Color(0.02f, 0.28f, 0.46f, 0.97f));
                    }
                }
                else
                {
                    if (canActivateSelectedCard)
                    {
                        AddContextAction(ref actionCount, "Activate", runtime.ActivateOrSetSelected, new Color(0.95f, 0.72f, 0.08f, 0.97f));
                    }

                    if (canSetSpellTrap)
                    {
                        AddContextAction(ref actionCount, "Set", runtime.SetSpellTrapSelected, new Color(0.25f, 0.3f, 0.48f, 0.97f));
                    }
                }
            }
            else if (allowActions && selectedMonster >= 0)
            {
                if (canAttack)
                {
                    AddContextAction(
                        ref actionCount,
                        selectingAttackTarget ? "Select Target" : "Attack",
                        runtime.AttackSelected,
                        new Color(0.88f, 0.16f, 0.12f, 0.97f),
                        !selectingAttackTarget);
                }

                if (canActivateSelectedCard)
                {
                    AddContextAction(ref actionCount, "Activate", runtime.ActivateOrSetSelected, new Color(0.95f, 0.72f, 0.08f, 0.97f));
                }

                if (canFlipSummon || canChangeStance)
                {
                    AddContextAction(
                        ref actionCount,
                        canFlipSummon ? "Flip Summon" : "Change Position",
                        runtime.ToggleSelectedMonsterStance,
                        new Color(0.68f, 0.18f, 0.62f, 0.97f));
                }
            }
            else if (allowActions && selectedSpellTrap >= 0 && canActivateSpellTrapZone)
            {
                AddContextAction(ref actionCount, "Activate", runtime.ActivateSelectedZone, new Color(0.95f, 0.72f, 0.08f, 0.97f));
            }
            else if (allowActions && selectedExtraDeck >= 0 && canActivateSelectedCard)
            {
                AddContextAction(ref actionCount, ExtraDeckSummonLabel(game, selectedExtraDeck), runtime.ActivateOrSetSelected, new Color(0.08f, 0.52f, 0.95f, 0.97f));
            }
            else if (allowActions && (selectedExtraMonster >= 0 || selectedGraveyard >= 0) && canActivateSelectedCard)
            {
                AddContextAction(ref actionCount, selectedGraveyard >= 0 ? "Activate GY" : "Activate", runtime.ActivateOrSetSelected, new Color(0.95f, 0.72f, 0.08f, 0.97f));
            }

            LayoutActionButtons(actionCount);
            PositionActionLayer(selectedHand, state.Local.Hand.Count, hasBoardActionAnchor, boardActionAnchor);
            pendulumButton.gameObject.SetActive(canPendulumSummon);
            pendulumButton.interactable = canPendulumSummon;
            nextPhaseButton.interactable = game.CanUseLocalActions() || state.GameOver;
            Text nextPhaseLabel = nextPhaseButton.GetComponentInChildren<Text>();
            if (nextPhaseLabel != null)
            {
                nextPhaseLabel.text = state.GameOver ? "Restart" : PhaseButtonLabel(state.Phase);
            }
        }

        private string ExtraDeckSummonLabel(CleanDuelGame game, int extraDeckIndex)
        {
            if (game.CanSynchroSummonFromExtra(extraDeckIndex))
            {
                return "Synchro Summon";
            }

            if (game.CanXyzSummonFromExtra(extraDeckIndex))
            {
                return "Xyz Summon";
            }

            return "Link Summon";
        }
        private void BuildTopChrome(RectTransform canvas)
        {
            RectTransform gear = Panel("Gear", canvas, new Color(0.01f, 0.02f, 0.04f, 0.78f));
            Anchor(gear, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26f, -92f), new Vector2(92f, -26f));
            Text gearText = Label("Gear Text", gear, 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.28f, 0.85f, 1f, 1f));
            gearText.text = "O";
            Stretch(gearText.rectTransform);

            RectTransform rival = Panel("Rival LP Panel", canvas, new Color(0.015f, 0.012f, 0.025f, 0.92f));
            Anchor(rival, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-444f, -116f), new Vector2(-28f, -28f));
            AddVertical(rival.gameObject, 14, 18, 8, 8, 0f, TextAnchor.MiddleRight);
            rivalNameText = Label("Rival Name", rival, 26, FontStyle.Bold, TextAnchor.MiddleRight, Color.white);
            AddLayout(rivalNameText.gameObject, -1f, 32f, 1f, 0f);
            rivalLpText = Label("Rival LP", rival, 45, FontStyle.Bold, TextAnchor.MiddleRight, Color.white);
            AddLayout(rivalLpText.gameObject, -1f, 48f, 1f, 0f);
        }

        private void BuildPlayerPanel(RectTransform canvas)
        {
            RectTransform panel = Panel("Player LP Panel", canvas, new Color(0.015f, 0.012f, 0.025f, 0.92f));
            Anchor(panel, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(40f, 24f), new Vector2(428f, 126f));
            AddVertical(panel.gameObject, 92, 18, 8, 8, 0f, TextAnchor.MiddleLeft);
            localNameText = Label("Player Name", panel, 25, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            AddLayout(localNameText.gameObject, -1f, 32f, 1f, 0f);
            localLpText = Label("Player LP", panel, 44, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            AddLayout(localLpText.gameObject, -1f, 50f, 1f, 0f);

            RectTransform avatar = Panel("Player Avatar", canvas, new Color(0.58f, 0.86f, 0.42f, 1f));
            Anchor(avatar, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(54f, 40f), new Vector2(122f, 108f));
            Text skull = Label("Avatar Icon", avatar, 36, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.02f, 0.02f, 0.02f, 1f));
            skull.text = "X";
            Stretch(skull.rectTransform);
        }

        private void BuildPhaseOrb(RectTransform canvas)
        {
            phaseCircleSprite = CreateCircleSprite(96);

            GameObject orbObject = new GameObject("Round Phase Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            orbObject.transform.SetParent(canvas, false);
            RectTransform orb = orbObject.GetComponent<RectTransform>();
            orb.anchorMin = new Vector2(1f, 0.5f);
            orb.anchorMax = new Vector2(1f, 0.5f);
            orb.pivot = new Vector2(0.5f, 0.5f);
            orb.sizeDelta = new Vector2(104f, 104f);
            orb.anchoredPosition = new Vector2(-145f, 120f);

            phaseOrbImage = orbObject.GetComponent<Image>();
            phaseOrbImage.sprite = phaseCircleSprite;
            phaseOrbImage.color = new Color(0.02f, 0.42f, 0.92f, 0.98f);
            phaseOrbImage.alphaHitTestMinimumThreshold = 0.1f;

            Outline outline = orbObject.GetComponent<Outline>();
            outline.effectColor = new Color(0.74f, 0.94f, 1f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);

            phaseOrbButton = orbObject.GetComponent<Button>();
            phaseOrbButton.targetGraphic = phaseOrbImage;
            phaseOrbButton.navigation = new Navigation { mode = Navigation.Mode.None };
            phaseOrbButton.onClick.AddListener(TogglePhaseMenu);

            phaseOrbText = Label("Phase Text", orb, 18, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            phaseOrbText.raycastTarget = false;
            phaseOrbText.resizeTextForBestFit = true;
            phaseOrbText.resizeTextMinSize = 12;
            phaseOrbText.resizeTextMaxSize = 18;
            Anchor(phaseOrbText.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 10f), new Vector2(-10f, -10f));

            GameObject menuObject = new GameObject("Large Phase Menu", typeof(RectTransform), typeof(Image), typeof(Outline));
            menuObject.transform.SetParent(canvas, false);
            phaseMicroMenu = menuObject.GetComponent<RectTransform>();
            phaseMicroMenu.anchorMin = new Vector2(1f, 0.5f);
            phaseMicroMenu.anchorMax = new Vector2(1f, 0.5f);
            phaseMicroMenu.pivot = new Vector2(0.5f, 0.5f);
            phaseMicroMenu.sizeDelta = new Vector2(360f, 230f);
            phaseMicroMenu.anchoredPosition = new Vector2(-390f, 120f);

            phaseMenuBackground = menuObject.GetComponent<Image>();
            phaseMenuBackground.color = new Color(0.015f, 0.055f, 0.13f, 0.97f);
            Outline menuOutline = menuObject.GetComponent<Outline>();
            menuOutline.effectColor = new Color(0.08f, 0.58f, 1f, 0.95f);
            menuOutline.effectDistance = new Vector2(3f, -3f);

            phaseMenuTitle = Label("Phase Menu Title", phaseMicroMenu, 19, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            phaseMenuTitle.text = "YOUR TURN PHASES";
            phaseMenuTitle.raycastTarget = false;
            Anchor(phaseMenuTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -38f), new Vector2(-16f, -8f));

            string[] phaseCodes = { "DP", "SP", "M1", "BP", "M2", "EP" };
            string[] phaseNames = { "DRAW", "STANDBY", "MAIN 1", "BATTLE", "MAIN 2", "END" };
            for (int i = 0; i < phaseCodes.Length; i++)
            {
                GameObject stepObject = new GameObject(phaseCodes[i], typeof(RectTransform), typeof(Image));
                stepObject.transform.SetParent(phaseMicroMenu, false);
                RectTransform stepRect = stepObject.GetComponent<RectTransform>();
                stepRect.anchorMin = new Vector2(0.5f, 0.5f);
                stepRect.anchorMax = new Vector2(0.5f, 0.5f);
                stepRect.pivot = new Vector2(0.5f, 0.5f);
                stepRect.sizeDelta = new Vector2(78f, 78f);
                int column = i % 3;
                int row = i / 3;
                stepRect.anchoredPosition = new Vector2(-108f + column * 108f, row == 0 ? 34f : -62f);

                Image stepImage = stepObject.GetComponent<Image>();
                stepImage.sprite = phaseCircleSprite;
                stepImage.raycastTarget = false;
                phaseStepImages.Add(stepImage);

                Text stepLabel = Label("Label", stepObject.transform, 13, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
                stepLabel.text = phaseCodes[i] + "\n" + phaseNames[i];
                stepLabel.raycastTarget = false;
                stepLabel.resizeTextForBestFit = true;
                stepLabel.resizeTextMinSize = 10;
                stepLabel.resizeTextMaxSize = 13;
                Anchor(stepLabel.rectTransform, Vector2.zero, Vector2.one, new Vector2(7f, 7f), new Vector2(-7f, -7f));
                phaseStepLabels.Add(stepLabel);
            }

            phaseMicroMenu.gameObject.SetActive(false);
        }

        private void BuildInspector(RectTransform canvas)
        {
            inspectorPanel = Panel("Card Inspector", canvas, new Color(0.02f, 0.035f, 0.065f, 0.94f));
            Anchor(inspectorPanel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, -356f), new Vector2(422f, 356f));
            AddVertical(inspectorPanel.gameObject, 16, 16, 14, 14, 8f, TextAnchor.UpperLeft);

            inspectorTitle = Label("Inspector Title", inspectorPanel, 30, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            AddLayout(inspectorTitle.gameObject, -1f, 42f, 1f, 0f);
            inspectorStats = Label("Inspector Stats", inspectorPanel, 21, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.82f, 0.54f, 1f));
            AddLayout(inspectorStats.gameObject, -1f, 86f, 1f, 0f);
            inspectorBody = Label("Inspector Body", inspectorPanel, 21, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.72f, 0.96f, 1f, 1f));
            AddLayout(inspectorBody.gameObject, -1f, -1f, 1f, 1f);
        }

        private void BuildRivalHand(RectTransform canvas)
        {
            GameObject layer = new GameObject("Rival Hand Layer", typeof(RectTransform));
            layer.transform.SetParent(canvas, false);
            rivalHandLayer = layer.GetComponent<RectTransform>();
            Anchor(rivalHandLayer, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-440f, -150f), new Vector2(440f, -34f));
        }

        private void BuildHand(RectTransform canvas)
        {
            GameObject layer = new GameObject("Hand Layer", typeof(RectTransform));
            layer.transform.SetParent(canvas, false);
            handLayer = layer.GetComponent<RectTransform>();
            Anchor(handLayer, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-480f, 0f), new Vector2(480f, 225f));
        }

        private void BuildActions(RectTransform canvas)
        {
            GameObject layer = new GameObject("Action Layer", typeof(RectTransform));
            layer.transform.SetParent(canvas, false);
            actionLayer = layer.GetComponent<RectTransform>();
            Anchor(actionLayer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-132f, -30f), new Vector2(132f, 30f));

            primaryActionButton = AddActionButton("Primary Action", actionLayer, Vector2.zero, null, new Color(0.08f, 0.52f, 0.95f, 0.97f));
            secondaryActionButton = AddActionButton("Secondary Action", actionLayer, Vector2.zero, null, new Color(0.25f, 0.3f, 0.48f, 0.97f));
            HideContextActions();
        }

        private void BuildQuickButtons(RectTransform canvas)
        {
            pendulumButton = AddCornerButton("Pendulum", canvas, new Vector2(1f, 0f), new Vector2(-468f, 30f), runtime.PendulumSummon);
            nextPhaseButton = AddCornerButton("Next", canvas, new Vector2(1f, 0f), new Vector2(-144f, 30f), runtime.AdvancePhase);
            AddCornerButton("Restart", canvas, new Vector2(1f, 0f), new Vector2(-36f, 30f), runtime.RestartDuel);
        }
        private void BuildLog(RectTransform canvas)
        {
            RectTransform log = Panel("Duel Log", canvas, new Color(0.01f, 0.015f, 0.026f, 0.62f));
            Anchor(log, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-396f, -304f), new Vector2(-30f, -132f));
            logText = Label("Log Text", log, 16, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.8f, 0.92f, 1f, 1f));
            Anchor(logText.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 8f), new Vector2(-10f, -8f));
        }

        private void BuildChoicePrompt(RectTransform canvas)
        {
            choicePanel = Panel("Choice Prompt", canvas, new Color(0.015f, 0.02f, 0.035f, 0.96f));
            Anchor(choicePanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-360f, -300f), new Vector2(360f, 300f));
            AddVertical(choicePanel.gameObject, 18, 18, 14, 14, 7f, TextAnchor.UpperCenter);

            choiceKind = Label("Choice Kind", choicePanel, 14, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.45f, 0.9f, 1f, 1f));
            AddLayout(choiceKind.gameObject, -1f, 22f, 1f, 0f);
            choiceTitle = Label("Choice Title", choicePanel, 26, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayout(choiceTitle.gameObject, -1f, 34f, 1f, 0f);
            choiceBody = Label("Choice Body", choicePanel, 16, FontStyle.Normal, TextAnchor.UpperCenter, new Color(0.72f, 0.96f, 1f, 1f));
            AddLayout(choiceBody.gameObject, -1f, 44f, 1f, 0f);

            choiceCancelButton = AddChoiceButton("Cancel Choice", choicePanel, runtime.CancelPendingChoice);
            Text cancelLabel = choiceCancelButton.GetComponentInChildren<Text>();
            cancelLabel.text = "Cancel";
            choicePanel.gameObject.SetActive(false);
        }
        private void BuildGameOverOverlay(RectTransform canvas)
        {
            GameObject root = new GameObject("Game Over Overlay", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas, false);
            gameOverOverlay = root.GetComponent<RectTransform>();
            Stretch(gameOverOverlay);

            gameOverScrim = root.GetComponent<Image>();
            gameOverScrim.color = new Color(0.005f, 0.008f, 0.014f, 0.58f);
            gameOverScrim.raycastTarget = false;

            GameObject bannerObject = new GameObject("Result Banner", typeof(RectTransform), typeof(Image));
            bannerObject.transform.SetParent(gameOverOverlay, false);
            gameOverBanner = bannerObject.GetComponent<Image>();
            gameOverBanner.raycastTarget = false;
            Anchor(gameOverBanner.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-510f, -162f), new Vector2(510f, 162f));

            GameObject accentObject = new GameObject("Result Accent", typeof(RectTransform), typeof(Image));
            accentObject.transform.SetParent(gameOverBanner.transform, false);
            gameOverAccent = accentObject.GetComponent<Image>();
            gameOverAccent.raycastTarget = false;
            Anchor(gameOverAccent.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -9f), Vector2.zero);

            Text upperRule = Label("Result Upper Rule", gameOverBanner.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.66f, 0.96f, 1f, 0.86f));
            upperRule.text = "DUEL COMPLETE";
            upperRule.raycastTarget = false;
            Anchor(upperRule.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -55f), new Vector2(-32f, -20f));

            gameOverTitle = Label("Result Title", gameOverBanner.transform, 108, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            gameOverTitle.resizeTextForBestFit = true;
            gameOverTitle.resizeTextMinSize = 54;
            gameOverTitle.resizeTextMaxSize = 108;
            gameOverTitle.verticalOverflow = VerticalWrapMode.Overflow;
            gameOverTitle.raycastTarget = false;
            Anchor(gameOverTitle.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 1f), new Vector2(30f, -118f), new Vector2(-30f, -48f));

            gameOverSubtitle = Label("Result Subtitle", gameOverBanner.transform, 27, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.78f, 0.94f, 1f, 1f));
            gameOverSubtitle.resizeTextForBestFit = true;
            gameOverSubtitle.resizeTextMinSize = 17;
            gameOverSubtitle.resizeTextMaxSize = 27;
            gameOverSubtitle.raycastTarget = false;
            Anchor(gameOverSubtitle.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(46f, -34f), new Vector2(-46f, 34f));

            gameOverHint = Label("Result Hint", gameOverBanner.transform, 21, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.62f, 1f));
            gameOverHint.text = "Restart om opnieuw te spelen";
            gameOverHint.raycastTarget = false;
            Anchor(gameOverHint.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(40f, 34f), new Vector2(-40f, 78f));

            gameOverOverlay.gameObject.SetActive(false);
        }

        private void RenderRivalHand(CleanDuelGame game)
        {
            ClearChildren(rivalHandLayer);
            int count = game.State.Rival.Hand.Count;
            float spacing = Mathf.Min(70f, 560f / Mathf.Max(1, count));
            float start = -spacing * (count - 1) * 0.5f;
            for (int i = 0; i < count; i++)
            {
                RivalHandBack(rivalHandLayer, start + spacing * i, (i - (count - 1) * 0.5f) * 2.5f);
            }
        }

        private void RivalHandBack(Transform parent, float x, float angle)
        {
            GameObject go = new GameObject("Rival Hand Card", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(58f, 84f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);

            Image image = go.GetComponent<Image>();
            Sprite backSprite = artCache.GetCardBackSprite();
            if (backSprite != null)
            {
                image.sprite = backSprite;
                image.preserveAspect = true;
                image.color = Color.white;
                return;
            }

            image.color = new Color(0.025f, 0.045f, 0.095f, 0.92f);
            Text mark = Label("Hidden Mark", go.transform, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.55f, 0.9f, 1f, 1f));
            mark.text = "?";
            mark.raycastTarget = false;
            Stretch(mark.rectTransform);
        }

        private void RenderHand(CleanDuelGame game, int selectedHand)
        {
            ClearChildren(handLayer);
            int count = game.State.Local.Hand.Count;
            float spacing = Mathf.Min(126f, 700f / Mathf.Max(1, count));
            float start = -spacing * (count - 1) * 0.5f;
            for (int i = 0; i < count; i++)
            {
                CleanCard card = game.State.Local.Hand[i];
                int index = i;
                Button button = CardButton(card, handLayer, i == selectedHand);
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(start + spacing * i, i == selectedHand ? 42f : 14f);
                rect.localRotation = Quaternion.Euler(0f, 0f, (i - (count - 1) * 0.5f) * -4.5f);
                button.onClick.AddListener(delegate { runtime.SelectHand(index); });
            }
        }

        private Button CardButton(CleanCard card, Transform parent, bool selected)
        {
            GameObject go = new GameObject(card.Definition.Name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = selected ? new Vector2(130f, 190f) : new Vector2(112f, 164f);
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);

            Image frame = go.GetComponent<Image>();
            frame.color = selected ? new Color(1f, 0.84f, 0.24f, 1f) : new Color(0.02f, 0.035f, 0.055f, 1f);
            Button button = go.GetComponent<Button>();
            button.targetGraphic = frame;
            button.navigation = new Navigation { mode = Navigation.Mode.None };

            Sprite sprite = artCache.GetSprite(card.Definition.Id);
            if (sprite != null)
            {
                GameObject art = new GameObject("Art", typeof(RectTransform), typeof(Image));
                art.transform.SetParent(go.transform, false);
                Image image = art.GetComponent<Image>();
                image.sprite = sprite;
                image.preserveAspect = true;
                image.raycastTarget = false;
                Anchor(image.rectTransform, Vector2.zero, Vector2.one, new Vector2(5f, 5f), new Vector2(-5f, -5f));
            }
            else
            {
                Text name = Label("Name", go.transform, 13, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
                name.text = ShortName(card.Definition.Name, 18);
                Stretch(name.rectTransform);
            }

            return button;
        }

        private void RenderInspector(CleanCard card, bool revealFaceDownCard, CleanDuelState state, CleanDuelGame game)
        {
            inspectorPanel.gameObject.SetActive(card != null || state.GameOver);
            if (state.GameOver)
            {
                inspectorTitle.text = state.WinnerText;
                inspectorStats.text = "DUEL COMPLETE";
                inspectorBody.text = "Restart om opnieuw te spelen.";
                return;
            }

            if (card == null)
            {
                return;
            }

            bool concealCard = card.FaceDown && !revealFaceDownCard;
            inspectorTitle.text = concealCard ? card.PublicName : card.Definition.Name;
            inspectorStats.text = concealCard
                ? card.Definition.Kind == CleanCardKind.Monster ? "Face-down Defense" : "Set Spell/Trap"
                : card.FaceDown
                    ? (card.Definition.Kind == CleanCardKind.Monster ? "Your Set Monster  |  " : "Your Set Spell/Trap  |  ") + FullStats(card)
                    : FullStats(card);
            inspectorBody.text = concealCard ? "Hidden opponent card." : BodyText(card, game);
        }

        private void RenderLog(CleanDuelState state)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = Mathf.Max(0, state.Log.Count - 6); i < state.Log.Count; i++)
            {
                builder.Append("- ");
                builder.AppendLine(state.Log[i]);
            }
            logText.text = builder.ToString();
        }
        private void RenderGameOverOverlay(CleanDuelState state)
        {
            bool visible = state != null && state.GameOver;
            gameOverOverlay.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            bool localWon = IsWinnerTextForPlayer(state, state.Local);
            bool rivalWon = IsWinnerTextForPlayer(state, state.Rival);
            if (localWon)
            {
                gameOverTitle.text = "YOU WIN";
                gameOverTitle.color = new Color(0.62f, 1f, 0.82f, 1f);
                gameOverBanner.color = new Color(0.02f, 0.14f, 0.11f, 0.94f);
                gameOverAccent.color = new Color(0.12f, 1f, 0.66f, 1f);
                gameOverSubtitle.color = new Color(0.75f, 1f, 0.92f, 1f);
            }
            else if (rivalWon)
            {
                gameOverTitle.text = "YOU LOSE";
                gameOverTitle.color = new Color(1f, 0.4f, 0.34f, 1f);
                gameOverBanner.color = new Color(0.18f, 0.035f, 0.035f, 0.94f);
                gameOverAccent.color = new Color(1f, 0.19f, 0.12f, 1f);
                gameOverSubtitle.color = new Color(1f, 0.78f, 0.72f, 1f);
            }
            else
            {
                gameOverTitle.text = "DRAW";
                gameOverTitle.color = new Color(1f, 0.88f, 0.42f, 1f);
                gameOverBanner.color = new Color(0.16f, 0.12f, 0.04f, 0.94f);
                gameOverAccent.color = new Color(1f, 0.78f, 0.16f, 1f);
                gameOverSubtitle.color = new Color(1f, 0.92f, 0.68f, 1f);
            }

            gameOverSubtitle.text = string.IsNullOrEmpty(state.WinnerText) ? "Duel complete." : state.WinnerText;
            gameOverHint.text = "Restart om opnieuw te spelen";
        }

        private bool IsWinnerTextForPlayer(CleanDuelState state, CleanPlayerState player)
        {
            return state != null && player != null && !string.IsNullOrEmpty(state.WinnerText) &&
                   state.WinnerText.StartsWith(player.Name + " wins", System.StringComparison.OrdinalIgnoreCase);
        }

        private void RenderChoicePrompt(CleanChoicePrompt prompt)
        {
            bool visible = prompt != null;
            choicePanel.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            Color promptAccent;
            switch (prompt.Kind)
            {
                case CleanPromptKind.CardEffect:
                    choiceKind.text = "CARD EFFECT";
                    promptAccent = new Color(0.9f, 0.25f, 0.7f, 1f);
                    break;
                case CleanPromptKind.TriggerEffect:
                    choiceKind.text = "TRIGGER EFFECT";
                    promptAccent = new Color(1f, 0.86f, 0.16f, 1f);
                    break;
                case CleanPromptKind.SpecialSummon:
                    choiceKind.text = "SPECIAL SUMMON";
                    promptAccent = new Color(0.15f, 0.82f, 1f, 1f);
                    break;
                default:
                    choiceKind.text = "SELECT ACTION";
                    promptAccent = new Color(0.45f, 0.9f, 1f, 1f);
                    break;
            }

            choiceKind.color = promptAccent;
            choiceTitle.text = prompt.Title;
            choiceBody.text = prompt.Body;
            EnsureChoiceButtonCount(prompt.Options.Count);
            for (int i = 0; i < choiceButtons.Count; i++)
            {
                bool hasOption = i < prompt.Options.Count;
                choiceButtons[i].gameObject.SetActive(hasOption);
                if (!hasOption)
                {
                    continue;
                }

                CleanChoiceOption option = prompt.Options[i];
                Image artwork = choiceButtonArtwork[i];
                Sprite artworkSprite = option.CardDefinitionId > 0 ? artCache.GetSprite(option.CardDefinitionId) : null;
                bool showArtwork = artworkSprite != null;
                artwork.gameObject.SetActive(showArtwork);
                artwork.sprite = artworkSprite;
                RectTransform optionLabelRect = choiceButtonLabels[i].rectTransform;
                optionLabelRect.offsetMin = new Vector2(showArtwork ? 52f : 8f, 3f);
                optionLabelRect.offsetMax = new Vector2(-8f, -3f);
                choiceButtons[i].interactable = option.IsAvailable;
                choiceButtons[i].GetComponent<Image>().color = option.IsAvailable
                    ? new Color(promptAccent.r * 0.55f, promptAccent.g * 0.55f, promptAccent.b * 0.55f, 0.96f)
                    : new Color(0.18f, 0.19f, 0.22f, 0.92f);

                string optionLabel = prompt.Kind == CleanPromptKind.TriggerEffect && prompt.CanCancel
                    ? "Activate: " + option.Label
                    : option.Label;
                choiceButtonLabels[i].text = option.IsAvailable
                    ? optionLabel + "\n" + option.Detail
                    : optionLabel + "\nUnavailable: " + option.DisabledReason;
            }

            Text cancelLabel = choiceCancelButton.GetComponentInChildren<Text>();
            if (cancelLabel != null)
            {
                cancelLabel.text = prompt.Kind == CleanPromptKind.TriggerEffect
                    ? "Do not activate"
                    : prompt.Kind == CleanPromptKind.SpecialSummon
                        ? "Cancel Special Summon"
                        : "Cancel";
            }

            choiceCancelButton.gameObject.SetActive(prompt.CanCancel);
            choiceCancelButton.transform.SetAsLastSibling();
        }

        private void HideContextActions()
        {
            primaryActionButton.gameObject.SetActive(false);
            secondaryActionButton.gameObject.SetActive(false);
        }

        private void AddContextAction(ref int actionCount, string label, UnityAction action, Color color, bool interactable = true)
        {
            if (actionCount >= 2)
            {
                return;
            }

            Button button = actionCount == 0 ? primaryActionButton : secondaryActionButton;
            button.gameObject.SetActive(true);
            button.interactable = interactable;
            button.onClick.RemoveAllListeners();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }

            button.GetComponent<Image>().color = color;
            Text buttonLabel = button.GetComponentInChildren<Text>();
            if (buttonLabel != null)
            {
                buttonLabel.text = label;
            }

            actionCount++;
        }

        private void LayoutActionButtons(int actionCount)
        {
            actionLayer.gameObject.SetActive(actionCount > 0);
            if (actionCount == 1)
            {
                primaryActionButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }

            if (actionCount == 2)
            {
                primaryActionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-64f, 0f);
                secondaryActionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(64f, 0f);
            }
        }

        private void PositionActionLayer(int selectedHand, int handCount, bool hasBoardActionAnchor, Vector2 boardActionAnchor)
        {
            if (!actionLayer.gameObject.activeSelf)
            {
                return;
            }

            if (selectedHand >= 0)
            {
                float spacing = Mathf.Min(126f, 700f / Mathf.Max(1, handCount));
                float start = -spacing * (handCount - 1) * 0.5f;
                float x = start + spacing * selectedHand;
                actionLayer.anchoredPosition = new Vector2(x, -canvasRect.rect.height * 0.5f + 278f);
                return;
            }

            Vector2 localPoint;
            if (hasBoardActionAnchor && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, boardActionAnchor, null, out localPoint))
            {
                float halfWidth = ActionButtonGroupWidth() * 0.5f;
                float clampedX = Mathf.Clamp(
                    localPoint.x,
                    -canvasRect.rect.width * 0.5f + halfWidth + 8f,
                    canvasRect.rect.width * 0.5f - halfWidth - 8f);
                actionLayer.anchoredPosition = new Vector2(clampedX, localPoint.y);
                return;
            }

            actionLayer.anchoredPosition = new Vector2(0f, -80f);
        }

        private float ActionButtonGroupWidth()
        {
            return secondaryActionButton.gameObject.activeSelf ? 252f : 124f;
        }

        private string FullStats(CleanCard card)
        {
            if (card.Definition.Kind == CleanCardKind.Monster)
            {
                string typeLine = card.Definition.TypeLine ?? string.Empty;
                string rankLabel = typeLine.IndexOf("Xyz", System.StringComparison.OrdinalIgnoreCase) >= 0 ? "Rank " : "Level ";
                return rankLabel + card.Definition.Level + "  ATK " + card.CurrentAttack + "  DEF " + card.CurrentDefense;
            }

            return card.Definition.Kind + "  " + card.Definition.TypeLine;
        }

        private string BodyText(CleanCard card, CleanDuelGame game)
        {
            // Show the official text from cards.cdb. Lua file names and inferred effect
            // families are developer diagnostics, not card text and not game rules.
            return !string.IsNullOrEmpty(card.Definition.Text) ? card.Definition.Text : card.Definition.TypeLine;
        }

        private bool IsNormalMonster(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            if (card.Definition.Id == 89631139 || card.Definition.Id == 23995346)
            {
                return true;
            }

            string typeLine = card.Definition.TypeLine ?? string.Empty;
            return typeLine.IndexOf("Normal", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                   typeLine.IndexOf("Effect", System.StringComparison.OrdinalIgnoreCase) < 0;
        }

        private void TogglePhaseMenu()
        {
            phaseMenuOpen = !phaseMenuOpen;
            if (phaseMicroMenu != null)
            {
                phaseMicroMenu.gameObject.SetActive(phaseMenuOpen);
            }
        }

        private void RenderPhaseControl(CleanDuelState state)
        {
            bool localTurn = state.TurnPlayer == CleanPlayerId.Local;
            Color accent = localTurn
                ? new Color(0.02f, 0.48f, 1f, 0.98f)
                : new Color(0.92f, 0.1f, 0.08f, 0.98f);
            Color completed = new Color(accent.r * 0.56f, accent.g * 0.56f, accent.b * 0.56f, 0.94f);
            Color upcoming = new Color(accent.r * 0.2f, accent.g * 0.2f, accent.b * 0.2f, 0.9f);
            int currentPhase = Mathf.Clamp((int)state.Phase, 0, phaseStepImages.Count - 1);

            phaseOrbImage.color = accent;
            phaseOrbText.text = (localTurn ? "YOUR TURN" : "RIVAL TURN") + "\nT" + state.TurnNumber + "  " + PhaseCode(state.Phase);
            phaseOrbText.color = Color.white;
            phaseOrbButton.interactable = !state.GameOver;

            Outline outline = phaseOrbButton.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = localTurn
                    ? new Color(0.68f, 0.92f, 1f, 0.96f)
                    : new Color(1f, 0.58f, 0.48f, 0.96f);
            }

            phaseMenuBackground.color = localTurn
                ? new Color(0.01f, 0.055f, 0.15f, 0.98f)
                : new Color(0.15f, 0.025f, 0.02f, 0.98f);
            phaseMenuTitle.text = localTurn ? "YOUR TURN - 6 PHASES" : "RIVAL TURN - 6 PHASES";
            phaseMenuTitle.color = localTurn ? new Color(0.68f, 0.92f, 1f, 1f) : new Color(1f, 0.68f, 0.58f, 1f);
            Outline menuOutline = phaseMicroMenu.GetComponent<Outline>();
            if (menuOutline != null)
            {
                menuOutline.effectColor = localTurn
                    ? new Color(0.08f, 0.58f, 1f, 0.95f)
                    : new Color(0.95f, 0.12f, 0.08f, 0.95f);
            }

            if (state.GameOver)
            {
                phaseMenuOpen = false;
            }

            phaseMicroMenu.gameObject.SetActive(phaseMenuOpen && !state.GameOver);
            for (int i = 0; i < phaseStepImages.Count; i++)
            {
                bool current = i == currentPhase;
                phaseStepImages[i].color = current ? accent : i < currentPhase ? completed : upcoming;
                phaseStepLabels[i].color = current
                    ? Color.white
                    : new Color(0.76f + accent.r * 0.2f, 0.76f + accent.g * 0.2f, 0.76f + accent.b * 0.2f, 0.92f);
                phaseStepImages[i].rectTransform.localScale = current ? Vector3.one * 1.08f : Vector3.one * 0.94f;
            }

            Image nextPhaseImage = nextPhaseButton != null ? nextPhaseButton.GetComponent<Image>() : null;
            if (nextPhaseImage != null)
            {
                nextPhaseImage.color = localTurn
                    ? new Color(0.02f, 0.35f, 0.72f, 0.96f)
                    : new Color(0.62f, 0.08f, 0.06f, 0.96f);
            }
        }

        private string PhaseCode(CleanDuelPhase phase)
        {
            switch (phase)
            {
                case CleanDuelPhase.Draw:
                    return "DP";
                case CleanDuelPhase.Standby:
                    return "SP";
                case CleanDuelPhase.Main:
                    return "M1";
                case CleanDuelPhase.Battle:
                    return "BP";
                case CleanDuelPhase.Main2:
                    return "M2";
                case CleanDuelPhase.End:
                    return "EP";
                default:
                    return "M1";
            }
        }

        private Sprite CreateCircleSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Runtime Phase Circle";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;

            Color32[] pixels = new Color32[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius - distance + 1f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            sprite.name = "Runtime Phase Circle";
            return sprite;
        }

        private string PhaseLabel(CleanDuelPhase phase)
        {
            switch (phase)
            {
                case CleanDuelPhase.Draw:
                    return "Draw";
                case CleanDuelPhase.Standby:
                    return "Standby";
                case CleanDuelPhase.Main:
                    return "Main 1";
                case CleanDuelPhase.Battle:
                    return "Battle";
                case CleanDuelPhase.Main2:
                    return "Main 2";
                case CleanDuelPhase.End:
                    return "End";
                default:
                    return "Main 1";
            }
        }

        private string PhaseButtonLabel(CleanDuelPhase phase)
        {
            switch (phase)
            {
                case CleanDuelPhase.Draw:
                    return "Standby";
                case CleanDuelPhase.Standby:
                    return "Main 1";
                case CleanDuelPhase.Main:
                    return "Battle";
                case CleanDuelPhase.Battle:
                    return "Choose";
                case CleanDuelPhase.Main2:
                    return "End";
                case CleanDuelPhase.End:
                    return "End";
                default:
                    return "Next";
            }
        }

        private string ShortName(string value, int max)
        {
            return value.Length <= max ? value : value.Substring(0, max - 1) + ".";
        }

        private Button AddActionButton(string text, Transform parent, Vector2 position, UnityAction action, Color color)
        {
            GameObject go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(120f, 52f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            Image image = go.GetComponent<Image>();
            image.color = color;
            Button button = go.GetComponent<Button>();
            if (action != null)
            {
                button.onClick.AddListener(action);
            }

            Text label = Label("Label", go.transform, 16, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            label.text = text;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 12;
            label.resizeTextMaxSize = 16;
            Stretch(label.rectTransform);
            return button;
        }

        private void EnsureChoiceButtonCount(int count)
        {
            while (choiceButtons.Count < count)
            {
                int index = choiceButtons.Count;
                Button button = AddChoiceButton("Choice " + index, choicePanel, delegate { runtime.ChoosePendingOption(index); });
                choiceButtons.Add(button);
                choiceButtonLabels.Add(button.GetComponentInChildren<Text>());
                GameObject artworkObject = new GameObject("Card Artwork", typeof(RectTransform), typeof(Image));
                artworkObject.transform.SetParent(button.transform, false);
                Image artwork = artworkObject.GetComponent<Image>();
                artwork.preserveAspect = true;
                artwork.raycastTarget = false;
                RectTransform artworkRect = artwork.rectTransform;
                artworkRect.anchorMin = new Vector2(0f, 0.5f);
                artworkRect.anchorMax = new Vector2(0f, 0.5f);
                artworkRect.pivot = new Vector2(0.5f, 0.5f);
                artworkRect.sizeDelta = new Vector2(34f, 48f);
                artworkRect.anchoredPosition = new Vector2(26f, 0f);
                choiceButtonArtwork.Add(artwork);
            }
        }

        private Button AddChoiceButton(string text, Transform parent, UnityAction action)
        {
            GameObject go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 52f);
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.08f, 0.35f, 0.72f, 0.96f);
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(action);
            Text label = Label("Label", go.transform, 15, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 11;
            label.resizeTextMaxSize = 15;
            Stretch(label.rectTransform);
            AddLayout(go, -1f, 52f, 1f, 0f);
            return button;
        }

        private Button AddCornerButton(string text, RectTransform canvas, Vector2 anchor, Vector2 position, UnityAction action)
        {
            GameObject go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(88f, 68f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = position;
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.02f, 0.35f, 0.62f, 0.94f);
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(action);
            Text label = Label("Label", go.transform, 17, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            label.text = text;
            Stretch(label.rectTransform);
            return button;
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

        private Text Label(string name, Transform parent, int size, FontStyle style, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private void AddVertical(GameObject go, int left, int right, int top, int bottom, float spacing, TextAnchor alignment)
        {
            VerticalLayoutGroup layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(left, right, top, bottom);
            layout.spacing = spacing;
            layout.childAlignment = alignment;
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
                Transform child = parent.GetChild(i);
                child.SetParent(null, false);
                Object.Destroy(child.gameObject);
            }
        }
    }
}



