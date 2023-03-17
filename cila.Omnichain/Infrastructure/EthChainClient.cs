
using Nethereum.Web3;
using Nethereum.Contracts;
using System.Text;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;

namespace cila.Omnichain.Infrastructure
{

    public class EthChainClient : IChainClient
    {
        private Web3 _web3;
        private ContractHandler _contract;

        public EthChainClient(string rpc, string contract, string pk)
        {
            var account = new Nethereum.Web3.Accounts.Account(pk);
            _web3 = new Web3(account, rpc);
            _contract = _web3.Eth.GetContractHandler(contract);
        }

        public async Task SendAsync(OmnichainOperation op)
        {
            var function = _contract.GetFunction<DispatchFunction>();
            var req = new DispatchFunction
            {
                OpBytes = op.ByteData
            };

            var res = await function.CallAsync<string>(req);
        }
    }
}