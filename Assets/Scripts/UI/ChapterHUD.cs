using UnityEngine;
using UnityEngine.UI;
using RogueLike.Combat;
using RogueLike.Managers;

namespace RogueLike.UI
{
    public class ChapterHUD : MonoBehaviour
    {
        [SerializeField] private Health playerHealth;
        [SerializeField] private Image[] healthDots;
        [SerializeField] private Color filledHealthDotColor = new Color(0.85f, 0.15f, 0.15f);
        [SerializeField] private Color emptyHealthDotColor = new Color(0.3f, 0.1f, 0.1f, 0.5f);
        [SerializeField] private Image[] experienceSegments;
        [SerializeField] private Color filledExperienceColor = new Color(0.45f, 0.85f, 0.25f);
        [SerializeField] private Color emptyExperienceColor = new Color(0.15f, 0.3f, 0.1f, 0.6f);
        [SerializeField] private Text levelText;
        [SerializeField] private Text killCountText;
        [SerializeField] private Text currencyText;

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
            }

            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.OnExperienceChanged += HandleExperienceChanged;
                ExperienceManager.Instance.OnLevelUp += HandleLevelUp;
            }

            if (KillCounter.Instance != null)
            {
                KillCounter.Instance.OnKillCountChanged += HandleKillCountChanged;
            }

            if (MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.OnGoldChanged += HandleGoldChanged;
            }
        }

        // Start (not OnEnable) so every object's Awake has already run —
        // Unity only guarantees Awake-before-OnEnable within the same
        // object, not across different objects, so reading other objects'
        // state from OnEnable can race their Awake and see default values.
        private void Start()
        {
            if (playerHealth != null)
            {
                HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
            }

            if (KillCounter.Instance != null)
            {
                HandleKillCountChanged(KillCounter.Instance.Count);
            }

            if (ExperienceManager.Instance != null)
            {
                HandleExperienceChanged(ExperienceManager.Instance.CurrentExperience, ExperienceManager.Instance.ExperienceToNextLevel);
            }

            if (MetaProgressionManager.Instance != null)
            {
                HandleGoldChanged(MetaProgressionManager.Instance.Gold);
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }

            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.OnExperienceChanged -= HandleExperienceChanged;
                ExperienceManager.Instance.OnLevelUp -= HandleLevelUp;
            }

            if (KillCounter.Instance != null)
            {
                KillCounter.Instance.OnKillCountChanged -= HandleKillCountChanged;
            }

            if (MetaProgressionManager.Instance != null)
            {
                MetaProgressionManager.Instance.OnGoldChanged -= HandleGoldChanged;
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (healthDots == null)
            {
                return;
            }

            for (int i = 0; i < healthDots.Length; i++)
            {
                if (healthDots[i] == null)
                {
                    continue;
                }

                bool slotExists = i < max;
                healthDots[i].gameObject.SetActive(slotExists);
                if (slotExists)
                {
                    healthDots[i].color = i < current ? filledHealthDotColor : emptyHealthDotColor;
                }
            }
        }

        private void HandleExperienceChanged(int current, int required)
        {
            if (experienceSegments == null)
            {
                return;
            }

            float ratio = required > 0 ? (float)current / required : 0f;
            int litCount = Mathf.Clamp(Mathf.FloorToInt(ratio * experienceSegments.Length), 0, experienceSegments.Length);

            for (int i = 0; i < experienceSegments.Length; i++)
            {
                if (experienceSegments[i] == null)
                {
                    continue;
                }

                experienceSegments[i].color = i < litCount ? filledExperienceColor : emptyExperienceColor;
            }
        }

        private void HandleLevelUp(int level)
        {
            if (levelText != null)
            {
                levelText.text = $"Lv. {level}";
            }
        }

        private void HandleKillCountChanged(int count)
        {
            if (killCountText != null)
            {
                killCountText.text = count.ToString();
            }
        }

        private void HandleGoldChanged(int gold)
        {
            if (currencyText != null)
            {
                currencyText.text = gold.ToString();
            }
        }
    }
}
