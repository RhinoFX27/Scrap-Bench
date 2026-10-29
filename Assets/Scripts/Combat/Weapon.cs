using UnityEngine;
using ScrapBench.Weapons; // attachments (MOD-6)

namespace ScrapBench.Combat
{
    // Final stats after attachments. The weapon-modding screen (MOD-12) can display this.
    public struct WeaponStats
    {
        public float Damage;
        public float FireRate;
        public float Accuracy;
        public float CritChance;
        public float CritDamage;
        public float Penetration;
        public float ExtraProjectileChance; // multishot: 0.1 = 10% chance of an extra bullet
        public int Projectiles;
    }

    public class Weapon : MonoBehaviour
    {
        [SerializeField] private WeaponData data;
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform muzzle;
        [SerializeField] private Team team = Team.Player;
        [SerializeField] private WeaponAttachments attachments;

        private float nextFireTime;

        public WeaponData Data => data;
        public WeaponAttachments Attachments => attachments;

        private void Awake()
        {
            if (attachments == null) attachments = GetComponent<WeaponAttachments>();
        }

        public void SetData(WeaponData newData) { data = newData; }

        private float Bonus(StatType stat)
        {
            return attachments != null ? attachments.GetTotalBonus(stat) : 0f;
        }

        // Damage, fire rate and crit damage scale multiplicatively (x1.10 etc).
        // Chance-type stats (accuracy, crit chance, penetration, multishot) are added and clamped.
        public WeaponStats GetStats()
        {
            var s = new WeaponStats();
            if (data == null) return s;

            s.Damage = data.BaseDamage * (1f + Bonus(StatType.Damage));
            s.FireRate = data.BaseFireRate * (1f + Bonus(StatType.FireRate));
            s.CritDamage = data.BaseCritDamage * (1f + Bonus(StatType.CritDamage));
            s.Accuracy = Mathf.Clamp01(data.BaseAccuracy + Bonus(StatType.Accuracy));
            s.CritChance = Mathf.Clamp01(data.BaseCritChance + Bonus(StatType.CritChance));
            s.Penetration = Mathf.Clamp01(data.BasePenetration + Bonus(StatType.Penetration));
            s.ExtraProjectileChance = Bonus(StatType.Multishot);
            s.Projectiles = data.ProjectilesPerShot;
            return s;
        }

        public bool TryFire(Vector2 direction)
        {
            if (data == null || projectilePrefab == null) return false;
            if (Time.time < nextFireTime) return false;

            var s = GetStats();
            nextFireTime = Time.time + 1f / Mathf.Max(0.01f, s.FireRate);

            // Multishot: whole part is guaranteed extra bullets, fraction is a chance for one more.
            int count = s.Projectiles + Mathf.FloorToInt(s.ExtraProjectileChance);
            float frac = s.ExtraProjectileChance - Mathf.Floor(s.ExtraProjectileChance);
            if (Random.value < frac) count++;

            float spread = (1f - s.Accuracy) * data.MaxSpreadDegrees;
            float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Vector2 origin = muzzle != null ? (Vector2)muzzle.position : (Vector2)transform.position;

            for (int i = 0; i < count; i++)
            {
                bool crit = Random.value < s.CritChance;
                var info = new DamageInfo
                {
                    amount = s.Damage * (crit ? s.CritDamage : 1f),
                    isCrit = crit,
                    armorPenetration = s.Penetration,
                    sourceTeam = team
                };

                float angle = baseAngle + Random.Range(-spread, spread);
                var p = Instantiate(projectilePrefab, origin, Quaternion.Euler(0f, 0f, angle));
                p.Init(info, data.ProjectileSpeed, data.ProjectileLifetime);
            }
            return true;
        }
    }
}
