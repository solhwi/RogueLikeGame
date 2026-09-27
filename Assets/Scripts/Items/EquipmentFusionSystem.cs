using System.Collections.Generic;
using System.Linq;

namespace RogueLike.Items
{
    // Same-rarity copies of the same equipment fuse into the next rarity.
    // Cost per rarity is data-driven since it isn't flat (higher tiers need
    // fewer copies) — see Survivor.io's fusion table for reference.
    public static class EquipmentFusionSystem
    {
        private static readonly Dictionary<EquipmentRarity, int> RequiredCountForUpgrade = new Dictionary<EquipmentRarity, int>
        {
            { EquipmentRarity.Common, 3 },
            { EquipmentRarity.Uncommon, 3 },
            { EquipmentRarity.Rare, 3 },
            { EquipmentRarity.Elite, 3 },
            { EquipmentRarity.Epic, 2 },
        };

        public static bool CanFuse(EquipmentDefinition definition, EquipmentRarity rarity, IReadOnlyList<EquipmentInstance> inventory, out int requiredCount)
        {
            requiredCount = 0;
            if (!RequiredCountForUpgrade.TryGetValue(rarity, out requiredCount))
            {
                return false;
            }

            int owned = inventory.Count(item => item.Definition == definition && item.Rarity == rarity);
            return owned >= requiredCount;
        }

        public static EquipmentInstance Fuse(EquipmentDefinition definition, EquipmentRarity rarity, List<EquipmentInstance> inventory)
        {
            if (!CanFuse(definition, rarity, inventory, out int requiredCount))
            {
                return null;
            }

            var consumed = inventory.Where(item => item.Definition == definition && item.Rarity == rarity).Take(requiredCount).ToList();
            foreach (var item in consumed)
            {
                inventory.Remove(item);
            }

            var upgraded = new EquipmentInstance(definition, (EquipmentRarity)((int)rarity + 1));
            inventory.Add(upgraded);
            return upgraded;
        }
    }
}
