using System.Collections.Generic;
using UnityEngine;

namespace RogueLike.Items
{
    // Every item in the game, keyed by ItemDefinition.ItemKey. A save file
    // can only store that key string, so this is what turns it back into a
    // real ItemDefinition reference on load.
    [CreateAssetMenu(menuName = "Roguelike/Items/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private ItemDefinition[] items;

        private Dictionary<string, ItemDefinition> byKey;

        public bool TryGetByKey(string itemKey, out ItemDefinition definition)
        {
            if (byKey == null)
            {
                BuildLookup();
            }

            return byKey.TryGetValue(itemKey, out definition);
        }

        private void BuildLookup()
        {
            byKey = new Dictionary<string, ItemDefinition>();
            if (items == null)
            {
                return;
            }

            foreach (var item in items)
            {
                if (item != null && !string.IsNullOrEmpty(item.ItemKey))
                {
                    byKey[item.ItemKey] = item;
                }
            }
        }
    }
}
