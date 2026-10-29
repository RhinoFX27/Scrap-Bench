using System.Collections.Generic;
using UnityEngine;

namespace ScrapBench.Combat
{
    // Spawns random enemies in a ring around the player.
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private List<Enemy> enemyPrefabs = new List<Enemy>();
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int maxAlive = 15;
        [SerializeField] private float minRadius = 8f;
        [SerializeField] private float maxRadius = 12f;

        private readonly List<Enemy> alive = new List<Enemy>();
        private Transform player;
        private float nextSpawnTime;

        private void Update()
        {
            alive.RemoveAll(e => e == null);

            if (player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p == null) return;
                player = p.transform;
            }

            if (enemyPrefabs.Count == 0 || alive.Count >= maxAlive) return;
            if (Time.time < nextSpawnTime) return;

            nextSpawnTime = Time.time + spawnInterval;
            Spawn();
        }

        private void Spawn()
        {
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            if (prefab == null) return;

            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir == Vector2.zero) dir = Vector2.right;
            Vector2 pos = (Vector2)player.position + dir * Random.Range(minRadius, maxRadius);

            alive.Add(Instantiate(prefab, pos, Quaternion.identity));
        }
    }
}
