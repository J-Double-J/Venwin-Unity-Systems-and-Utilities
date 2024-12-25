using System.Collections.Generic;

namespace Venwin.Collections
{
    public class ReadOnlyQueue<T>
    {
        private readonly Queue<T> _queue;

        public ReadOnlyQueue(Queue<T> queue)
        {
            _queue = queue;
        }

        /// <summary>
        /// Gets the count of the queue.
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Allows viewing the front item without removal.
        /// </summary>
        /// <returns>The front item.</returns>
        public T Peek() => _queue.Peek();

        /// <summary>
        /// Read only enumeration.
        /// </summary>
        /// <returns>Enumeration of the queue items.</returns>
        public IEnumerable<T> AsEnumerable() => _queue;
    }
}
