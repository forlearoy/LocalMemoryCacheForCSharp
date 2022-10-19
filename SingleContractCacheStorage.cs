using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectProcessService.utils.localCache
{
    /// <summary>
    /// 缓存单例demo
    /// 虚拟业务为 通过项目编号和令牌获取合同
    /// Contract和ContractService类只是作为demo展示，实际业务需要自己实现
    /// </summary>
    /// 
    public sealed class SingleContractCacheStorage : SingleCacheStorageAbstract<Contract>
    {

        private static readonly SingleContractCacheStorage instance = new SingleContractCacheStorage();

        /*
        private SingleContractCacheStorage() : base()
        {

        }
        */

        public static SingleContractCacheStorage GetInstance
        {
            get
            {
                return instance;
            }
        }

        protected override Dictionary<string, CacheNode<Contract>> Init()
        {
            List<Contract> contracts = new ContractBLL().GetAll();
            Dictionary<string, CacheNode<Contract>> dic = new Dictionary<string, CacheNode<Contract>>();
            contracts.ForEach(contract =>
            {
                string key = this.BuildKey(contract.ProjectCode, contract.Token);
                dic.Remove(key);
                dic.Add(key, new CacheNode<Contract>(contract, this.cacheTimeLimitSecond));
            });

            return dic;
        }

        /// <summary>
        /// 从缓存获取合同，缓存查不到就穿透到库查，查出来再更新缓存并返回合同，查不出来返回null，如果开启缓存穿透则null也会被写进缓存
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Contract GetData(string projectCode, string token)
        {
            Contract contract = base.GetData(() => this.BuildKey(projectCode, token), () => this.GetFromDb(projectCode, token));
            return contract;
        }

        private string BuildKey(string projectCode, string token)
        {
            string key = string.Format("{0}|{1}", projectCode, token);
            return key;
        }

        /// <summary>
        /// 从库中拿合同
        /// </summary>
        /// <param name="projectCode"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private Contract GetFromDb(string projectCode, string token)
        {
            return new ContractService().GetContract(projectCode, token);
        }

    }


}
