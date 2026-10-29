using System;
using UnityEngine;

namespace ScrapBench.Combat
{
    // Put on anything that can take damage (player and enemies).
    public class Health : MonoBehaviour
    {
        [SerializeField] private Team team = Team.Player;
        [SerializeField] private float maxHealth = 100f;
        [Tooltip("0 = no armor, 0.5 = takes 50% less damage")]
        [SerializeField, Range(0f, 0.9f)] private float armor = 0f;

        public float Current { get; private set; }
        public float Max => maxHealth;
        public Team Team => team;
        public bool IsDead { get; private set; }

        public event Action<float, bool> Damaged; // (damage dealt, wasCrit)
        public event Action Died;

        private void Awake()
        {
            Current = maxHealth;
        }

        public void SetTeam(Team t) { team = t; }

        public void Configure(float max, float armorValue)
        {
            maxHealth = max;
            armor = Mathf.Clamp(armorValue, 0f, 0.9f);
            Current = max;
        }

        public float TakeDamage(DamageInfo info)
        {
            if (IsDead || info.sourceTeam == team) return 0f;

            float effectiveArmor = armor * (1f - Mathf.Clamp01(info.armorPenetration));
            float dmg = info.amount * (1f - effectiveArmor);

            Current = Mathf.Max(0f, Current - dmg);
            Damaged?.Invoke(dmg, info.isCrit);

            if (Current <= 0f)
            {
                IsDead = true;
                Died?.Invoke();
            }
            return dmg;
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            Current = Mathf.Min(maxHealth, Current + amount);
        }
    }
}
