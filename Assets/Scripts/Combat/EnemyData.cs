using UnityEngine;

namespace ScrapBench.Combat
{
    // Stats for an enemy type. Add new enemies (MOD-14) by creating more of these.
    // Create via: Right-click > Create > Scrap-Bench > Enemy Data
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Scrap-Bench/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Tooltip("Also the Codex entry id that unlocks when this enemy is killed.")]
        [SerializeField] private string id = "enemy";
        [SerializeField] private string displayName = "Enemy";
        [SerializeField] private float maxHealth = 30f;
        [SerializeField, Range(0f, 0.9f)] private float armor = 0f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private float contactDamage = 8f;
        [SerializeField] private float contactCooldown = 1f;
        [SerializeField] private int scrapMin = 1;
        [SerializeField] private int scrapMax = 3;

        public string Id => id;
        public string DisplayName => displayName;
        public float MaxHealth => maxHealth;
        public float Armor => armor;
        public float MoveSpeed => moveSpeed;
        public float ContactDamage => contactDamage;
        public float ContactCooldown => contactCooldown;
        public int ScrapMin => scrapMin;
        public int ScrapMax => scrapMax;
    }
}
