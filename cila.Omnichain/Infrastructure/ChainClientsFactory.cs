using cila.Omnichain.Infrastructure;
using Cila;

namespace cila
{
    public class ChainClientsFactory 
    {
        private readonly ChainsService chainsService;

        public ChainClientsFactory(ChainsService chainsService)
        {
            this.chainsService = chainsService;
        }

        public IChainClient GetChainClient(string chainId)
        {
            var chain = chainsService.Get(chainId);
            return new EthChainClient(chain.RPC,chain.CQRSContract, chain.PrivateKey);
        }
    }
}