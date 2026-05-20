using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

namespace IcarusGame.DuelPrototype
{
    public sealed class DuelGameBootstrap : MonoBehaviour
    {
        private const float CardPortraitAspectRatio = 421f / 614f;

        private readonly Color arenaBackground = new Color(0.035f, 0.045f, 0.06f, 1f);
        private readonly Color boardPanel = new Color(0.08f, 0.12f, 0.15f, 0.98f);
        private readonly Color zoneEmpty = new Color(0.11f, 0.16f, 0.19f, 0.95f);
        private readonly Color monsterColor = new Color(0.42f, 0.27f, 0.13f, 1f);
        private readonly Color spellColor = new Color(0.08f, 0.36f, 0.32f, 1f);
        private readonly Color trapColor = new Color(0.39f, 0.14f, 0.39f, 1f);
        private readonly Color selectedColor = new Color(0.92f, 0.64f, 0.2f, 1f);

        private DuelEngine engine;
        private Font uiFont;

        private Text titleText;
        private Text turnText;
        private Text opponentLpText;
        private Text playerLpText;
        private Text opponentInfoText;
        private Text playerInfoText;
        private Text inspectorTitleText;
        private Text inspectorKindText;
        private Text inspectorStatsText;
        private Text inspectorRulesText;
        private Text inspectorArtFallbackText;
        private RawImage inspectorArtImage;
        private Text bannerText;
        private Text logText;

        private RectTransform phaseRail;
        private RectTransform opponentMonstersRow;
        private RectTransform opponentSpellsRow;
        private RectTransform playerMonstersRow;
        private RectTransform playerSpellsRow;
        private RectTransform handRow;
        private RectTransform actionsRow;

        private int selectedHandIndex = -1;
        private int selectedMonsterZone = -1;
        private int selectedSpellZone = -1;
        private bool opponentRoutineRunning;
        private bool playerAutoPhaseRoutineRunning;
        private readonly Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
        private readonly HashSet<string> loadingImageUrls = new HashSet<string>();
        private readonly HashSet<string> failedImageUrls = new HashSet<string>();

        private void Awake()
        {
            engine = new DuelEngine();
            EnsureEventSystem();
            BuildInterface();
            engine.StartNewDuel(YugiohCardDatabase.BuildStarterDeck());
            Refresh();
        }

        private void Refresh()
        {
            selectedHandIndex = ClampSelection(selectedHandIndex, engine.Player.Hand.Count);
            if (selectedMonsterZone < -1 || selectedMonsterZone >= DuelEngine.ZoneCount)
            {
                selectedMonsterZone = -1;
            }

            if (selectedSpellZone < -1 || selectedSpellZone >= DuelEngine.ZoneCount)
            {
                selectedSpellZone = -1;
            }

            RefreshHeader();
            RefreshPhaseRail();
            RefreshInspector();
            RefreshZones();
            RefreshHand();
            RefreshActions();
            RefreshLog();

            if (engine.ActivePlayerId == DuelPlayerId.Opponent && !engine.IsGameOver && !opponentRoutineRunning)
            {
                StartCoroutine(OpponentTurnRoutine());
            }
            else if (engine.ActivePlayerId == DuelPlayerId.Player &&
                     !engine.IsGameOver &&
                     !playerAutoPhaseRoutineRunning &&
                     IsAutomaticPlayerPhase())
            {
                StartCoroutine(PlayerAutoPhaseRoutine());
            }
        }

        private IEnumerator OpponentTurnRoutine()
        {
            opponentRoutineRunning = true;
            yield return new WaitForSeconds(0.35f);

            while (!engine.IsGameOver && engine.ActivePlayerId == DuelPlayerId.Opponent)
            {
                if (engine.CurrentPhase == DuelPhase.Draw || engine.CurrentPhase == DuelPhase.Standby)
                {
                    engine.AdvancePhase();
                }
                else if (engine.CurrentPhase == DuelPhase.Main1)
                {
                    engine.RunOpponentMainPhase();
                    engine.AdvancePhase();
                }
                else if (engine.CurrentPhase == DuelPhase.Battle)
                {
                    engine.RunOpponentBattlePhase();
                    engine.AdvancePhase();
                }
                else if (engine.CurrentPhase == DuelPhase.Main2)
                {
                    engine.RunOpponentMainPhase();
                    engine.AdvancePhase();
                }
                else
                {
                    engine.AdvancePhase();
                }

                Refresh();
                yield return new WaitForSeconds(0.55f);
            }

            opponentRoutineRunning = false;
            Refresh();
        }

        private IEnumerator PlayerAutoPhaseRoutine()
        {
            playerAutoPhaseRoutineRunning = true;
            yield return new WaitForSeconds(0.45f);

            while (!engine.IsGameOver &&
                   engine.ActivePlayerId == DuelPlayerId.Player &&
                   IsAutomaticPlayerPhase())
            {
                ClearSelectedIndexes();
                engine.AdvancePhase();
                Refresh();
                yield return new WaitForSeconds(0.45f);
            }

            playerAutoPhaseRoutineRunning = false;
            Refresh();
        }

