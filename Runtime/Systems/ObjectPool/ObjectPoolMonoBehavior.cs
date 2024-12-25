using System;
using System.Collections.Generic;
using UnityEngine;
using Venwin.Collections;
using Object = UnityEngine.Object;

#nullable enable

namespace Venwin.ObjectPool
{
    /// <summary>
    /// Object Pool that works with <see cref="MonoBehaviour"/>s. Checks if <typeparamref name="T"/> is a <see cref="IIsPooledEntity"/> so that it can be aware of what pool it belongs to.
    /// </summary>
    public class ObjectPoolMonoBehavior<T> : IObjectPoolMonoBehavior where T : MonoBehaviour
    {
        public T Prefab { get; }
        public int PoolSize { get; }

        /// <summary>
        /// Gets called whenever an object is tried to be retrieved from the pool and none are available.
        /// </summary>
        /// <remarks>
        /// If there is additional setup that needs to be done on new pooled objects beyond the prefab it can done with this action.<br/>
        /// <strong>Note:</strong> this is not called when creating the initial pool. There is an appropriate parameter for that.
        /// </remarks>
        public Action<T>? OnGetPooledObjectCreation = null;

        private Queue<T> _pool = new();
        private Vector3 spawnLocation;
        private Quaternion spawnRotation;

        public ReadOnlyQueue<T> Pool { get; private set; }

        public ObjectPoolMonoBehavior(T pooledObject, int poolSize = 10)
        {
            Prefab = pooledObject;
            PoolSize = poolSize;
            Pool = new ReadOnlyQueue<T>(_pool);
        }

        /// <summary>
        /// Creates the initial object pool.
        /// </summary>
        /// <remarks>
        /// If <typeparamref name="T"/> implements <see cref="IIsPooledEntity"/> then this pool registers itself as the owning pool for that entity.<br/>
        /// Note that it grabs the first script that implements that interface.
        /// </remarks>
        /// <param name="parentPoolObject">The parent of this pool object for organized heirarchy purposes</param>
        /// <param name="createdObjectCallback">Called if there is addtional set up required before the object calls <code>SetActive(false)</code></param>
        public void CreateInitialPool(Transform parentPoolObject, Action<T>? createdObjectCallback = null)
        {
            for (int i = 0; i < PoolSize; i++)
            {
                T obj = Object.Instantiate(Prefab.gameObject, spawnLocation, Quaternion.identity, parentPoolObject).GetComponent<T>();
                createdObjectCallback?.Invoke(obj);

                IIsPooledEntity<T> pooledEnt = obj.GetComponent<IIsPooledEntity<T>>();
                pooledEnt.ObjectPool = this;

                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Gets a pooled object if any are available. Otherwise creates a new one.
        /// </summary>
        /// <param name="spawnLocation">Where to spawn the object.</param>
        /// <param name="spawnRotation">What orientation to spawn the object.</param>
        /// <returns>The re-enabled monobehavior at <paramref name="spawnLocation"/> with a <paramref name="spawnRotation"/>.</returns>
        public T GetPooledObject(Vector3 spawnLocation, Quaternion spawnRotation)
        {
            // Return a pooled object if available
            if (_pool.Count > 0)
            {
                T obj = _pool.Dequeue();
                obj.transform.SetPositionAndRotation(spawnLocation, spawnRotation);
                obj.gameObject.SetActive(true); // Activate the object
                return obj;
            }
            else
            {
                T obj = Object.Instantiate(Prefab, spawnLocation, spawnRotation);
                OnGetPooledObjectCreation?.Invoke(obj);
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
        /// <param name="obj">The monobehavior to put back in this pool.</param>
        public void ReturnToPool(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetPositionAndRotation(spawnLocation, spawnRotation);
            _pool.Enqueue(obj);
        }
    }
}
