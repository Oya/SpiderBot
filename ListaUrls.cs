using System;
using System.Threading;
using System.Collections.Generic;
namespace Robotinic_2._0
{
    public class ListaUrls
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private Dictionary<string, bool> innerCache = new Dictionary<string, bool>();

        public bool Read(string key)
        {
            cacheLock.EnterReadLock();
            try
            {
                return innerCache[key];
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        public bool Contains(string key)
        {
            //cacheLock.EnterReadLock();
            try
            {
                return innerCache.ContainsKey(key);
            }
            finally
            {
                //cacheLock.ExitReadLock();
            }
        }

        public void Add(string key, bool value)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Add(key, value);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public bool AddWithTimeout(string key, bool value, int timeout)
        {
            if (cacheLock.TryEnterWriteLock(timeout))
            {
                try
                {
                    innerCache.Add(key, value);
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public AddOrUpdateStatus AddOrUpdate(string key, bool value)
        {
            cacheLock.EnterUpgradeableReadLock();
            try
            {
                bool result = false;
                if (innerCache.TryGetValue(key, out result))
                {
                    if (result == value)
                    {
                        return AddOrUpdateStatus.Unchanged;
                    }
                    else
                    {
                        cacheLock.EnterWriteLock();
                        try
                        {
                            innerCache[key] = value;
                        }
                        finally
                        {
                            cacheLock.ExitWriteLock();
                        }
                        return AddOrUpdateStatus.Updated;
                    }
                }
                else
                {
                    cacheLock.EnterWriteLock();
                    try
                    {
                        innerCache.Add(key, value);
                    }
                    finally
                    {
                        cacheLock.ExitWriteLock();
                    }
                    return AddOrUpdateStatus.Added;
                }
            }
            finally
            {
                cacheLock.ExitUpgradeableReadLock();
            }
        }

        public void Delete(string key)
        {
            cacheLock.EnterWriteLock();
            try
            {
                innerCache.Remove(key);
            }
            finally
            {
                cacheLock.ExitWriteLock();
            }
        }

        public enum AddOrUpdateStatus
        {
            Added,
            Updated,
            Unchanged
        };

        public string ReadNext()
        {
            cacheLock.EnterReadLock();
            try
            {
                foreach (var item in innerCache)
                {
                    if (item.Value == false)
                    {
                        cacheLock.ExitReadLock();
                        this.AddOrUpdate(item.Key, true);
                        return item.Key;
                    }
                }
                return "";
            }
            finally
            {
                if (cacheLock.IsReadLockHeld)
                    cacheLock.ExitReadLock();
            }
        }

        public int Count()
        {
            return innerCache.Count;
        }
    }
}