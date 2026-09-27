using UnityEngine;

namespace RogueLike.Combat
{
    [CreateAssetMenu(menuName = "Roguelike/Skills/Active Skill", fileName = "NewActiveSkill")]
    public class ActiveSkillDefinition : SkillDefinition
    {
        [System.Serializable]
        public struct LevelStats
        {
            public int damage;
            public float cooldown;
            public int projectileCount;
            public int pierceCount;
            public float projectileSpeed;
        }

        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float range = 8f;
        [SerializeField] private LevelStats[] levelStats;

        public override SkillCategory Category => SkillCategory.Active;
        public GameObject ProjectilePrefab => projectilePrefab;
        public float Range => range;

        public LevelStats GetStats(int level)
        {
            if (levelStats == null || levelStats.Length == 0)
            {
                return default;
            }

            int index = Mathf.Clamp(level - 1, 0, levelStats.Length - 1);
            return levelStats[index];
        }
    }
}
