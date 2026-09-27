using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RogueLike.Items
{
    // Holds every item the run currently owns (found, bought, rewarded...),
    // independent of which of them are equipped into skill slots. Separate
    // from Items.PlayerInventory, which tracks gear/equipment rarity — this
    // one is for item-skills only.
    public class ItemInventory : MonoBehaviour
    {
        // Items owned from the start of the run (e.g. a starter weapon).
        // Runtime pickups always go through Add() instead — this field only
        // exists so a run has something to equip before its first drop.
        [SerializeField] private ItemDefinition[] startingItems;

        public event Action<ItemInstance> OnItemAdded;
        public event Action<ItemInstance> OnItemRemoved;

        private readonly List<ItemInstance> items = new List<ItemInstance>();

        public IReadOnlyList<ItemInstance> Items => items;

        private void Awake()
        {
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
            return instance;
        }

        public bool Remove(ItemInstance instance)
        {
            bool removed = items.Remove(instance);
            if (removed)
            {
                OnItemRemoved?.Invoke(instance);
            }

            return removed;
        }

        public bool Owns(ItemDefinition definition)
        {
            return items.Any(item => item.Definition == definition);
        }
    }
}
