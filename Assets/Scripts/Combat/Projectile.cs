using UnityEngine;

namespace ScrapBench.Combat
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private DamageInfo info;
        private float speed;
        private bool spent;

        public void Init(DamageInfo damageInfo, float projectileSpeed, float lifetime)
        {
            info = damageInfo;
            speed = projectileSpeed;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += transform.right * (speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (spent || other.isTrigger) return;

            var health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                if (health.Team == info.sourceTeam) return; // ignore friendlies
                health.TakeDamage(info);
            }

            // Hit an enemy or a wall: bullet is used up.
            spent = true;
            Destroy(gameObject);
        }
    }
}
