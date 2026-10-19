using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelRankedView
    {
        private static readonly Color Background = new Color(0.025f, 0.045f, 0.08f, 0.99f);
        private static readonly Color Card = new Color(0.055f, 0.09f, 0.14f, 1f);
        private static readonly Color Gold = new Color(1f, 0.78f, 0.38f, 1f);
        private static readonly Color Muted = new Color(0.64f, 0.75f, 0.84f, 1f);
        private GameObject canvasObject;
        private GameObject overlay;
        private Button badgeButton;
        private Font font;
        private bool rankedEnabled;

        public bool IsOpen { get { return overlay != null && overlay.activeSelf; } }

        public void Build(Transform parent, CleanDuelAccessSession session)
        {
            rankedEnabled = session != null && session.RankedEnabled;
            CleanDuelRankedSnapshot snapshot = rankedEnabled ? session.Ranked : null;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            canvasObject = new GameObject("Icarus Ranked", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            RectTransform badge = Panel("Rank Badge Button", canvasObject.transform, Background);
            Rect(badge, new Vector2(0f, 1f), new Vector2(108f, -20f), new Vector2(274f, 62f), new Vector2(0f, 1f));
            badgeButton = badge.gameObject.AddComponent<Button>();
            badgeButton.targetGraphic = badge.GetComponent<Image>();
            badgeButton.interactable = rankedEnabled;
            badgeButton.navigation = new Navigation { mode = Navigation.Mode.None };
            badgeButton.onClick.AddListener(() => SetOpen(!IsOpen));
            if (snapshot != null) Badge(badge, snapshot.Rank, new Vector2(5f, -3f), 56f);
            Text badgeTitle = Label("Rank Badge Title", badge, !rankedEnabled ? "RANKED UIT" : snapshot == null ? "RANKED" : snapshot.Rank.Name.ToUpperInvariant() + " " + snapshot.Level, 18, snapshot == null ? Gold : RankAccent(snapshot.Rank), FontStyle.Bold);
            Rect(badgeTitle.rectTransform, new Vector2(0f, 1f), new Vector2(68f, -8f), new Vector2(198f, 24f));
            Text badgeSubtitle = Label("Rank Badge Subtitle", badge, !rankedEnabled ? "Niet beschikbaar voor dit account" : snapshot == null ? "Serverniveau niet beschikbaar" : (snapshot.IsSimulation ? "SIM  /  " : string.Empty) + snapshot.Rank.PatronName + "  /  " + snapshot.LevelWins + "/4 zeges", 13, Muted);
            Rect(badgeSubtitle.rectTransform, new Vector2(0f, 1f), new Vector2(68f, -34f), new Vector2(198f, 20f));

            overlay = Panel("Ranked Overlay", canvasObject.transform, new Color(0f, 0.01f, 0.025f, 0.88f)).gameObject;
            Stretch(overlay.GetComponent<RectTransform>());
            RectTransform panel = Panel("Ranked Panel", overlay.transform, Background);
            Rect(panel, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1160f, 800f), new Vector2(0.5f, 0.5f));
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = Gold;
            outline.effectDistance = new Vector2(1f, -1f);
            Text title = Label("Ranked Title", panel, "RANKED  //  VAN BRONZE 4 NAAR MASTER 1", 30, Gold, FontStyle.Bold);
            Rect(title.rectTransform, new Vector2(0f, 1f), new Vector2(32f, -26f), new Vector2(980f, 44f));
            Text season = Label("Ranked Season", panel, snapshot == null ? "Geen geldige niveaugegevens ontvangen. Werk de server bij en log opnieuw in." : (snapshot.IsSimulation ? "SIMULATIESEIZOEN  //  " : "SERVERSEIZOEN  //  ") + snapshot.SeasonId + "  //  " + session.DisplayName, 15, Muted);
            Rect(season.rectTransform, new Vector2(0f, 1f), new Vector2(34f, -77f), new Vector2(1000f, 28f));
            Button close = CloseButton(panel);
            close.onClick.AddListener(() => SetOpen(false));
            BuildSummary(panel, snapshot);
            BuildGallery(panel, snapshot);
            Text practice = Label("Ranked Practice Notice", panel,
                "Oefenduel tegen AI: dit duel verandert je rang of diamanten niet. Online ranked-matchmaking is nog niet gekoppeld.\nElke promotie: +100 diamanten, of +200 totaal bij een rangovergang. Ook opnieuw promoveren wordt beloond.", 16, Muted);
            Rect(practice.rectTransform, new Vector2(0f, 1f), new Vector2(34f, -674f), new Vector2(1090f, 60f));
            Text refresh = Label("Ranked Refresh Notice", panel, "Serverstand bij inloggen. Log opnieuw in voor een gewijzigde rang of wallet. Simulaties wijzigen de echte wallet niet.", 14, Muted);
            Rect(refresh.rectTransform, new Vector2(0f, 1f), new Vector2(34f, -751f), new Vector2(1090f, 25f));
            overlay.SetActive(false);
        }

        public void SetOpen(bool open)
        {
            if (overlay != null) overlay.SetActive(open && rankedEnabled);
        }

        public void Dispose()
        {
            if (canvasObject != null) Object.Destroy(canvasObject);
            canvasObject = null;
            overlay = null;
            badgeButton = null;
        }

        private void BuildSummary(RectTransform panel, CleanDuelRankedSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Text missing = Label("Ranked Summary", panel,
                    "Bekijk de metaal- en edelsteenrangen met hun Griekse beschermers.\nJe eigen rang verschijnt zodra de server geldige niveaugegevens meestuurt.", 22, Color.white);
                Rect(missing.rectTransform, new Vector2(0f, 1f), new Vector2(34f, -137f), new Vector2(1080f, 120f));
                return;
            }

            Badge(panel, snapshot.Rank, new Vector2(30f, -121f), 140f);
            Text current = Label("Ranked Summary", panel, snapshot.DisplayName, 32, Color.white, FontStyle.Bold);
            Rect(current.rectTransform, new Vector2(0f, 1f), new Vector2(192f, -128f), new Vector2(880f, 45f));
            Text wallet = Label("Ranked Wallet", panel, (snapshot.IsSimulation ? "Simulatiewallet: " : "Diamanten: ") + snapshot.Diamonds.ToString("N0", CultureInfo.GetCultureInfo("nl-NL")), 22, Gold, FontStyle.Bold);
            wallet.resizeTextForBestFit = true;
            wallet.resizeTextMinSize = 12;
            wallet.resizeTextMaxSize = 22;
            Rect(wallet.rectTransform, new Vector2(0f, 1f), new Vector2(195f, -180f), new Vector2(380f, 34f));
            string decay = snapshot.NextDecayAt.HasValue
                ? "Per 30 dagen niet spelen: 1 niveau omlaag.\nVolgende controle: " + snapshot.NextDecayAt.Value.UtcDateTime.ToString("dd-MM-yyyy HH:mm 'UTC'", CultureInfo.InvariantCulture)
                : "Per 30 dagen niet spelen: 1 niveau omlaag.\nNog geen ranked-duel gespeeld.";
            Text inactivity = Label("Ranked Decay", panel, decay, 15, Muted);
            Rect(inactivity.rectTransform, new Vector2(0f, 1f), new Vector2(590f, -178f), new Vector2(500f, 46f));
            Text stats = Label("Ranked Record", panel, snapshot.Games + " duels  ·  " + snapshot.Wins + " gewonnen  /  " + snapshot.Losses + " verloren  /  " + snapshot.Draws + " gelijk", 18, Muted);
            Rect(stats.rectTransform, new Vector2(0f, 1f), new Vector2(195f, -227f), new Vector2(890f, 28f));
            bool highestLevel = snapshot.LadderStep == CleanDuelRankCatalog.StepCount - 1;
            Text levelProgress = Label("Ranked Level Progress", panel,
                (highestLevel ? "Topniveau: " : "Promotie: ") + snapshot.LevelWins + " / " + snapshot.WinsRequired + " zeges   //   Degradatie: " + snapshot.LevelLosses + " / " + snapshot.LossesRequired + " nederlagen", 20, RankAccent(snapshot.Rank), FontStyle.Bold);
            Rect(levelProgress.rectTransform, new Vector2(0f, 1f), new Vector2(34f, -279f), new Vector2(1090f, 30f));
            float progress = snapshot.LevelWins / (float)snapshot.WinsRequired;
            RectTransform track = Panel("Rank Progress Track", panel, Card);
            Rect(track, new Vector2(0f, 1f), new Vector2(34f, -318f), new Vector2(1090f, 8f));
            RectTransform fill = Panel("Rank Progress Fill", track, RankAccent(snapshot.Rank));
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(progress, 1f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            string progressDescription = highestLevel
                ? "Hoogste niveau bereikt: " + snapshot.DisplayName + ". Geen winreeks nodig; tellers worden bij niveauverandering gereset."
                : "Nog " + (snapshot.WinsRequired - snapshot.LevelWins) + " zeges tot " + CleanDuelRankCatalog.DisplayForStep(snapshot.LadderStep + 1) + ". Geen winreeks nodig; tellers resetten bij niveauverandering.";
            Text progressText = Label("Rank Progress Text", panel, progressDescription, 15, Muted);
            Rect(progressText.rectTransform, new Vector2(0f, 1f), new Vector2(34f, -337f), new Vector2(1090f, 28f));
        }

        private void BuildGallery(RectTransform panel, CleanDuelRankedSnapshot snapshot)
        {
            for (int index = 0; index < CleanDuelRankCatalog.Count; index++)
            {
                CleanDuelRankDefinition rank = CleanDuelRankCatalog.At(index);
                bool current = snapshot != null && snapshot.Rank.Id == rank.Id;
                Color accent = RankAccent(rank);
                RectTransform card = Panel("Rank Gallery " + rank.Id, panel, current ? Color.Lerp(Card, accent, 0.14f) : Card);
                Rect(card, new Vector2(0f, 1f), new Vector2(34f + (index % 3) * 368f, -374f - (index / 3) * 149f), new Vector2(354f, 134f));
                if (current)
                {
                    Outline outline = card.gameObject.AddComponent<Outline>();
                    outline.effectColor = accent;
                    outline.effectDistance = new Vector2(1f, -1f);
                }
                Badge(card, rank, new Vector2(5f, -10f), 114f);
                Text name = Label("Rank Name", card, rank.Name.ToUpperInvariant(), 21, accent, FontStyle.Bold);
                Rect(name.rectTransform, new Vector2(0f, 1f), new Vector2(128f, -14f), new Vector2(218f, 28f));
                Text patron = Label("Rank Patron", card, rank.PatronName, 16, Color.white);
                Rect(patron.rectTransform, new Vector2(0f, 1f), new Vector2(128f, -45f), new Vector2(218f, 23f));
                Text levels = Label("Rank Levels", card, rank.Levels + " niveaus  //  " + rank.Levels + " → 1", 17, accent);
                Rect(levels.rectTransform, new Vector2(0f, 1f), new Vector2(128f, -73f), new Vector2(218f, 24f));
                Text motif = Label("Rank Motif", card, rank.Motif, 13, Muted);
                Rect(motif.rectTransform, new Vector2(0f, 1f), new Vector2(128f, -102f), new Vector2(218f, 22f));
            }
        }

        private void Badge(Transform parent, CleanDuelRankDefinition rank, Vector2 position, float size)
        {
            Texture2D texture = Resources.Load<Texture2D>("Ranked/" + rank.Id);
            if (texture == null)
            {
                Text fallback = Label("Rank Emblem Fallback " + rank.Id, parent, rank.Name.Substring(0, 1), Mathf.RoundToInt(size * 0.45f), RankAccent(rank), FontStyle.Bold);
                fallback.alignment = TextAnchor.MiddleCenter;
                Rect(fallback.rectTransform, new Vector2(0f, 1f), position, new Vector2(size, size));
                return;
            }
            GameObject imageObject = new GameObject("Rank Emblem " + rank.Id, typeof(RectTransform), typeof(RawImage));
            imageObject.transform.SetParent(parent, false);
            RawImage image = imageObject.GetComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            Rect(image.rectTransform, new Vector2(0f, 1f), position, new Vector2(size, size));
        }

        private Button CloseButton(RectTransform parent)
        {
            RectTransform rect = Panel("Close Ranked", parent, Card);
            Rect(rect, new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(54f, 44f), new Vector2(1f, 1f));
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Text text = Label("Close Ranked Label", rect, "X", 24, Color.white, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;
            Stretch(text.rectTransform);
            return button;
        }

        private static Color RankAccent(CleanDuelRankDefinition rank)
        {
            switch (rank.Id)
            {
                case "icarus": return new Color(0.91f, 0.63f, 0.39f, 1f);
                case "hermes": return new Color(0.8f, 0.86f, 0.93f, 1f);
                case "athena": return Gold;
                case "apollo": return new Color(0.7f, 0.97f, 0.91f, 1f);
                case "poseidon": return new Color(0.47f, 0.85f, 1f, 1f);
                case "zeus": return new Color(0.88f, 0.64f, 1f, 1f);
                default: return Gold;
            }
        }

        private RectTransform Panel(string name, Transform parent, Color color)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            gameObject.GetComponent<Image>().color = color;
            return gameObject.GetComponent<RectTransform>();
        }

        private Text Label(string name, Transform parent, string value, int size, Color color, FontStyle style = FontStyle.Normal)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            Text text = gameObject.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        private static void Rect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            Rect(rect, anchor, position, size, new Vector2(0f, 1f));
        }

        private static void Rect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
