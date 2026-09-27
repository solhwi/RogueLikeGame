using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RogueLike.Combat
{
    // Holds the run's equipped skills (max 6 total, active and passive
    // sharing one pool of slots) and applies level-up choices, including
    // breakthrough (돌파) evolution when a maxed active skill's evolution
    // recipe is satisfied by an owned passive.
    public class PlayerSkillLoadout : MonoBehaviour
    {
        [SerializeField] private int maxSkills = 6;
        [SerializeField] private EvolutionRecipe[] evolutionRecipes;

        public event Action<SkillInstance> OnActiveSkillAdded;
        public event Action<SkillInstance> OnActiveSkillEvolved;
        public event Action<SkillInstance> OnPassiveSkillAdded;
        public event Action<SkillInstance> OnSkillLeveledUp;

        private readonly List<SkillInstance> activeSkills = new List<SkillInstance>();
        private readonly List<SkillInstance> passiveSkills = new List<SkillInstance>();

        public IReadOnlyList<SkillInstance> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillInstance> PassiveSkills => passiveSkills;

        public int TotalSkillCount => activeSkills.Count + passiveSkills.Count;
        public bool HasFreeSlot => TotalSkillCount < maxSkills;

        public bool CanOffer(SkillDefinition definition)
        {
            var existing = Find(definition);
            if (existing != null)
            {
                return !existing.IsMaxLevel;
            }

            return HasFreeSlot;
        }

        public void ApplyChoice(SkillDefinition definition)
        {
            var existing = Find(definition);
            if (existing != null)
            {
                existing.LevelUp();
                OnSkillLeveledUp?.Invoke(existing);
                TryEvolve(existing);
                return;
            }

            var instance = new SkillInstance(definition);

            if (definition.Category == SkillCategory.Active)
            {
                activeSkills.Add(instance);
                OnActiveSkillAdded?.Invoke(instance);
            }
            else
            {
                passiveSkills.Add(instance);
                OnPassiveSkillAdded?.Invoke(instance);
                TryEvolveAllMaxed();
            }
        }

        public float GetPassiveBonus(StatType type)
        {
            float total = 0f;

            foreach (var skill in passiveSkills)
            {
                if (skill.Definition is PassiveSkillDefinition passive)
                {
                    total += passive.GetBonusAtLevel(type, skill.Level);
                }
            }

            return total;
        }

        private SkillInstance Find(SkillDefinition definition)
        {
            return activeSkills.Concat(passiveSkills).FirstOrDefault(s => s.Definition == definition);
        }

        private void TryEvolveAllMaxed()
        {
            foreach (var activeInstance in activeSkills.Where(s => s.IsMaxLevel).ToList())
            {
                TryEvolve(activeInstance);
            }
        }

        private void TryEvolve(SkillInstance activeInstance)
        {
            if (!(activeInstance.Definition is ActiveSkillDefinition active) || !activeInstance.IsMaxLevel)
            {
                return;
            }
            if (evolutionRecipes == null)
            {
                return;
            }

            foreach (var recipe in evolutionRecipes)
            {
                if (recipe.BaseActiveSkill != active)
                {
                    continue;
                }
                if (passiveSkills.All(p => p.Definition != recipe.RequiredPassiveSkill))
                {
                    continue;
                }

                activeSkills.Remove(activeInstance);
                var evolved = new SkillInstance(recipe.EvolvedSkill, 1);
                activeSkills.Add(evolved);
                OnActiveSkillEvolved?.Invoke(evolved);
                return;
            }
        }
    }
}
