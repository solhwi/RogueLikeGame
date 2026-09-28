using UnityEngine;
using RogueLike.Combat;

namespace RogueLike.Items
{
    // Second reference implementation of the item-skill pattern, alongside
    // ProjectileItemDefinition (fire-on-cooldown) and StatBonusItemDefinition
    // (pure passive): a companion object that orbits the character for as
    // long as the item stays equipped, instead of firing on a cooldown.
    //
    // Its damage reuses Combat.Hazard — the same "damage anything
    // IDamageable on contact" component enemy hitboxes already use — so
    // this file only has to manage the blade's position and lifecycle, not
    // its own collision/damage logic.
    [CreateAssetMenu(menuName = "Roguelike/Items/Orbiting Blade Item", fileName = "NewOrbitingBladeItem")]
    public class OrbitingBladeItemDefinition : ItemDefinition
    {
        [SerializeField] private GameObject bladePrefab;
        [SerializeField] private float orbitRadius = 2f;
        [SerializeField] private float orbitDegreesPerSecond = 180f;
        [SerializeField] private int damage = 2;

        public override ItemSkillBehavior CreateBehavior()
        {
            return new OrbitingBladeBehavior(this);
        }

        private class OrbitingBladeBehavior : ItemSkillBehavior
        {
            private readonly OrbitingBladeItemDefinition definition;
            private GameObject blade;
            private float angleDegrees;

            public OrbitingBladeBehavior(OrbitingBladeItemDefinition definition)
            {
                this.definition = definition;
            }

            public override void OnEquip(ItemUser user)
            {
                blade = Object.Instantiate(definition.bladePrefab, user.Transform.position, Quaternion.identity);
                if (blade.TryGetComponent<Hazard>(out var hazard))
                {
                    hazard.SetDamage(definition.damage);
                }
            }

            public override void Tick(float deltaTime, ItemUser user)
            {
                if (blade == null)
                {
                    return;
                }

                angleDegrees += definition.orbitDegreesPerSecond * deltaTime;
                Vector2 offset = (Vector2)(Quaternion.Euler(0f, 0f, angleDegrees) * Vector3.right) * definition.orbitRadius;
                blade.transform.position = (Vector2)user.Transform.position + offset;
            }

            public override void OnUnequip(ItemUser user)
            {
                if (blade != null)
                {
                    Object.Destroy(blade);
                    blade = null;
                }
            }
        }
    }
}
