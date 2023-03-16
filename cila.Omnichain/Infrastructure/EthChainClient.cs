
using Nethereum.Web3;
using Nethereum.Contracts;

namespace cila.Omnichain.Infrastructure
{

    public class EthChainClient : IChainClient
    {
        private Web3 _web3;
        private Contract _contract;

        public EthChainClient(string rpc, string contract, string abi)
        {
            _web3 = new Web3(rpc);
            _contract = _web3.Eth.GetContract(abi, contract);
        }

        async Task Send(OmnichainOperation op)
        {
            var dispatchOperationHandler = _contract.GetFunction<OmnichainOperation>();
            var result = await dispatchOperationHandler.CallAsync(op);
        }

        void IChainClient.Send(OmnichainOperation op)
        {
            Send(op).GetAwaiter().GetResult();
        }
    }
}