using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueLike.Combat;

namespace RogueLike.UI
{
    // Shows the player's currently equipped active/passive skills as icons
    // (with a level number badge), kept in sync with PlayerSkillLoadout's
    // add/level-up/evolve events rather than polled every frame.
    public class SkillLoadoutUI : MonoBehaviour
    {
        [SerializeField] private PlayerSkillLoadout loadout;
        [SerializeField] private Image[] skillIcons;
        [SerializeField] private Text[] skillLevels;

        private readonly List<SkillInstance> combinedSkills = new List<SkillInstance>();

        private void OnEnable()
        {
            if (loadout == null)
            {
                return;
            }

            loadout.OnActiveSkillAdded += HandleLoadoutChanged;
            loadout.OnActiveSkillEvolved += HandleLoadoutChanged;
            loadout.OnPassiveSkillAdded += HandleLoadoutChanged;
            loadout.OnSkillLeveledUp += HandleLoadoutChanged;
        }

        private void OnDisable()
        {
            if (loadout == null)
            {
                return;
            }

            loadout.OnActiveSkillAdded -= HandleLoadoutChanged;
            loadout.OnActiveSkillEvolved -= HandleLoadoutChanged;
            loadout.OnPassiveSkillAdded -= HandleLoadoutChanged;
            loadout.OnSkillLeveledUp -= HandleLoadoutChanged;
        }

        private void Start()
        {
            Refresh();
        }

        private void HandleLoadoutChanged(SkillInstance changed)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (loadout == null || skillIcons == null)
            {
                return;
            }

            combinedSkills.Clear();
            combinedSkills.AddRange(loadout.ActiveSkills);
            combinedSkills.AddRange(loadout.PassiveSkills);

            for (int i = 0; i < skillIcons.Length; i++)
            {
                if (skillIcons[i] == null)
                {
                    continue;
                }

                bool hasSkill = i < combinedSkills.Count;
                skillIcons[i].gameObject.SetActive(hasSkill);

                if (skillLevels != null && i < skillLevels.Length && skillLevels[i] != null)
                {
                    skillLevels[i].text = hasSkill ? combinedSkills[i].Level.ToString() : string.Empty;
                }

                if (!hasSkill)
                {
                    continue;
                }

                skillIcons[i].sprite = combinedSkills[i].Definition.Icon;
            }
        }
    }
}
