using UnityEngine;
using RogueLike.Managers;
using RogueLike.Utility;

namespace RogueLike.Player
{
    public class ExperienceGem : MonoBehaviour
    {
        [SerializeField] private int experienceValue = 1;
        [SerializeField] private float attractSpeed = 12f;

        private Transform attractTarget;
        private ObjectPool sourcePool;

        public void Initialize(ObjectPool pool)
        {
            sourcePool = pool;
            attractTarget = null;
        }

        public void Attract(Transform target)
        {
            attractTarget = target;
        }

        private void Update()
        {
            if (attractTarget == null)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, attractTarget.position, attractSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, attractTarget.position) < 0.15f)
            {
                Collect();
            }
        }

        private void Collect()
        {
            ExperienceManager.Instance?.AddExperience(experienceValue);

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
