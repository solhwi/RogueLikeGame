using UnityEngine;

namespace RogueLike.Combat
{
    public interface IDamageable
    {
        void TakeDamage(int amount, Vector2 sourcePosition);
    }
}
