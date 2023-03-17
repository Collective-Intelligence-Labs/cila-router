
using Nethereum.Web3;
using Nethereum.Contracts;
using System.Text;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;

namespace cila.Omnichain.Infrastructure
{

    public class EthChainClient : IChainClient
    {
        private Web3 _web3;
        private ContractHandler _contract;
        private Account _account;

        public EthChainClient(string rpc, string contract, string pk)
        {
            _account = new Account(pk);
            _web3 = new Web3(_account, rpc);
            _contract = _web3.Eth.GetContractHandler(contract);
        }

        public async Task SendAsync(OmnichainOperation op)
        {
            var function = _contract.GetFunction<DispatchFunction>();
            var req = new DispatchFunction
            {
                OpBytes = op.ByteData
            };

            var gasEstimate = await _contract.EstimateGasAsync<DispatchFunction>(req);
            var res = await function.CallAsync<string>(req, from: _account.Address, gasEstimate, new HexBigInteger(0));
        }
    }
}