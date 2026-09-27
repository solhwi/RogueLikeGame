using UnityEngine;
using RogueLike.Combat;
using RogueLike.Utility;

namespace RogueLike.Items
{
    // Reference implementation of the item-skill pattern: an item that,
    // while equipped, auto-fires a projectile at the nearest enemy on its
    // own cooldown, entirely on its own — nothing outside this file drives
    // the firing.
    //
    // To add a new item type, follow this shape: subclass ItemDefinition
    // with whatever data it needs, override CreateBehavior() to return a
    // matching ItemSkillBehavior, and implement Tick(). Nothing outside this
    // file changes — ItemInventory, ItemSkillLoadout and the UI stay generic.
    [CreateAssetMenu(menuName = "Roguelike/Items/Projectile Item", fileName = "NewProjectileItem")]
    public class ProjectileItemDefinition : ItemDefinition
    {
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float range = 8f;
        [SerializeField] private float cooldown = 1f;
        [SerializeField] private int damage = 5;
        [SerializeField] private int projectileCount = 1;
        [SerializeField] private int pierceCount = 0;
        [SerializeField] private float projectileSpeed = 10f;

        public override ItemSkillBehavior CreateBehavior()
        {
            return new ProjectileBehavior(this);
        }

        private class ProjectileBehavior : ItemSkillBehavior
        {
            private readonly ProjectileItemDefinition definition;
            private float cooldownTimer;

            public ProjectileBehavior(ProjectileItemDefinition definition)
            {
                this.definition = definition;
            }

            public override void Tick(float deltaTime, ItemUser user)
            {
                cooldownTimer -= deltaTime;
                if (cooldownTimer > 0f)
                {
                    return;
                }

                var target = user.FindNearestEnemy(definition.range);
                if (target == null)
                {
                    return;
                }

                var pool = ObjectPool.GetOrCreate(definition.projectilePrefab);
                if (pool == null)
                {
                    return;
                }

                Vector2 origin = user.Transform.position;
                Vector2 direction = ((Vector2)target.position - origin).normalized;

                for (int i = 0; i < definition.projectileCount; i++)
                {
                    var instance = pool.Get(origin, Quaternion.identity);
                    if (instance != null && instance.TryGetComponent<Projectile>(out var projectile))
                    {
                        projectile.Launch(origin, direction, definition.projectileSpeed, definition.damage, definition.pierceCount, pool);
                    }
                }

                float cooldownReduction = user.GetPassiveBonus(StatType.CooldownReduction);
                cooldownTimer = definition.cooldown * Mathf.Clamp01(1f - cooldownReduction);
            }
        }
    }
}
