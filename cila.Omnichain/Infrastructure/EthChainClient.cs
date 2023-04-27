
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
using System.Numerics;

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

            var opBytes = op.ToByteArray();
            
            var req = new DispatchFunction
            {
                OpBytes = opBytes
            };

            req.FromAddress = _account.Address;

            var _queryHandler = _web3.Eth.GetContractQueryHandler<DispatchFunction>();
            var txHandler = _web3.Eth.GetContractTransactionHandler<DispatchFunction>();

            var gasEstimate = await txHandler.EstimateGasAsync(_contract.ContractAddress, req);
            req.Gas = new BigInteger(2) * gasEstimate;

            var gasPrice = _web3.Eth.GasPrice.SendRequestAsync().GetAwaiter().GetResult();
            req.GasPrice = new BigInteger(2) * gasPrice;

            var tx = await txHandler.SendRequestAsync(_contract.ContractAddress, req);
            TransactionReceipt receipt = null; // await txHandler.SendRequestAndWaitForReceiptAsync(_contract.ContractAddress, req);
            return new ChainResponse {
                ChainId = _web3.Eth.ChainId.SendRequestAsync().GetAwaiter().GetResult().ToString(),
                ContractAddress = receipt?.ContractAddress ?? _contract.ContractAddress,
                EffectiveGasPrice = receipt?.EffectiveGasPrice.ToUlong() ?? (ulong)req.GasPrice,
                GasUsed = receipt?.GasUsed.ToUlong() ?? (ulong)req.Gas,
                CumulativeGasUsed = receipt?.CumulativeGasUsed.ToUlong() ?? (ulong)req.Gas,
                BlockHash = receipt?.BlockHash ?? "Unknown",
                BlockNumber = receipt?.BlockNumber.ToUlong() ?? _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync().GetAwaiter().GetResult().ToUlong(),
                Logs = receipt?.Logs.ToString() ?? string.Empty,
                TransactionHash = receipt?.TransactionHash ?? tx,
                TransactionIndex = receipt?.TransactionIndex.ToUlong() ?? 0
            };
        }


    }

    public class ChainResponse
    {
        public string ChainId { get; set; }
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