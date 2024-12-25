using UnityEngine;

namespace Venwin.ObjectPool
{
    /// <summary>
    /// Denotes that an entity should be aware of what ObjectPool it belongs to.<br/>
    /// Most obvious use case is to allow temporary objects to quickly put themselves back into a pool without searching for the relevant pools.
    /// </summary>
    /// <typeparam name="T">The type of pool that can manage this object. Should just be the concrete type implementing this interface.</typeparam>
    public interface IIsPooledEntity<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Gets or sets the <see cref="ObjectPool"/> for the pooled entity.
        /// </summary>
        public ObjectPoolMonoBehavior<T> ObjectPool { get; set; }
    }
}
