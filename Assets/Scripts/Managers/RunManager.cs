using UnityEngine;
using RogueLike.Combat;
using RogueLike.Core;
using RogueLike.Level;

namespace RogueLike.Managers
{
    // Orchestrates one chapter run: wires the chapter's data into the wave
    // spawner and map bounds, and turns player-death events into the
    // GameManager state that ends the run.
    public class RunManager : MonoBehaviour
    {
        [SerializeField] private ChapterDefinition chapter;
        [SerializeField] private Transform player;
        [SerializeField] private Health playerHealth;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private ChapterBounds bounds;
        [SerializeField] private PlayerSkillLoadout skillLoadout;
        [SerializeField] private SkillDefinition[] availableSkillPool;
        [SerializeField] private int skillChoiceCount = 3;

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDied += HandleRunFailed;
            }
            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.OnLevelUp += HandleLevelUp;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDied -= HandleRunFailed;
            }
            if (ExperienceManager.Instance != null)
            {
                ExperienceManager.Instance.OnLevelUp -= HandleLevelUp;
            }
        }

        private void Start()
        {
            if (chapter == null)
            {
                return;
            }

            waveSpawner.Initialize(chapter.WaveData);
            bounds.Initialize(chapter.MapType, chapter.MapBoundsSize, player.position);
            ExperienceManager.Instance?.ResetRun();
            KillCounter.Instance?.ResetRun();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }
            waveSpawner.Tick(Time.deltaTime);
        }

        public SkillDefinition[] GetSkillChoices()
        {
            var candidates = System.Array.FindAll(availableSkillPool, skillLoadout.CanOffer);
            Shuffle(candidates);

            int take = Mathf.Min(skillChoiceCount, candidates.Length);
            var result = new SkillDefinition[take];
            System.Array.Copy(candidates, result, take);
            return result;
        }

        public void ChooseSkill(SkillDefinition definition)
        {
            skillLoadout.ApplyChoice(definition);
            GameManager.Instance?.ExitLevelUpSelection();
        }

        private void HandleLevelUp(int newLevel)
        {
            GameManager.Instance?.EnterLevelUpSelection();
        }

        private void HandleRunFailed()
        {
            GameManager.Instance?.SetState(GameState.GameOver);
        }

        private static void Shuffle(SkillDefinition[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}
