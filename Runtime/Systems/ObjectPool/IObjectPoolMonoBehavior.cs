using System;
using UnityEngine;

namespace Venwin.ObjectPool
{
    /// <summary>
    /// Base interface for ObjectPools.
    /// </summary>
    /// <remarks>
    /// Not very functional but allows a common type across generics of <see cref="ObjectPoolMonoBehavior{T}"/>.
    /// </remarks>
    internal interface IObjectPoolMonoBehavior
    {
        int PoolSize { get; }
    }
}