
using Nethereum.Web3;
using Nethereum.Contracts;
using System.Text;
using Nethereum.Contracts.ContractHandlers;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using System.ComponentModel;
using Nethereum.ABI;
using Nethereum.Contracts.QueryHandlers;
using Google.Protobuf;

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

        public async Task<ChainResponse> SendAsync(Operation op)
        {
            if (op == null)
                await Task.FromResult(true);

            var function = _contract.GetFunction<DispatchFunction>();

            var abi = new ABIEncode();

            var opBytes = op.ToByteArray();
            
            // TODO
            var req = new DispatchFunction
            {
                OpBytes = opBytes
            };

            //req.FromAddress = "0xE56AEaFD75c5cB891813f6A117FAFD24F7FD979A";
            //req.FromAddress = "0x0E8AB7131548af0D9798375B1cc9B5d06322bD60";
            req.FromAddress = _account.Address;

            var _queryHandler = _web3.Eth.GetContractQueryHandler<DispatchFunction>();
            var txHandler = _web3.Eth.GetContractTransactionHandler<DispatchFunction>();
            var gasEstimate = await txHandler.EstimateGasAsync(_contract.ContractAddress, req);
            req.Gas = gasEstimate.Value;

            var receipt = await txHandler.SendRequestAndWaitForReceiptAsync(_contract.ContractAddress, req);
            return new ChainResponse {
                ContractAddress = receipt.ContractAddress,
                EffectiveGasPrice = receipt.EffectiveGasPrice.ToUlong(),
                GasUsed = receipt.GasUsed.ToUlong(),
                CumulativeGasUsed = receipt.CumulativeGasUsed.ToUlong(),
                BlockHash = receipt.BlockHash,
                BlockNumber = receipt.BlockNumber.ToUlong(),
                Logs = receipt.Logs.ToString()
            };
            //var r = await _queryHandler.QueryAsync<int>(_contract.ContractAddress, req);
            //var res = await function.CallAsync<string>(req, from: _account.Address, new HexBigInteger(300000), new HexBigInteger(0));
        }


    }

    public class ChainResponse
    {
        public string ContractAddress { get; set; }
        public ulong EffectiveGasPrice { get; set; }
        public ulong GasUsed { get; set; }
        public ulong CumulativeGasUsed { get; set; }
        public ulong BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public ulong TransactionIndex { get; set; }
        public string TransactionHash { get; set; }
        public string Logs { get; set; }
    }
}