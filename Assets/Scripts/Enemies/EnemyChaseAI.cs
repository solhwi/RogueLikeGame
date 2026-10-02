using UnityEngine;
using RogueLike.Combat;
using RogueLike.Managers;
using RogueLike.Player;
using RogueLike.Utility;

namespace RogueLike.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Hazard))]
    public class EnemyChaseAI : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Health health;
        private EnemyDefinition definition;
        private Transform target;
        private ObjectPool sourcePool;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            health.OnDied -= HandleDied;
        }

        public void Initialize(EnemyDefinition enemyDefinition, Transform chaseTarget, ObjectPool pool)
        {
            definition = enemyDefinition;
            target = chaseTarget;
            sourcePool = pool;

            health.SetMaxHealth(definition.MaxHealth);
            health.SetCurrentHealth(definition.MaxHealth);

            if (TryGetComponent<Hazard>(out var hazard))
            {
                hazard.SetDamage(definition.ContactDamage);
            }
        }

        private void FixedUpdate()
        {
            if (target == null || definition == null)
            {
                return;
            }

            Vector2 direction = ((Vector2)target.position - rb.position).normalized;
            rb.MovePosition(rb.position + direction * definition.MoveSpeed * Time.fixedDeltaTime);
        }

        private void HandleDied()
        {
            KillCounter.Instance?.RegisterKill();
            SpawnExperienceGem();

            if (definition != null && definition.GoldReward > 0)
            {
                MetaProgressionManager.Instance?.AddGold(definition.GoldReward);
            }

            if (sourcePool != null)
            {
                sourcePool.Release(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void SpawnExperienceGem()
        {
            if (definition == null || definition.ExperienceGemPrefab == null)
            {
                return;
            }

            var gemPool = ObjectPool.GetOrCreate(definition.ExperienceGemPrefab);
            if (gemPool == null)
            {
                return;
            }

            var instance = gemPool.Get(transform.position, Quaternion.identity);
            if (instance != null && instance.TryGetComponent<ExperienceGem>(out var gem))
            {
                gem.Initialize(gemPool);
            }
        }
    }
}
