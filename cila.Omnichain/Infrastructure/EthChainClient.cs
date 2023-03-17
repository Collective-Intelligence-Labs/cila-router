
using Nethereum.Web3;
using Nethereum.Contracts;
using System.Text;

namespace cila.Omnichain.Infrastructure
{

    public class EthChainClient : IChainClient
    {
        private Web3 _web3;
        public Contract _contract;

        public EthChainClient(string rpc, string contract)
        {
            _web3 = new Web3(rpc);
            _contract = _web3.Eth.GetContract<OmnichainOperation>(contract);
        }

        public EthChainClient(string rpc, string contract, string abi)
        {
            _web3 = new Web3(rpc);
            _contract = _web3.Eth.GetContract(abi, contract);
        }

        public async Task SendAsync(OmnichainOperation op)
        {
            var function = _contract.GetFunction("dispatch");
            //var p = function.ConvertJsonToObjectInputParameters("{\n  \"opBytes\": \"hello\"\n}");


            //var dispatchOperationHandler = _contract.GetFunction<OmnichainOperation>();
            var result = function.CallAsync(op).GetAwaiter().GetResult;

            

            var a = 10;
        }
    }
}