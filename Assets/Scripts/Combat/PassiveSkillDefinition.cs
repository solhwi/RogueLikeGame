using UnityEngine;

namespace RogueLike.Combat
{
    public enum StatType
    {
        DamageMultiplier,
        MoveSpeedMultiplier,
        MaxHealth,
        PickupRange,
        GoldGainMultiplier,
        CooldownReduction
    }

    [CreateAssetMenu(menuName = "Roguelike/Skills/Passive Skill", fileName = "NewPassiveSkill")]
    public class PassiveSkillDefinition : SkillDefinition
    {
        [System.Serializable]
        public struct StatBonus
        {
            public StatType statType;
            public float valuePerLevel;
        }

        [SerializeField] private StatBonus[] statBonuses;

        public override SkillCategory Category => SkillCategory.Passive;

        public float GetBonusAtLevel(StatType type, int level)
        {
            float total = 0f;

            if (statBonuses == null)
            {
                return total;
            }

            foreach (var bonus in statBonuses)
            {
                if (bonus.statType == type)
                {
                    total += bonus.valuePerLevel * level;
                }
            }

            return total;
        }
    }
}
