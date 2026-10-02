using UnityEngine;

namespace ScrapBench.Weapons
{
    // All vertical scaling lives here. To add a rarity, add an enum value
    // in AttachmentEnums.cs and a case below. Tiers scale automatically.
    public static class AttachmentScaling
    {
        public const float TierGrowth = 0.20f; // +20% per tier above T1
        public const int MaxTier = 5;          // raise this to add more tiers

        public static float GetRarityMult(Rarity rarity)
        {
            switch (rarity)
            {
                case Rarity.Common:    return 1.00f;
                case Rarity.Uncommon:  return 1.25f;
                case Rarity.Rare:      return 1.60f;
                case Rarity.Epic:      return 2.00f;
                case Rarity.Legendary: return 2.50f;
                default:               return 1.00f;
            }
        }

        public static float GetTierMult(int tier)
        {
            return 1f + TierGrowth * (Mathf.Max(tier, 1) - 1);
        }

        // Hard caps for fast-scaling stats (total bonus, 0.75 = +75%).
        public static float GetMaxBonus(StatType stat)
        {
            switch (stat)
            {
                case StatType.CritChance: return 0.75f;
                case StatType.Multishot:  return 1.00f;
                default:                  return float.MaxValue;
            }
        }
    }
}
