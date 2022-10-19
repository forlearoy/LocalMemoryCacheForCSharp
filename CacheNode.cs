using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProcessService.utils.localCache
{
    public class CacheNode<T> : CacheNodeParent
    {

        public CacheNode()
        {

        }

        public CacheNode(T t, int seconds)
        {
            this.CacheData = t;
            this.ExpireTime = DateTime.Now.AddSeconds(seconds);
        }

        /// <summary>
        /// 被缓存的对象
        /// </summary>
        public T CacheData { get; set; }

    }
}
