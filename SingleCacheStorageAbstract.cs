using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProcessService.utils.localCache
{
    public abstract class SingleCacheStorageAbstract<T>
    {

        private Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        protected bool doNotPenetration;
        protected int cacheTimeLimitSecond;
        protected int nullValueCacheTimeLimitSecond;


        protected SingleCacheStorageAbstract()
        {
            this.doNotPenetration = true; // Convert.ToBoolean(this.config.AppSettings.Settings["do-not-penetration"].Value);
            this.cacheTimeLimitSecond = 7200; // Convert.ToInt32(config.AppSettings.Settings["cache-time-limit"].Value);
            this.nullValueCacheTimeLimitSecond = 300; // Convert.ToInt32(config.AppSettings.Settings["do-not-penetration-cache-time-limit"].Value);

            this.DataSource = this.Init();
        }

        /// <summary>
        /// 初始化kv数据集合的抽象方法
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<string, CacheNode<T>> Init();

        protected Dictionary<string, CacheNode<T>> DataSource { get; set; }

        /// <summary>
        /// 从缓存获取，缓存查不到就穿透到库查，查出来再更新缓存并返回，查不出来返回null，如果开启缓存穿透则null也会被写进缓存
        /// </summary>
        /// <param name="getKeyFunc">获取key的闭包</param>
        /// <param name="func">从库取数据的闭包</param>
        /// <returns></returns>
        // public T GetData(string key, Func<T> func)
        public T GetData(Func<string> getKeyFunc, Func<T> func)
        {
            try
            {
                string key = getKeyFunc();
                CacheNode<T> node = new CacheNode<T>();
                bool flag = this.DataSource.TryGetValue(key, out node);

                CachedStatusEnum cacheStatus = this.CachedStatue(flag, node);
                switch (cacheStatus)
                {
                    case CachedStatusEnum.Cached:
                        {
                            // 命中缓存
                            return node.CacheData;
                        }
                    case CachedStatusEnum.Expired: // 缓存已过期
                    case CachedStatusEnum.Uncached: // 不在缓存中
                    default:
                        {
                            T t = func();
                            bool setFlag = this.SetData(key, t);
                            return t;
                        }
                }
            }
            catch (Exception e)
            {
                Log.Error("GetData Error:" + e.Message);
                throw e;
            }

        }

        public List<T> GetAllData()
        {
            List<T> list = this.DataSource.Values.ToList()
                .FindAll(i => i.ExpireTime <= DateTime.Now)
                .Select(i => i.CacheData)
                .ToList();

            return list;
        }

        private bool SetData(string key, T t)
        {
            if (t == null && !this.doNotPenetration)
            {
                // 未开启防穿透的时候，null不写入缓存
                return false;
            }

            int seconds = this.doNotPenetration && t == null ? this.nullValueCacheTimeLimitSecond : this.cacheTimeLimitSecond;

            return this.SetData(key, t, seconds);
        }

        private bool SetData(string key, T t, int seconds)
        {
            this.DataSource[key] = new CacheNode<T>(t, seconds);
            return true;
        }

        /// <summary>
        /// 判断缓存状态
        /// </summary>
        /// <param name="isCached"></param>
        /// <param name="cacheData"></param>
        /// <returns></returns>
        private CachedStatusEnum CachedStatue(bool isCached, CacheNodeParent cacheData)
        {
            if (!isCached)
            {
                return CachedStatusEnum.Uncached;
            }
            if (cacheData.ExpireTime > DateTime.Now)
            {
                return CachedStatusEnum.Expired;
            }
            return CachedStatusEnum.Cached;
        }

    }

}
