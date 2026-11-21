using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScrapBench.GridPrototype
{
    // Attach to your weapon (or let SBModBenchDemo add one automatically).
    public sealed class SBModLoadout : MonoBehaviour
    {
        [Min(1)] public int columns = 3;
        [Min(1)] public int rows = 3;
        public SBWeaponStats baseStats = SBWeaponStats.Default;
        public List<SBModDefinition> catalog = SBModDefinition.CreateDefaults();
        private SBModGridModel grid;
        public event Action StatsChanged;

        public SBModGridModel Grid
        {
            get
            {
                if (grid == null)
                {
                    grid = new SBModGridModel(Mathf.Max(1, columns), Mathf.Max(1, rows));
                    grid.Changed += OnGridChanged;
                }
                return grid;
            }
        }

        public SBWeaponStats CurrentStats { get { return Grid.Calculate(baseStats); } }
        private void OnGridChanged() { if (StatsChanged != null) StatsChanged(); }
        private void OnDestroy() { if (grid != null) grid.Changed -= OnGridChanged; }
    }
}
