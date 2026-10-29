using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

namespace ScrapBench.Codex
{
    // Builds the whole Codex screen in code, so no prefab or Canvas setup is needed.
    // Put this on the same "Codex" GameObject as CodexManager.
    // Open with the C key, or call Open() / Close() / Toggle() from a menu button.
    public class CodexUI : MonoBehaviour
    {
        [SerializeField] private bool enableHotkey = true;
        [SerializeField] private bool pauseGameWhenOpen = true;

        private static readonly Color PanelColor = new Color(0.05f, 0.06f, 0.08f, 0.97f);
        private static readonly Color BoxColor = new Color(1f, 1f, 1f, 0.06f);
        private static readonly Color ButtonColor = new Color(0.16f, 0.18f, 0.22f, 1f);
        private static readonly Color TabSelectedColor = new Color(0.85f, 0.45f, 0.2f, 1f);

        private GameObject root;
        private bool built;
        private bool isOpen;
        private float previousTimeScale = 1f;

        private TextMeshProUGUI headerText;
        private RectTransform listContent;
        private Image detailIcon;
        private TextMeshProUGUI detailTitle;
        private TextMeshProUGUI detailSub;
        private TextMeshProUGUI detailBody;
        private readonly Dictionary<CodexCategory, Image> tabImages = new Dictionary<CodexCategory, Image>();

        private CodexCategory currentCategory = CodexCategory.Items;

        public bool IsOpen => isOpen;

        private void Update()
        {
            if (enableHotkey && TogglePressed()) Toggle();
        }

        private void OnDestroy()
        {
            if (isOpen && pauseGameWhenOpen) Time.timeScale = previousTimeScale;
            Unsubscribe();
        }

