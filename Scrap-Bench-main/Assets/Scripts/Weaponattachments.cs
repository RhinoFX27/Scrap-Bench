using System.Collections.Generic;
using UnityEngine;
 
namespace ScrapBench.Weapons
{
    public class WeaponAttachments : MonoBehaviour
    {
        [SerializeField] private List<AttachmentInstance> equipped = new List<AttachmentInstance>();
 
        public IReadOnlyList<AttachmentInstance> Equipped => equipped;
 
        public void Equip(AttachmentInstance attachment)
        {
            if (attachment != null && attachment.data != null) equipped.Add(attachment);
        }
 
        public void Unequip(AttachmentInstance attachment)
        {
            equipped.Remove(attachment);
        }
 
        public float GetTotalBonus(StatType stat)
        {
            float total = 0f;
            foreach (var a in equipped)
            {
                if (a != null && a.data != null && a.Stat == stat)
                    total += a.Value;
            }
            return Mathf.Min(total, AttachmentScaling.GetMaxBonus(stat));
        }
 
        public float ApplyTo(float baseValue, StatType stat)
        {
            return baseValue * (1f + GetTotalBonus(stat));
        }
 
        [ContextMenu("Log Attachment Bonuses")]
        private void LogBonuses()
        {
            foreach (var a in equipped) Debug.Log(a);
            foreach (StatType s in System.Enum.GetValues(typeof(StatType)))
            {
                float b = GetTotalBonus(s);
                if (b > 0f) Debug.Log($"TOTAL {s}: +{b * 100f:0.#}%");
            }
        }
    }
}
 
