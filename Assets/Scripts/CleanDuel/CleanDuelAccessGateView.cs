using System;
using UnityEngine;
using UnityEngine.UI;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelAccessGateView
    {
        private static readonly Color Background = new Color(0.015f, 0.055f, 0.05f, 1f);
        private static readonly Color Panel = new Color(0.035f, 0.105f, 0.09f, 0.98f);
        private static readonly Color Field = new Color(0.02f, 0.075f, 0.068f, 1f);
        private static readonly Color Accent = new Color(0.02f, 0.92f, 0.66f, 1f);
        private static readonly Color TextPrimary = new Color(0.9f, 1f, 0.97f, 1f);
        private static readonly Color TextMuted = new Color(0.54f, 0.72f, 0.67f, 1f);
        private static readonly Color Error = new Color(1f, 0.43f, 0.39f, 1f);

        private GameObject canvasObject;
        private Font font;
        private InputField userInput;
        private InputField codeInput;
        private Button submitButton;
        private Text submitLabel;
        private Text statusText;
        private Action<string, string> submitted;

        public void Build(Transform parent, string rememberedUser, Action<string, string> onSubmitted)
        {
            submitted = onSubmitted;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            canvasObject = new GameObject("Icarus Game Access", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 250;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            Stretch(canvasObject.GetComponent<RectTransform>());

            GameObject background = ImageObject("Access Background", canvasObject.transform, Background);
            Stretch(background.GetComponent<RectTransform>());
            BuildGridLines(background.transform);

            RectTransform panel = ImageObject("Access Panel", background.transform, Panel).GetComponent<RectTransform>();
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(650f, 700f);
            panel.anchoredPosition = Vector2.zero;
            Outline outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.04f, 0.48f, 0.36f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);

            RectTransform accent = ImageObject("Top Accent", panel, Accent).GetComponent<RectTransform>();
            SetRect(accent, new Vector2(0.5f, 1f), new Vector2(0f, -3f), new Vector2(650f, 6f), new Vector2(0.5f, 1f));

            Text brand = Label("Brand", panel, "ICARUS  //  SECURE GAME ACCESS", 15, FontStyle.Bold, TextAnchor.MiddleCenter, Accent);
            SetRect(brand.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(560f, 30f), new Vector2(0.5f, 1f));
            Text title = Label("Title", panel, "Welkom terug, duelist", 38, FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -106f), new Vector2(560f, 52f), new Vector2(0.5f, 1f));
            Text intro = Label("Intro", panel, "Log in met de gebruiker uit het Icarus-adminpaneel.\nHet duel start pas nadat de server toegang heeft verleend.", 17, FontStyle.Normal, TextAnchor.UpperCenter, TextMuted);
            intro.horizontalOverflow = HorizontalWrapMode.Wrap;
            intro.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(intro.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(530f, 64f), new Vector2(0.5f, 1f));

            userInput = BuildInput(panel, "E-MAIL OF GEBRUIKERS-ID", "speler@example.com", -270f, false);
            codeInput = BuildInput(panel, "GAME-TOEGANGSCODE", "Voer je persoonlijke toegangscode in", -405f, true);
            userInput.text = rememberedUser ?? string.Empty;

            statusText = Label("Status", panel, "", 16, FontStyle.Bold, TextAnchor.MiddleCenter, Error);
            statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            statusText.verticalOverflow = VerticalWrapMode.Overflow;
            SetRect(statusText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -508f), new Vector2(530f, 58f), new Vector2(0.5f, 1f));

            submitButton = BuildButton(panel, -585f);
            submitButton.onClick.AddListener(Submit);

            Text privacy = Label("Privacy", panel, "Je toegangscode wordt versleuteld via HTTPS verstuurd en niet lokaal bewaard.", 13, FontStyle.Normal, TextAnchor.MiddleCenter, TextMuted);
            SetRect(privacy.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 34f), new Vector2(560f, 24f), new Vector2(0.5f, 0f));

            if (string.IsNullOrWhiteSpace(userInput.text)) userInput.Select();
            else codeInput.Select();
        }

        public void SetBusy(bool busy)
        {
            userInput.interactable = !busy;
            codeInput.interactable = !busy;
            submitButton.interactable = !busy;
            submitLabel.text = busy ? "TOEGANG CONTROLEREN..." : "VEILIG INLOGGEN";
            if (busy) SetStatus("Verbinding maken met gameserver.rene007.nl...", Accent);
        }

        public void ShowError(string message)
        {
            SetBusy(false);
            SetStatus(message, Error);
            codeInput.text = string.Empty;
            codeInput.Select();
        }

        public void Dispose()
        {
            if (canvasObject != null) UnityEngine.Object.Destroy(canvasObject);
            canvasObject = null;
            submitted = null;
        }

        private void Submit()
        {
            string user = userInput.text.Trim();
            string code = codeInput.text.Trim();
            if (string.IsNullOrWhiteSpace(user))
            {
                SetStatus("Vul je e-mailadres of gebruikers-ID in.", Error);
                userInput.Select();
                return;
            }

            if (code.Length < 12)
            {
                SetStatus("De game-toegangscode bestaat uit minimaal 12 tekens.", Error);
                codeInput.Select();
                return;
            }

            submitted?.Invoke(user, code);
        }

        private InputField BuildInput(Transform parent, string labelText, string placeholderText, float y, bool password)
        {
            Text label = Label(labelText, parent, labelText, 14, FontStyle.Bold, TextAnchor.MiddleLeft, Accent);
            SetRect(label.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, y + 38f), new Vector2(530f, 28f), new Vector2(0.5f, 1f));

            GameObject fieldObject = ImageObject(labelText + " Input", parent, Field);
            RectTransform fieldRect = fieldObject.GetComponent<RectTransform>();
            SetRect(fieldRect, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(530f, 62f), new Vector2(0.5f, 1f));
            Outline outline = fieldObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.06f, 0.3f, 0.24f, 1f);
            outline.effectDistance = new Vector2(1f, -1f);

            InputField input = fieldObject.AddComponent<InputField>();
            Text value = Label("Value", fieldObject.transform, "", 18, FontStyle.Normal, TextAnchor.MiddleLeft, TextPrimary);
            value.supportRichText = false;
            RectTransform valueRect = value.rectTransform;
            valueRect.anchorMin = Vector2.zero;
            valueRect.anchorMax = Vector2.one;
            valueRect.offsetMin = new Vector2(20f, 8f);
            valueRect.offsetMax = new Vector2(-20f, -8f);
            Text placeholder = Label("Placeholder", fieldObject.transform, placeholderText, 17, FontStyle.Italic, TextAnchor.MiddleLeft, new Color(TextMuted.r, TextMuted.g, TextMuted.b, 0.7f));
            RectTransform placeholderRect = placeholder.rectTransform;
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(20f, 8f);
            placeholderRect.offsetMax = new Vector2(-20f, -8f);
            input.textComponent = value;
            input.placeholder = placeholder;
            input.targetGraphic = fieldObject.GetComponent<Image>();
            input.lineType = InputField.LineType.SingleLine;
            input.contentType = password ? InputField.ContentType.Password : InputField.ContentType.Standard;
            input.characterLimit = password ? 128 : 300;
            input.caretColor = Accent;
            input.selectionColor = new Color(Accent.r, Accent.g, Accent.b, 0.32f);
            input.ForceLabelUpdate();
            return input;
        }

        private Button BuildButton(Transform parent, float y)
        {
            GameObject buttonObject = ImageObject("Secure Login Button", parent, Accent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            SetRect(rect, new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(530f, 64f), new Vector2(0.5f, 1f));
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonObject.GetComponent<Image>();
            ColorBlock colors = button.colors;
            colors.normalColor = Accent;
            colors.highlightedColor = new Color(0.13f, 1f, 0.77f, 1f);
            colors.pressedColor = new Color(0.01f, 0.66f, 0.48f, 1f);
            colors.disabledColor = new Color(0.11f, 0.34f, 0.29f, 1f);
            button.colors = colors;
            submitLabel = Label("Button Label", buttonObject.transform, "VEILIG INLOGGEN", 17, FontStyle.Bold, TextAnchor.MiddleCenter, Background);
            Stretch(submitLabel.rectTransform);
            return button;
        }

        private void BuildGridLines(Transform parent)
        {
            for (int index = -5; index <= 5; index++)
            {
                GameObject line = ImageObject("Grid Line", parent, new Color(0.03f, 0.26f, 0.2f, 0.14f));
                RectTransform rect = line.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.sizeDelta = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(0f, index * 100f);
            }
        }

        private void SetStatus(string message, Color color)
        {
            statusText.text = message ?? string.Empty;
            statusText.color = color;
        }

        private GameObject ImageObject(string name, Transform parent, Color color)
        {
            GameObject created = new GameObject(name, typeof(RectTransform), typeof(Image));
            created.transform.SetParent(parent, false);
            created.GetComponent<Image>().color = color;
            return created;
        }

        private Text Label(string name, Transform parent, string value, int size, FontStyle style, TextAnchor alignment, Color color)
        {
            GameObject created = new GameObject(name, typeof(RectTransform), typeof(Text));
            created.transform.SetParent(parent, false);
            Text text = created.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = color;
            text.raycastTarget = false;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
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
