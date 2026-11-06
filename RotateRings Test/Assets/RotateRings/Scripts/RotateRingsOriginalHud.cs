using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rebuilds the original Rotate Rings gameplay HUD as an independent overlay.
/// Attach it to the existing screen-space canvas; the parent canvas should use
/// the source game's 1080 x 2160 portrait reference resolution.
/// </summary>
public sealed class RotateRingsOriginalHud : MonoBehaviour {
    const float IntroDuration = .416667f;
    static readonly Color Ink = new Color(.30f, .20f, .14f, 1f);
    static readonly Color Cream = new Color(1f, .99490005f, .95754719f, 1f);

    public static Color OriginalCreamColor => Cream;
    public static Texture2D OriginalCreamTexture => Resources.Load<Texture2D>("OriginalUI/cream_background");

    public sealed class Actions {
        public Action Settings;
        public Action Firework;
        public Action Hammer;
        public Action Undo;
        public Action Hint;
        public Action Home;
        public Action Restart;
        public Action Music;
    }

    readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    Actions actions;
    RectTransform topContent;
    RectTransform propContent;
    CanvasGroup topGroup;
    CanvasGroup propGroup;
    Text levelLabel;
    Text scoreLabel;
    Text musicLabel;
    GameObject settingsOverlay;
    bool musicEnabled = true;

