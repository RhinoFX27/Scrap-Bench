using UnityEngine;
 
namespace ScrapBench.Weapons
{
    
    [CreateAssetMenu(fileName = "NewAttachment", menuName = "Scrap-Bench/Attachment")]
    public class AttachmentData : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private StatType stat;
        [Tooltip("Bonus at Common, Tier 1. 0.10 = +10%")]
        [SerializeField] private float baseValue = 0.10f;
 
        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public StatType Stat => stat;
        public float BaseValue => baseValue;
 
        public float GetValue(Rarity rarity, int tier)
        {
            return baseValue
                 * AttachmentScaling.GetRarityMult(rarity)
                 * AttachmentScaling.GetTierMult(tier);
        }
    }
}
