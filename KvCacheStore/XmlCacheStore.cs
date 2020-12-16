using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace KvCacheStore
{
    public class XmlCacheStore : ICacheStore
    {
        public string XmlPath { get; set; }

        XmlDocument doc = new XmlDocument();

        private Dictionary<string, string> _kvDic = new Dictionary<string, string>(8);

        public XmlCacheStore() : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CacheStore.xml"))
        {

        }

        public XmlCacheStore(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            try
            {
                doc.Load(path);

                var cacheStore = doc.SelectSingleNode("CacheStore");

                var caches = cacheStore.SelectNodes("Cache");

                foreach (XmlNode cache in caches)
                {
                    this._kvDic.Add(cache.Attributes["key"].Value, cache.Attributes["value"].Value);
                }
            }
            catch (Exception ex)
            {
                var cacheStore = doc.CreateElement("CacheStore");

                doc.AppendChild(cacheStore);
            }

            this.XmlPath = path;
        }

        public T Get<T>(string key)
        {
            if (this._kvDic.TryGetValue(key, out var value))
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(value);
                }
                catch (Exception ex)
                {
                    throw new InvalidCastException($"Cannot convert value to {typeof(T).FullName}");
                }
            }

            return default(T);
        }

        public void Set<T>(string key, T t)
        {
            string value = JsonConvert.SerializeObject(t);

            this._kvDic[key] = value;

            var cacheStore = doc.SelectSingleNode("CacheStore");

            if (cacheStore == null)
            {
                cacheStore = doc.CreateElement("CacheStore");

                doc.AppendChild(cacheStore);
            }

            var cache = cacheStore.SelectSingleNode($"Cache[@key=\"{key}\"]");

            if (cache == null)
            {
                var newCache = doc.CreateElement("Cache");

                newCache.SetAttribute("key", key);

                newCache.SetAttribute("value", value);

                cacheStore.AppendChild(newCache);
            }
            else
            {
                cache.Attributes["key"].Value = key;

                cache.Attributes["value"].Value = value;
            }

            doc.Save(this.XmlPath);
        }
    }
}
