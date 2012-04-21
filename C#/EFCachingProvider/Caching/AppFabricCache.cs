using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationServer.Caching;
using System.Security.Cryptography;

namespace EFCachingProvider.Caching
{
    public class AppFabricCache : ICache
    {
        private DataCache _cache;

        public AppFabricCache(DataCache cache)
        {
            _cache = cache;
        }

        public bool GetItem(string key, out object value)
        {
            key = GetCacheKey(key);
            value = _cache.Get(key);
            return value != null;
        }

        // Creates a hash of the query to store as the key 
        private string GetCacheKey(string query)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(query);
            string hashString = Convert.ToBase64String(MD5.Create().ComputeHash(bytes));
            return hashString;
        }

        public void PutItem(string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan slidingExpiration, DateTime absoluteExpiration)
        {

            key = GetCacheKey(key);
            _cache.Put(key, value, absoluteExpiration - DateTime.Now, dependentEntitySets.Select(c => new DataCacheTag(c)).ToList());

            foreach (var dep in dependentEntitySets)
            {
                CreateRegionIfNeeded(dep);
                _cache.Put(key, "", dep);
            }

        }

        private void CreateRegionIfNeeded(string regionName)
        {
            try
            {
                _cache.CreateRegion(regionName);
            }
            catch (DataCacheException de)
            {
                if (de.ErrorCode != DataCacheErrorCode.RegionAlreadyExists)
                {
                    throw;
                }

            }
        }

        public void InvalidateSets(IEnumerable<string> entitySets)
        {
            // Go through the list of objects in each of the sets.
            foreach (var dep in entitySets)
            {
                foreach (var val in _cache.GetObjectsInRegion(dep))
                {
                    _cache.Remove(val.Key);
                }
            }
        }

        public void InvalidateItem(string key)
        {
            key = GetCacheKey(key);
            DataCacheItem item = _cache.GetCacheItem(key);
            _cache.Remove(key);
            foreach (var tag in item.Tags)
            {
                _cache.Remove(key, tag.ToString());
            }
        }

        private static ICache CreateAppFabricCache()
        {
            var server = new List<DataCacheServerEndpoint>();
            server.Add(new DataCacheServerEndpoint("localhost", 22233));

            var conf = new DataCacheFactoryConfiguration();
            conf.Servers = server;

            DataCacheFactory fac = new DataCacheFactory(conf);
            return new AppFabricCache(fac.GetDefaultCache());

        }

    }
}
