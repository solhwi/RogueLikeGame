using UnityEngine;

namespace RogueLike.Items
{
    public enum EquipmentRarity
    {
        Common,
        Uncommon,
        Rare,
        Elite,
        Epic,
        Legendary
    }

    public enum EquipmentSlot
    {
        Weapon,
        Armor
    }

    [CreateAssetMenu(menuName = "Roguelike/Items/Equipment", fileName = "NewEquipment")]
    public class EquipmentDefinition : ScriptableObject
    {
        [SerializeField] private string equipmentId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private EquipmentSlot slot;
        [SerializeField] private float baseStatValue;
        [SerializeField] private EquipmentRarity raritySkillThreshold = EquipmentRarity.Uncommon;
        [SerializeField, TextArea] private string raritySkillDescription;

        public string EquipmentId => equipmentId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public EquipmentSlot Slot => slot;
        public EquipmentRarity RaritySkillThreshold => raritySkillThreshold;
        public string RaritySkillDescription => raritySkillDescription;

        public float GetStatValue(EquipmentRarity rarity)
        {
            return baseStatValue * (1 + (int)rarity * 0.5f);
        }

        public bool HasRaritySkillAt(EquipmentRarity rarity)
        {
            return rarity >= raritySkillThreshold;
        }
    }
}
