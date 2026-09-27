namespace RogueLike.Combat
{
    // Runtime state for one equipped skill (its level). Plain class, not a
    // MonoBehaviour — owned and tracked by PlayerSkillLoadout.
    public class SkillInstance
    {
        public SkillDefinition Definition { get; }
        public int Level { get; private set; }

        public SkillInstance(SkillDefinition definition, int level = 1)
        {
            Definition = definition;
            Level = level;
        }

        public bool IsMaxLevel => Level >= Definition.MaxLevel;

        public void LevelUp()
        {
            if (!IsMaxLevel)
            {
                Level++;
            }
        }
    }
}
