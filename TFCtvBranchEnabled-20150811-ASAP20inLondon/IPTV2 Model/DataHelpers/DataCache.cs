using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// using Soss.Client;
using Microsoft.ApplicationServer.Caching;

namespace IPTV2_Model
{
    //static class DataCache
    //public class DataCache
    //{
    //    // public static CreatePolicy CacheCreatePolicy { get; set; }
    //    public static Microsoft.ApplicationServer.Caching.DataCache Cache { get; set; }

    //    public static TimeSpan CacheDuration { get; set; }

    //    static DataCache()
    //    {
    //        //DataCacheFactoryConfiguration config = new DataCacheFactoryConfiguration();
    //        //config.IsCompressionEnabled = true;
    //        //config.MaxConnectionsToServer = 1;

    //        //DataCacheFactory cacheFactory = new DataCacheFactory(config);
    //        DataCacheFactory cacheFactory = new DataCacheFactory();
    //        var cache = cacheFactory.GetDefaultCache();

    //        //cacheCreatePolicy.TimeoutMinutes = 5;
    //        //cacheCreatePolicy.IsAbsoluteTimeout = true;

    //        //cache.DefaultCreatePolicy = cacheCreatePolicy;

    //        Cache = cache;
    //        // CacheCreatePolicy = cacheCreatePolicy;
    //        CacheDuration = new TimeSpan(0, 10, 0);
    //    }

    //    public static DataCacheItemVersion Add(string key, object value)
    //    {
    //        return Cache.Add(key, value, CacheDuration);
    //    }

    //    public static DataCacheItemVersion Add(string key, object value, TimeSpan cacheDuration)
    //    {
    //        return Cache.Add(key, value, cacheDuration);
    //    }

    //    public static DataCacheItemVersion Put(string key, object value)
    //    {
    //        return Cache.Put(key, value, CacheDuration);
    //    }

    //    public static DataCacheItemVersion Put(string key, object value, TimeSpan cacheDuration)
    //    {
    //        return Cache.Put(key, value, cacheDuration);
    //    }
    //}


    //static class DataCache
    public class DataCache
    {
        public static TimeSpan CacheDuration { get; set; }
        private static DataCacheFactory _factory;
        private static Microsoft.ApplicationServer.Caching.DataCache _cache;

        public static DataCacheFactory cacheFactory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = new DataCacheFactory();
                }

                return _factory;
            }
        }

        public static Microsoft.ApplicationServer.Caching.DataCache Cache
        {
            get
            {
                if (_cache == null)
                {
                    _cache = cacheFactory.GetDefaultCache();
                }

                return _cache;
            }
        }

        static DataCache()
        {
            CacheDuration = new TimeSpan(0, 10, 0);
        }

        public static DataCacheItemVersion Add(string key, object value)
        {
            return Cache.Add(key, value, CacheDuration);
        }

        public static DataCacheItemVersion Add(string key, object value, TimeSpan cacheDuration)
        {
            return Cache.Add(key, value, cacheDuration);
        }

        public static DataCacheItemVersion Put(string key, object value)
        {
            return Cache.Put(key, value, CacheDuration);
        }

        public static DataCacheItemVersion Put(string key, object value, TimeSpan cacheDuration)
        {
            return Cache.Put(key, value, cacheDuration);
        }

        public static void Refresh()
        {
            try
            {
                var factory = _factory;
                if (factory != null)
                {
                    factory.Dispose();
                    _factory = null;
                }

                _cache = null;
                // Clear DataCacheFactory._connectionPool
                var coreAssembly = typeof(DataCacheItem).Assembly;
                var simpleSendReceiveModulePoolType = coreAssembly.GetType("Microsoft.ApplicationServer.Caching.SimpleSendReceiveModulePool", throwOnError: true);
                var connectionPoolField = typeof(DataCacheFactory).GetField("_connectionPool", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                connectionPoolField.SetValue(null, Activator.CreateInstance(simpleSendReceiveModulePoolType));
            }
            catch (Exception) { }
        }
    }
}