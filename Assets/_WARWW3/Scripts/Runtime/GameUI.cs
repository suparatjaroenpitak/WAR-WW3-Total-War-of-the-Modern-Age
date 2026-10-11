using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WarWW3.Core;

namespace WarWW3
{
    public sealed class GameUI : MonoBehaviour
    {
        sealed class Binding { public TMP_Text text; public Func<string> value; }
        readonly List<Binding> bindings = new List<Binding>();
        Canvas canvas;
        RectTransform root, overlay, selectionBox;
        GameLocalizer locale;
        BattleController battle;
        TMP_FontAsset font;
        TMP_Text counts, selectionLabel, performance;
        float nextRefresh, frameAverage;
        readonly Color ink = new Color(0.035f, 0.065f, 0.085f, 0.95f);
        readonly Color accent = new Color(0.25f, 0.85f, 0.85f);
        public bool BlocksWorld { get { return overlay != null || battle == null || battle.Paused || battle.Simulation.Outcome != MatchOutcome.Running; } }
        public static bool PointerOverUI() { return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(); }
        public void Initialize(GameLocalizer localizer)
        {
            locale = localizer; locale.Changed += RefreshText;
            var source = Resources.Load<Font>("Fonts/Sarabun-Regular");
            if (source == null) throw new InvalidOperationException("Sarabun font missing. See Docs/en/UNITY_SETUP.md.");
            font = TMP_FontAsset.CreateFontAsset(source);
            font.name = "Sarabun Runtime Atlas"; font.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            var go = new GameObject("Command interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1600, 900); scaler.matchWidthOrHeight = 0.5f;
            root = go.GetComponent<RectTransform>();
            if (EventSystem.current == null) new GameObject("UI input", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        RectTransform Box(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size, Color? color = null)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor; rect.pivot = pivot; rect.anchoredPosition = position; rect.sizeDelta = size;
            if (color.HasValue) { var image = go.AddComponent<Image>(); image.color = color.Value; }
            return rect;
        }
        TMP_Text Label(Transform parent, string key, Vector2 position, Vector2 size, float fontSize = 24, Color? color = null)
        {
            var rect = Box(key, parent, new Vector2(0, 1), new Vector2(0, 1), position, size);
            var text = rect.gameObject.AddComponent<TextMeshProUGUI>(); text.font = font; text.fontSize = fontSize; text.color = color ?? Color.white;
            text.textWrappingMode = TextWrappingModes.NoWrap; text.overflowMode = TextOverflowModes.Ellipsis; text.raycastTarget = false; text.margin = new Vector4(0, 3, 0, 3);
            if (!string.IsNullOrEmpty(key)) Bind(text, () => locale.Text(key));
            return text;
        }
        void Bind(TMP_Text text, Func<string> value) { bindings.Add(new Binding { text = text, value = value }); text.text = value(); }
        Button Button(Transform parent, string key, Vector2 position, Vector2 size, Action action, bool primary = false)
        {
            var rect = Box(key, parent, new Vector2(0, 1), new Vector2(0, 1), position, size, primary ? new Color(0.08f, 0.4f, 0.44f) : new Color(0.14f, 0.2f, 0.24f));
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>(); button.onClick.AddListener(() => action());
            var colors = button.colors; colors.highlightedColor = new Color(0.65f, 0.9f, 0.9f); colors.pressedColor = new Color(0.5f, 0.7f, 0.75f); button.colors = colors;
            var text = Label(rect, key, new Vector2(16, -5), size - new Vector2(30, 8), 24); text.alignment = TextAlignmentOptions.MidlineLeft;
            return button;
        }
        void Clear()
        {
            bindings.Clear(); for (int i = root.childCount - 1; i >= 0; i--) Destroy(root.GetChild(i).gameObject);
            overlay = null; selectionBox = null;
        }
        public void ShowMenu()
        {
            Clear();
            var panel = Box("Main menu", root, new Vector2(0.06f, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(580, 730), ink);
            Label(panel, "brand", new Vector2(36, -28), new Vector2(510, 85), 62, accent);
            Label(panel, "subtitle", new Vector2(38, -119), new Vector2(510, 42), 24);
            Label(panel, "prototype", new Vector2(38, -168), new Vector2(510, 38), 21, new Color(0.9f, 0.8f, 0.5f));
            Button(panel, "menu.skirmish", new Vector2(36, -236), new Vector2(508, 62), GameBootstrap.StartBattle, true);
            Button(panel, "menu.language", new Vector2(36, -312), new Vector2(508, 58), locale.ToggleLanguage);
            Button(panel, "menu.settings", new Vector2(36, -384), new Vector2(508, 58), ShowSettings);
            Button(panel, "menu.quit", new Vector2(36, -456), new Vector2(508, 58), Quit);
            Label(panel, "menu.briefing", new Vector2(38, -554), new Vector2(510, 150), 22);
            var right = Box("Region information", root, new Vector2(0.97f, 0.06f), new Vector2(1, 0), Vector2.zero, new Vector2(680, 112), ink);
            Label(right, "region.name", new Vector2(24, -16), new Vector2(635, 40), 28, accent);
            Label(right, "region.detail", new Vector2(24, -62), new Vector2(635, 36), 20);
        }
        public void ShowBattle(BattleController controller)
        {
            battle = controller; Clear();
            var top = Box("Command bar", root, new Vector2(0, 1), new Vector2(0, 1), Vector2.zero, new Vector2(0, 82), ink); top.anchorMax = Vector2.one; top.sizeDelta = new Vector2(0, 82);
            Label(top, "brand", new Vector2(24, -13), new Vector2(240, 50), 34, accent);
            counts = Label(top, "", new Vector2(300, -22), new Vector2(630, 42), 23);
            var language = Button(top, "menu.language", new Vector2(1020, -16), new Vector2(210, 48), locale.ToggleLanguage); language.GetComponent<RectTransform>().anchorMin = language.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1); language.GetComponent<RectTransform>().anchoredPosition = new Vector2(-450, -16);
            var pause = Button(top, "menu.pause", new Vector2(0, -16), new Vector2(190, 48), TogglePause); pause.GetComponent<RectTransform>().anchorMin = pause.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1); pause.GetComponent<RectTransform>().anchoredPosition = new Vector2(-214, -16);
            var bottom = Box("Field manual", root, new Vector2(0, 0), Vector2.zero, Vector2.zero, new Vector2(0, 140), ink); bottom.anchorMax = new Vector2(1, 0); bottom.sizeDelta = new Vector2(0, 140);
            selectionLabel = Label(bottom, "", new Vector2(24, -10), new Vector2(1050, 36), 24, accent);
            Label(bottom, "controls.orders", new Vector2(24, -50), new Vector2(1500, 35), 21);
            Label(bottom, "controls.camera", new Vector2(24, -91), new Vector2(1500, 35), 21);
            performance = Label(top, "", new Vector2(790, -25), new Vector2(220, 32), 18);
        }
        RectTransform Modal(string title)
        {
            if (overlay != null) Destroy(overlay.gameObject);
            overlay = Box("Modal backdrop", root, new Vector2(0, 0), Vector2.zero, Vector2.zero, Vector2.zero, new Color(0, 0, 0, 0.7f)); overlay.anchorMax = Vector2.one;
            var panel = Box("Modal", overlay, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(660, 600), ink);
            Label(panel, title, new Vector2(36, -30), new Vector2(590, 70), 38, accent);
            if (battle != null) battle.Paused = true;
            return panel;
        }
        public void ShowSettings()
        {
            var panel = Modal("menu.settings");
            var graphics = Button(panel, "", new Vector2(36, -135), new Vector2(588, 66), () => { GraphicsOptions.Apply((PlayerPrefs.GetInt("graphics", 1) + 1) % 3); RefreshText(); });
            Bind(graphics.GetComponentInChildren<TMP_Text>(), () => locale.Text("settings.quality", locale.Text("settings.preset." + PlayerPrefs.GetInt("graphics", 1))));
            Label(panel, "settings.note", new Vector2(36, -225), new Vector2(588, 150), 23);
            Button(panel, "menu.back", new Vector2(36, -472), new Vector2(588, 62), () => { if (battle == null) CloseModal(); else ShowPause(); });
        }
        public void TogglePause()
        {
            if (battle == null || battle.Simulation.Outcome != MatchOutcome.Running) return;
            if (overlay != null) CloseModal(); else ShowPause();
        }
        void ShowPause()
        {
            var panel = Modal("menu.pause");
            Button(panel, "menu.resume", new Vector2(36, -140), new Vector2(588, 64), CloseModal, true);
            Button(panel, "menu.settings", new Vector2(36, -226), new Vector2(588, 64), ShowSettings);
            Button(panel, "menu.restart", new Vector2(36, -312), new Vector2(588, 64), GameBootstrap.StartBattle);
            Button(panel, "menu.main", new Vector2(36, -398), new Vector2(588, 64), GameBootstrap.MainMenu);
        }
        void CloseModal() { if (overlay != null) Destroy(overlay.gameObject); overlay = null; if (battle != null) battle.Paused = false; }
        public void ShowOutcome(MatchOutcome outcome)
        {
            var panel = Modal(outcome == MatchOutcome.PlayerVictory ? "result.victory" : outcome == MatchOutcome.Draw ? "result.draw" : "result.defeat");
            Label(panel, "result.detail", new Vector2(36, -135), new Vector2(588, 150), 25);
            Button(panel, "menu.restart", new Vector2(36, -330), new Vector2(588, 64), GameBootstrap.StartBattle, true);
            Button(panel, "menu.main", new Vector2(36, -422), new Vector2(588, 64), GameBootstrap.MainMenu);
        }
        public void SetSelectionBox(Rect? screenRect)
        {
            if (!screenRect.HasValue) { if (selectionBox != null) selectionBox.gameObject.SetActive(false); return; }
            if (selectionBox == null) selectionBox = Box("Selection rectangle", root, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.2f, 0.9f, 0.9f, 0.2f));
            selectionBox.GetComponent<Image>().raycastTarget = false; selectionBox.gameObject.SetActive(true);
            float scale = canvas.scaleFactor; Rect rect = screenRect.Value; selectionBox.anchoredPosition = rect.position / scale; selectionBox.sizeDelta = rect.size / scale;
        }
        void Update()
        {
            frameAverage = Mathf.Lerp(frameAverage, Time.unscaledDeltaTime, 0.05f);
            if (battle == null || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;
            counts.text = locale.Text("hud.forces", battle.Simulation.CountAlive(0), battle.Simulation.CountAlive(1));
            selectionLabel.text = locale.Text(battle.AttackMoveArmed ? "hud.attackmove" : "hud.selected", battle.SelectedCount);
            performance.text = locale.Text("hud.performance", Mathf.RoundToInt(1 / Mathf.Max(0.001f, frameAverage)), battle.Simulation.Tick);
        }
        void RefreshText() { for (int i = bindings.Count - 1; i >= 0; i--) { if (bindings[i].text == null) bindings.RemoveAt(i); else bindings[i].text.text = bindings[i].value(); } }
        static void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        void OnDestroy() { if (locale != null) locale.Changed -= RefreshText; if (font != null) { if (font.material != null) Destroy(font.material); foreach (var texture in font.atlasTextures) if (texture != null) Destroy(texture); Destroy(font); } }
    }
}
