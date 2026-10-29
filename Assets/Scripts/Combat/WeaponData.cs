using UnityEngine;

namespace ScrapBench.Combat
{
    // Base stats for a gun type. Attachments modify these at runtime.
    // Create via: Right-click > Create > Scrap-Bench > Weapon Data
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Scrap-Bench/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [SerializeField] private string id = "weapon";
        [SerializeField] private string displayName = "Weapon";
        [SerializeField] private float baseDamage = 10f;
        [Tooltip("Shots per second")]
        [SerializeField] private float baseFireRate = 3f;
        [Tooltip("0..1. Higher = less bullet spread")]
        [SerializeField, Range(0f, 1f)] private float baseAccuracy = 0.8f;
        [SerializeField, Range(0f, 1f)] private float baseCritChance = 0.05f;
        [Tooltip("Damage multiplier on a crit (1.5 = +50%)")]
        [SerializeField] private float baseCritDamage = 1.5f;
        [Tooltip("0..1. Fraction of enemy armor ignored")]
        [SerializeField, Range(0f, 1f)] private float basePenetration = 0f;
        [SerializeField] private int projectilesPerShot = 1;
        [SerializeField] private float projectileSpeed = 14f;
        [SerializeField] private float projectileLifetime = 1.5f;
        [Tooltip("Spread in degrees at 0 accuracy")]
        [SerializeField] private float maxSpreadDegrees = 20f;

        public string Id => id;
        public string DisplayName => displayName;
        public float BaseDamage => baseDamage;
        public float BaseFireRate => baseFireRate;
        public float BaseAccuracy => baseAccuracy;
        public float BaseCritChance => baseCritChance;
        public float BaseCritDamage => baseCritDamage;
        public float BasePenetration => basePenetration;
        public int ProjectilesPerShot => projectilesPerShot;
        public float ProjectileSpeed => projectileSpeed;
        public float ProjectileLifetime => projectileLifetime;
        public float MaxSpreadDegrees => maxSpreadDegrees;
    }
}