    /// <summary>
    /// Adds the HUD below an existing canvas/panel and returns the independent
    /// controller. Buttons call the matching optional Actions delegate.
    /// </summary>
    public static RotateRingsOriginalHud Build(RectTransform parent, int displayLevel,
        Action onSettings, Action onFirework, Action onHammer,
        Action onUndo = null, Action onHint = null, Action onHome = null,
        Action onRestart = null, Action onMusic = null, bool playIntro = true) {
        if (parent == null) throw new ArgumentNullException(nameof(parent));
        var root = new GameObject("Rotate Rings Original HUD", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        var hud = root.AddComponent<RotateRingsOriginalHud>();
        hud.actions = new Actions {
            Settings = onSettings,
            Firework = onFirework,
            Hammer = onHammer,
            Undo = onUndo,
            Hint = onHint,
            Home = onHome,
            Restart = onRestart,
            Music = onMusic
        };
        hud.BuildVisuals(displayLevel, playIntro);
        return hud;
    }

    public void SetLevel(int displayLevel) {
        if (levelLabel != null) levelLabel.text = "Level\n" + Mathf.Max(1, displayLevel);
    }

    public void SetScore(int score) {
        if (scoreLabel != null) scoreLabel.text = Mathf.Max(0, score).ToString();
    }

    public void SetMusicState(bool enabled) {
        musicEnabled = enabled;
        if (musicLabel != null) musicLabel.text = "Music: " + (musicEnabled ? "On" : "Off");
    }

    public void SetSettingsVisible(bool visible) {
        if (settingsOverlay != null) settingsOverlay.SetActive(visible);
    }

    void BuildVisuals(int displayLevel, bool playIntro) {
        var rootRect = GetComponent<RectTransform>();
        Stretch(rootRect);
        var font = Resources.Load<Font>("OriginalUI/Roboto-ExtraBold");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // The source prefab's TopContent is 1080 x 232, anchored to the top.
        topContent = CreateRect("TopContent", transform, new Vector2(.5f, 1f),
            new Vector2(.5f, 1f), new Vector2(.5f, .5f), new Vector2(1080f, 232f), new Vector2(0f, -116f));
        topGroup = topContent.gameObject.AddComponent<CanvasGroup>();
        CreateHeader(font, displayLevel);

        // The source PropContent is 1080 x 200, centered 240 px above bottom.
        propContent = CreateRect("PropContent", transform, new Vector2(.5f, 0f),
            new Vector2(.5f, 0f), new Vector2(.5f, .5f), new Vector2(1080f, 200f), new Vector2(0f, 240f));
        propGroup = propContent.gameObject.AddComponent<CanvasGroup>();
        CreateBooster(-80f, "Firework", "rocket", () => actions.Firework?.Invoke());
        CreateBooster(80f, "Hammer", "hammer", () => actions.Hammer?.Invoke());

        if (playIntro) StartCoroutine(PlayIntro());
    }

    void CreateHeader(Font font, int displayLevel) {
        // Source evidence: left group 195.5 x 191 at (134.75, -95.50).
        var left = CreateRect("LevelGroup", topContent, new Vector2(0f, 1f),
            new Vector2(0f, 1f), new Vector2(.5f, .5f), new Vector2(195.497f, 191.003f),
            new Vector2(134.749f, -95.501f));
        levelLabel = MakeText("LevelLabel", "Level\n" + Mathf.Max(1, displayLevel), left, font,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(.5f, .5f),
            new Vector2(195.5f, 191f), Vector2.zero, 36, Ink, TextAnchor.MiddleLeft);
        levelLabel.resizeTextForBestFit = true;
        levelLabel.resizeTextMinSize = 24;
        levelLabel.resizeTextMaxSize = 36;

        // Source Middle is centered at the top edge offset -104, with 378 x 134.
        var badge = CreateRect("ScoreBadge", topContent, new Vector2(.5f, 1f),
            new Vector2(.5f, 1f), new Vector2(.5f, .5f), new Vector2(378f, 134f), new Vector2(0f, -104f));
        var badgeImage = badge.gameObject.AddComponent<Image>();
        badgeImage.sprite = SpriteAsset("score_badge");
        badgeImage.type = Image.Type.Simple;
        badgeImage.preserveAspect = true;
        badgeImage.raycastTarget = false;
        scoreLabel = MakeText("Score", "2050", badge, font, Vector2.zero, Vector2.one,
            new Vector2(.5f, .5f), new Vector2(320f, 100f), Vector2.zero, 58, Ink, TextAnchor.MiddleCenter);
        scoreLabel.fontStyle = FontStyle.Bold;

        // Original setting sprite/button RectTransform: 116 x 116 at (-89,-106) from top-right.
        var setting = CreateRect("SettingsButton", topContent, new Vector2(1f, 1f),
            new Vector2(1f, 1f), new Vector2(.5f, .5f), new Vector2(116f, 116f), new Vector2(-89f, -106f));
        var button = setting.gameObject.AddComponent<Button>();
        var image = setting.gameObject.AddComponent<Image>();
        image.sprite = SpriteAsset("settings");
        image.preserveAspect = true;
        image.raycastTarget = true;
        button.targetGraphic = image;
        button.onClick.AddListener(ToggleSettings);
    }

    void CreateBooster(float x, string label, string icon, Action click) {
        var slot = CreateRect(label + "Booster", propContent, new Vector2(.5f, .5f),
            new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(128f, 152f), new Vector2(x, 0f));
        var image = slot.gameObject.AddComponent<Image>();
        image.sprite = SpriteAsset("booster_button");
        image.preserveAspect = true;
        var button = slot.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => click?.Invoke());

        var iconRect = CreateRect("Icon", slot, new Vector2(.5f, .5f), new Vector2(.5f, .5f),
            new Vector2(.5f, .5f), new Vector2(78f, 82f), new Vector2(0f, 8f));
        var iconImage = iconRect.gameObject.AddComponent<Image>();
        iconImage.sprite = SpriteAsset(icon);
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;
    }

    void ToggleSettings() {
        if (settingsOverlay == null) {
            CreateSettingsOverlay();
            settingsOverlay.SetActive(true);
        } else settingsOverlay.SetActive(!settingsOverlay.activeSelf);
        actions.Settings?.Invoke();
    }

