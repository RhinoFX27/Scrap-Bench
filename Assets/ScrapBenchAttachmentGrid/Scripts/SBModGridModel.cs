using System;
using System.Collections.Generic;

namespace ScrapBench.GridPrototype
{
    public enum SBModStat { Damage, FireRate, CriticalDamage, Accuracy, CriticalChance, Penetration, Multishot }
    public enum SBModTier { Common, Legendary }

    [Serializable]
    public sealed class SBModDefinition
    {
        public string Name;
        public SBModStat Stat;
        public int Width;
        public int Height;
        public float CommonBonus;
        public float LegendaryBonus;

        public SBModDefinition(string name, SBModStat stat, int width, int height, float common, float legendary)
        {
            Name = name; Stat = stat; Width = width; Height = height;
            CommonBonus = common; LegendaryBonus = legendary;
        }

        public float Bonus(SBModTier tier)
        {
            return tier == SBModTier.Legendary ? LegendaryBonus : CommonBonus;
        }

        // Sizes other than the 1 x 3 magazine are editable prototype choices.
        public static List<SBModDefinition> CreateDefaults()
        {
            return new List<SBModDefinition>
            {
                new SBModDefinition("Hollow Point", SBModStat.Damage, 1, 1, .10f, .45f),
                new SBModDefinition("Extended Mag", SBModStat.FireRate, 1, 3, .06f, .27f),
                new SBModDefinition("Suppressor", SBModStat.CriticalDamage, 2, 1, .10f, .45f),
                new SBModDefinition("Sight", SBModStat.Accuracy, 1, 1, .08f, .36f),
                new SBModDefinition("Under Barrel Laser", SBModStat.CriticalChance, 2, 1, .05f, .225f),
                new SBModDefinition("Armor Piercing Rounds", SBModStat.Penetration, 1, 1, .10f, .45f),
                new SBModDefinition("Buckshot", SBModStat.Multishot, 1, 2, .10f, .45f)
            };
        }
    }

    public sealed class SBPlacedMod
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public SBModStat Stat { get; private set; }
        public SBModTier Tier { get; private set; }
        public float Bonus { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Column { get; internal set; }
        public int Row { get; internal set; }
        public bool Rotated { get; internal set; }
        public int CellWidth { get { return Rotated ? Height : Width; } }
        public int CellHeight { get { return Rotated ? Width : Height; } }

        internal SBPlacedMod(int id, SBModDefinition definition, SBModTier tier, int column, int row, bool rotated)
        {
            Id = id; Name = definition.Name; Stat = definition.Stat; Tier = tier;
            Bonus = definition.Bonus(tier); Width = definition.Width; Height = definition.Height;
            Column = column; Row = row; Rotated = rotated;
        }
    }

    [Serializable]
    public struct SBWeaponStats
    {
        public float Damage;
        public float ShotsPerSecond;
        public float CriticalMultiplier;
        public float Accuracy;
        public float CriticalChance;
        public float Penetration;
        public float MultishotChance;

        public static SBWeaponStats Default
        {
            get
            {
                return new SBWeaponStats { Damage = 10f, ShotsPerSecond = 4f,
                    CriticalMultiplier = 1.5f, Accuracy = .60f, CriticalChance = .10f };
            }
        }
    }

    // Pure C#: this can be reused by a Canvas UI, player inventory or workbench.
    public sealed class SBModGridModel
    {
        private readonly List<SBPlacedMod> items = new List<SBPlacedMod>();
        private int nextId = 1;
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public IReadOnlyList<SBPlacedMod> Items { get; private set; }
        public event Action Changed;

        public SBModGridModel(int columns, int rows)
        {
            if (columns < 1 || rows < 1) throw new ArgumentException("Grid dimensions must be positive.");
            Columns = columns; Rows = rows; Items = items.AsReadOnly();
        }

        public SBPlacedMod Find(int id) { return items.Find(item => item.Id == id); }

        public SBPlacedMod At(int column, int row)
        {
            return items.Find(item => column >= item.Column && row >= item.Row &&
                column < item.Column + item.CellWidth && row < item.Row + item.CellHeight);
        }

        public bool CanPlace(int width, int height, int column, int row, int ignoredId = 0)
        {
            if (width < 1 || height < 1 || column < 0 || row < 0 || column >= Columns || row >= Rows ||
                width > Columns - column || height > Rows - row) return false;
            foreach (SBPlacedMod item in items)
            {
                if (item.Id == ignoredId) continue;
                bool overlaps = column < item.Column + item.CellWidth && column + width > item.Column &&
                    row < item.Row + item.CellHeight && row + height > item.Row;
                if (overlaps) return false;
            }
            return true;
        }

        public bool TryAdd(SBModDefinition definition, SBModTier tier, int column, int row, bool rotated, out int id)
        {
            id = 0;
            if (definition == null) return false;
            int width = rotated ? definition.Height : definition.Width;
            int height = rotated ? definition.Width : definition.Height;
            if (!CanPlace(width, height, column, row)) return false;
            id = nextId++;
            items.Add(new SBPlacedMod(id, definition, tier, column, row, rotated));
            Notify();
            return true;
        }

        // Validate before changing anything: invalid drops preserve the old placement.
        public bool TryMove(int id, int column, int row, bool rotated)
        {
            SBPlacedMod item = Find(id);
            if (item == null) return false;
            int width = rotated ? item.Height : item.Width;
            int height = rotated ? item.Width : item.Height;
            if (!CanPlace(width, height, column, row, id)) return false;
            item.Column = column; item.Row = row; item.Rotated = rotated;
            Notify();
            return true;
        }

        public bool Remove(int id)
        {
            int removed = items.RemoveAll(item => item.Id == id);
            if (removed > 0) Notify();
            return removed > 0;
        }

        public void Clear() { items.Clear(); Notify(); }
        private void Notify() { if (Changed != null) Changed(); }

        public SBWeaponStats Calculate(SBWeaponStats baseStats)
        {
            float[] bonuses = new float[7];
            foreach (SBPlacedMod item in items) bonuses[(int)item.Stat] += item.Bonus;
            // Always start from the original stats. Never feed modified stats back into this method.
            return new SBWeaponStats
            {
                Damage = baseStats.Damage * (1f + bonuses[(int)SBModStat.Damage]),
                ShotsPerSecond = baseStats.ShotsPerSecond * (1f + bonuses[(int)SBModStat.FireRate]),
                CriticalMultiplier = baseStats.CriticalMultiplier * (1f + bonuses[(int)SBModStat.CriticalDamage]),
                Accuracy = Clamp(baseStats.Accuracy + bonuses[(int)SBModStat.Accuracy], 0f, 1f),
                CriticalChance = Clamp(baseStats.CriticalChance + bonuses[(int)SBModStat.CriticalChance], 0f, .75f),
                Penetration = Clamp(baseStats.Penetration + bonuses[(int)SBModStat.Penetration], 0f, 1f),
                MultishotChance = Clamp(baseStats.MultishotChance + bonuses[(int)SBModStat.Multishot], 0f, 1f)
            };
        }

        private static float Clamp(float value, float min, float max) { return Math.Min(max, Math.Max(min, value)); }
    }
}
