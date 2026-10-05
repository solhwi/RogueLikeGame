using System.Collections.Generic;
using UnityEngine;
using RogueLike.Items;

namespace RogueLike.UI
{
    // Inventory panel: opened from an ItemSkillSlotUI with the slot it
    // should fill. Lists every owned item; tapping one equips it into that
    // slot and closes the panel. Entries are pooled across refreshes rather
    // than re-instantiated, and stay in sync with both ItemInventory
    // (new/removed items) and ItemSkillLoadout (so the equipped badge
    // follows swaps made from another slot too).
    public class ItemInventoryUI : MonoBehaviour
    {
        [SerializeField] private ItemInventory inventory;
        [SerializeField] private ItemSkillLoadout loadout;
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform listParent;
        [SerializeField] private ItemInventoryEntryUI entryPrefab;

        private readonly List<ItemInventoryEntryUI> spawnedEntries = new List<ItemInventoryEntryUI>();
        private int targetSlotIndex = -1;

        private void OnEnable()
        {
            if (inventory != null)
            {
                inventory.OnItemAdded += HandleInventoryChanged;
                inventory.OnItemRemoved += HandleInventoryChanged;
            }
            if (loadout != null)
            {
                loadout.OnSlotChanged += HandleSlotChanged;
            }
        }

        private void OnDisable()
        {
            if (inventory != null)
            {
                inventory.OnItemAdded -= HandleInventoryChanged;
                inventory.OnItemRemoved -= HandleInventoryChanged;
            }
            if (loadout != null)
            {
                loadout.OnSlotChanged -= HandleSlotChanged;
            }
        }

        public void Open(int slotIndex)
        {
            targetSlotIndex = slotIndex;
            if (panel != null)
            {
                panel.SetActive(true);
            }

            Refresh();
        }

        public void Close()
        {
            targetSlotIndex = -1;
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void HandleInventoryChanged(ItemInstance _) => RefreshIfOpen();

        private void HandleSlotChanged(int _, ItemInstance __) => RefreshIfOpen();

        private void RefreshIfOpen()
        {
            if (panel != null && panel.activeSelf)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            if (inventory == null || listParent == null || entryPrefab == null)
            {
                return;
            }

            while (spawnedEntries.Count < inventory.Items.Count)
            {
                spawnedEntries.Add(Instantiate(entryPrefab, listParent));
            }

            for (int i = 0; i < spawnedEntries.Count; i++)
            {
                bool hasItem = i < inventory.Items.Count;
                spawnedEntries[i].gameObject.SetActive(hasItem);
                if (hasItem)
                {
                    spawnedEntries[i].Bind(inventory.Items[i], HandleItemClicked);
                }
            }
        }

        private void HandleItemClicked(ItemInstance item)
        {
            if (targetSlotIndex < 0 || loadout == null)
            {
                return;
            }

            loadout.TryEquip(targetSlotIndex, item);
            Close();
        }
    }
}
