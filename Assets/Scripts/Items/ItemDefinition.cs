using UnityEngine;

namespace RogueLike.Items
{
    // Base data for one item type. An item IS a skill — this is the only
    // skill concept in the game, active and passive alike: equipping it
    // into a skill slot is what makes it act, via the behavior it creates
    // below. Adding a new item type never touches ItemInventory,
    // ItemSkillLoadout, or the UI — it's a new ItemDefinition subclass
    // (data) paired with a new ItemSkillBehavior subclass (logic).
    public abstract class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemKey;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField, TextArea] private string description;

        // Unique key code identifying this item, independent of the asset
        // name/guid — used to look up or reference the item by data (e.g.
        // save files, drop tables) rather than by direct asset reference.
        public string ItemKey => itemKey;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public string Description => description;

        // Creates this item's own skill logic. Called once per equip by
        // ItemSkillLoadout; the returned behavior is ticked every frame with
        // the equipping character until the item is unequipped.
        public abstract ItemSkillBehavior CreateBehavior();
    }
}
