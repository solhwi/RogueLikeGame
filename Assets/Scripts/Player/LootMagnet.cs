using UnityEngine;
using RogueLike.Combat;
using RogueLike.Items;

namespace RogueLike.Player
{
    public class LootMagnet : MonoBehaviour
    {
        [SerializeField] private float baseRadius = 2f;
        [SerializeField] private LayerMask lootLayer;

        private ItemSkillLoadout loadout;

        private void Awake()
        {
            loadout = GetComponent<ItemSkillLoadout>();
        }

        private void Update()
        {
            float radius = baseRadius + (loadout != null ? loadout.GetPassiveBonus(StatType.PickupRange) : 0f);
            var hits = Physics2D.OverlapCircleAll(transform.position, radius, lootLayer);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<ExperienceGem>(out var gem))
                {
                    gem.Attract(transform);
                }
            }
        }
    }
}
