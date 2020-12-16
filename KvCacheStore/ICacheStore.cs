using System;
using System.Collections.Generic;
using System.Text;

namespace KvCacheStore
{
    public interface ICacheStore
    {
        void Set<T>(string key, T value);

        T Get<T>(string key);
    }
}
