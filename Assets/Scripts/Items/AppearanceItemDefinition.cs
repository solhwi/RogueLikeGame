using UnityEngine;

namespace RogueLike.Items
{
    // Matches CharacterEquipmentInventory.equippedItems' slot order (see
    // its own header comment): one item code per slot, resolved to an
    // actual character/prop variant by the ported ItemTable asset.
    public enum AppearanceSlot
    {
        Body = 0,
        Hair = 1,
        Eye = 2,
        Face = 3,
        Accessory = 4,
        Prop = 5,
    }

    // A purely cosmetic item: equipping it changes the character's 3D
    // appearance (via CharacterAppearanceAdapter) and does nothing else —
    // no Tick behavior, same "no separate passive item type" shape as
    // StatBonusItemDefinition. itemCode must match a key already present in
    // the ported ItemTable asset (Assets/Bundles/Datas/ItemTable.asset) —
    // this definition doesn't add new entries there, it only equips one.
    [CreateAssetMenu(menuName = "Roguelike/Items/Appearance Item", fileName = "NewAppearanceItem")]
    public class AppearanceItemDefinition : ItemDefinition
    {
        [SerializeField] private AppearanceSlot slot;
        [SerializeField] private string itemCode;

        public override ItemSkillBehavior CreateBehavior()
        {
            return new AppearanceBehavior(this);
        }

        private class AppearanceBehavior : ItemSkillBehavior
        {
            private readonly AppearanceItemDefinition definition;

            public AppearanceBehavior(AppearanceItemDefinition definition)
            {
                this.definition = definition;
            }

            public override void OnEquip(ItemUser user)
            {
                user.Appearance?.SetSlot((int)definition.slot, definition.itemCode);
            }

            public override void OnUnequip(ItemUser user)
            {
                user.Appearance?.SetSlot((int)definition.slot, null);
            }

            public override void Tick(float deltaTime, ItemUser user)
            {
            }
        }
    }
}
