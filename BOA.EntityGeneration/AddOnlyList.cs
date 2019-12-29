using System;
using System.Collections;
using System.Collections.Generic;

namespace BOA.Collections
{
    /// <summary>
    ///     The add only list
    /// </summary>
    [Serializable]
    public class AddOnlyList<T> : IReadOnlyList<T>
    {
        #region Fields
        /// <summary>
        ///     The items
        /// </summary>
        readonly List<T> items = new List<T>();
        #endregion

        #region Public Properties
        /// <summary>
        ///     Gets the count.
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }
        #endregion

        #region Public Indexers
        /// <summary>
        ///     Gets the the specified index.
        /// </summary>
        public T this[int index]
        {
            get { return items[index]; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Adds the specified value.
        /// </summary>
        public void Add(T value)
        {
            items.Add(value);
        }

        /// <summary>
        ///     Adds the range.
        /// </summary>
        public void AddRange(IEnumerable<T> collection)
        {
            items.AddRange(collection);
        }

        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }
        #endregion

        #region Explicit Interface Methods
        /// <summary>
        ///     Gets the enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) items).GetEnumerator();
        }
        #endregion
    }
}