using UnityEngine;
using RogueLike.Combat;

namespace RogueLike.Items
{
    // A purely passive item: no Tick behavior, just a flat contribution to
    // one stat for as long as it stays equipped (e.g. a move-speed or
    // pickup-range boost). Same ItemDefinition/ItemSkillBehavior shape as
    // any other item — see ProjectileItemDefinition for the active-behavior
    // counterpart.
    [CreateAssetMenu(menuName = "Roguelike/Items/Stat Bonus Item", fileName = "NewStatBonusItem")]
    public class StatBonusItemDefinition : ItemDefinition
    {
        [SerializeField] private StatType statType;
        [SerializeField] private float bonusValue;

        public override ItemSkillBehavior CreateBehavior()
        {
            return new StatBonusBehavior(this);
        }

        private class StatBonusBehavior : ItemSkillBehavior
        {
            private readonly StatBonusItemDefinition definition;

            public StatBonusBehavior(StatBonusItemDefinition definition)
            {
                this.definition = definition;
            }

            public override void Tick(float deltaTime, ItemUser user)
            {
            }

            public override float GetStatBonus(StatType type)
            {
                return type == definition.statType ? definition.bonusValue : 0f;
            }
        }
    }
}
