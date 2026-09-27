using UnityEngine;
using RogueLike.Enemies;

namespace RogueLike.Level
{
    [System.Serializable]
    public struct WaveEntry
    {
        public EnemyDefinition enemy;
        public float startTime;
        public float endTime;
        public float spawnInterval;
        public int countPerSpawn;
    }

    [CreateAssetMenu(menuName = "Roguelike/Level/Wave Data", fileName = "NewWaveData")]
    public class WaveData : ScriptableObject
    {
        [SerializeField] private WaveEntry[] entries;
        [SerializeField] private EnemyDefinition bossEnemy;
        [SerializeField] private float bossSpawnTime = 420f;

        public WaveEntry[] Entries => entries;
        public EnemyDefinition BossEnemy => bossEnemy;
        public float BossSpawnTime => bossSpawnTime;
    }
}
