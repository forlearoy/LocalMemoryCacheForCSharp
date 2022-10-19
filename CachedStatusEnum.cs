using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProcessService.utils.localCache
{
    enum CachedStatusEnum : int
    {
        // 不在缓存中
        Uncached = 0,
        // 被缓存
        Cached = 1,
        // 缓存已过期
        Expired = 2,
    }
}
