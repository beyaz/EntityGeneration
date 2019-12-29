using System;
using System.Collections.Generic;

namespace BOA.Common.Helpers
{
    public static class DictionaryExtensions
    {
        #region Public Methods
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                return;
            }

            dictionary.Add(key, value);
        }
        #endregion
    }
}