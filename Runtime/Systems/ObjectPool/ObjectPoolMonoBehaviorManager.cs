using System;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

#nullable enable

namespace Venwin.ObjectPool
{
    /// <summary>
    /// Manages a group of ObjectPools and creates a proper heirarchy for all the pooled objects in the game.
    /// </summary>
    public class ObjectPoolMonoBehaviorManager : MonoBehaviour
    {
        private static ObjectPoolMonoBehaviorManager instance;
        public static ObjectPoolMonoBehaviorManager? Instance
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
            private set
            {
                if (value == null) { throw new ArgumentException($"Cannot set {nameof(ObjectPoolMonoBehaviorManager)}.{Instance} to a null value!"); }
                instance = value;
            }
        }

        private readonly static List<PoolLookup> poolData = new();

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
        /// <typeparam name="T">Type of object managed by the object pool.</typeparam>
        /// <param name="name">Name of the game object pool.</param>
        /// <param name="pooledObject">The object being created in the pool.</param>
        /// <param name="poolSize">Number of elements in the pool.</param>
        /// <returns>The created or found pool.</returns>
        public static ObjectPoolMonoBehavior<T> CreatePool<T>(string name, T pooledObject, int poolSize = 10) where T : MonoBehaviour
        {
            return CreatePool(name, pooledObject, poolSize, null);
        }

        /// <summary>
        /// Creates a pool and a Game Object to store all the pooled objects neatly.
        /// </summary>
        /// <typeparam name="T">Type of object managed by the object pool.</typeparam>
        /// <param name="name">Name of the game object pool.</param>
        /// <param name="pooledObject">The object being created in the pool.</param>
        /// <param name="poolSize">Number of elements in the pool.</param>
        /// <param name="instanceCreatedCallback">Callback function for each created instance of <typeparamref name="T"/>.</param>
        /// <returns>The created or found pool.</returns>
        public static ObjectPoolMonoBehavior<T> CreatePool<T>(string name, T pooledObject, int poolSize = 10, Action<T>? instanceCreatedCallback = null) where T : MonoBehaviour
        {
            PoolLookup lookup = poolData.Find(d => d.Name == name);

            if (lookup == null)
            {
                GameObject objectPool = new(name);
                objectPool.transform.parent = Instance!.transform;

                ObjectPoolMonoBehavior<T> pool = new(pooledObject, poolSize);
                pool.CreateInitialPool(objectPool.transform, instanceCreatedCallback);

                lookup = new PoolLookup(name, objectPool, pool);
                poolData.Add(lookup);
                return pool;
            }
            else
            {
                Debug.LogWarning($"There is already a pool named {name}");
                return (ObjectPoolMonoBehavior<T>)lookup.Pool;
            }
        }

        /// <summary>
        /// Tries to find the object pool by name. Type must also match.
        /// </summary>
        /// <typeparam name="T">Type managed by pool.</typeparam>
        /// <param name="name">Name of the pool.</param>
        /// <param name="pool">The found pool, else null.</param>
        /// <returns>True if it could find the pool that the name and matches type, else false.</returns>
        public static bool TryFindPool<T>(string name, out ObjectPoolMonoBehavior<T>? pool) where T : MonoBehaviour
        {
            pool = null;
            PoolLookup lookup = poolData.Find(d => d.Name == name);

            if(lookup == null) { return false; }

            try
            {
                pool = (ObjectPoolMonoBehavior<T>)lookup.Pool;
            }
            catch
            {
                Debug.LogError($"{nameof(ObjectPoolMonoBehaviorManager)} found a pool with the name `{name}` but couldn't successfully cast it to type {typeof(T).Name}! Returning false.");
                return false;
            }

            return true;
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
            public IObjectPoolMonoBehavior Pool;

            public PoolLookup(string name, GameObject poolObject, IObjectPoolMonoBehavior pool)
            {
                Name = name;
                PoolObject = poolObject;
                Pool = pool;
            }
        }
    }

}
