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
        private Image inspectorArtwork;
        private Text inspectorTitle;
        private Text inspectorStats;
        private Text inspectorType;
        private Text inspectorBody;
        private Text localLpText;
        private Text rivalLpText;
        private Text localNameText;
        private Text rivalNameText;
        private RectTransform rivalStatusPanel;
        private Text phaseOrbText;
        private Button phaseOrbButton;
        private Image phaseOrbImage;
        private RectTransform phaseMicroMenu;
        private Image phaseMenuBackground;
        private Text phaseMenuTitle;
        private Button duelMenuButton;
        private RectTransform duelMenuPanel;
        private Button aiDifficultyButton;
        private Sprite phaseCircleSprite;
        private Sprite roundedPanelSprite;
        private Sprite roundedButtonSprite;
        private bool phaseMenuOpen;
        private bool duelMenuOpen;
        private readonly List<Image> phaseStepImages = new List<Image>();
        private readonly List<Button> phaseStepButtons = new List<Button>();
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
        private Button gameOverRestartButton;
        private readonly List<Button> choiceButtons = new List<Button>();
        private readonly List<Text> choiceButtonLabels = new List<Text>();
        private readonly List<Image> choiceButtonArtwork = new List<Image>();
        private readonly List<Text> phaseStepLabels = new List<Text>();
        private Button choiceCancelButton;
        private Button primaryActionButton;
        private Button secondaryActionButton;
        private Button pendulumButton;

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

            phaseCircleSprite = CreateCircleSprite(96);
            roundedPanelSprite = CreateRoundedRectSprite(64, 13f);
            roundedButtonSprite = CreateRoundedRectSprite(64, 20f);

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
            BuildDuelMenu(canvasRect);
            BuildPlayerPanel(canvasRect);
            BuildPhaseOrb(canvasRect);
            BuildInspector(canvasRect);
            BuildRivalHand(canvasRect);
            BuildHand(canvasRect);
            BuildActions(canvasRect);
            BuildQuickButtons(canvasRect);
            BuildLog(canvasRect);
            rivalStatusPanel.SetAsLastSibling();
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
            localLpText.text = "LP  " + Mathf.Max(0, state.Local.LifePoints);
            rivalNameText.text = state.Rival.Name;
            rivalLpText.text = "LP  " + Mathf.Max(0, state.Rival.LifePoints);
            RenderPhaseControl(game);

            RenderInspector(selectedCard, revealSelectedFaceDownCard, state, game);
            RenderRivalHand(game);
            RenderHand(game, selectedHand);
            RenderLog(state);
            RenderDuelMenu(state);
            RenderChoicePrompt(state.GameOver ? null : state.PendingChoice);
            RenderGameOverOverlay(state);
            bool allowActions = !state.GameOver &&
                                (state.PendingChoice == null || game.IsLocalQuickEffectWindowOpen());
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
            bool canActivateSpellTrapZone = allowActions &&
                                                selectedSpellTrap >= 0 &&
                                                (game.CanActivateSetCard(selectedSpellTrap) ||
                                                 game.CanActivateLocalSpellTrapResponse(selectedSpellTrap));
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
            RectTransform gear = Panel("Gear", canvas, new Color(0.01f, 0.045f, 0.09f, 0.94f));
            Anchor(gear, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -80f), new Vector2(80f, -20f));
            Image gearImage = gear.GetComponent<Image>();
            gearImage.sprite = phaseCircleSprite;
            gearImage.type = Image.Type.Simple;
            duelMenuButton = gear.gameObject.AddComponent<Button>();
            duelMenuButton.targetGraphic = gearImage;
            duelMenuButton.navigation = new Navigation { mode = Navigation.Mode.None };
            duelMenuButton.onClick.AddListener(ToggleDuelMenu);
            Text gearText = Label("Gear Text", gear, 16, FontStyle.Bold, TextAnchor.MiddleCenter, CleanDuelVisualTheme.LocalGlow);
            gearText.text = "MENU";
            gearText.raycastTarget = false;
            Stretch(gearText.rectTransform);

            rivalStatusPanel = Panel("Opponent LP Panel", canvas, CleanDuelVisualTheme.PanelRaised);
            Anchor(rivalStatusPanel, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-386f, -106f), new Vector2(-20f, -18f));
            Outline rivalOutline = rivalStatusPanel.gameObject.AddComponent<Outline>();
            rivalOutline.effectColor = new Color(
                CleanDuelVisualTheme.RivalAccent.r,
                CleanDuelVisualTheme.RivalAccent.g,
                CleanDuelVisualTheme.RivalAccent.b,
                0.85f);
            rivalOutline.effectDistance = new Vector2(2f, -2f);
            AddVertical(rivalStatusPanel.gameObject, 14, 24, 8, 8, 0f, TextAnchor.MiddleRight);
            rivalNameText = Label("Opponent Name", rivalStatusPanel, 20, FontStyle.Bold, TextAnchor.MiddleRight, CleanDuelVisualTheme.RivalGlow);
            AddLayout(rivalNameText.gameObject, -1f, 26f, 1f, 0f);
            rivalLpText = Label("Opponent LP", rivalStatusPanel, 38, FontStyle.Bold, TextAnchor.MiddleRight, Color.white);
            AddLayout(rivalLpText.gameObject, -1f, 44f, 1f, 0f);
        }

        private void BuildDuelMenu(RectTransform canvas)
        {
            duelMenuPanel = Panel("Duel Menu", canvas, new Color(0.012f, 0.04f, 0.08f, 0.98f));
            Anchor(duelMenuPanel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -332f), new Vector2(304f, -92f));
            AddVertical(duelMenuPanel.gameObject, 14, 14, 14, 14, 8f, TextAnchor.UpperCenter);

            Text title = Label("Duel Menu Title", duelMenuPanel, 19, FontStyle.Bold, TextAnchor.MiddleCenter, CleanDuelVisualTheme.TextPrimary);
            title.text = "DUEL MENU";
            AddLayout(title.gameObject, -1f, 30f, 1f, 0f);

            aiDifficultyButton = AddChoiceButton("Opponent AI Difficulty", duelMenuPanel, runtime.CycleRivalAiDifficulty);
            aiDifficultyButton.GetComponent<Image>().color = new Color(0.12f, 0.48f, 0.72f, 0.97f);
            aiDifficultyButton.GetComponentInChildren<Text>().text = "AI: " + runtime.RivalAiModeLabel;

            Button surrenderButton = AddChoiceButton("Surrender Duel", duelMenuPanel, RequestSurrenderFromMenu);
            surrenderButton.GetComponent<Image>().color = CleanDuelVisualTheme.AlertCoral;
            surrenderButton.GetComponentInChildren<Text>().text = "Surrender Duel";

            Button closeButton = AddChoiceButton("Close Duel Menu", duelMenuPanel, CloseDuelMenu);
            closeButton.GetComponent<Image>().color = new Color(0.08f, 0.3f, 0.48f, 0.97f);
            closeButton.GetComponentInChildren<Text>().text = "Close";
            duelMenuPanel.gameObject.SetActive(false);
        }

        private void BuildPlayerPanel(RectTransform canvas)
        {
            RectTransform panel = Panel("Player LP Panel", canvas, CleanDuelVisualTheme.Panel);
            Anchor(panel, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 16f), new Vector2(350f, 100f));
            AddVertical(panel.gameObject, 82, 14, 7, 7, 0f, TextAnchor.MiddleLeft);
            localNameText = Label("Player Name", panel, 20, FontStyle.Bold, TextAnchor.MiddleLeft, CleanDuelVisualTheme.TextPrimary);
            AddLayout(localNameText.gameObject, -1f, 26f, 1f, 0f);
            localLpText = Label("Player LP", panel, 36, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            AddLayout(localLpText.gameObject, -1f, 42f, 1f, 0f);

            RectTransform avatar = Panel("Player Avatar", canvas, new Color(0.22f, 0.62f, 0.28f, 1f));
            Anchor(avatar, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(28f, 24f), new Vector2(92f, 88f));
            Image avatarImage = avatar.GetComponent<Image>();
            avatarImage.sprite = phaseCircleSprite;
            avatarImage.type = Image.Type.Simple;
            Text skull = Label("Avatar Icon", avatar, 26, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            skull.text = "DW";
            Stretch(skull.rectTransform);
        }

        private void BuildPhaseOrb(RectTransform canvas)
        {
            GameObject orbObject = new GameObject("Round Phase Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            orbObject.transform.SetParent(canvas, false);
            RectTransform orb = orbObject.GetComponent<RectTransform>();
            orb.anchorMin = new Vector2(1f, 0.5f);
            orb.anchorMax = new Vector2(1f, 0.5f);
            orb.pivot = new Vector2(0.5f, 0.5f);
            orb.sizeDelta = new Vector2(98f, 98f);
            orb.anchoredPosition = new Vector2(-226f, 54f);

            phaseOrbImage = orbObject.GetComponent<Image>();
            phaseOrbImage.sprite = phaseCircleSprite;
            phaseOrbImage.color = CleanDuelVisualTheme.GoldAccent;
            phaseOrbImage.alphaHitTestMinimumThreshold = 0.1f;

            Outline outline = orbObject.GetComponent<Outline>();
            outline.effectColor = new Color(1f, 0.94f, 0.62f, 0.95f);
            outline.effectDistance = new Vector2(3f, -3f);

            phaseOrbButton = orbObject.GetComponent<Button>();
            phaseOrbButton.targetGraphic = phaseOrbImage;
            phaseOrbButton.navigation = new Navigation { mode = Navigation.Mode.None };
            phaseOrbButton.onClick.AddListener(TogglePhaseMenu);

            phaseOrbText = Label("Phase Text", orb, 17, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
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
            phaseMicroMenu.anchoredPosition = new Vector2(-420f, 54f);

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
                GameObject stepObject = new GameObject(phaseCodes[i], typeof(RectTransform), typeof(Image), typeof(Button));
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
                stepImage.raycastTarget = true;
                phaseStepImages.Add(stepImage);
                Button stepButton = stepObject.GetComponent<Button>();
                stepButton.targetGraphic = stepImage;
                stepButton.navigation = new Navigation { mode = Navigation.Mode.None };
                CleanDuelPhase targetPhase = (CleanDuelPhase)i;
                stepButton.onClick.AddListener(delegate { SelectPhaseFromMenu(targetPhase); });
                phaseStepButtons.Add(stepButton);

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
            inspectorPanel = Panel("Card Inspector", canvas, CleanDuelVisualTheme.Panel);
            Anchor(inspectorPanel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(14f, -310f), new Vector2(328f, 310f));
            Outline outline = inspectorPanel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.28f, 0.64f, 0.9f, 0.72f);
            outline.effectDistance = new Vector2(2f, -2f);

            inspectorTitle = Label("Inspector Title", inspectorPanel, 24, FontStyle.Bold, TextAnchor.MiddleLeft, CleanDuelVisualTheme.TextPrimary);
            inspectorTitle.resizeTextForBestFit = true;
            inspectorTitle.resizeTextMinSize = 17;
            inspectorTitle.resizeTextMaxSize = 24;
            Anchor(inspectorTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -48f), new Vector2(-12f, -8f));

            RectTransform artworkFrame = Panel("Inspector Artwork Frame", inspectorPanel, new Color(0.15f, 0.27f, 0.42f, 1f));
            Anchor(artworkFrame, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -218f), new Vector2(130f, -54f));
            GameObject artworkObject = new GameObject("Inspector Artwork", typeof(RectTransform), typeof(Image));
            artworkObject.transform.SetParent(artworkFrame, false);
            inspectorArtwork = artworkObject.GetComponent<Image>();
            inspectorArtwork.preserveAspect = true;
            inspectorArtwork.raycastTarget = false;
            Anchor(inspectorArtwork.rectTransform, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));

            inspectorStats = Label("Inspector Stats", inspectorPanel, 18, FontStyle.Bold, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.66f, 1f));
            Anchor(inspectorStats.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(144f, -214f), new Vector2(-10f, -62f));

            inspectorType = Label("Inspector Type", inspectorPanel, 14, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            inspectorType.resizeTextForBestFit = true;
            inspectorType.resizeTextMinSize = 11;
            inspectorType.resizeTextMaxSize = 14;
            Anchor(inspectorType.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -252f), new Vector2(-12f, -222f));

            inspectorBody = Label("Inspector Body", inspectorPanel, 16, FontStyle.Normal, TextAnchor.UpperLeft, CleanDuelVisualTheme.TextSecondary);
            inspectorBody.lineSpacing = 0.92f;
            Anchor(inspectorBody.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 12f), new Vector2(-12f, -260f));
        }

        private void BuildRivalHand(RectTransform canvas)
        {
            GameObject layer = new GameObject("Rival Hand Layer", typeof(RectTransform));
            layer.transform.SetParent(canvas, false);
            rivalHandLayer = layer.GetComponent<RectTransform>();
            Anchor(rivalHandLayer, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-470f, -145f), new Vector2(470f, -12f));
        }

        private void BuildHand(RectTransform canvas)
        {
            GameObject layer = new GameObject("Hand Layer", typeof(RectTransform));
            layer.transform.SetParent(canvas, false);
            handLayer = layer.GetComponent<RectTransform>();
            Anchor(handLayer, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-500f, -10f), new Vector2(500f, 188f));
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
            pendulumButton = AddCornerButton("Pendulum", canvas, new Vector2(1f, 0f), new Vector2(-390f, 22f), runtime.PendulumSummon);
        }
        private void BuildLog(RectTransform canvas)
        {
            RectTransform log = Panel("Duel Log", canvas, new Color(0.008f, 0.018f, 0.034f, 0.76f));
            Anchor(log, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-340f, -218f), new Vector2(-20f, -112f));
            logText = Label("Log Text", log, 13, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.76f, 0.9f, 1f, 0.9f));
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
            gameOverHint.text = "Duel complete";
            gameOverHint.raycastTarget = false;
            Anchor(gameOverHint.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(40f, 76f), new Vector2(-40f, 116f));

            gameOverRestartButton = AddActionButton(
                "Play Again",
                gameOverBanner.transform,
                new Vector2(0f, -126f),
                runtime.RestartDuel,
                new Color(0.05f, 0.48f, 0.68f, 0.98f));
            gameOverRestartButton.GetComponent<RectTransform>().sizeDelta = new Vector2(184f, 46f);

            gameOverOverlay.gameObject.SetActive(false);
        }

        private void RenderRivalHand(CleanDuelGame game)
        {
            ClearChildren(rivalHandLayer);
            int count = game.State.Rival.Hand.Count;
            float spacing = Mathf.Min(82f, 650f / Mathf.Max(1, count));
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
            rect.sizeDelta = new Vector2(72f, 104f);
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
            float spacing = Mathf.Min(112f, 660f / Mathf.Max(1, count));
            float start = -spacing * (count - 1) * 0.5f;
            for (int i = 0; i < count; i++)
            {
                CleanCard card = game.State.Local.Hand[i];
                int index = i;
                Button button = CardButton(card, handLayer, i == selectedHand);
                RectTransform rect = button.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(start + spacing * i, i == selectedHand ? 30f : 4f);
                rect.localRotation = Quaternion.Euler(0f, 0f, (i - (count - 1) * 0.5f) * -3.2f);
                button.onClick.AddListener(delegate { runtime.SelectHand(index); });
            }
        }

        private Button CardButton(CleanCard card, Transform parent, bool selected)
        {
            GameObject go = new GameObject(card.Definition.Name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = selected ? new Vector2(120f, 176f) : new Vector2(102f, 150f);
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
                inspectorArtwork.gameObject.SetActive(false);
                inspectorTitle.text = state.WinnerText;
                inspectorStats.text = "DUEL COMPLETE";
                inspectorType.text = string.Empty;
                inspectorBody.text = "Restart om opnieuw te spelen.";
                return;
            }

            if (card == null)
            {
                return;
            }

            bool concealCard = card.FaceDown && !revealFaceDownCard;
            Sprite artworkSprite = concealCard ? artCache.GetCardBackSprite() : artCache.GetSprite(card.Definition.Id);
            inspectorArtwork.gameObject.SetActive(artworkSprite != null);
            inspectorArtwork.sprite = artworkSprite;
            inspectorArtwork.color = Color.white;
            inspectorTitle.text = concealCard ? card.PublicName : card.Definition.Name;
            inspectorStats.text = concealCard
                ? card.Definition.Kind == CleanCardKind.Monster ? "FACE-DOWN\nDEFENSE" : "SET\nSPELL / TRAP"
                : InspectorStats(card);
            inspectorType.text = concealCard ? "[ HIDDEN CARD ]" : "[ " + (card.Definition.TypeLine ?? card.Definition.Kind.ToString()) + " ]";
            inspectorBody.text = concealCard ? "Hidden opponent card." : BodyText(card, game);
        }

        private void RenderLog(CleanDuelState state)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = Mathf.Max(0, state.Log.Count - 4); i < state.Log.Count; i++)
            {
                builder.AppendLine(state.Log[i]);
            }
            logText.text = builder.ToString();
        }

        private void RenderDuelMenu(CleanDuelState state)
        {
            bool canOpen = state != null && !state.GameOver && state.PendingChoice == null;
            duelMenuButton.interactable = canOpen;
            if (aiDifficultyButton != null)
            {
                aiDifficultyButton.interactable = canOpen && runtime.CanChangeRivalAiMode;
                aiDifficultyButton.GetComponentInChildren<Text>().text = "AI: " + runtime.RivalAiModeLabel;
            }
            if (!canOpen)
            {
                duelMenuOpen = false;
            }

            duelMenuPanel.gameObject.SetActive(duelMenuOpen && canOpen);
        }

        private void RenderGameOverOverlay(CleanDuelState state)
        {
            bool visible = state != null && state.GameOver;
            gameOverOverlay.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            bool localWon = state.Result != null && state.Result.HasWinner && state.Result.Winner == CleanPlayerId.Local;
            bool rivalWon = state.Result != null && state.Result.HasWinner && state.Result.Winner == CleanPlayerId.Rival;
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
            gameOverHint.text = state.Result == null
                ? "Duel complete"
                : ResultReasonLabel(state.Result.Reason) + "  •  TURN " + state.Result.TurnNumber;
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
                primaryActionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-58f, 0f);
                secondaryActionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(58f, 0f);
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
                float spacing = Mathf.Min(112f, 660f / Mathf.Max(1, handCount));
                float start = -spacing * (handCount - 1) * 0.5f;
                float x = start + spacing * selectedHand;
                actionLayer.anchoredPosition = new Vector2(x, -canvasRect.rect.height * 0.5f + 226f);
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
            return secondaryActionButton.gameObject.activeSelf ? 230f : 112f;
        }

        private string InspectorStats(CleanCard card)
        {
            if (card.Definition.Kind != CleanCardKind.Monster)
            {
                return card.Definition.Kind.ToString().ToUpperInvariant();
            }

            string typeLine = card.Definition.TypeLine ?? string.Empty;
            if (typeLine.IndexOf("Link", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "LINK  " + card.Definition.LinkRating + "\nATK  " + card.CurrentAttack;
            }

            bool xyz = typeLine.IndexOf("Xyz", System.StringComparison.OrdinalIgnoreCase) >= 0;
            return (xyz ? "RANK" : "LEVEL") + "  " + (xyz ? card.Definition.Rank : card.Definition.Level) + "\nATK  " + card.CurrentAttack + "\nDEF  " + card.CurrentDefense;
        }

        private string FullStats(CleanCard card)
        {
            if (card.Definition.Kind == CleanCardKind.Monster)
            {
                string typeLine = card.Definition.TypeLine ?? string.Empty;
                if (typeLine.IndexOf("Link", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Link " + card.Definition.LinkRating + "  ATK " + card.CurrentAttack;
                }

                bool xyz = typeLine.IndexOf("Xyz", System.StringComparison.OrdinalIgnoreCase) >= 0;
                return (xyz ? "Rank " : "Level ") + (xyz ? card.Definition.Rank : card.Definition.Level) + "  ATK " + card.CurrentAttack + "  DEF " + card.CurrentDefense;
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
            duelMenuOpen = false;
            phaseMenuOpen = !phaseMenuOpen;
            if (phaseMicroMenu != null)
            {
                phaseMicroMenu.gameObject.SetActive(phaseMenuOpen);
            }
        }

        private void ToggleDuelMenu()
        {
            phaseMenuOpen = false;
            duelMenuOpen = !duelMenuOpen;
            duelMenuPanel.gameObject.SetActive(duelMenuOpen);
        }

        private void CloseDuelMenu()
        {
            duelMenuOpen = false;
            duelMenuPanel.gameObject.SetActive(false);
        }

        private void RequestSurrenderFromMenu()
        {
            CloseDuelMenu();
            runtime.RequestSurrender();
        }

        private void SelectPhaseFromMenu(CleanDuelPhase targetPhase)
        {
            phaseMenuOpen = false;
            runtime.SelectPhase(targetPhase);
        }

        private void RenderPhaseControl(CleanDuelGame game)
        {
            CleanDuelState state = game.State;
            bool localTurn = state.TurnPlayer == CleanPlayerId.Local;
            Color accent = localTurn ? CleanDuelVisualTheme.GoldAccent : CleanDuelVisualTheme.RivalAccent;
            Color completed = new Color(accent.r * 0.56f, accent.g * 0.56f, accent.b * 0.56f, 0.94f);
            Color upcoming = new Color(accent.r * 0.2f, accent.g * 0.2f, accent.b * 0.2f, 0.9f);
            int currentPhase = Mathf.Clamp((int)state.Phase, 0, phaseStepImages.Count - 1);

            phaseOrbImage.color = accent;
            phaseOrbText.text = "Turn " + state.TurnNumber + "\n" + PhaseLabel(state.Phase);
            phaseOrbText.color = Color.white;
            phaseOrbButton.interactable = !state.GameOver;

            Outline outline = phaseOrbButton.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = localTurn
                    ? new Color(1f, 0.94f, 0.62f, 0.96f)
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
                bool selectable = game.CanSelectPhase((CleanDuelPhase)i);
                phaseStepButtons[i].interactable = selectable;
                phaseStepImages[i].color = current
                    ? accent
                    : i < currentPhase
                        ? completed
                        : selectable
                            ? Color.Lerp(upcoming, accent, 0.48f)
                            : upcoming;
                phaseStepLabels[i].color = current
                    ? Color.white
                    : new Color(0.76f + accent.r * 0.2f, 0.76f + accent.g * 0.2f, 0.76f + accent.b * 0.2f, 0.92f);
                phaseStepImages[i].rectTransform.localScale = current ? Vector3.one * 1.08f : Vector3.one * 0.94f;
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

        private string ResultReasonLabel(CleanDuelEndReason reason)
        {
            switch (reason)
            {
                case CleanDuelEndReason.LifePoints:
                    return "LP REACHED ZERO";
                case CleanDuelEndReason.DeckOut:
                    return "DECK-OUT";
                case CleanDuelEndReason.CardEffect:
                    return "CARD EFFECT";
                case CleanDuelEndReason.Surrender:
                    return "SURRENDER";
                case CleanDuelEndReason.Disconnect:
                    return "DISCONNECT";
                case CleanDuelEndReason.ServerCancelled:
                    return "MATCH CANCELLED";
                case CleanDuelEndReason.Draw:
                    return "DRAW";
                default:
                    return "DUEL COMPLETE";
            }
        }

        private Sprite CreateRoundedRectSprite(int size, float radius)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Runtime Rounded UI";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;
            texture.hideFlags = HideFlags.HideAndDontSave;

            Color32[] pixels = new Color32[size * size];
            float half = size * 0.5f;
            float inset = half - radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(Mathf.Abs(x + 0.5f - half) - inset, 0f);
                    float dy = Mathf.Max(Mathf.Abs(y + 0.5f - half) - inset, 0f);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(radius - distance + 0.75f) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            float border = Mathf.Ceil(radius + 1f);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                size,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
            sprite.name = "Runtime Rounded UI";
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


        private string ShortName(string value, int max)
        {
            return value.Length <= max ? value : value.Substring(0, max - 1) + ".";
        }

        private Button AddActionButton(string text, Transform parent, Vector2 position, UnityAction action, Color color)
        {
            GameObject go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(110f, 44f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.sprite = roundedButtonSprite;
            image.type = Image.Type.Sliced;
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
            image.sprite = roundedButtonSprite;
            image.type = Image.Type.Sliced;
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
            image.sprite = roundedButtonSprite;
            image.type = Image.Type.Sliced;
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
            image.sprite = roundedPanelSprite;
            image.type = Image.Type.Sliced;
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


