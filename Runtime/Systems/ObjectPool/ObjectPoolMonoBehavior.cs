using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Venwin.ObjectPool
{
    /// <summary>
    /// Object Pool that works with MonoBehaviors. Checks if a GameObject has an <see cref="IIsPooledEntity"/> so that it can be aware of what pool it belongs to.
    /// </summary>
    public class ObjectPoolMonoBehavior
    {
        public GameObject Prefab { get; }
        public int PoolSize { get; }

        private Queue<GameObject> pool;
        private Vector3 spawnLocation;
        private Quaternion spawnRotation;

        public ObjectPoolMonoBehavior(GameObject pooledObject, int poolSize = 10)
        {
            Prefab = pooledObject;
            PoolSize = poolSize;
        }

        /// <summary>
        /// Creates the initial object pool.
        /// </summary>
        /// <remarks>
        /// If <see cref="GameObject"/> implements <see cref="IIsPooledEntity"/> then it registers itself as the pool for that entity.<br/>
        /// Note that it grabs the first script that implements that interface.
        /// </remarks>
        /// <param name="createdObjectCallback">Called if there is addtional set up required before the object calls <code>SetActive(false)</code></param>
        public void CreateInitialPool(Transform parentPoolObject, Action<GameObject> createdObjectCallback = null)
        {
            pool = new Queue<GameObject>();
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject obj = Object.Instantiate(Prefab, spawnLocation, Quaternion.identity, parentPoolObject);
                createdObjectCallback?.Invoke(obj);

                IIsPooledEntity pooledEnt = obj.GetComponent<IIsPooledEntity>();
                pooledEnt.ObjectPool = this;

                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Gets a pooled object if any are available. Otherwise creates a new one.
        /// </summary>
        /// <param name="spawnLocation">Where to spawn the object.</param>
        /// <param name="spawnRotation">What orientation to spawn the object.</param>
        /// <returns>The re-enabled game object at <paramref name="spawnLocation"/> with a <paramref name="spawnRotation"/>.</returns>
        public GameObject GetPooledObject(Vector3 spawnLocation, Quaternion spawnRotation)
        {
            // Return a pooled object if available
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.transform.SetPositionAndRotation(spawnLocation, spawnRotation);
                obj.SetActive(true); // Activate the object
                return obj;
            }
            else
            {
                GameObject obj = Object.Instantiate(Prefab, spawnLocation, spawnRotation);
                return obj;
            }
        }

        /// <summary>
        /// Returns a game object back to the pool.
        /// </summary>
        /// <remarks>
        /// Please note that there are no checks done on if the object was originally in this pool. Anything can be put back in.<br/>
        /// Implementors should be careful to ensure that incorrect objects are not added.
        /// </remarks>
        /// <param name="obj">The Gameobject to put back in this pool.</param>
        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetPositionAndRotation(spawnLocation, spawnRotation);
            pool.Enqueue(obj);
        }
    }
}
