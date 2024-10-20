namespace Venwin.ObjectPool
{
    /// <summary>
    /// Denotes that an entity should be aware of what ObjectPool it belongs to.<br/>
    /// Most obvious use case is to allow temporary objects to quickly put themselves back into a pool without searching for the relevant pools.
    /// </summary>
    public interface IIsPooledEntity
    {
        /// <summary>
        /// Gets or sets the <see cref="ObjectPool"/> for the pooled entity.
        /// </summary>
        public ObjectPoolMonoBehavior ObjectPool { get; set; }
    }
}
