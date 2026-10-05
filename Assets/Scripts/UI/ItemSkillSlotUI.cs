using UnityEngine;
using UnityEngine.UI;
using RogueLike.Items;

namespace RogueLike.UI
{
    // One item-skill slot: shows the equipped item's icon, name and skill
    // description, and opens the inventory panel (targeted at this slot) on
    // click so the player can swap what's equipped here. Kept in sync with
    // ItemSkillLoadout's slot-changed event rather than polled every frame.
    [RequireComponent(typeof(Button))]
    public class ItemSkillSlotUI : MonoBehaviour
    {
        [SerializeField] private ItemSkillLoadout loadout;
        [SerializeField] private ItemInventoryUI inventoryUI;
        [SerializeField] private int slotIndex;
        [SerializeField] private Image icon;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text descriptionLabel;
        [SerializeField] private GameObject emptyState;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(HandleClick);
        }

        private void OnEnable()
        {
            if (loadout != null)
            {
                loadout.OnSlotChanged += HandleSlotChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (loadout != null)
            {
                loadout.OnSlotChanged -= HandleSlotChanged;
            }
        }

        private void HandleClick()
        {
            inventoryUI?.Open(slotIndex);
        }

        private void HandleSlotChanged(int changedSlot, ItemInstance item)
        {
            if (changedSlot == slotIndex)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            var item = loadout != null ? loadout.GetEquipped(slotIndex) : null;
            bool hasItem = item != null;

            if (emptyState != null)
            {
                emptyState.SetActive(!hasItem);
            }
            if (icon != null)
            {
                icon.gameObject.SetActive(hasItem);
                icon.sprite = hasItem ? item.Definition.Icon : null;
            }
            if (nameLabel != null)
            {
                nameLabel.text = hasItem ? item.Definition.DisplayName : string.Empty;
            }
            if (descriptionLabel != null)
            {
                descriptionLabel.text = hasItem ? item.Definition.Description : string.Empty;
            }
        }
    }
}
