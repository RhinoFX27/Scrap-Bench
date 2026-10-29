using System;
using UnityEngine;

namespace ScrapBench.Combat
{
    // The player's Scrap currency. The shop (MOD-2) spends from this.
    public class Wallet : MonoBehaviour
    {
        [SerializeField] private int startingScrap = 0;

        public int Scrap { get; private set; }
        public event Action<int> Changed;

        private void Awake()
        {
            Scrap = startingScrap;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;
            Scrap += amount;
            Changed?.Invoke(Scrap);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Scrap < amount) return false;
            Scrap -= amount;
            Changed?.Invoke(Scrap);
            return true;
        }
    }
}
