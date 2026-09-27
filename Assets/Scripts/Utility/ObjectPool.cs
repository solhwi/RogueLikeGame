using System.Collections.Generic;
using UnityEngine;

namespace RogueLike.Utility
{
    /// <summary>
    /// Simple prefab pool for frequently spawned objects (projectiles,
    /// particle bursts, pickups) to avoid Instantiate/Destroy churn.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;
        [SerializeField] private bool expandable = true;

        private readonly Queue<GameObject> pool = new Queue<GameObject>();
        private bool initialized;

        private static readonly Dictionary<GameObject, ObjectPool> runtimePools = new Dictionary<GameObject, ObjectPool>();

        private void Awake()
        {
            if (prefab != null)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (initialized || prefab == null)
            {
                return;
            }
            initialized = true;

            for (int i = 0; i < initialSize; i++)
            {
                pool.Enqueue(CreateInstance());
            }
        }

        // Lazily creates (or reuses) a pool for a prefab that isn't wired up
        // in the scene ahead of time, e.g. a skill's projectile chosen at
        // runtime or an enemy prefab referenced by data.
        public static ObjectPool GetOrCreate(GameObject prefab, int initialSize = 10)
        {
            if (prefab == null)
            {
                return null;
            }
            if (runtimePools.TryGetValue(prefab, out var existing) && existing != null)
            {
                return existing;
            }

            var host = new GameObject($"Pool_{prefab.name}");
            var pool = host.AddComponent<ObjectPool>();
            pool.prefab = prefab;
            pool.initialSize = initialSize;
            pool.Initialize();
            runtimePools[prefab] = pool;
            return pool;
        }

        private GameObject CreateInstance()
        {
            GameObject instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            return instance;
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject instance = pool.Count > 0 ? pool.Dequeue() : (expandable ? CreateInstance() : null);
            if (instance == null)
            {
                return null;
            }

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }

        public void Release(GameObject instance)
        {
            instance.SetActive(false);
            instance.transform.SetParent(transform);
            pool.Enqueue(instance);
        }
    }
}
