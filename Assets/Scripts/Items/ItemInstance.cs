namespace RogueLike.Items
{
    // Runtime state for one owned item copy. Plain class, not a
    // MonoBehaviour — owned and tracked by ItemInventory.
    public class ItemInstance
    {
        public ItemDefinition Definition { get; }

        // Kept in sync by ItemSkillLoadout so inventory UI can show which
        // owned copy is currently equipped without asking the loadout.
        public bool IsEquipped { get; internal set; }

        public ItemInstance(ItemDefinition definition)
        {
            Definition = definition;
        }
    }
}