        private bool TogglePressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.C);
#else
            return false;
#endif
        }

        // ---------------- Public API ----------------

        public void Toggle()
        {
            if (isOpen) Close(); else Open();
        }

        public void Open()
        {
            if (isOpen) return;
            if (CodexManager.Instance == null || CodexManager.Instance.Database == null)
            {
                Debug.LogWarning("[Codex] CodexManager or its database is missing.");
                return;
            }

            Build();
            isOpen = true;
            root.SetActive(true);

            if (pauseGameWhenOpen)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            CodexManager.Instance.EntryUnlocked += OnEntryUnlocked;
            SetCategory(currentCategory);
        }

        public void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            if (root != null) root.SetActive(false);
            if (pauseGameWhenOpen) Time.timeScale = previousTimeScale;
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (CodexManager.Instance != null) CodexManager.Instance.EntryUnlocked -= OnEntryUnlocked;
        }

        private void OnEntryUnlocked(CodexEntry entry)
        {
            if (isOpen) SetCategory(currentCategory);
        }

        // ---------------- Content ----------------

        private void SetCategory(CodexCategory category)
        {
            currentCategory = category;

            foreach (var kv in tabImages)
                kv.Value.color = kv.Key == category ? TabSelectedColor : ButtonColor;

            var manager = CodexManager.Instance;
            var db = manager.Database;

            headerText.text = $"CODEX    {manager.UnlockedCount}/{db.Entries.Count} discovered";

            foreach (Transform child in listContent) Destroy(child.gameObject);

            var entries = db.GetByCategory(category);
            CodexEntry first = null;

            foreach (var entry in entries)
            {
                var captured = entry;
                bool isUnlocked = manager.IsUnlocked(entry.Id);
                string label = isUnlocked ? entry.Title : "???";
                NewButton(listContent, label, 70f, () => ShowEntry(captured));
                if (first == null) first = entry;
            }

            if (first != null)
            {
                ShowEntry(first);
            }
            else
            {
                detailIcon.enabled = false;
                detailTitle.text = "No entries yet";
                detailSub.text = "";
                detailBody.text = "Nothing has been added to this category.";
            }
        }

        private void ShowEntry(CodexEntry entry)
        {
            bool isUnlocked = CodexManager.Instance.IsUnlocked(entry.Id);

            detailIcon.sprite = isUnlocked ? entry.Icon : null;
            detailIcon.enabled = isUnlocked && entry.Icon != null;

            if (!isUnlocked)
            {
                detailTitle.text = "???";
                detailSub.text = "Not yet discovered.";
                detailBody.text = "Find this in the world to unlock its entry.";
                return;
            }

            detailTitle.text = entry.Title;
            detailSub.text = entry.ShortDescription;

            string body = entry.LongDescription;
            if (!string.IsNullOrWhiteSpace(entry.Lore))
                body += "\n\n<b>Lore</b>\n<i>" + entry.Lore + "</i>";
            detailBody.text = body;
        }

        // ---------------- UI construction ----------------

        private void Build()
        {
            if (built) return;
            built = true;

            EnsureEventSystem();

            var canvasGO = new GameObject("CodexCanvas",
                typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            root = canvasGO;

            // Background panel
            var panel = NewImage("Panel", root.transform, Vector2.zero, Vector2.one, PanelColor);

            // Header + close button
            headerText = NewText("Header", panel.transform, new Vector2(0.02f, 0.92f), new Vector2(0.8f, 0.99f),
                "CODEX", 44f, TextAlignmentOptions.MidlineLeft);
            headerText.fontStyle = FontStyles.Bold;

            var closeBtn = NewButton(panel.transform, "X", 0f, Close, false);
            SetAnchors((RectTransform)closeBtn.transform, new Vector2(0.93f, 0.93f), new Vector2(0.98f, 0.99f));
            closeBtn.GetComponentInChildren<TMP_Text>().alignment = TextAlignmentOptions.Center;

            // Category tabs
            var tabRow = NewRect("Tabs", panel.transform, new Vector2(0.02f, 0.84f), new Vector2(0.62f, 0.91f));
            var hlg = tabRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            foreach (CodexCategory cat in Enum.GetValues(typeof(CodexCategory)))
            {
                var captured = cat;
                var tab = NewButton(tabRow, cat.ToString(), 0f, () => SetCategory(captured), false);
                tab.GetComponentInChildren<TMP_Text>().alignment = TextAlignmentOptions.Center;
                tabImages[cat] = tab.GetComponent<Image>();
            }

            // Entry list (left)
            listContent = BuildScrollList(panel.transform, new Vector2(0.02f, 0.04f), new Vector2(0.35f, 0.82f));

            // Detail panel (right)
            var detail = NewImage("Detail", panel.transform, new Vector2(0.38f, 0.04f), new Vector2(0.98f, 0.82f), BoxColor);

            var iconImg = NewImage("Icon", detail.transform, new Vector2(0.03f, 0.72f), new Vector2(0.20f, 0.96f), Color.white);
            detailIcon = iconImg;
            detailIcon.preserveAspect = true;
            detailIcon.enabled = false;

            detailTitle = NewText("Title", detail.transform, new Vector2(0.23f, 0.84f), new Vector2(0.97f, 0.97f),
                "", 46f, TextAlignmentOptions.MidlineLeft);
            detailTitle.fontStyle = FontStyles.Bold;

            detailSub = NewText("Sub", detail.transform, new Vector2(0.23f, 0.70f), new Vector2(0.97f, 0.84f),
                "", 28f, TextAlignmentOptions.TopLeft);
            detailSub.color = new Color(0.8f, 0.8f, 0.85f, 1f);

            detailBody = NewText("Body", detail.transform, new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.68f),
                "", 28f, TextAlignmentOptions.TopLeft);
            detailBody.enableAutoSizing = true;
            detailBody.fontSizeMin = 18f;
            detailBody.fontSizeMax = 30f;

            root.SetActive(false);
        }

        private void EnsureEventSystem()
        {
#if UNITY_2023_1_OR_NEWER
            if (FindFirstObjectByType<EventSystem>() != null) return;
#else
            if (FindObjectOfType<EventSystem>() != null) return;
#endif
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            go.transform.SetParent(transform, false);
        }

        private RectTransform BuildScrollList(Transform parent, Vector2 aMin, Vector2 aMax)
        {
            var bg = NewImage("List", parent, aMin, aMax, BoxColor);
            var scroll = bg.gameObject.AddComponent<ScrollRect>();

            var viewport = NewImage("Viewport", bg.transform, Vector2.zero, Vector2.one, new Color(0f, 0f, 0f, 0.01f));
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = NewRect("Content", viewport.transform, new Vector2(0f, 1f), new Vector2(1f, 1f));
            content.pivot = new Vector2(0.5f, 1f);

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = (RectTransform)viewport.transform;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            return content;
        }

        // ---------------- Small UI helpers ----------------

        private static void SetAnchors(RectTransform rt, Vector2 aMin, Vector2 aMax)
        {
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static RectTransform NewRect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            SetAnchors(rt, aMin, aMax);
            return rt;
        }

        private static Image NewImage(string name, Transform parent, Vector2 aMin, Vector2 aMax, Color color)
        {
            var rt = NewRect(name, parent, aMin, aMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, Vector2 aMin, Vector2 aMax,
            string text, float size, TextAlignmentOptions align)
        {
            var rt = NewRect(name, parent, aMin, aMax);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = Color.white;
            t.raycastTarget = false;
            return t;
        }

        // height > 0 gives the button a fixed height inside a vertical list.
        private static Button NewButton(Transform parent, string label, float height, UnityAction onClick, bool leftAlign = true)
        {
            var go = new GameObject(label + "Button",
                typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);

            var img = go.GetComponent<Image>();
            img.color = ButtonColor;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 0.92f, 1f, 1f);
            colors.pressedColor = new Color(0.65f, 0.65f, 0.7f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(onClick);

            if (height > 0f) go.GetComponent<LayoutElement>().preferredHeight = height;

            var text = NewText("Label", go.transform, Vector2.zero, Vector2.one, label, 30f,
                leftAlign ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Center);
            text.margin = new Vector4(16f, 0f, 16f, 0f);

            return btn;
        }
    }
}
