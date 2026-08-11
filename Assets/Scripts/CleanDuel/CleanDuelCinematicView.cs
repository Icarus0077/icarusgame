using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelCinematicView
    {
        private const int BlueEyesWhiteDragonCardId = 89631139;
        private const int DarkMagicianCardId = 46986414;

        private readonly CleanDuelArtCache artCache;
        private Font font;
        private Transform cinematicCanvas;
        private RectTransform overlay;
        private CanvasGroup canvasGroup;
        private Image leftWash;
        private Image rightWash;
        private Image blueAura;
        private Image darkAura;
        private Image energyBeam;
        private Image clashCore;
        private Image bluePortrait;
        private Image darkPortrait;
        private RawImage blueEyesModelImage;
        private CleanDuelBlueEyesSummonModel blueEyesModel;
        private Text blueFallback;
        private Text darkFallback;
        private Text blueName;
        private Text darkName;
        private Text title;
        private Text subtitle;
        private bool blueEyesModelUnavailable;
        private bool showingBlueEyesModel;
        private bool skipRequested;

        public CleanDuelCinematicView(CleanDuelArtCache artCache)
        {
            this.artCache = artCache;
        }

        public void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            Sprite circleSprite = CreateCircleSprite(128);
            GameObject canvasObject = new GameObject(
                "Duel Cinematic Layer",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            cinematicCanvas = canvasObject.transform;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            Stretch(canvasObject.GetComponent<RectTransform>());

            GameObject overlayObject = new GameObject(
                "Cinematic Overlay",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(Image),
                typeof(Button));
            overlayObject.transform.SetParent(canvasObject.transform, false);
            overlay = overlayObject.GetComponent<RectTransform>();
            Stretch(overlay);
            canvasGroup = overlayObject.GetComponent<CanvasGroup>();
            Image scrim = overlayObject.GetComponent<Image>();
            scrim.color = new Color(0.003f, 0.006f, 0.018f, 0.94f);
            Button skipButton = overlayObject.GetComponent<Button>();
            skipButton.transition = Selectable.Transition.None;
            skipButton.onClick.AddListener(RequestSkip);

            leftWash = AddImage("Blue Energy Field", overlay, null, new Color(0.025f, 0.25f, 0.48f, 0.76f));
            SetNormalizedRect(leftWash.rectTransform, Vector2.zero, new Vector2(0.56f, 1f));
            rightWash = AddImage("Dark Energy Field", overlay, null, new Color(0.28f, 0.025f, 0.44f, 0.78f));
            SetNormalizedRect(rightWash.rectTransform, new Vector2(0.44f, 0f), Vector2.one);

            blueAura = AddImage("Blue Arc Ring", overlay, circleSprite, CleanDuelVisualTheme.LocalGlow);
            SetCenteredRect(blueAura.rectTransform, new Vector2(600f, 600f), new Vector2(-430f, 8f));
            darkAura = AddImage("Dark Arc Ring", overlay, circleSprite, new Color(0.82f, 0.24f, 1f, 1f));
            SetCenteredRect(darkAura.rectTransform, new Vector2(600f, 600f), new Vector2(430f, 8f));

            blueEyesModelImage = AddRawImage("Blue-Eyes 3D Summon Model", overlay, null);
            SetCenteredRect(blueEyesModelImage.rectTransform, new Vector2(1080f, 720f), new Vector2(0f, -8f));

            energyBeam = AddImage("Clash Beam", overlay, null, Color.white);
            SetCenteredRect(energyBeam.rectTransform, new Vector2(1120f, 24f), Vector2.zero);
            clashCore = AddImage("Clash Core", overlay, circleSprite, Color.white);
            SetCenteredRect(clashCore.rectTransform, new Vector2(120f, 120f), Vector2.zero);
            Outline coreGlow = clashCore.gameObject.AddComponent<Outline>();
            coreGlow.effectColor = new Color(0.55f, 0.92f, 1f, 0.9f);
            coreGlow.effectDistance = new Vector2(8f, -8f);

            bluePortrait = AddPortrait(
                "Blue-Eyes Portrait",
                new Vector2(-430f, -12f),
                new Color(0.08f, 0.34f, 0.58f, 1f),
                out blueFallback);
            darkPortrait = AddPortrait(
                "Dark Magician Portrait",
                new Vector2(430f, -12f),
                new Color(0.32f, 0.06f, 0.48f, 1f),
                out darkFallback);

            blueName = AddText("Blue-Eyes Name", overlay, 27, FontStyle.Bold, TextAnchor.MiddleCenter, CleanDuelVisualTheme.LocalGlow);
            SetCenteredRect(blueName.rectTransform, new Vector2(560f, 58f), new Vector2(-430f, -288f));
            darkName = AddText("Dark Magician Name", overlay, 27, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.9f, 0.56f, 1f, 1f));
            SetCenteredRect(darkName.rectTransform, new Vector2(560f, 58f), new Vector2(430f, -288f));

            title = AddText("Cinematic Title", overlay, 58, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            SetCenteredRect(title.rectTransform, new Vector2(1320f, 92f), new Vector2(0f, 420f));
            title.resizeTextForBestFit = true;
            title.resizeTextMinSize = 36;
            title.resizeTextMaxSize = 58;

            subtitle = AddText("Cinematic Subtitle", overlay, 23, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.76f, 0.91f, 1f, 1f));
            SetCenteredRect(subtitle.rectTransform, new Vector2(1180f, 58f), new Vector2(0f, -404f));

            Text skipHint = AddText("Skip Hint", overlay, 16, FontStyle.Normal, TextAnchor.MiddleRight, new Color(0.74f, 0.8f, 0.9f, 0.78f));
            skipHint.text = "CLICK TO SKIP";
            Anchor(skipHint.rectTransform, new Vector2(0.72f, 0f), new Vector2(1f, 0f), new Vector2(0f, 22f), new Vector2(-30f, 66f));

            HideImmediate();
        }

        public IEnumerator Play(CleanDuelCinematicEvent cinematicEvent)
        {
            if (cinematicEvent == null || overlay == null)
            {
                yield break;
            }

            Configure(cinematicEvent);
            skipRequested = false;
            overlay.gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            const float enterDuration = 0.3f;
            float holdDuration = cinematicEvent.Kind == CleanDuelCinematicKind.RivalryClimax ? 1.25f : 1.05f;
            const float exitDuration = 0.24f;
            float totalDuration = enterDuration + holdDuration + exitDuration;
            float elapsed = 0f;
            while (elapsed < totalDuration && !skipRequested)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / totalDuration);
                canvasGroup.alpha = elapsed < enterDuration
                    ? Smooth01(elapsed / enterDuration)
                    : elapsed > enterDuration + holdDuration
                        ? 1f - Smooth01((elapsed - enterDuration - holdDuration) / exitDuration)
                        : 1f;
                Animate(cinematicEvent, normalized);
                yield return null;
            }

            if (skipRequested)
            {
                float fade = canvasGroup.alpha;
                float skipElapsed = 0f;
                while (skipElapsed < 0.08f)
                {
                    skipElapsed += Time.unscaledDeltaTime;
                    canvasGroup.alpha = Mathf.Lerp(fade, 0f, Mathf.Clamp01(skipElapsed / 0.08f));
                    yield return null;
                }
            }

            HideImmediate();
        }

        public void HideImmediate()
        {
            skipRequested = true;
            if (overlay == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            overlay.gameObject.SetActive(false);
            showingBlueEyesModel = false;
            if (blueEyesModelImage != null)
            {
                blueEyesModelImage.gameObject.SetActive(false);
            }
            if (blueEyesModel != null)
            {
                blueEyesModel.SetVisible(false);
            }
        }

        public void Dispose()
        {
            HideImmediate();
            if (blueEyesModelImage != null)
            {
                blueEyesModelImage.texture = null;
            }
            if (blueEyesModel != null)
            {
                blueEyesModel.Dispose();
                blueEyesModel = null;
            }
            cinematicCanvas = null;
        }

        private void Configure(CleanDuelCinematicEvent cinematicEvent)
        {
            bool rivalry = cinematicEvent.Kind == CleanDuelCinematicKind.RivalryClimax;
            bool wantsBlueEyesModel = cinematicEvent.Kind == CleanDuelCinematicKind.SignatureSummon &&
                                      cinematicEvent.AttackerCardId == BlueEyesWhiteDragonCardId;
            showingBlueEyesModel = wantsBlueEyesModel && EnsureBlueEyesModel();
            if (rivalry)
            {
                Sprite blueSprite = artCache != null ? artCache.GetSprite(BlueEyesWhiteDragonCardId) : null;
                Sprite darkSprite = artCache != null ? artCache.GetSprite(DarkMagicianCardId) : null;
                ConfigurePortrait(bluePortrait, blueFallback, blueSprite, "WHITE\nDRAGON", new Color(0.06f, 0.3f, 0.54f, 1f));
                ConfigurePortrait(darkPortrait, darkFallback, darkSprite, "DARK\nMAGICIAN", new Color(0.3f, 0.045f, 0.46f, 1f));
                blueName.text = "BLUE-EYES WHITE DRAGON";
                darkName.text = "DARK MAGICIAN";
                title.text = "RIVALRY CLIMAX";
                subtitle.text = "LOW LP  /  LEGENDARY BATTLE  /  LP <= " + cinematicEvent.Value;
                leftWash.color = new Color(0.018f, 0.24f, 0.48f, 0.78f);
                rightWash.color = new Color(0.28f, 0.018f, 0.43f, 0.8f);
                energyBeam.color = new Color(0.82f, 0.94f, 1f, 0.94f);
                blueAura.color = CleanDuelVisualTheme.LocalGlow;
                darkAura.color = new Color(0.82f, 0.24f, 1f, 1f);
                blueName.color = CleanDuelVisualTheme.LocalGlow;
                darkName.color = new Color(0.9f, 0.56f, 1f, 1f);
            }
            else
            {
                Color accent = AccentFor(cinematicEvent);
                Sprite primarySprite = artCache != null ? artCache.GetSprite(cinematicEvent.AttackerCardId) : null;
                ConfigurePortrait(
                    bluePortrait,
                    blueFallback,
                    primarySprite,
                    FallbackLabel(cinematicEvent.AttackerName),
                    new Color(accent.r * 0.34f, accent.g * 0.34f, accent.b * 0.34f, 1f));
                blueName.text = cinematicEvent.AttackerName.ToUpperInvariant();
                darkName.text = string.Empty;
                title.text = TitleFor(cinematicEvent);
                subtitle.text = SubtitleFor(cinematicEvent);
                leftWash.color = new Color(accent.r * 0.38f, accent.g * 0.38f, accent.b * 0.38f, 0.82f);
                rightWash.color = new Color(accent.r * 0.12f, accent.g * 0.12f, accent.b * 0.12f, 0.88f);
                energyBeam.color = new Color(accent.r, accent.g, accent.b, 0.96f);
                blueAura.color = accent;
                blueName.color = accent;
            }

            bluePortrait.gameObject.SetActive(!showingBlueEyesModel);
            blueEyesModelImage.gameObject.SetActive(showingBlueEyesModel);
            if (blueEyesModel != null)
            {
                blueEyesModel.SetVisible(showingBlueEyesModel);
            }
            blueAura.gameObject.SetActive(true);
            blueName.gameObject.SetActive(true);
            darkPortrait.gameObject.SetActive(rivalry);
            darkAura.gameObject.SetActive(rivalry);
            darkName.gameObject.SetActive(rivalry);
            clashCore.color = energyBeam.color;

            blueName.rectTransform.anchoredPosition = rivalry ? new Vector2(-430f, -288f) : new Vector2(0f, -288f);
            blueAura.rectTransform.anchoredPosition = rivalry ? new Vector2(-430f, 8f) : new Vector2(0f, 8f);
            darkName.rectTransform.anchoredPosition = new Vector2(430f, -288f);
        }

        private bool EnsureBlueEyesModel()
        {
            if (blueEyesModel != null)
            {
                return blueEyesModel.Texture != null;
            }

            if (blueEyesModelUnavailable || cinematicCanvas == null || blueEyesModelImage == null)
            {
                return false;
            }

            CleanDuelBlueEyesSummonModel candidate = new CleanDuelBlueEyesSummonModel();
            try
            {
                candidate.Build(cinematicCanvas);
                if (candidate.Texture == null)
                {
                    throw new System.InvalidOperationException("The 3D preview did not create a render texture.");
                }

                blueEyesModel = candidate;
                blueEyesModelImage.texture = blueEyesModel.Texture;
                return true;
            }
            catch (System.Exception exception)
            {
                candidate.Dispose();
                DisableBlueEyesModel(exception);
                return false;
            }
        }

        private void DisableBlueEyesModel(System.Exception exception)
        {
            if (blueEyesModel != null)
            {
                blueEyesModel.SetVisible(false);
                blueEyesModel.Dispose();
                blueEyesModel = null;
            }

            blueEyesModelUnavailable = true;
            showingBlueEyesModel = false;
            if (blueEyesModelImage != null)
            {
                blueEyesModelImage.texture = null;
                blueEyesModelImage.gameObject.SetActive(false);
            }
            if (bluePortrait != null)
            {
                bluePortrait.gameObject.SetActive(true);
            }

            if (exception != null)
            {
                Debug.LogWarning(
                    "Blue-Eyes 3D summon preview could not be created. The card summon cut-in remains available. " +
                    exception.Message);
            }
        }

        private void Animate(CleanDuelCinematicEvent cinematicEvent, float normalized)
        {
            bool rivalry = cinematicEvent.Kind == CleanDuelCinematicKind.RivalryClimax;
            float reveal = Smooth01(Mathf.InverseLerp(0f, 0.24f, normalized));
            float impact = Smooth01(Mathf.InverseLerp(0.16f, 0.48f, normalized));
            float pulse = 1f + Mathf.Sin(normalized * Mathf.PI * 8f) * 0.035f * impact;

            if (showingBlueEyesModel)
            {
                try
                {
                    blueEyesModel.Animate(normalized);
                    blueEyesModelImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.88f, 1f, reveal);
                    blueEyesModelImage.color = new Color(1f, 1f, 1f, reveal);
                }
                catch (System.Exception exception)
                {
                    DisableBlueEyesModel(exception);
                }
            }

            if (rivalry)
            {
                bluePortrait.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-880f, -12f), new Vector2(-430f, -12f), reveal);
                darkPortrait.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(880f, -12f), new Vector2(430f, -12f), reveal);
            }
            else
            {
                bluePortrait.rectTransform.anchoredPosition = Vector2.Lerp(new Vector2(-900f, -12f), new Vector2(0f, -12f), reveal);
            }

            bluePortrait.rectTransform.localScale = Vector3.one * pulse;
            darkPortrait.rectTransform.localScale = Vector3.one * pulse;
            blueAura.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.55f, 1.08f, impact);
            darkAura.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.55f, 1.08f, impact);
            blueAura.rectTransform.localEulerAngles = new Vector3(0f, 0f, normalized * 75f);
            darkAura.rectTransform.localEulerAngles = new Vector3(0f, 0f, -normalized * 75f);
            energyBeam.rectTransform.localScale = new Vector3(impact, Mathf.Lerp(0.25f, 1f, impact), 1f);
            clashCore.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.15f, 1.25f, impact) * pulse;
        }

        private static string TitleFor(CleanDuelCinematicEvent cinematicEvent)
        {
            switch (cinematicEvent.Kind)
            {
                case CleanDuelCinematicKind.SignatureFinalBlow:
                    return "FINAL BLOW";
                case CleanDuelCinematicKind.SignatureSummon:
                    return "SIGNATURE SUMMON";
                case CleanDuelCinematicKind.EffectActivation:
                    return "CARD EFFECT";
                case CleanDuelCinematicKind.GenericSummon:
                    return SummonLabel(cinematicEvent.SummonKind) + " SUMMON";
                default:
                    return "DUEL CINEMATIC";
            }
        }

        private static string SubtitleFor(CleanDuelCinematicEvent cinematicEvent)
        {
            string cardName = cinematicEvent.AttackerName.ToUpperInvariant();
            switch (cinematicEvent.Kind)
            {
                case CleanDuelCinematicKind.SignatureFinalBlow:
                    return cardName + "  /  DIRECT ATTACK  /  " + cinematicEvent.Value + " DAMAGE";
                case CleanDuelCinematicKind.SignatureSummon:
                    return cardName + "  /  OWN SUMMON CUT-IN";
                case CleanDuelCinematicKind.EffectActivation:
                    return cardName + "  /  EFFECT ACTIVATED";
                case CleanDuelCinematicKind.GenericSummon:
                    return cardName + "  /  " + SummonLabel(cinematicEvent.SummonKind) + " PROCEDURE";
                default:
                    return cardName;
            }
        }

        private static string SummonLabel(CleanSummonPresentationKind summonKind)
        {
            return summonKind == CleanSummonPresentationKind.None
                ? "SPECIAL"
                : summonKind.ToString().ToUpperInvariant();
        }

        private static Color AccentFor(CleanDuelCinematicEvent cinematicEvent)
        {
            if (cinematicEvent.Kind == CleanDuelCinematicKind.EffectActivation)
            {
                return new Color(1f, 0.74f, 0.2f, 1f);
            }

            if (cinematicEvent.Kind == CleanDuelCinematicKind.SignatureFinalBlow ||
                cinematicEvent.Kind == CleanDuelCinematicKind.SignatureSummon)
            {
                return cinematicEvent.AttackerCardId == DarkMagicianCardId
                    ? new Color(0.86f, 0.36f, 1f, 1f)
                    : new Color(0.45f, 0.9f, 1f, 1f);
            }

            switch (cinematicEvent.SummonKind)
            {
                case CleanSummonPresentationKind.Ritual:
                    return new Color(0.28f, 0.58f, 1f, 1f);
                case CleanSummonPresentationKind.Fusion:
                    return new Color(0.82f, 0.32f, 1f, 1f);
                case CleanSummonPresentationKind.Synchro:
                    return new Color(0.72f, 0.96f, 1f, 1f);
                case CleanSummonPresentationKind.Xyz:
                    return new Color(1f, 0.78f, 0.22f, 1f);
                case CleanSummonPresentationKind.Pendulum:
                    return new Color(0.25f, 1f, 0.62f, 1f);
                case CleanSummonPresentationKind.Link:
                    return new Color(0.15f, 0.68f, 1f, 1f);
                default:
                    return Color.white;
            }
        }

        private static string FallbackLabel(string cardName)
        {
            if (string.IsNullOrEmpty(cardName))
            {
                return "CARD\nANIMATION";
            }

            string normalized = cardName.ToUpperInvariant();
            return normalized.Length <= 22 ? normalized : normalized.Substring(0, 22) + "...";
        }

        private Image AddPortrait(string name, Vector2 position, Color fallbackColor, out Text fallback)
        {
            Image portrait = AddImage(name, overlay, null, fallbackColor);
            SetCenteredRect(portrait.rectTransform, new Vector2(300f, 420f), position);
            portrait.preserveAspect = true;
            Outline outline = portrait.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.78f, 0.9f, 1f, 0.9f);
            outline.effectDistance = new Vector2(4f, -4f);
            fallback = AddText("Procedural Placeholder", portrait.transform, 31, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            fallback.resizeTextForBestFit = true;
            fallback.resizeTextMinSize = 18;
            fallback.resizeTextMaxSize = 31;
            Stretch(fallback.rectTransform);
            return portrait;
        }

        private static void ConfigurePortrait(Image portrait, Text fallback, Sprite sprite, string fallbackText, Color fallbackColor)
        {
            portrait.sprite = sprite;
            portrait.color = sprite != null ? Color.white : fallbackColor;
            fallback.gameObject.SetActive(sprite == null);
            fallback.text = fallbackText;
        }

        private Image AddImage(string name, Transform parent, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static RawImage AddRawImage(string name, Transform parent, Texture texture)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            RawImage image = go.GetComponent<RawImage>();
            image.texture = texture;
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        private Text AddText(string name, Transform parent, int fontSize, FontStyle style, TextAnchor alignment, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Sprite CreateCircleSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "CinematicCircle";
            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - distance + 1f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private void RequestSkip()
        {
            skipRequested = true;
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static void SetCenteredRect(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static void SetNormalizedRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Anchor(RectTransform rect, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
