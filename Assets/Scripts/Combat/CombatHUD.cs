using UnityEngine;

namespace ScrapBench.Combat
{
    // Quick debug HUD (no Canvas needed): health, scrap and live weapon stats.
    // Replace with a proper UI later.
    public class CombatHUD : MonoBehaviour
    {
        private Health health;
        private Wallet wallet;
        private Weapon weapon;
        private GUIStyle style;

        private void Awake()
        {
            health = GetComponent<Health>();
            wallet = GetComponent<Wallet>();
            weapon = GetComponent<Weapon>();
        }

        private void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(GUI.skin.label) { fontSize = 22 };
                style.normal.textColor = Color.white;
            }

            float y = 10f;
            if (health != null) GUI.Label(new Rect(16, y, 600, 30), $"HP: {health.Current:0} / {health.Max:0}", style);
            y += 30f;
            if (wallet != null) GUI.Label(new Rect(16, y, 600, 30), $"Scrap: {wallet.Scrap}", style);
            y += 30f;

            if (weapon != null && weapon.Data != null)
            {
                var s = weapon.GetStats();
                GUI.Label(new Rect(16, y, 900, 30),
                    $"{weapon.Data.DisplayName}  DMG {s.Damage:0.#}  RATE {s.FireRate:0.#}/s  ACC {s.Accuracy * 100f:0}%", style);
                y += 30f;
                GUI.Label(new Rect(16, y, 900, 30),
                    $"CRIT {s.CritChance * 100f:0}% x{s.CritDamage:0.##}  PEN {s.Penetration * 100f:0}%  MULTI +{s.ExtraProjectileChance * 100f:0}%", style);
            }
        }
    }
}
