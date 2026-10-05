using UnityEngine;
using RogueLike.CharacterRendering;

namespace RogueLike.Items
{
    // Bridges the item system to the ported CharacterModule
    // (CharacterEquipmentInventory, under Assets/Scripts/CharacterModule —
    // brought over from the CharacterDemo project as-is, so it stays in the
    // global namespace rather than RogueLike.*) and the RenderTexture-based
    // 2D view of it (CharacterRenderView, ported/adapted from
    // SebamoGameClient). Equipping an AppearanceItemDefinition writes the
    // item's code into the right equippedItems slot and asks
    // CharacterRenderView to rebuild and re-render the visible 3D parts.
    public class CharacterAppearanceAdapter : MonoBehaviour
    {
        [SerializeField] private CharacterEquipmentInventory inventory;
        [SerializeField] private CharacterRenderView renderView;

        public void SetSlot(int slotIndex, string itemCode)
        {
            if (inventory == null || renderView == null)
            {
                return;
            }
            if (slotIndex < 0 || slotIndex >= inventory.equippedItems.Length)
            {
                return;
            }

            inventory.equippedItems[slotIndex] = itemCode;
            renderView.Refresh();
        }
    }
}
