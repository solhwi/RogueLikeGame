using UnityEngine;

namespace RogueLike.Combat
{
    public enum SkillCategory
    {
        Active,
        Passive
    }

    public abstract class SkillDefinition : ScriptableObject
    {
        [SerializeField] private string skillId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField, TextArea] private string description;
        [SerializeField] private int maxLevel = 5;

        public string SkillId => skillId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public string Description => description;
        public int MaxLevel => maxLevel;

        public abstract SkillCategory Category { get; }
    }
}
