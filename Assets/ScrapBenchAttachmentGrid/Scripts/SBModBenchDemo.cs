using UnityEngine;

namespace ScrapBench.GridPrototype
{
    // Prototype UI: no Canvas, prefabs, art or input-package configuration needed.
    public sealed class SBModBenchDemo : MonoBehaviour
    {
        public SBModLoadout loadout;
        [Tooltip("Show a small button during gameplay. Disable when using your own pause-menu button.")]
        public bool showOpenButton = true;
        [Tooltip("Leave off so the grid starts hidden.")]
        public bool startOpen = false;
        [Tooltip("Disable if your existing pause menu already controls Time.timeScale.")]
        public bool pauseWhileOpen = true;
        [Range(.5f, 1f)] public float panelScreenFraction = .88f;
        public bool IsOpen { get; private set; }
        private bool ownsTimePause;
        private float previousTimeScale;
        private SBModDefinition draggedDefinition;
        private int draggedId;
        private bool dragging;
        private bool rotated;
        private int capturedControl;
        private Vector2 grabOffset;
        private SBModTier tier;
        private SBModTier dragTier;
        private string message = "Drag an attachment from the left into the grid.";
        private string shotResult = "Press Test shot to try the current stats.";
        private float nextShotTime;
        private GUIStyle itemStyle;
        private GUIStyle textStyle;
        private GUIStyle headingStyle;
        private const float Cell = 76f;
        private static readonly Vector2 GridOrigin = new Vector2(354, 138);
        private static readonly Rect RemoveArea = new Rect(354, 458, 252, 64);

        private void Awake()
        {
            if (loadout == null) loadout = GetComponent<SBModLoadout>();
            if (loadout == null) loadout = gameObject.AddComponent<SBModLoadout>();
        }

        private void Start() { if (startOpen) Open(); }

        // These methods can also be assigned to an existing pause-menu Button's OnClick.
        public void Open()
        {
            if (IsOpen || !isActiveAndEnabled) return;
            IsOpen = true;
            if (pauseWhileOpen && Time.timeScale > 0f)
            {
                previousTimeScale = Time.timeScale;
                ownsTimePause = true;
                Time.timeScale = 0f;
            }
        }

        public void Close()
        {
            CancelDrag();
            IsOpen = false;
            if (ownsTimePause)
            {
                // Do not overwrite a nonzero speed set by another system while open.
                if (Time.timeScale == 0f) Time.timeScale = previousTimeScale;
                ownsTimePause = false;
            }
        }

        public void Toggle() { if (IsOpen) Close(); else Open(); }

        private Rect CatalogRect(int index) { return new Rect(24, 138 + index * 66, 306, 58); }
        private Rect ItemRect(int col, int row, int width, int height)
        {
            return new Rect(GridOrigin.x + col * Cell, GridOrigin.y + row * Cell, width * Cell, height * Cell);
        }

