using UnityEngine;

namespace RogueLike.Items
{
    // Bridges the item system to the ported CharacterModule
    // (CharacterEquipmentInventory / CharacterAssetComponent, under
    // Assets/Scripts/CharacterModule — brought over from the CharacterDemo
    // project as-is, so they stay in the global namespace rather than
    // RogueLike.*). Equipping an AppearanceItemDefinition writes the item's
    // code into the right equippedItems slot and asks CharacterAssetComponent
    // to rebuild the visible 3D parts.
    public class CharacterAppearanceAdapter : MonoBehaviour
    {
        [SerializeField] private CharacterEquipmentInventory inventory;
        [SerializeField] private CharacterAssetComponent assetComponent;

        public void SetSlot(int slotIndex, string itemCode)
        {
            if (inventory == null || assetComponent == null)
            {
                return;
            }
            if (slotIndex < 0 || slotIndex >= inventory.equippedItems.Length)
            {
                return;
            }

            inventory.equippedItems[slotIndex] = itemCode;
            assetComponent.Refresh();
        }
    }
}
