using System.Collections.Generic;
using UnityEngine;
using RogueLike.Enemies;
using RogueLike.Utility;

namespace RogueLike.Level
{
    // Ring-spawns enemies around the player based on elapsed chapter time.
    // Driven externally by RunManager.Tick so it stays paused with the game.
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private float spawnRadius = 10f;

        private WaveData waveData;
        private float elapsed;
        private bool bossSpawned;
        private readonly Dictionary<WaveEntry, float> nextSpawnTimers = new Dictionary<WaveEntry, float>();

        public void Initialize(WaveData data)
        {
            waveData = data;
            elapsed = 0f;
            bossSpawned = false;
            nextSpawnTimers.Clear();
        }

        public void Tick(float deltaTime)
        {
            if (waveData == null)
            {
                return;
            }

            elapsed += deltaTime;

            foreach (var entry in waveData.Entries)
            {
                if (elapsed < entry.startTime || elapsed > entry.endTime)
                {
                    continue;
                }

                nextSpawnTimers.TryGetValue(entry, out float timer);
                timer -= deltaTime;

                if (timer <= 0f)
                {
                    for (int i = 0; i < entry.countPerSpawn; i++)
                    {
                        SpawnEnemy(entry.enemy);
                    }
                    timer = entry.spawnInterval;
                }

                nextSpawnTimers[entry] = timer;
            }

            if (!bossSpawned && waveData.BossEnemy != null && elapsed >= waveData.BossSpawnTime)
            {
                bossSpawned = true;
                SpawnEnemy(waveData.BossEnemy);
            }
        }

        private void SpawnEnemy(EnemyDefinition definition)
        {
            if (definition == null || player == null)
            {
                return;
            }

            var pool = ObjectPool.GetOrCreate(definition.Prefab);
            if (pool == null)
            {
                return;
            }

            Vector2 offset = Random.insideUnitCircle.normalized * spawnRadius;
            Vector3 spawnPosition = player.position + (Vector3)offset;

            var instance = pool.Get(spawnPosition, Quaternion.identity);
            if (instance != null && instance.TryGetComponent<EnemyChaseAI>(out var ai))
            {
                ai.Initialize(definition, player, pool);
            }
        }
    }
}
