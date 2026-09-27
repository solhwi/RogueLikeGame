using System;
using UnityEngine;
using RogueLike.Core;

namespace RogueLike.Managers
{
    public class ExperienceManager : Singleton<ExperienceManager>
    {
        [SerializeField] private int baseExperienceToLevel = 10;
        [SerializeField] private float experienceGrowthPerLevel = 1.15f;

        public event Action<int> OnLevelUp;
        public event Action<int, int> OnExperienceChanged; // current, required

        public int Level { get; private set; } = 1;
        public int CurrentExperience { get; private set; }
        public int ExperienceToNextLevel { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            ExperienceToNextLevel = baseExperienceToLevel;
        }

        public void ResetRun()
        {
            Level = 1;
            CurrentExperience = 0;
            ExperienceToNextLevel = baseExperienceToLevel;
            OnExperienceChanged?.Invoke(CurrentExperience, ExperienceToNextLevel);
        }

        public void AddExperience(int amount)
        {
            CurrentExperience += amount;

            while (CurrentExperience >= ExperienceToNextLevel)
            {
                CurrentExperience -= ExperienceToNextLevel;
                Level++;
                ExperienceToNextLevel = Mathf.RoundToInt(ExperienceToNextLevel * experienceGrowthPerLevel);
                OnLevelUp?.Invoke(Level);
            }

            OnExperienceChanged?.Invoke(CurrentExperience, ExperienceToNextLevel);
        }
    }
}
