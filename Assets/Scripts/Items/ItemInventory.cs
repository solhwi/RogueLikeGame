using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RogueLike.Items
{
    [Serializable]
    public class ItemInventorySaveData
    {
        public List<string> ownedItemKeys = new List<string>();
    }

    // Every item the run currently owns (found, bought, rewarded...),
    // independent of which of them are equipped into skill slots.
    //
    // A ScriptableObject asset rather than a MonoBehaviour: it needs to be
    // reachable from anywhere (UI, loadout, save system) by a plain asset
    // reference instead of a scene lookup, and it needs to persist to disk
    // between sessions — same shape MetaProgressionManager already uses for
    // meta progression, just per-instance here instead of a scene singleton.
    [CreateAssetMenu(menuName = "Roguelike/Items/Item Inventory", fileName = "ItemInventory")]
    public class ItemInventory : ScriptableObject
    {
        private const string SaveFileName = "item_inventory.json";

        // Owned from the very first run, before there's anything to save.
        [SerializeField] private ItemDefinition[] startingItems;

        // Resolves a saved item key back into its ItemDefinition on load.
        [SerializeField] private ItemDatabase database;

        public event Action<ItemInstance> OnItemAdded;
        public event Action<ItemInstance> OnItemRemoved;

        private readonly List<ItemInstance> items = new List<ItemInstance>();

        public IReadOnlyList<ItemInstance> Items => items;

        private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        // Called once per run (RunManager.Start), not Awake/OnEnable — this
        // asset's lifetime isn't tied to the scene, so it can already be
        // holding a previous run's in-memory state (or, in the editor,
        // leftover Play Mode edits) by the time a new run starts.
        public void ResetRun()
        {
            items.Clear();

            if (File.Exists(SavePath))
            {
                Load();
                return;
            }

            if (startingItems == null)
            {
                return;
            }

            foreach (var definition in startingItems)
            {
                if (definition != null)
                {
                    Add(definition);
                }
            }
        }

        public ItemInstance Add(ItemDefinition definition)
        {
            var instance = new ItemInstance(definition);
            items.Add(instance);
            OnItemAdded?.Invoke(instance);
            Save();
            return instance;
        }

        public bool Remove(ItemInstance instance)
        {
            bool removed = items.Remove(instance);
            if (removed)
            {
                OnItemRemoved?.Invoke(instance);
                Save();
            }

            return removed;
        }

        public bool Owns(ItemDefinition definition)
        {
            return items.Any(item => item.Definition == definition);
        }

        private void Save()
        {
            var data = new ItemInventorySaveData();
            data.ownedItemKeys.AddRange(items.Select(item => item.Definition.ItemKey));
            File.WriteAllText(SavePath, JsonUtility.ToJson(data));
        }

        private void Load()
        {
            if (database == null)
            {
                return;
            }

            var data = JsonUtility.FromJson<ItemInventorySaveData>(File.ReadAllText(SavePath));
            foreach (var key in data.ownedItemKeys)
            {
                if (database.TryGetByKey(key, out var definition))
                {
                    Add(definition);
                }
            }
        }
    }
}
