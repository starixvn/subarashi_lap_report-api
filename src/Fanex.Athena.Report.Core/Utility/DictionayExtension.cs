using System;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Utility
{
    public static class DictionayExtension
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue defaultValue)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : defaultValue;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            TKey key,
            Func<TValue> defaultValueProvider)
        {
            var handler = defaultValueProvider != null ? defaultValueProvider.Invoke() : default(TValue);
            return dictionary.GetValueOrDefault(key, handler);
        }
    }
}