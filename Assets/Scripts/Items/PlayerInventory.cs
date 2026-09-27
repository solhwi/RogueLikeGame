using System;
using System.Collections.Generic;
using UnityEngine;

namespace RogueLike.Items
{
    public class PlayerInventory : MonoBehaviour
    {
        public event Action<EquipmentInstance> OnEquipmentAdded;
        public event Action<EquipmentInstance> OnEquipmentFused;

        private readonly List<EquipmentInstance> equipment = new List<EquipmentInstance>();

        public IReadOnlyList<EquipmentInstance> Equipment => equipment;

        public void AddEquipment(EquipmentDefinition definition, EquipmentRarity rarity)
        {
            var instance = new EquipmentInstance(definition, rarity);
            equipment.Add(instance);
            OnEquipmentAdded?.Invoke(instance);
        }

        public bool TryFuse(EquipmentDefinition definition, EquipmentRarity rarity)
        {
            var fused = EquipmentFusionSystem.Fuse(definition, rarity, equipment);
            if (fused == null)
            {
                return false;
            }

            OnEquipmentFused?.Invoke(fused);
            return true;
        }
    }
}