        private void BuildInterface()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            GameObject canvasObject = new GameObject("Icarus Duel Arena Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            Stretch(canvasRect);

            GameObject background = CreatePanel("Arena Background", canvasRect, arenaBackground);
            Stretch(background.GetComponent<RectTransform>());
            BuildArenaLines(background.transform);

            RectTransform topBar = CreateAnchoredPanel(
                "Top Bar",
                background.transform,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(18f, -88f),
                new Vector2(-18f, -14f),
                new Color(0.07f, 0.09f, 0.12f, 0.98f));
            AddHorizontal(topBar.gameObject, 12, 12, 8, 8, 12f, TextAnchor.MiddleCenter);

            titleText = CreateText("Title", topBar, 25, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.95f, 0.96f, 0.98f, 1f));
            titleText.text = "ICARUS DUEL ARENA";
            AddLayout(titleText.gameObject, 310f, 58f, 0f, 0f);

            opponentLpText = CreateText("Opponent LP", topBar, 22, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayout(opponentLpText.gameObject, 210f, 58f, 0f, 0f);

            turnText = CreateText("Turn", topBar, 20, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.84f, 0.91f, 1f, 1f));
            AddLayout(turnText.gameObject, 420f, 58f, 1f, 0f);

            playerLpText = CreateText("Player LP", topBar, 22, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayout(playerLpText.gameObject, 210f, 58f, 0f, 0f);

            CreateButton("Nieuw duel", topBar, OnRestart, new Color(0.42f, 0.18f, 0.2f, 1f), 145f, 46f);

            RectTransform inspector = CreateAnchoredPanel(
                "Card Inspector",
                background.transform,
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(18f, 18f),
                new Vector2(318f, -100f),
                new Color(0.075f, 0.085f, 0.105f, 0.98f));
            AddVertical(inspector.gameObject, 14, 14, 14, 14, 10f);
            BuildInspector(inspector);

            RectTransform logPanel = CreateAnchoredPanel(
                "Duel Log",
                background.transform,
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(-348f, 18f),
                new Vector2(-18f, -100f),
                new Color(0.065f, 0.075f, 0.095f, 0.98f));
            AddVertical(logPanel.gameObject, 14, 14, 14, 14, 10f);
            CreateText("Log Header", logPanel, 19, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.93f, 0.94f, 0.97f, 1f)).text = "Duel Log";
            logText = CreateText("Log Text", logPanel, 15, FontStyle.Normal, TextAnchor.UpperLeft, new Color(0.78f, 0.86f, 0.93f, 1f));
            AddLayout(logText.gameObject, -1f, -1f, 1f, 1f);

            RectTransform board = CreateAnchoredPanel(
                "Board",
                background.transform,
                new Vector2(0f, 0f),
                new Vector2(1f, 1f),
                new Vector2(334f, 18f),
                new Vector2(-364f, -100f),
                boardPanel);
            AddVertical(board.gameObject, 16, 16, 14, 14, 8f);

            BuildPlayerStrip(board, true);
            opponentMonstersRow = CreateZoneRow("Opponent Monsters", board, 104f);
            opponentSpellsRow = CreateZoneRow("Opponent Spells", board, 78f);
            BuildCenterStrip(board);
            playerSpellsRow = CreateZoneRow("Player Spells", board, 78f);
            playerMonstersRow = CreateZoneRow("Player Monsters", board, 104f);
            BuildPlayerStrip(board, false);
            BuildHandStrip(board);
        }

        private void BuildArenaLines(Transform parent)
        {
            for (int i = 0; i < 7; i++)
            {
                float y = 0.16f + i * 0.11f;
                RectTransform line = CreateAnchoredPanel(
                    "Arena Line",
                    parent,
                    new Vector2(0f, y),
                    new Vector2(1f, y),
                    new Vector2(0f, -1f),
                    new Vector2(0f, 1f),
                    new Color(0.12f, 0.18f, 0.22f, 0.18f));
                line.SetAsFirstSibling();
            }
        }

        private void BuildInspector(RectTransform inspector)
        {
            RectTransform cardPreview = CreatePanel("Card Preview", inspector, new Color(0.13f, 0.12f, 0.12f, 1f)).GetComponent<RectTransform>();
            AddLayout(cardPreview.gameObject, -1f, 470f, 0f, 0f);
            AddVertical(cardPreview.gameObject, 12, 12, 12, 12, 8f);
            VerticalLayoutGroup previewLayout = cardPreview.GetComponent<VerticalLayoutGroup>();
            previewLayout.childAlignment = TextAnchor.MiddleCenter;
            previewLayout.childForceExpandWidth = false;

            inspectorTitleText = CreateText("Inspector Title", cardPreview, 22, FontStyle.Bold, TextAnchor.UpperCenter, Color.white);
            AddLayout(inspectorTitleText.gameObject, 260f, 52f, 0f, 0f);

            RectTransform artFrame = CreatePanel("Inspector Art Frame", cardPreview, new Color(0.04f, 0.045f, 0.055f, 1f)).GetComponent<RectTransform>();
            AddLayout(artFrame.gameObject, 210f, 306f, 0f, 0f);

            inspectorArtImage = CreateRawImage("Inspector Art", artFrame, new Color(1f, 1f, 1f, 0.95f));
            Stretch(inspectorArtImage.GetComponent<RectTransform>());
            AddAspectFitter(inspectorArtImage.gameObject);

            inspectorArtFallbackText = CreateText("Inspector Art Fallback", artFrame, 42, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.47f, 1f));
            Stretch(inspectorArtFallbackText.GetComponent<RectTransform>());
            inspectorArtFallbackText.text = "DUEL";

