using System;
using UnityEngine;
using ScrapBench.Codex; // codex unlock on kill (MOD-16)

namespace ScrapBench.Combat
{
    // Basic enemy: chases the player, damages on contact, drops Scrap on death.
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyData data;

        private Rigidbody2D rb;
        private Health health;
        private Transform target;
        private float nextHitTime;

        public EnemyData Data => data;

        // Other systems (waves, quests, score) can listen to this.
        public static event Action<Enemy> Killed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
            health.SetTeam(Team.Enemy);
            if (data != null) health.Configure(data.MaxHealth, data.Armor);
            health.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (health != null) health.Died -= OnDied;
        }

        private void FindTarget()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }

        private void FixedUpdate()
        {
            if (data == null) return;
            if (target == null) FindTarget();
            if (target == null)
            {
                PhysicsCompat.SetVelocity(rb, Vector2.zero);
                return;
            }

            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            PhysicsCompat.SetVelocity(rb, dir * data.MoveSpeed);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (data == null || Time.time < nextHitTime) return;

            var other = collision.collider.GetComponentInParent<Health>();
            if (other == null || other.Team == Team.Enemy) return;

            other.TakeDamage(new DamageInfo
            {
                amount = data.ContactDamage,
                sourceTeam = Team.Enemy
            });
            nextHitTime = Time.time + data.ContactCooldown;
        }

        private void OnDied()
        {
            if (data != null)
            {
                if (target != null)
                {
                    var wallet = target.GetComponent<Wallet>();
                    if (wallet != null) wallet.Add(UnityEngine.Random.Range(data.ScrapMin, data.ScrapMax + 1));
                }
                CodexManager.TryUnlock(data.Id);
            }

            Killed?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
