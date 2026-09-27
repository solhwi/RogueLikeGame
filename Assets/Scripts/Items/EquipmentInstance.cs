namespace RogueLike.Items
{
    public class EquipmentInstance
    {
        public EquipmentDefinition Definition { get; }
        public EquipmentRarity Rarity { get; private set; }

        public EquipmentInstance(EquipmentDefinition definition, EquipmentRarity rarity)
        {
            Definition = definition;
            Rarity = rarity;
        }

        public float CurrentStatValue => Definition.GetStatValue(Rarity);

        public void Upgrade(EquipmentRarity newRarity)
        {
            Rarity = newRarity;
        }
    }
}
