using System;
using UnityEngine;
using RogueLike.Combat;
using RogueLike.Player;

namespace RogueLike.Items
{
    // Fixed set of skill slots. Equipping an owned item into a slot creates
    // that item's own ItemSkillBehavior and ticks it every frame for as long
    // as it stays equipped — that's the entire "equip it and it acts on its
    // own" pipeline, and it never changes when a new item type is added.
    [RequireComponent(typeof(PlayerController))]
    public class ItemSkillLoadout : MonoBehaviour
    {
        [SerializeField] private int slotCount = 6;
        [SerializeField] private LayerMask enemyLayer;

        // slot index, item now occupying it (null when the slot was cleared)
        public event Action<int, ItemInstance> OnSlotChanged;

        private ItemInstance[] equippedItems;
        private ItemSkillBehavior[] behaviors;
        private ItemUser user;

        public int SlotCount => slotCount;

        private void Awake()
        {
            equippedItems = new ItemInstance[slotCount];
            behaviors = new ItemSkillBehavior[slotCount];
            user = new ItemUser(GetComponent<PlayerController>(), enemyLayer, this);
        }

        private void Update()
        {
            for (int i = 0; i < slotCount; i++)
            {
                behaviors[i]?.Tick(Time.deltaTime, user);
            }
        }

        public ItemInstance GetEquipped(int slotIndex)
        {
            return IsValidSlot(slotIndex) ? equippedItems[slotIndex] : null;
        }

        // First empty slot, or -1 if every slot is occupied — used to
        // auto-equip a newly picked item without displacing another one.
        public int FindFreeSlot()
        {
            for (int i = 0; i < slotCount; i++)
            {
                if (equippedItems[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        // Every equipped item's contribution to this stat, summed. Purely
        // passive items (see ItemSkillBehavior) only ever show up here.
        public float GetPassiveBonus(StatType type)
        {
            float total = 0f;

            foreach (var behavior in behaviors)
            {
                if (behavior != null)
                {
                    total += behavior.GetStatBonus(type);
                }
            }

            return total;
        }

        public bool TryEquip(int slotIndex, ItemInstance item)
        {
            if (!IsValidSlot(slotIndex) || item == null)
            {
                return false;
            }

            // An owned item can only occupy one slot at a time — pull it out
            // of wherever it already is before placing it here.
            int currentSlot = Array.IndexOf(equippedItems, item);
            if (currentSlot == slotIndex)
            {
                return true;
            }
            if (currentSlot >= 0)
            {
                Clear(currentSlot);
                OnSlotChanged?.Invoke(currentSlot, null);
            }

            Clear(slotIndex);

            equippedItems[slotIndex] = item;
            item.IsEquipped = true;

            var behavior = item.Definition.CreateBehavior();
            behaviors[slotIndex] = behavior;
            behavior?.OnEquip(user);

            OnSlotChanged?.Invoke(slotIndex, item);
            return true;
        }

        public void Unequip(int slotIndex)
        {
            if (IsValidSlot(slotIndex))
            {
                Clear(slotIndex);
                OnSlotChanged?.Invoke(slotIndex, null);
            }
        }

        private void Clear(int slotIndex)
        {
            var previousItem = equippedItems[slotIndex];
            if (previousItem != null)
            {
                previousItem.IsEquipped = false;
            }

            behaviors[slotIndex]?.OnUnequip(user);
            behaviors[slotIndex] = null;
            equippedItems[slotIndex] = null;
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < slotCount;
        }
    }
}
