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
        private Text logText;
        private RectTransform handLayer;
        private RectTransform actionLayer;
        private RectTransform choicePanel;
        private Text choiceTitle;
        private Text choiceBody;
        private readonly Button[] choiceButtons = new Button[2];
        private readonly Text[] choiceButtonLabels = new Text[2];
        private Button choiceCancelButton;
        private Button summonButton;
        private Button setMonsterButton;
        private Button activateButton;
        private Button setActivateButton;
        private Button activateZoneButton;
        private Button stanceButton;
        private Button directAttackButton;
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

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            Stretch(canvasRect);

            BuildTopChrome(canvasRect);
            BuildPlayerPanel(canvasRect);
            BuildPhaseOrb(canvasRect);
            BuildInspector(canvasRect);
            BuildHand(canvasRect);
            BuildActions(canvasRect);
            BuildQuickButtons(canvasRect);
            BuildLog(canvasRect);
            BuildChoicePrompt(canvasRect);
        }

        public void Render(CleanDuelGame game, CleanCard selectedCard, int selectedHand, int selectedMonster, int selectedSpellTrap)
        {
            CleanDuelState state = game.State;
            localNameText.text = state.Local.Name;
            localLpText.text = "LP  " + state.Local.LifePoints;
            rivalNameText.text = state.Rival.Name;
            rivalLpText.text = "LP  " + state.Rival.LifePoints;
            phaseOrbText.text = "Turn " + state.TurnNumber + "\n" + PhaseLabel(state.Phase);

            RenderInspector(selectedCard, state, game);
            RenderHand(game, selectedHand);
            RenderLog(state);
            RenderChoicePrompt(state.PendingChoice);
            PositionActionLayer(selectedHand, selectedMonster, selectedSpellTrap);

            CleanCard selectedSpellTrapCard = selectedSpellTrap >= 0 && selectedSpellTrap < CleanPlayerState.ZoneCount
                ? state.Local.SpellsTraps[selectedSpellTrap]
                : null;
            Text activateZoneLabel = activateZoneButton.GetComponentInChildren<Text>();
            if (activateZoneLabel != null)
            {
                activateZoneLabel.text = selectedSpellTrapCard != null && !selectedSpellTrapCard.FaceDown ? "Activate" : "Flip";
            }

            bool selectedNormalMonster = IsNormalMonster(selectedCard);
            bool canActivateSelectedCard = !selectedNormalMonster &&
                                           ((selectedHand >= 0 && game.CanActivateHandCard(selectedHand)) ||
                                            (selectedMonster >= 0 && game.CanActivateMonsterZoneCard(selectedMonster)));
            activateButton.gameObject.SetActive(!selectedNormalMonster);
            summonButton.interactable = selectedHand >= 0 && game.CanNormalSummon(selectedHand);
            setMonsterButton.interactable = selectedHand >= 0 && game.CanSetMonster(selectedHand);
            activateButton.interactable = canActivateSelectedCard;
            setActivateButton.interactable = selectedHand >= 0 && game.CanSetSpellTrap(selectedHand);
            activateZoneButton.interactable = selectedSpellTrap >= 0 && game.CanActivateSetCard(selectedSpellTrap);
            stanceButton.interactable = selectedMonster >= 0 && game.CanUseLocalActions() && state.Phase == CleanDuelPhase.Main;
            directAttackButton.interactable = selectedMonster >= 0 && game.CanAttackWith(selectedMonster);
            nextPhaseButton.interactable = game.CanUseLocalActions() || state.GameOver;
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
            RectTransform orb = Panel("Phase Orb", canvas, new Color(0.02f, 0.12f, 0.2f, 0.86f));
            Anchor(orb, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-314f, -42f), new Vector2(-204f, 68f));
            phaseOrbText = Label("Phase Text", orb, 24, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.75f, 0.92f, 1f, 1f));
            Stretch(phaseOrbText.rectTransform);
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
            Anchor(actionLayer, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-230f, 178f), new Vector2(230f, 338f));

            summonButton = AddActionButton("Summon", actionLayer, new Vector2(0f, 0f), runtime.NormalSummonSelected, new Color(0.08f, 0.52f, 0.95f, 0.95f));
            setMonsterButton = AddActionButton("Set", actionLayer, new Vector2(132f, 0f), runtime.SetMonsterSelected, new Color(0.02f, 0.28f, 0.46f, 0.95f));
            activateButton = AddActionButton("Activate", actionLayer, new Vector2(-132f, 0f), runtime.ActivateOrSetSelected, new Color(0.95f, 0.85f, 0.04f, 0.95f));
            setActivateButton = AddActionButton("Set S/T", actionLayer, new Vector2(264f, 0f), runtime.ActivateOrSetSelected, new Color(0.25f, 0.3f, 0.48f, 0.95f));
            activateZoneButton = AddActionButton("Flip", actionLayer, new Vector2(-264f, 0f), runtime.ActivateSelectedZone, new Color(0.68f, 0.18f, 0.62f, 0.95f));
        }

        private void BuildQuickButtons(RectTransform canvas)
        {
            directAttackButton = AddCornerButton("Attack", canvas, new Vector2(1f, 0f), new Vector2(-360f, 30f), runtime.AttackDirectly);
            stanceButton = AddCornerButton("Stance", canvas, new Vector2(1f, 0f), new Vector2(-252f, 30f), runtime.ToggleSelectedMonsterStance);
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
            Anchor(choicePanel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-330f, -190f), new Vector2(330f, 210f));
            AddVertical(choicePanel.gameObject, 18, 18, 16, 16, 10f, TextAnchor.UpperCenter);

            choiceTitle = Label("Choice Title", choicePanel, 28, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayout(choiceTitle.gameObject, -1f, 38f, 1f, 0f);
            choiceBody = Label("Choice Body", choicePanel, 18, FontStyle.Normal, TextAnchor.UpperCenter, new Color(0.72f, 0.96f, 1f, 1f));
            AddLayout(choiceBody.gameObject, -1f, 54f, 1f, 0f);

            for (int i = 0; i < choiceButtons.Length; i++)
            {
                int index = i;
                choiceButtons[i] = AddChoiceButton("Choice " + i, choicePanel, delegate { runtime.ChoosePendingOption(index); });
                choiceButtonLabels[i] = choiceButtons[i].GetComponentInChildren<Text>();
            }

            choiceCancelButton = AddChoiceButton("Cancel Choice", choicePanel, runtime.CancelPendingChoice);
            Text cancelLabel = choiceCancelButton.GetComponentInChildren<Text>();
            cancelLabel.text = "Cancel";
            choicePanel.gameObject.SetActive(false);
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

        private void RenderInspector(CleanCard card, CleanDuelState state, CleanDuelGame game)
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

            inspectorTitle.text = card.PublicName;
            inspectorStats.text = card.FaceDown ? "Face-down Defense" : FullStats(card);
            inspectorBody.text = card.FaceDown ? "Hidden card." : BodyText(card, game);
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

        private void RenderChoicePrompt(CleanChoicePrompt prompt)
        {
            bool visible = prompt != null;
            choicePanel.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            choiceTitle.text = prompt.Title;
            choiceBody.text = prompt.Body;
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                bool hasOption = i < prompt.Options.Count;
                choiceButtons[i].gameObject.SetActive(hasOption);
                if (!hasOption)
                {
                    continue;
                }

                CleanChoiceOption option = prompt.Options[i];
                choiceButtons[i].interactable = option.IsAvailable;
                choiceButtons[i].GetComponent<Image>().color = option.IsAvailable
                    ? new Color(0.08f, 0.35f, 0.72f, 0.96f)
                    : new Color(0.18f, 0.19f, 0.22f, 0.92f);

                choiceButtonLabels[i].text = option.IsAvailable
                    ? option.Label + "\n" + option.Detail
                    : option.Label + "\nUnavailable: " + option.DisabledReason;
            }
        }

        private void PositionActionLayer(int selectedHand, int selectedMonster, int selectedSpellTrap)
        {
            actionLayer.gameObject.SetActive(selectedHand >= 0 || selectedMonster >= 0 || selectedSpellTrap >= 0);
        }

        private string FullStats(CleanCard card)
        {
            if (card.Definition.Kind == CleanCardKind.Monster)
            {
                return "Level " + card.Definition.Level + "  ATK " + card.CurrentAttack + "  DEF " + card.CurrentDefense;
            }

            return card.Definition.Kind + "  " + card.Definition.TypeLine;
        }

        private string BodyText(CleanCard card, CleanDuelGame game)
        {
            string text = !string.IsNullOrEmpty(card.Definition.Text) ? card.Definition.Text : card.Definition.TypeLine;
            string lua = game != null ? game.DescribeLua(card) : string.Empty;
            return string.IsNullOrEmpty(lua) ? text : text + "\n\n" + lua;
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
        private string PhaseLabel(CleanDuelPhase phase)
        {
            switch (phase)
            {
                case CleanDuelPhase.Draw:
                    return "Draw";
                case CleanDuelPhase.Battle:
                    return "Battle";
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
            rect.sizeDelta = new Vector2(118f, 118f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            Image image = go.GetComponent<Image>();
            image.color = color;
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(action);
            Text label = Label("Label", go.transform, 18, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            label.text = text;
            Stretch(label.rectTransform);
            return button;
        }

        private Button AddChoiceButton(string text, Transform parent, UnityAction action)
        {
            GameObject go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 78f);
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.08f, 0.35f, 0.72f, 0.96f);
            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(action);
            Text label = Label("Label", go.transform, 15, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 11;
            label.resizeTextMaxSize = 15;
            Stretch(label.rectTransform);
            AddLayout(go, -1f, 78f, 1f, 0f);
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



