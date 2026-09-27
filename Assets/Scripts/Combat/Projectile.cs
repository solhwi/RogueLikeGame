using UnityEngine;
using RogueLike.Utility;

namespace RogueLike.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float lifetime = 3f;

        private ObjectPool sourcePool;
        private Vector2 direction;
        private float speed;
        private int damage;
        private int pierceRemaining;
        private float lifeTimer;

        public void Launch(Vector2 origin, Vector2 travelDirection, float projectileSpeed, int projectileDamage, int pierceCount, ObjectPool pool)
        {
            transform.position = origin;
            direction = travelDirection.normalized;
            speed = projectileSpeed;
            damage = projectileDamage;
            pierceRemaining = pierceCount;
            sourcePool = pool;
            lifeTimer = lifetime;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Despawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<IDamageable>(out var damageable))
            {
                return;
            }

            damageable.TakeDamage(damage, transform.position);
            pierceRemaining--;
            if (pierceRemaining < 0)
            {
                Despawn();
            }
        }

        private void Despawn()
        {
            if (sourcePool != null)
            {
                sourcePool.Release(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
