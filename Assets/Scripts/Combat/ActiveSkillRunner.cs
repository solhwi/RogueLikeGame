using System.Collections.Generic;
using UnityEngine;
using RogueLike.Utility;

namespace RogueLike.Combat
{
    // Drives auto-fire for every equipped active skill: ticks each one's own
    // cooldown and fires at the nearest enemy in range when it's ready.
    [RequireComponent(typeof(PlayerSkillLoadout))]
    public class ActiveSkillRunner : MonoBehaviour
    {
        [SerializeField] private LayerMask enemyLayer;

        private PlayerSkillLoadout loadout;
        private readonly Dictionary<SkillInstance, float> cooldownTimers = new Dictionary<SkillInstance, float>();

        private void Awake()
        {
            loadout = GetComponent<PlayerSkillLoadout>();
        }

        private void Update()
        {
            foreach (var skill in loadout.ActiveSkills)
            {
                if (!(skill.Definition is ActiveSkillDefinition active))
                {
                    continue;
                }

                cooldownTimers.TryGetValue(skill, out float timer);
                timer -= Time.deltaTime;

                if (timer <= 0f && TryFire(skill, active))
                {
                    timer = active.GetStats(skill.Level).cooldown;
                }

                cooldownTimers[skill] = timer;
            }
        }

        private bool TryFire(SkillInstance skill, ActiveSkillDefinition active)
        {
            var target = AutoTargeting.FindNearestEnemy(transform.position, active.Range, enemyLayer);
            if (target == null)
            {
                return false;
            }

            var stats = active.GetStats(skill.Level);
            var pool = ObjectPool.GetOrCreate(active.ProjectilePrefab);
            if (pool == null)
            {
                return false;
            }

            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;

            for (int i = 0; i < stats.projectileCount; i++)
            {
                var instance = pool.Get(transform.position, Quaternion.identity);
                if (instance != null && instance.TryGetComponent<Projectile>(out var projectile))
                {
                    projectile.Launch(transform.position, direction, stats.projectileSpeed, stats.damage, stats.pierceCount, pool);
                }
            }

            return true;
        }
    }
}