        private void OnGUI()
        {
            if (loadout == null) return;
            if (!IsOpen)
            {
                if (showOpenButton && GUI.Button(new Rect(16, 16, 170, 36), "Attachments")) Open();
                return;
            }
            Event e = Event.current;
            if (!dragging && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                Close(); e.Use(); return;
            }
            SBModGridModel grid = loadout.Grid;
            // Extra room is allocated when the grid/catalog is enlarged in the Inspector.
            float statsX = Mathf.Max(642f, GridOrigin.x + grid.Columns * Cell + 40f);
            float viewWidth = statsX + 396f;
            float viewHeight = Mathf.Max(724f, Mathf.Max(160f + loadout.catalog.Count * 66f,
                GridOrigin.y + grid.Rows * Cell + 240f));
            float scale = Mathf.Min(Screen.width / viewWidth, Screen.height / viewHeight) *
                Mathf.Clamp(panelScreenFraction, .5f, 1f);
            if (scale <= 0) return;
            Vector2 offset = new Vector2((Screen.width - viewWidth * scale) / 2f,
                (Screen.height - viewHeight * scale) / 2f);
            Vector2 mouse = (e.mousePosition - offset) / scale;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(offset.x, offset.y, 0), Quaternion.identity, Vector3.one * scale);
            int control = GUIUtility.GetControlID(782631, FocusType.Passive);
            if (itemStyle == null)
            {
                itemStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, wordWrap = true, fontSize = 14 };
                itemStyle.normal.textColor = Color.white;
                textStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 16 };
                headingStyle = new GUIStyle(textStyle) { fontSize = 24, fontStyle = FontStyle.Bold };
            }

            Fill(new Rect(0, 0, viewWidth, viewHeight), new Color(.055f, .065f, .09f));
            GUI.Label(new Rect(24, 18, viewWidth - 200, 38), "SCRAP-BENCH / ATTACHMENT GRID", headingStyle);
            if (GUI.Button(new Rect(viewWidth - 144, 20, 120, 34), "Close"))
            {
                Close(); GUI.matrix = previousMatrix; return;
            }
            GUI.Label(new Rect(24, 62, 990, 48), "Drag to equip or move. R rotates. Right-click removes. Esc cancels a drag, otherwise closes.", textStyle);
            GUI.Label(new Rect(24, 110, 300, 28), "ALL MODS AVAILABLE / unlimited test copies", GUI.skin.label);

            GUI.enabled = !dragging;
            tier = (SBModTier)GUI.Toolbar(new Rect(statsX, 110, 360, 28), (int)tier, new[] { "Common T1", "Legendary T5" });
            GUI.enabled = true;

            for (int i = 0; i < loadout.catalog.Count; i++)
            {
                SBModDefinition definition = loadout.catalog[i];
                if (definition == null) continue;
                Rect rect = CatalogRect(i);
                Fill(rect, StatColor(definition.Stat) * .7f);
                GUI.Label(rect, definition.Name + "  |  " + definition.Width + " x " + definition.Height +
                    "\n+" + (definition.Bonus(tier) * 100f).ToString("0.#") + "% " + definition.Stat, itemStyle);
            }

            for (int row = 0; row < grid.Rows; row++)
                for (int col = 0; col < grid.Columns; col++)
                    GUI.Box(ItemRect(col, row, 1, 1), "");

            foreach (SBPlacedMod item in grid.Items)
            {
                if (dragging && item.Id == draggedId) continue;
                DrawItem(ItemRect(item.Column, item.Row, item.CellWidth, item.CellHeight), item.Name,
                    item.Bonus, item.Stat, item.Tier, 1f);
            }

            Rect removeRect = new Rect(RemoveArea.x, Mathf.Max(RemoveArea.y, GridOrigin.y + grid.Rows * Cell + 26),
                RemoveArea.width, RemoveArea.height);
            GUI.Box(removeRect, "DROP HERE TO UNEQUIP\n(or right-click an equipped mod)");
            GUI.Label(new Rect(354, removeRect.y + 76, 252, 64), grid.Items.Count + " equipped attachments\nGrid space is the limit.", textStyle);
            GUI.enabled = !dragging;
            if (GUI.Button(new Rect(354, removeRect.y + 150, 252, 36), "Remove all attachments"))
            {
                grid.Clear(); message = "All attachments removed.";
            }
            GUI.enabled = true;

            SBWeaponStats stats = loadout.CurrentStats;
            GUI.Label(new Rect(statsX, 156, 360, 36), "LIVE WEAPON STATS", headingStyle);
            string statText = "Damage: " + stats.Damage.ToString("0.##") +
                "\nFire rate: " + stats.ShotsPerSecond.ToString("0.##") + " shots/sec" +
                "\nCritical damage: " + stats.CriticalMultiplier.ToString("0.###") + "x" +
                "\nAccuracy: " + (stats.Accuracy * 100f).ToString("0.#") + "%" +
                "\nCritical chance: " + (stats.CriticalChance * 100f).ToString("0.#") + "% (max 75%)" +
                "\nArmor ignored: " + (stats.Penetration * 100f).ToString("0.#") + "%" +
                "\nExtra bullet chance: " + (stats.MultishotChance * 100f).ToString("0.#") + "%";
            GUI.Label(new Rect(statsX, 202, 360, 194), statText, textStyle);
            GUI.Label(new Rect(statsX, 402, 360, 48), "Shot simulation: target has 40% damage reduction from armor.", textStyle);
            GUI.enabled = !dragging && Time.unscaledTime >= nextShotTime && stats.ShotsPerSecond > 0;
            if (GUI.Button(new Rect(statsX, 458, 360, 38), "Test shot")) TestShot(stats);
            GUI.enabled = true;
            GUI.Label(new Rect(statsX, 508, 360, 144), shotResult, textStyle);
            GUI.Label(new Rect(24, viewHeight - 56, viewWidth - 48, 50), message, textStyle);

            if (dragging)
            {
                int width = rotated ? draggedDefinition.Height : draggedDefinition.Width;
                int height = rotated ? draggedDefinition.Width : draggedDefinition.Height;
                Vector2 topLeft = mouse - grabOffset;
                int col = Mathf.RoundToInt((topLeft.x - GridOrigin.x) / Cell);
                int row = Mathf.RoundToInt((topLeft.y - GridOrigin.y) / Cell);
                Rect gridRect = ItemRect(0, 0, grid.Columns, grid.Rows);
                bool valid = gridRect.Contains(mouse) && grid.CanPlace(width, height, col, row, draggedId);
                if (gridRect.Contains(mouse))
                    Fill(ItemRect(col, row, width, height), valid ? new Color(.1f, .7f, .3f, .6f) : new Color(.9f, .1f, .1f, .6f));
                DrawItem(new Rect(topLeft.x, topLeft.y, width * Cell, height * Cell), draggedDefinition.Name,
                    draggedDefinition.Bonus(dragTier), draggedDefinition.Stat, dragTier, .8f);

                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
                {
                    rotated = !rotated;
                    grabOffset = new Vector2(Cell / 2, Cell / 2);
                    e.Use();
                }
                else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
                {
                    CancelDrag(); message = "Cancelled. Your loadout is unchanged."; e.Use();
                }
                else if (e.type == EventType.MouseUp && e.button == 0)
                {
                    bool placed = false;
                    if (draggedId != 0 && removeRect.Contains(mouse))
                    {
                        grid.Remove(draggedId); message = "Attachment removed.";
                    }
                    else
                    {
                        if (valid)
                        {
                            int newId;
                            placed = draggedId == 0
                                ? grid.TryAdd(draggedDefinition, dragTier, col, row, rotated, out newId)
                                : grid.TryMove(draggedId, col, row, rotated);
                        }
                        message = placed ? "Attachment placed. Bonuses recalculated." : "No room there. Your loadout is unchanged.";
                    }
                    CancelDrag(); e.Use();
                }
                else if (e.type == EventType.MouseDrag) e.Use();
            }
            else if (e.type == EventType.MouseDown && GUIUtility.hotControl == 0)
            {
                int col = Mathf.FloorToInt((mouse.x - GridOrigin.x) / Cell);
                int row = Mathf.FloorToInt((mouse.y - GridOrigin.y) / Cell);
                SBPlacedMod item = grid.At(col, row);
                if (item != null && e.button == 1)
                {
                    grid.Remove(item.Id); message = "Attachment removed. Bonuses recalculated."; e.Use();
                }
                else if (e.button == 0)
                {
                    if (item != null)
                    {
                        SBModDefinition copy = new SBModDefinition(item.Name, item.Stat, item.Width, item.Height, item.Bonus, item.Bonus);
                        BeginDrag(copy, item.Id, item.Tier, item.Rotated,
                            mouse - new Vector2(GridOrigin.x + item.Column * Cell, GridOrigin.y + item.Row * Cell), control);
                        e.Use();
                    }
                    else
                    {
                        for (int i = 0; i < loadout.catalog.Count; i++)
                        {
                            if (loadout.catalog[i] != null && CatalogRect(i).Contains(mouse))
                            {
                                BeginDrag(loadout.catalog[i], 0, tier, false, new Vector2(Cell / 2, Cell / 2), control);
                                e.Use(); break;
                            }
                        }
                    }
                }
            }
            GUI.matrix = previousMatrix;
        }

        private void BeginDrag(SBModDefinition definition, int id, SBModTier itemTier, bool rotation, Vector2 offset, int control)
        {
            draggedDefinition = definition; draggedId = id; dragTier = itemTier;
            rotated = rotation; grabOffset = offset; dragging = true;
            capturedControl = control; GUIUtility.hotControl = control;
        }

        private void CancelDrag()
        {
            if (dragging && GUIUtility.hotControl == capturedControl) GUIUtility.hotControl = 0;
            dragging = false; draggedId = 0; draggedDefinition = null;
        }
        private void OnDisable() { Close(); }
        private void OnApplicationFocus(bool focused) { if (!focused) CancelDrag(); }

        private void TestShot(SBWeaponStats stats)
        {
            nextShotTime = Time.unscaledTime + 1f / stats.ShotsPerSecond;
            int bullets = Random.value < stats.MultishotChance ? 2 : 1;
            float damage = 0f;
            int crits = 0;
            float maxSpread = (1f - stats.Accuracy) * 15f;
            for (int i = 0; i < bullets; i++)
            {
                bool crit = Random.value < stats.CriticalChance;
                if (crit) crits++;
                // Example armor formula, not an assertion about your existing combat code.
                damage += stats.Damage * (crit ? stats.CriticalMultiplier : 1f) * (1f - .4f * (1f - stats.Penetration));
            }
            shotResult = bullets + " bullet(s), " + crits + " critical hit(s)\n" +
                damage.ToString("0.##") + " simulated damage\nSpread range: +/-" + maxSpread.ToString("0.##") +
                " degrees\nShot interval: " + (1f / stats.ShotsPerSecond).ToString("0.###") + " sec";
        }

        private void DrawItem(Rect rect, string name, float bonus, SBModStat stat, SBModTier itemTier, float alpha)
        {
            Color border = itemTier == SBModTier.Legendary ? new Color(1f, .75f, .18f, alpha) : new Color(.65f, .75f, .8f, alpha);
            Fill(rect, border);
            Color color = StatColor(stat); color.a = alpha;
            Fill(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6), color);
            GUI.Label(new Rect(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 8),
                name + "\n+" + (bonus * 100f).ToString("0.#") + "%\n" + (itemTier == SBModTier.Common ? "T1" : "T5"), itemStyle);
        }

        private static void Fill(Rect rect, Color color)
        {
            Color old = GUI.color; GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = old;
        }
        private static Color StatColor(SBModStat stat)
        {
            switch (stat)
            {
                case SBModStat.Damage: return new Color(.65f, .2f, .25f);
                case SBModStat.FireRate: return new Color(.1f, .45f, .6f);
                case SBModStat.CriticalDamage: return new Color(.5f, .25f, .6f);
                case SBModStat.Accuracy: return new Color(.15f, .5f, .4f);
                case SBModStat.CriticalChance: return new Color(.65f, .25f, .48f);
                case SBModStat.Penetration: return new Color(.55f, .4f, .16f);
                default: return new Color(.3f, .35f, .65f);
            }
        }
    }
}
