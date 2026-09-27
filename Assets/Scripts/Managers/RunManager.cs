using UnityEngine;
using RogueLike.Combat;
using RogueLike.Core;
using RogueLike.Items;
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
        [SerializeField] private ItemInventory itemInventory;
        [SerializeField] private ItemSkillLoadout itemLoadout;
        [SerializeField] private ItemDefinition[] availableItemPool;
        [SerializeField] private int itemChoiceCount = 3;

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
            itemInventory?.ResetRun();
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }
            waveSpawner.Tick(Time.deltaTime);
        }

        // Items not already owned — there's no leveling yet, so a level-up
        // simply can't offer something the player already has.
        public ItemDefinition[] GetItemChoices()
        {
            var candidates = System.Array.FindAll(availableItemPool, definition => !itemInventory.Owns(definition));
            Shuffle(candidates);

            int take = Mathf.Min(itemChoiceCount, candidates.Length);
            var result = new ItemDefinition[take];
            System.Array.Copy(candidates, result, take);
            return result;
        }

        // Grants the item and, if there's a free skill slot, equips it right
        // away; otherwise it just sits in the inventory for the player to
        // swap in themselves later.
        public void ChooseItem(ItemDefinition definition)
        {
            var instance = itemInventory.Add(definition);

            int freeSlot = itemLoadout.FindFreeSlot();
            if (freeSlot >= 0)
            {
                itemLoadout.TryEquip(freeSlot, instance);
            }

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

        private static void Shuffle(ItemDefinition[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }
    }
}
