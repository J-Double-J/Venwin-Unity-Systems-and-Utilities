using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Venwin.ObjectPool
{
    /// <summary>
    /// Manages a group of ObjectPools and creates a proper heirarchy for all the pooled objects in the game.
    /// </summary>
    public class ObjectPoolMonoBehaviorManager : MonoBehaviour
    {
        private static ObjectPoolMonoBehaviorManager instance;
        public static ObjectPoolMonoBehaviorManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning($"There is no instance of {nameof(ObjectPoolMonoBehaviorManager)} found!");
                    return null;
                }
                else
                {
                    return instance;
                }
            }
            private set { instance = value; }
        }

        private static List<PoolLookup> poolData = new();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogError($"There can only be one {nameof(ObjectPoolMonoBehaviorManager)}");
            }
        }

        /// <summary>
        /// Creates a pool and a Game Object to store all the pooled objects neatly.
        /// </summary>
        /// <param name="name">Name of the game object pool.</param>
        /// <param name="pooledObject">The object being created in the pool.</param>
        /// <param name="poolSize">Number of elements in the pool.</param>
        /// <returns>The created or found pool.</returns>
        public static ObjectPoolMonoBehavior CreatePool(string name, GameObject pooledObject, int poolSize = 10)
        {
            PoolLookup lookup = poolData.Find(d => d.Name == name);

            if (lookup == null)
            {
                GameObject objectPool = new GameObject(name);
                objectPool.transform.parent = Instance.transform;

                ObjectPoolMonoBehavior pool = new(pooledObject, poolSize);
                pool.CreateInitialPool(objectPool.transform);

                lookup = new PoolLookup(name, objectPool, pool);
                return pool;
            }
            else
            {
                Debug.LogWarning($"There is already a pool named {name}");
                return lookup.Pool;
            }
        }


        /// <summary>
        /// Destroys the pool and all the descendants.
        /// </summary>
        /// <param name="name">Name of the pool to destroy.</param>
        public static void DestroyPool(string name)
        {
            PoolLookup lookup = poolData.Find(d => d.Name == name);

            if (lookup == null)
            {
                Debug.LogWarning($"There is no pool named {name}");
            }
            else
            {
                Destroy(lookup.PoolObject);
                poolData.Remove(lookup);
            }
        }

        private class PoolLookup
        {
            public string Name;
            public GameObject PoolObject;
            public ObjectPoolMonoBehavior Pool;

            public PoolLookup(string name, GameObject poolObject, ObjectPoolMonoBehavior pool)
            {
                Name = name;
                PoolObject = poolObject;
                Pool = pool;
            }
        }
    }

}
