using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Enyim.Caching;
using System.Security.Cryptography;
using Enyim.Caching.Memcached;

namespace EFCachingProvider.Caching
{
    public class MemcachedCache : ICache
    {
        private MemcachedClient _cache;

        public MemcachedCache(MemcachedClient cache)
        {
            _cache = cache;
        }

        public bool GetItem(string key, out object value)
        {
            key = GetCacheKey(key);
            value = _cache.Get(key);
            return value != null;
        }

        public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets,
            TimeSpan slidingExpiration, DateTime absoluteExpiration)
        {
            key = GetCacheKey(key);
            _cache.Store(StoreMode.Set, key, value, absoluteExpiration);

            //依赖未实现
        }

        public void InvalidateSets(IEnumerable<string> entitySets)
        {
            // Go through the list of objects in each of the sets.
            foreach (var dep in entitySets)
            {
                _cache.Remove(dep);
            }
            //依赖未实现
        }

        public void InvalidateItem(string key)
        {
            key = GetCacheKey(key);
            _cache.Remove(key);
        }

        // Creates a hash of the query to store as the key 
        private string GetCacheKey(string query)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(query);
            string hashString = Convert.ToBase64String(MD5.Create().ComputeHash(bytes));
            return hashString;
        }

        public static ICache CreateMemcachedCache()
        {
            var client = new MemcachedClient();
            return new MemcachedCache(client);

        }

    }
}