    void CreateSettingsOverlay() {
        settingsOverlay = new GameObject("SettingsOverlay", typeof(RectTransform), typeof(CanvasGroup));
        settingsOverlay.transform.SetParent(transform, false);
        Stretch(settingsOverlay.GetComponent<RectTransform>());
        var dim = settingsOverlay.AddComponent<Image>();
        dim.color = new Color(.12f, .09f, .06f, .40f);
        dim.raycastTarget = true;

        var font = Resources.Load<Font>("OriginalUI/Roboto-ExtraBold");
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var card = CreateRect("SettingsCard", settingsOverlay.transform, new Vector2(.5f, .5f),
            new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(760f, 1000f), Vector2.zero);
        var cardImage = card.gameObject.AddComponent<Image>();
        cardImage.color = Cream;
        cardImage.sprite = SpriteAsset("booster_button");
        cardImage.type = Image.Type.Sliced;
        cardImage.raycastTarget = false;
        MakeText("SettingsTitle", "SETTINGS", card, font, new Vector2(.5f, 1f), new Vector2(.5f, 1f),
            new Vector2(.5f, 1f), new Vector2(620f, 90f), new Vector2(0f, -78f), 42, Ink, TextAnchor.MiddleCenter);

        AddSettingsOption(card, font, "Undo", 150f, () => { settingsOverlay.SetActive(false); actions.Undo?.Invoke(); });
        AddSettingsOption(card, font, "Hint", 35f, () => { settingsOverlay.SetActive(false); actions.Hint?.Invoke(); });
        AddSettingsOption(card, font, "Home", -80f, () => { settingsOverlay.SetActive(false); actions.Home?.Invoke(); });
        AddSettingsOption(card, font, "Restart", -195f, () => { settingsOverlay.SetActive(false); actions.Restart?.Invoke(); });

        var musicButton = AddSettingsOption(card, font, "Music: " + (musicEnabled ? "On" : "Off"), -310f, () => {
            musicEnabled = !musicEnabled;
            if (musicLabel != null) musicLabel.text = "Music: " + (musicEnabled ? "On" : "Off");
            actions.Music?.Invoke();
        });
        musicLabel = musicButton.GetComponentInChildren<Text>();
        AddSettingsOption(card, font, "Close", -425f, () => settingsOverlay.SetActive(false));
    }

    Button AddSettingsOption(RectTransform card, Font font, string label, float y, Action onClick) {
        var rect = CreateRect(label + "Button", card, new Vector2(.5f, .5f), new Vector2(.5f, .5f),
            new Vector2(.5f, .5f), new Vector2(560f, 88f), new Vector2(0f, y));
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = SpriteAsset("score_badge");
        image.color = Color.white;
        image.preserveAspect = false;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => onClick?.Invoke());
        MakeText("Label", label, rect, font, Vector2.zero, Vector2.one, new Vector2(.5f, .5f),
            new Vector2(520f, 78f), Vector2.zero, 31, Ink, TextAnchor.MiddleCenter);
        return button;
    }

    IEnumerator PlayIntro() {
        var topCurve = new AnimationCurve(
            new Keyframe(0f, 118f),
            new Keyframe(.166667f, -123.911f),
            new Keyframe(IntroDuration, -116f));
        var propCurve = new AnimationCurve(
            new Keyframe(0f, -124f),
            new Keyframe(.166667f, 252.687f),
            new Keyframe(IntroDuration, 240f));
        var fadeCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(.166667f, 1f));
        topGroup.alpha = 0f;
        propGroup.alpha = 0f;
        var top = topContent.anchoredPosition;
        var prop = propContent.anchoredPosition;
        float elapsed = 0f;
        while (elapsed < IntroDuration) {
            elapsed = Mathf.Min(IntroDuration, elapsed + Time.unscaledDeltaTime);
            top.y = topCurve.Evaluate(elapsed);
            prop.y = propCurve.Evaluate(elapsed);
            topContent.anchoredPosition = top;
            propContent.anchoredPosition = prop;
            topGroup.alpha = fadeCurve.Evaluate(elapsed);
            propGroup.alpha = fadeCurve.Evaluate(elapsed);
            yield return null;
        }
        top.y = -116f;
        prop.y = 240f;
        topContent.anchoredPosition = top;
        propContent.anchoredPosition = prop;
        topGroup.alpha = propGroup.alpha = 1f;
    }

    Text MakeText(string name, string value, RectTransform parent, Font font,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size, Vector2 position,
        int sizePx, Color color, TextAnchor alignment) {
        var rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, size, position);
        if (anchorMin != anchorMax) {
            Vector2 stretch = anchorMax - anchorMin;
            rect.sizeDelta = size - Vector2.Scale(parent.rect.size, stretch);
        }
        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = sizePx;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    Sprite SpriteAsset(string key) {
        if (sprites.TryGetValue(key, out var cached)) return cached;
        var texture = Resources.Load<Texture2D>("OriginalUI/" + key);
        if (texture == null) return null;
        var sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
        sprite.name = "OriginalUI_" + key;
        sprites[key] = sprite;
        return sprite;
    }

    static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 size, Vector2 position) {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        return rect;
    }

    static void Stretch(RectTransform rect) {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
