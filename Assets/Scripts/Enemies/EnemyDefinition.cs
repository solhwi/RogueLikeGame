using UnityEngine;

namespace RogueLike.Enemies
{
    public enum EnemyTier
    {
        Normal,
        Elite,
        Boss
    }

    [CreateAssetMenu(menuName = "Roguelike/Enemies/Enemy Definition", fileName = "NewEnemy")]
    public class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private string enemyId;
        [SerializeField] private GameObject prefab;
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private EnemyTier tier;
        [SerializeField] private GameObject experienceGemPrefab;
        [SerializeField] private int experienceReward = 1;

        public string EnemyId => enemyId;
        public GameObject Prefab => prefab;
        public int MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public int ContactDamage => contactDamage;
        public EnemyTier Tier => tier;
        public GameObject ExperienceGemPrefab => experienceGemPrefab;
        public int ExperienceReward => experienceReward;
    }
}
