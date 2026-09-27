using UnityEngine;

namespace RogueLike.Combat
{
    /// <summary>
    /// Attach to spikes, lava, enemy hitboxes, etc. Damages anything
    /// implementing IDamageable on contact.
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        [SerializeField] private int damage = 1;

        public void SetDamage(int amount)
        {
            damage = amount;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryDamage(collision.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryDamage(other);
        }

        private void TryDamage(Component other)
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage, transform.position);
            }
        }
    }
}
