using System;
using UnityEngine;

namespace ScrapBench.Weapons
{
    // A specific attachment the player owns: which type, what rarity, what tier.
    [Serializable]
    public class AttachmentInstance
    {
        public AttachmentData data;
        public Rarity rarity = Rarity.Common;
        [Range(1, AttachmentScaling.MaxTier)] public int tier = 1;

        public StatType Stat => data != null ? data.Stat : StatType.Damage;
        public float Value => data != null ? data.GetValue(rarity, tier) : 0f;

        public override string ToString()
        {
            if (data == null) return "(empty)";
            return $"{rarity} {data.DisplayName} T{tier}: +{Value * 100f:0.#}% {data.Stat}";
        }
    }
}
