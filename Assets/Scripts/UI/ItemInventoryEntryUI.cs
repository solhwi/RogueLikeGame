using System;
using UnityEngine;
using UnityEngine.UI;
using RogueLike.Items;

namespace RogueLike.UI
{
    // One row in the inventory list. Purely a view — ItemInventoryUI binds
    // it to an ItemInstance and a click callback each refresh.
    [RequireComponent(typeof(Button))]
    public class ItemInventoryEntryUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text descriptionLabel;
        [SerializeField] private GameObject equippedBadge;

        private Button button;
        private ItemInstance boundItem;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void Bind(ItemInstance item, Action<ItemInstance> onClicked)
        {
            boundItem = item;

            if (icon != null)
            {
                icon.sprite = item.Definition.Icon;
            }
            if (nameLabel != null)
            {
                nameLabel.text = item.Definition.DisplayName;
            }
            if (descriptionLabel != null)
            {
                descriptionLabel.text = item.Definition.Description;
            }
            if (equippedBadge != null)
            {
                equippedBadge.SetActive(item.IsEquipped);
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClicked?.Invoke(boundItem));
        }
    }
}
