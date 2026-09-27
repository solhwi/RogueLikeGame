namespace RogueLike.Items
{
    // One item's own skill logic. Plain class (not a MonoBehaviour), owned
    // and ticked by ItemSkillLoadout for as long as the item stays equipped.
    // Each item type subclasses this to do whatever it wants — fire a
    // projectile, spawn an orbiting companion, buff the character — without
    // ItemSkillLoadout needing to know anything about it beyond this shape.
    public abstract class ItemSkillBehavior
    {
        // Called once when the item is equipped into a slot, before the
        // first Tick. Override for one-time setup (e.g. spawning a visual
        // companion object).
        public virtual void OnEquip(ItemUser user)
        {
        }

        // Called once when the item is unequipped (swapped out or slot
        // cleared). Override to release whatever OnEquip set up.
        public virtual void OnUnequip(ItemUser user)
        {
        }

        // Called every frame the item stays equipped.
        public abstract void Tick(float deltaTime, ItemUser user);
    }
}
