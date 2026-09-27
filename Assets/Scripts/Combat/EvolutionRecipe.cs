using UnityEngine;

namespace RogueLike.Combat
{
    // A maxed active skill + a specific passive together "breakthrough"
    // (돌파) into a stronger evolved active skill. See PlayerSkillLoadout.
    [CreateAssetMenu(menuName = "Roguelike/Skills/Evolution Recipe", fileName = "NewEvolutionRecipe")]
    public class EvolutionRecipe : ScriptableObject
    {
        [SerializeField] private ActiveSkillDefinition baseActiveSkill;
        [SerializeField] private PassiveSkillDefinition requiredPassiveSkill;
        [SerializeField] private ActiveSkillDefinition evolvedSkill;

        public ActiveSkillDefinition BaseActiveSkill => baseActiveSkill;
        public PassiveSkillDefinition RequiredPassiveSkill => requiredPassiveSkill;
        public ActiveSkillDefinition EvolvedSkill => evolvedSkill;
    }
}
