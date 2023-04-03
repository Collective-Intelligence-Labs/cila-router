
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

        public async Task SendAsync(Operation1 op)
        {
            if (op == null)
                await Task.FromResult(true);

            var function = _contract.GetFunction<DispatchFunction>();

            var abi = new ABIEncode();
            var pbOperation = new Operation()
            {
                RouterId = ByteString.CopyFrom(op.RouterId)
            };

            foreach (var c in op.Commands)
            {
                pbOperation.Commands.Add(new global::Command()
                {
                    AggregateId = ByteString.CopyFrom(c.AggregateId),
                    CmdPayload = ByteString.CopyFrom(c.CmdPayload),
                    CmdSignature = ByteString.CopyFrom(c.CmdSignature),
                    CmdType = (CommandType)c.CmdType
                });
            }


            var opBytes = pbOperation.ToByteArray();
            var bs = BitConverter.ToString(opBytes);

            // TODO
            var req = new DispatchFunction
            {
                OpBytes = opBytes
            };

            req.FromAddress = "0x56df413b990726847Ce8dEd1A77Cce2BC5dE4eDf";
            //req.FromAddress = "0x0E8AB7131548af0D9798375B1cc9B5d06322bD60";

            var _queryHandler = _web3.Eth.GetContractQueryHandler<DispatchFunction>();
            var txHandler = _web3.Eth.GetContractTransactionHandler<DispatchFunction>();
            var gasEstimate = await txHandler.EstimateGasAsync(_contract.ContractAddress, req);
            req.Gas = gasEstimate.Value;

            var res = await txHandler.SendRequestAsync(_contract.ContractAddress, req);
            //var r = await _queryHandler.QueryAsync<int>(_contract.ContractAddress, req);
            //var res = await function.CallAsync<string>(req, from: _account.Address, new HexBigInteger(300000), new HexBigInteger(0));
        }

        
    }
}