using UnityEngine;

namespace RogueLike.Combat
{
    public static class AutoTargeting
    {
        public static Transform FindNearestEnemy(Vector2 origin, float range, LayerMask enemyLayer)
        {
            var hits = Physics2D.OverlapCircleAll(origin, range, enemyLayer);

            Transform nearest = null;
            float nearestDistanceSqr = float.MaxValue;

            foreach (var hit in hits)
            {
                float distanceSqr = ((Vector2)hit.transform.position - origin).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearest = hit.transform;
                }
            }

            return nearest;
        }
    }
}
