using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Enyim.Caching;
using Enyim.Caching.Memcached;

namespace EFCachingProvider.Tests
{
    [TestClass]
    public class MemcachedTests
    {

        [TestMethod]
        public void dd()
        {
            var mc = new MemcachedClient();

            for (var i = 0; i < 100; i++)
                mc.Store(StoreMode.Set, "Hello", "World");

            mc.FlushAll();
        }

    }
}