            inspectorKindText = CreateText("Inspector Kind", cardPreview, 16, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.85f, 0.9f, 0.95f, 1f));
            AddLayout(inspectorKindText.gameObject, 260f, 28f, 0f, 0f);

            inspectorStatsText = CreateText("Inspector Stats", cardPreview, 18, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayout(inspectorStatsText.gameObject, 260f, 30f, 0f, 0f);

            RectTransform rulesPanel = CreatePanel("Selected Card Text Box", inspector, new Color(0.055f, 0.065f, 0.085f, 1f)).GetComponent<RectTransform>();
            AddLayout(rulesPanel.gameObject, -1f, -1f, 1f, 1f);
            AddVertical(rulesPanel.gameObject, 12, 12, 12, 12, 0f);

            inspectorRulesText = CreateText("Inspector Rules", rulesPanel, 21, FontStyle.Bold, TextAnchor.UpperLeft, new Color(0.92f, 0.95f, 0.98f, 1f));
            AddLayout(inspectorRulesText.gameObject, -1f, -1f, 1f, 1f);
        }

        private void BuildPlayerStrip(RectTransform board, bool opponent)
        {
            RectTransform strip = CreatePanel(opponent ? "Opponent Strip" : "Player Strip", board, new Color(0.04f, 0.055f, 0.075f, 0.95f)).GetComponent<RectTransform>();
            AddLayout(strip.gameObject, -1f, 42f, 0f, 0f);
            AddHorizontal(strip.gameObject, 12, 12, 4, 4, 8f, TextAnchor.MiddleCenter);

            Text name = CreateText(opponent ? "Opponent Name" : "Player Name", strip, 17, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            name.text = opponent ? "TEGENSTANDER" : "SPELER";
            AddLayout(name.gameObject, 190f, 34f, 0f, 0f);

            Text info = CreateText(opponent ? "Opponent Info" : "Player Info", strip, 15, FontStyle.Bold, TextAnchor.MiddleRight, new Color(0.78f, 0.86f, 0.93f, 1f));
            AddLayout(info.gameObject, -1f, 34f, 1f, 0f);

            if (opponent)
            {
                opponentInfoText = info;
            }
            else
            {
                playerInfoText = info;
            }
        }

        private void BuildCenterStrip(RectTransform board)
        {
            RectTransform center = CreatePanel("Center Strip", board, new Color(0.06f, 0.08f, 0.11f, 0.98f)).GetComponent<RectTransform>();
            AddLayout(center.gameObject, -1f, 126f, 0f, 0f);
            AddVertical(center.gameObject, 10, 10, 8, 8, 8f);

            phaseRail = CreatePanel("Phase Rail", center, new Color(1f, 1f, 1f, 0.02f)).GetComponent<RectTransform>();
            AddLayout(phaseRail.gameObject, -1f, 42f, 0f, 0f);
            AddHorizontal(phaseRail.gameObject, 2, 2, 2, 2, 6f, TextAnchor.MiddleCenter);

            bannerText = CreateText("Banner", center, 22, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.48f, 1f));
            AddLayout(bannerText.gameObject, -1f, 28f, 0f, 0f);

            actionsRow = CreatePanel("Actions", center, new Color(1f, 1f, 1f, 0.02f)).GetComponent<RectTransform>();
            AddLayout(actionsRow.gameObject, -1f, 42f, 0f, 0f);
            AddHorizontal(actionsRow.gameObject, 2, 2, 2, 2, 8f, TextAnchor.MiddleCenter);
        }

        private void BuildHandStrip(RectTransform board)
        {
            RectTransform handPanel = CreatePanel("Hand Strip", board, new Color(0.07f, 0.065f, 0.05f, 0.98f)).GetComponent<RectTransform>();
            AddLayout(handPanel.gameObject, -1f, 150f, 1f, 0f);
            AddVertical(handPanel.gameObject, 10, 10, 8, 8, 6f);

            Text handTitle = CreateText("Hand Title", handPanel, 16, FontStyle.Bold, TextAnchor.MiddleLeft, new Color(0.95f, 0.9f, 0.72f, 1f));
            handTitle.text = "HAND";
            AddLayout(handTitle.gameObject, -1f, 24f, 0f, 0f);

            handRow = CreatePanel("Hand Cards", handPanel, new Color(1f, 1f, 1f, 0.01f)).GetComponent<RectTransform>();
            AddLayout(handRow.gameObject, -1f, 112f, 1f, 0f);
            AddHorizontal(handRow.gameObject, 0, 0, 0, 0, 8f, TextAnchor.MiddleCenter);
            handRow.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = false;
        }

        private RectTransform CreateZoneRow(string name, RectTransform parent, float height)
        {
            RectTransform row = CreatePanel(name, parent, new Color(1f, 1f, 1f, 0.015f)).GetComponent<RectTransform>();
            AddLayout(row.gameObject, -1f, height, 0f, 0f);
            AddHorizontal(row.gameObject, 4, 4, 4, 4, 8f, TextAnchor.MiddleCenter);
            row.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = false;
            return row;
        }

        private void RefreshHeader()
        {
            opponentLpText.text = "OPP LP  " + Mathf.Max(0, engine.Opponent.LifePoints);
            playerLpText.text = "JOUW LP  " + Mathf.Max(0, engine.Player.LifePoints);
            turnText.text = "BEURT " + engine.TurnNumber + "  |  " + engine.GetActivePlayer().Name.ToUpperInvariant() + "  |  " + PhaseLabel(engine.CurrentPhase).ToUpperInvariant();
            opponentInfoText.text = BuildZoneInfo(engine.Opponent);
            playerInfoText.text = BuildZoneInfo(engine.Player);

            if (engine.IsGameOver)
            {
                bannerText.text = engine.WinnerText.ToUpperInvariant();
            }
            else if (engine.ActivePlayerId == DuelPlayerId.Opponent)
            {
                bannerText.text = "TEGENSTANDER DENKT...";
            }
            else
            {
                bannerText.text = PlayerPrompt();
            }
        }

        private void RefreshPhaseRail()
        {
            ClearChildren(phaseRail);

            DuelPhase[] phases =
            {
                DuelPhase.Draw,
                DuelPhase.Standby,
                DuelPhase.Main1,
                DuelPhase.Battle,
                DuelPhase.Main2,
                DuelPhase.End
            };

            for (int i = 0; i < phases.Length; i++)
            {
                DuelPhase phase = phases[i];
                bool active = engine.CurrentPhase == phase;
                Color color = active ? new Color(0.88f, 0.54f, 0.16f, 1f) : new Color(0.13f, 0.18f, 0.23f, 1f);
                Button button = CreateButton(PhaseShortLabel(phase), phaseRail, OnNextPhase, color, 95f, 34f);
                button.interactable = active &&
                                      engine.ActivePlayerId == DuelPlayerId.Player &&
                                      !engine.IsGameOver &&
                                      !IsAutomaticPlayerPhase();
            }
        }

        private void RefreshInspector()
        {
            DuelCard selected = GetSelectedCard();
            if (selected == null)
            {
                inspectorTitleText.text = "Geen kaart in focus";
                inspectorKindText.text = "Duel overzicht";
                inspectorStatsText.text = string.Empty;
                inspectorRulesText.fontSize = 18;
                inspectorRulesText.fontStyle = FontStyle.Normal;
                inspectorRulesText.text = BuildDuelOverviewText();
                inspectorArtImage.texture = null;
                inspectorArtFallbackText.text = "DUEL";
                inspectorArtFallbackText.enabled = true;
                return;
            }

            inspectorTitleText.text = selected.PublicName;
            inspectorKindText.text = selected.IsFaceDown ? "Verborgen kaart" : BuildKindText(selected);
            inspectorStatsText.text = BuildStatsText(selected);
            inspectorRulesText.fontSize = 21;
            inspectorRulesText.fontStyle = FontStyle.Bold;
            inspectorRulesText.text = selected.IsFaceDown ? "Deze kaart ligt face-down." : BuildReadableCardText(selected);
            RefreshInspectorImage(selected);
        }

        private void RefreshZones()
        {
            ClearChildren(opponentMonstersRow);
            ClearChildren(opponentSpellsRow);
            ClearChildren(playerMonstersRow);
            ClearChildren(playerSpellsRow);

            for (int zone = 0; zone < DuelEngine.ZoneCount; zone++)
            {
                BuildOpponentMonsterZone(zone);
                BuildOpponentSpellZone(zone);
                BuildPlayerSpellZone(zone);
                BuildPlayerMonsterZone(zone);
            }
        }

        private void BuildOpponentMonsterZone(int zone)
        {
            int capturedZone = zone;
            DuelCard card = engine.Opponent.MonsterZones[zone];
            Button button = CreateCardButton(card, "M" + (zone + 1), opponentMonstersRow, false, false);
            bool canTarget = selectedMonsterZone >= 0 && engine.CanAttackWithMonster(selectedMonsterZone) && card != null;
            button.interactable = canTarget;
            button.onClick.AddListener(() =>
            {
                engine.AttackMonster(selectedMonsterZone, capturedZone);
                ClearSelectedIndexes();
                Refresh();
            });
        }

        private void BuildOpponentSpellZone(int zone)
        {
            DuelCard card = engine.Opponent.SpellTrapZones[zone];
            Button button = CreateCardButton(card, "S/T" + (zone + 1), opponentSpellsRow, true, false);
            button.interactable = false;
        }

        private void BuildPlayerMonsterZone(int zone)
        {
            int capturedZone = zone;
            DuelCard card = engine.Player.MonsterZones[zone];
            Button button = CreateCardButton(card, "M" + (zone + 1), playerMonstersRow, false, selectedMonsterZone == zone);
            button.interactable = card != null && engine.ActivePlayerId == DuelPlayerId.Player && !engine.IsGameOver;
            button.onClick.AddListener(() =>
            {
                selectedMonsterZone = selectedMonsterZone == capturedZone ? -1 : capturedZone;
                selectedHandIndex = -1;
                selectedSpellZone = -1;
                Refresh();
            });
        }

        private void BuildPlayerSpellZone(int zone)
        {
            int capturedZone = zone;
            DuelCard card = engine.Player.SpellTrapZones[zone];
            Button button = CreateCardButton(card, "S/T" + (zone + 1), playerSpellsRow, true, selectedSpellZone == zone);
            button.interactable = card != null && engine.ActivePlayerId == DuelPlayerId.Player && !engine.IsGameOver;
            button.onClick.AddListener(() =>
            {
                selectedSpellZone = selectedSpellZone == capturedZone ? -1 : capturedZone;
                selectedHandIndex = -1;
                selectedMonsterZone = -1;
                Refresh();
            });
        }

        private void RefreshHand()
        {
            ClearChildren(handRow);

            for (int index = 0; index < engine.Player.Hand.Count; index++)
            {
                int capturedIndex = index;
                DuelCard card = engine.Player.Hand[index];
                Button button = CreateCardButton(card, string.Empty, handRow, false, selectedHandIndex == index);
                AddLayout(button.gameObject, CardWidthForHeight(108f), 108f, 0f, 0f);
                button.interactable = engine.ActivePlayerId == DuelPlayerId.Player && !engine.IsGameOver;
                button.onClick.AddListener(() =>
                {
                    selectedHandIndex = selectedHandIndex == capturedIndex ? -1 : capturedIndex;
                    selectedMonsterZone = -1;
                    selectedSpellZone = -1;
                    Refresh();
                });
            }
        }

        private void RefreshActions()
        {
            ClearChildren(actionsRow);

            if (engine.IsGameOver)
            {
                CreateButton("Nieuw duel", actionsRow, OnRestart, new Color(0.42f, 0.18f, 0.2f, 1f), 150f, 36f);
                return;
            }

            if (engine.ActivePlayerId != DuelPlayerId.Player)
            {
                Text wait = CreateText("Opponent Busy", actionsRow, 17, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.82f, 0.88f, 0.94f, 1f));
                wait.text = "Tegenstander speelt";
                AddLayout(wait.gameObject, -1f, 34f, 1f, 0f);
                return;
            }

            if (IsAutomaticPlayerPhase())
            {
                Text wait = CreateText("Automatic Phase", actionsRow, 17, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.95f, 0.86f, 0.55f, 1f));
                wait.text = "Automatische fase";
                AddLayout(wait.gameObject, -1f, 34f, 1f, 0f);
                return;
            }

            if (selectedHandIndex >= 0 && selectedHandIndex < engine.Player.Hand.Count)
            {
                BuildHandActions();
                return;
            }

            if (selectedMonsterZone >= 0 && engine.Player.MonsterZones[selectedMonsterZone] != null)
            {
                BuildMonsterActions();
                return;
            }

            if (selectedSpellZone >= 0 && engine.Player.SpellTrapZones[selectedSpellZone] != null)
            {
                BuildSpellTrapActions();
                return;
            }

            CreateButton("Volgende fase", actionsRow, OnNextPhase, new Color(0.18f, 0.35f, 0.58f, 1f), 150f, 36f);
            CreateButton("Beurt einde", actionsRow, OnEndTurn, new Color(0.25f, 0.27f, 0.32f, 1f), 130f, 36f);
        }

        private void BuildHandActions()
        {
            DuelCard card = engine.Player.Hand[selectedHandIndex];

            if (card.Definition.Kind == DuelCardKind.Monster)
            {
                Button summon = CreateButton("Summon", actionsRow, () =>
                {
                    engine.NormalSummonFromHand(selectedHandIndex, false);
                    ClearSelectedIndexes();
                    Refresh();
                }, new Color(0.2f, 0.44f, 0.28f, 1f), 120f, 36f);
                summon.interactable = engine.CanNormalSummonFromHand(selectedHandIndex, false);

                Button set = CreateButton("Set DEF", actionsRow, () =>
                {
                    engine.NormalSummonFromHand(selectedHandIndex, true);
                    ClearSelectedIndexes();
                    Refresh();
                }, new Color(0.19f, 0.34f, 0.48f, 1f), 120f, 36f);
                set.interactable = engine.CanNormalSummonFromHand(selectedHandIndex, true);
            }
            else
            {
                Button set = CreateButton("Set", actionsRow, () =>
                {
                    engine.SetSpellTrapFromHand(selectedHandIndex);
                    ClearSelectedIndexes();
                    Refresh();
                }, new Color(0.23f, 0.29f, 0.47f, 1f), 105f, 36f);
                set.interactable = engine.CanSetSpellTrapFromHand(selectedHandIndex);

                if (card.Definition.Kind == DuelCardKind.Spell)
                {
                    Button activate = CreateButton("Activeer", actionsRow, () =>
                    {
                        engine.ActivateSpellFromHand(selectedHandIndex);
                        ClearSelectedIndexes();
                        Refresh();
                    }, new Color(0.46f, 0.34f, 0.14f, 1f), 120f, 36f);
                    activate.interactable = engine.CanActivateSpellFromHand(selectedHandIndex);
                }
            }

            CreateButton("Annuleer", actionsRow, ClearSelection, new Color(0.23f, 0.24f, 0.27f, 1f), 115f, 36f);
        }

        private void BuildMonsterActions()
        {
            if (engine.IsMainPhase())
            {
                Button flip = CreateButton("Flip Summon", actionsRow, () =>
                {
                    engine.FlipSummonPlayerMonster(selectedMonsterZone);
                    Refresh();
                }, new Color(0.26f, 0.42f, 0.27f, 1f), 130f, 36f);
                flip.interactable = engine.CanFlipSummonPlayerMonster(selectedMonsterZone);

                Button position = CreateButton("Positie", actionsRow, () =>
                {
                    engine.TogglePlayerMonsterPosition(selectedMonsterZone);
                    Refresh();
                }, new Color(0.2f, 0.36f, 0.42f, 1f), 110f, 36f);
                position.interactable = engine.CanChangePlayerMonsterPosition(selectedMonsterZone);
            }

            if (engine.CanAttackWithMonster(selectedMonsterZone))
            {
                Button direct = CreateButton("Direct", actionsRow, () =>
                {
                    engine.DirectAttack(selectedMonsterZone);
                    ClearSelectedIndexes();
                    Refresh();
                }, new Color(0.48f, 0.18f, 0.16f, 1f), 110f, 36f);
                direct.interactable = CountOpponentMonsters() == 0;
            }

            CreateButton("Volgende fase", actionsRow, OnNextPhase, new Color(0.18f, 0.35f, 0.58f, 1f), 145f, 36f);
            CreateButton("Annuleer", actionsRow, ClearSelection, new Color(0.23f, 0.24f, 0.27f, 1f), 115f, 36f);
        }

        private void BuildSpellTrapActions()
        {
            Button activate = CreateButton("Activeer", actionsRow, () =>
            {
                engine.ActivateSetSpellTrap(selectedSpellZone);
                ClearSelectedIndexes();
                Refresh();
            }, new Color(0.46f, 0.34f, 0.14f, 1f), 120f, 36f);
            activate.interactable = engine.CanActivateSetSpellTrap(selectedSpellZone);

            CreateButton("Annuleer", actionsRow, ClearSelection, new Color(0.23f, 0.24f, 0.27f, 1f), 115f, 36f);
        }

        private void RefreshLog()
        {
            int start = Mathf.Max(0, engine.Log.Count - 15);
            StringBuilder builder = new StringBuilder();
            for (int i = engine.Log.Count - 1; i >= start; i--)
            {
                builder.Append("- ");
                builder.Append(engine.Log[i]);
                if (i > start)
                {
                    builder.AppendLine();
                }
            }

            logText.text = builder.ToString();
        }

        private DuelCard GetSelectedCard()
        {
            if (selectedHandIndex >= 0 && selectedHandIndex < engine.Player.Hand.Count)
            {
                return engine.Player.Hand[selectedHandIndex];
            }

            if (selectedMonsterZone >= 0 && selectedMonsterZone < DuelEngine.ZoneCount)
            {
                return engine.Player.MonsterZones[selectedMonsterZone];
            }

            if (selectedSpellZone >= 0 && selectedSpellZone < DuelEngine.ZoneCount)
            {
                return engine.Player.SpellTrapZones[selectedSpellZone];
            }

            return null;
        }

        private Button CreateCardButton(DuelCard card, string emptyLabel, Transform parent, bool compact, bool selected)
        {
            Color baseColor = card == null ? zoneEmpty : CardColor(card);
            Color color = selected ? selectedColor : baseColor;
            GameObject buttonObject = new GameObject("Card Slot", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Lighter(color, 0.12f);
            colors.pressedColor = Lighter(color, -0.08f);
            colors.selectedColor = Lighter(color, 0.08f);
            colors.disabledColor = new Color(color.r * 0.55f, color.g * 0.55f, color.b * 0.55f, 0.75f);
            button.colors = colors;

            AddVertical(buttonObject, 6, 6, 6, 6, 3f);
            float cardHeight = compact ? 68f : 94f;
            AddLayout(buttonObject, CardWidthForHeight(cardHeight), cardHeight, 0f, 0f);

            if (card != null && !card.IsFaceDown && card.Definition.HasImage)
            {
                RawImage cardArt = CreateRawImage("Card Art", buttonObject.transform, new Color(1f, 1f, 1f, compact ? 0.45f : 0.58f));
                LayoutElement artLayout = cardArt.gameObject.AddComponent<LayoutElement>();
                artLayout.ignoreLayout = true;
                RectTransform artRect = cardArt.GetComponent<RectTransform>();
                Stretch(artRect);
                artRect.offsetMin = new Vector2(4f, 4f);
                artRect.offsetMax = new Vector2(-4f, -4f);
                AddAspectFitter(cardArt.gameObject);
                StartCoroutine(LoadCardImage(card.Definition.ImageUrl, cardArt));
            }

            Text nameText = CreateText("Card Name", buttonObject.transform, compact ? 13 : 14, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            AddLayout(nameText.gameObject, -1f, compact ? 24f : 36f, 0f, 0f);

            Text bodyText = CreateText("Card Body", buttonObject.transform, compact ? 12 : 13, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.92f, 0.94f, 0.96f, 1f));
            AddLayout(bodyText.gameObject, -1f, compact ? 28f : 42f, 1f, 0f);

            if (card == null)
            {
                nameText.text = emptyLabel;
                bodyText.text = "EMPTY";
                return button;
            }

            nameText.text = CardDisplayName(card, compact);
            bodyText.text = CardBodyText(card, compact);
            return button;
        }

        private Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction onClick, Color color, float width, float height)
        {
            GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Lighter(color, 0.14f);
            colors.pressedColor = Lighter(color, -0.1f);
            colors.selectedColor = Lighter(color, 0.1f);
            colors.disabledColor = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, 0.72f);
            button.colors = colors;
            button.onClick.AddListener(onClick);

            Text text = CreateText("Label", buttonObject.transform, 15, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.GetComponent<RectTransform>());
            text.text = label;

            AddLayout(buttonObject, width, height, 0f, 0f);
            return button;
        }

        private RawImage CreateRawImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(parent, false);
            RawImage rawImage = imageObject.GetComponent<RawImage>();
            rawImage.color = color;
            rawImage.raycastTarget = false;
            return rawImage;
        }

        private float CardWidthForHeight(float height)
        {
            return Mathf.Round(height * CardPortraitAspectRatio);
        }

        private void AddAspectFitter(GameObject gameObject)
        {
            AspectRatioFitter fitter = gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = CardPortraitAspectRatio;
        }

        private void RefreshInspectorImage(DuelCard card)
        {
            inspectorArtImage.texture = null;
            if (card == null || card.IsFaceDown || !card.Definition.HasImage)
            {
                inspectorArtFallbackText.text = card != null && card.IsFaceDown ? "SET" : "DUEL";
                inspectorArtFallbackText.enabled = true;
                return;
            }

            inspectorArtFallbackText.text = "LOADING";
            inspectorArtFallbackText.enabled = true;
            StartCoroutine(LoadCardImage(card.Definition.ImageUrl, inspectorArtImage, () =>
            {
                if (inspectorArtImage.texture != null)
                {
                    inspectorArtFallbackText.enabled = false;
                }
            }));
        }

        private IEnumerator LoadCardImage(string imageUrl, RawImage target, System.Action onLoaded = null)
        {
            if (string.IsNullOrEmpty(imageUrl) || failedImageUrls.Contains(imageUrl))
            {
                yield break;
            }

            Texture2D cachedTexture;
            if (imageCache.TryGetValue(imageUrl, out cachedTexture))
            {
                if (target != null)
                {
                    target.texture = cachedTexture;
                }

                if (onLoaded != null)
                {
                    onLoaded();
                }

                yield break;
            }

            if (loadingImageUrls.Contains(imageUrl))
            {
                while (loadingImageUrls.Contains(imageUrl))
                {
                    yield return null;
                }

                if (imageCache.TryGetValue(imageUrl, out cachedTexture) && target != null)
                {
                    target.texture = cachedTexture;
                    if (onLoaded != null)
                    {
                        onLoaded();
                    }
                }

                yield break;
            }

            loadingImageUrls.Add(imageUrl);
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(request);
                    imageCache[imageUrl] = texture;
                    if (target != null)
                    {
                        target.texture = texture;
                    }

                    if (onLoaded != null)
                    {
                        onLoaded();
                    }
                }
                else
                {
                    failedImageUrls.Add(imageUrl);
                }
            }

            loadingImageUrls.Remove(imageUrl);
        }

        private string PlayerPrompt()
        {
            if (IsAutomaticPlayerPhase())
            {
                return "AUTOMATISCHE FASE";
            }

            if (selectedHandIndex >= 0 || selectedMonsterZone >= 0 || selectedSpellZone >= 0)
            {
                return "ACTIE KIEZEN";
            }

            if (engine.CurrentPhase == DuelPhase.Battle)
            {
                return "BATTLE PHASE";
            }

            return "JOUW ZET";
        }

        private bool IsAutomaticPlayerPhase()
        {
            return engine.CurrentPhase == DuelPhase.Draw ||
                   engine.CurrentPhase == DuelPhase.Standby ||
                   engine.CurrentPhase == DuelPhase.End;
        }

        private string BuildZoneInfo(DuelPlayerState player)
        {
            return "Deck " + player.Deck.Count +
                   "   Hand " + player.Hand.Count +
                   "   GY " + player.Graveyard.Count +
                   "   Banished " + player.Banished.Count;
        }

        private string BuildDuelOverviewText()
        {
            string lastEvent = engine.Log.Count > 0 ? engine.Log[engine.Log.Count - 1] : "Duel gestart.";
            return "Fase: " + PhaseLabel(engine.CurrentPhase) +
                   "\nActieve speler: " + engine.GetActivePlayer().Name +
                   "\nLaatste actie: " + lastEvent;
        }

        private string BuildStatsText(DuelCard card)
        {
            if (card == null || !card.IsMonster || card.IsFaceDown)
            {
                return string.Empty;
            }

            return "Level " + card.Definition.Level + "   ATK " + card.CurrentAttack + " / DEF " + card.CurrentDefense;
        }

        private string BuildKindText(DuelCard card)
        {
            if (card == null)
            {
                return string.Empty;
            }

            string text = string.IsNullOrEmpty(card.Definition.CardTypeText)
                ? card.Definition.Kind.ToString()
                : card.Definition.CardTypeText;

            if (!string.IsNullOrEmpty(card.Definition.Race))
            {
                text = card.Definition.Race + " / " + text;
            }

            if (!string.IsNullOrEmpty(card.Definition.Attribute))
            {
                text += " / " + card.Definition.Attribute;
            }

            if (!string.IsNullOrEmpty(card.Definition.Archetype))
            {
                text += " / " + card.Definition.Archetype;
            }

            return text;
        }

        private string BuildReadableCardText(DuelCard card)
        {
            if (card == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(BuildKindText(card));

            if (card.IsMonster)
            {
                builder.AppendLine();
                builder.Append("Level ");
                builder.Append(card.Definition.Level);
                builder.Append("   ATK ");
                builder.Append(card.CurrentAttack);
                builder.Append(" / DEF ");
                builder.Append(card.CurrentDefense);
            }

            if (!string.IsNullOrEmpty(card.Definition.RulesText))
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.Append(NormalizeDisplayedText(card.Definition.RulesText));
            }

            return builder.ToString();
        }

        private string NormalizeDisplayedText(string text)
        {
            return string.IsNullOrEmpty(text)
                ? string.Empty
                : text.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private string CardDisplayName(DuelCard card, bool compact)
        {
            if (card.IsFaceDown)
            {
                return card.Definition.Kind == DuelCardKind.Monster ? "Set Monster" : "Set " + card.Definition.Kind;
            }

            string name = card.Definition.Name;
            if (compact && name.Length > 16)
            {
                return name.Substring(0, 15) + ".";
            }

            return name;
        }

        private string CardBodyText(DuelCard card, bool compact)
        {
            if (card.IsFaceDown)
            {
                return "FACE-DOWN";
            }

            if (card.Definition.Kind == DuelCardKind.Monster)
            {
                string position = card.Position == MonsterBattlePosition.Attack ? "ATK" : "DEF";
                int value = card.Position == MonsterBattlePosition.Attack ? card.CurrentAttack : card.CurrentDefense;
                string attacked = card.HasAttacked ? "\nUSED" : string.Empty;
                return "Lv " + card.Definition.Level + "\n" + position + " " + value + attacked;
            }

            if (compact)
            {
                return card.Definition.Kind.ToString();
            }

            return card.Definition.Kind + "\n" + ShortEffect(card);
        }

        private string ShortEffect(DuelCard card)
        {
            switch (card.Definition.Effect)
            {
                case DuelEffectKind.DrawCards:
                    return "Draw " + card.Definition.EffectValue;
                case DuelEffectKind.GainLife:
                    return "+LP";
                case DuelEffectKind.DealDamage:
                    return "Damage";
                case DuelEffectKind.BoostOwnMonsters:
                    return "+ATK";
                case DuelEffectKind.DestroyWeakestOpponentMonster:
                    return "Destroy";
                case DuelEffectKind.NegateAttack:
                    return "Negate";
                case DuelEffectKind.DestroyAttacker:
                    return "Counter";
                default:
                    return "Effect";
            }
        }

        private Color CardColor(DuelCard card)
        {
            if (card.IsFaceDown)
            {
                return new Color(0.12f, 0.16f, 0.27f, 1f);
            }

            if (card.Definition.Kind == DuelCardKind.Monster)
            {
                return monsterColor;
            }

            if (card.Definition.Kind == DuelCardKind.Spell)
            {
                return spellColor;
            }

            return trapColor;
        }

        private int CountOpponentMonsters()
        {
            int count = 0;
            for (int zone = 0; zone < DuelEngine.ZoneCount; zone++)
            {
                if (engine.Opponent.MonsterZones[zone] != null)
                {
                    count++;
                }
            }

            return count;
        }

        private int ClampSelection(int value, int maxExclusive)
        {
            if (value < 0 || value >= maxExclusive)
            {
                return -1;
            }

            return value;
        }

        private string PhaseLabel(DuelPhase phase)
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

        private string PhaseShortLabel(DuelPhase phase)
        {
            switch (phase)
            {
                case DuelPhase.Draw:
                    return "DP";
                case DuelPhase.Standby:
                    return "SP";
                case DuelPhase.Main1:
                    return "M1";
                case DuelPhase.Battle:
                    return "BP";
                case DuelPhase.Main2:
                    return "M2";
                case DuelPhase.End:
                    return "EP";
                default:
                    return phase.ToString();
            }
        }

        private void OnNextPhase()
        {
            if (engine.ActivePlayerId != DuelPlayerId.Player || engine.IsGameOver || IsAutomaticPlayerPhase())
            {
                return;
            }

            ClearSelectedIndexes();
            engine.AdvancePhase();
            Refresh();
        }

        private void OnEndTurn()
        {
            if (engine.ActivePlayerId != DuelPlayerId.Player || engine.IsGameOver || IsAutomaticPlayerPhase())
            {
                return;
            }

            ClearSelectedIndexes();
            engine.EndCurrentTurn();
            Refresh();
        }

        private void OnRestart()
        {
            StopAllCoroutines();
            opponentRoutineRunning = false;
            playerAutoPhaseRoutineRunning = false;
            loadingImageUrls.Clear();
            ClearSelectedIndexes();
            engine.StartNewDuel(YugiohCardDatabase.BuildStarterDeck());
            Refresh();
        }

        private void ClearSelection()
        {
            ClearSelectedIndexes();
            Refresh();
        }

        private void ClearSelectedIndexes()
        {
            selectedHandIndex = -1;
            selectedMonsterZone = -1;
            selectedSpellZone = -1;
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            eventSystemObject.transform.SetParent(transform, false);
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private RectTransform CreateAnchoredPanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
        {
            RectTransform rectTransform = CreatePanel(name, parent, color).GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
            return rectTransform;
        }

        private Text CreateText(string name, Transform parent, int size, FontStyle style, TextAnchor alignment, Color color)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.font = uiFont;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private void AddVertical(GameObject gameObject, int left, int right, int top, int bottom, float spacing)
        {
            VerticalLayoutGroup layout = gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(left, right, top, bottom);
            layout.spacing = spacing;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        private void AddHorizontal(GameObject gameObject, int left, int right, int top, int bottom, float spacing, TextAnchor alignment)
        {
            HorizontalLayoutGroup layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(left, right, top, bottom);
            layout.spacing = spacing;
            layout.childAlignment = alignment;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;
        }

        private void AddLayout(GameObject gameObject, float preferredWidth, float preferredHeight, float flexibleWidth, float flexibleHeight)
        {
            LayoutElement layout = gameObject.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = gameObject.AddComponent<LayoutElement>();
            }

            if (preferredWidth > 0f)
            {
                layout.preferredWidth = preferredWidth;
            }

            if (preferredHeight > 0f)
            {
                layout.preferredHeight = preferredHeight;
            }

            layout.flexibleWidth = flexibleWidth;
            layout.flexibleHeight = flexibleHeight;
        }

        private void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private Color Lighter(Color color, float amount)
        {
            return new Color(
                Mathf.Clamp01(color.r + amount),
                Mathf.Clamp01(color.g + amount),
                Mathf.Clamp01(color.b + amount),
                color.a);
        }

        private void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
}
