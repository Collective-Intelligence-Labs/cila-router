
using Nethereum.Web3;
using Nethereum.Contracts;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;

namespace cila.Omnichain.Infrastructure
{
    public interface IChainClient
    {
        void Send(OmniChainOperation op);
    }

    public class OmniChainOperation : CallInput
    {
        [Parameter("payload", "_payload", 1)]
        public byte[] Payload { get; private set; }

        public OmniChainOperation(byte[] payload)
        {
            Payload = payload;
        }
    }

    public class EthChainClient : IChainClient
    {
        private Web3 _web3;
        private Contract _contract;

        public EthChainClient(string rpc, string contract, string abi)
        {
            _web3 = new Web3(rpc);
            _contract = _web3.Eth.GetContract(abi, contract);
        }

        async Task Send(OmniChainOperation op)
        {
            var dispatchOperationHandler = _contract.GetFunction<OmniChainOperation>();
            var result = await dispatchOperationHandler.CallAsync(op);
        }

        void IChainClient.Send(OmniChainOperation op)
        {
            Send(op).GetAwaiter().GetResult();
        }
    }

    public class OmniChainEvent : IEventDTO
    {
        public string ChainID { get; set; } = string.Empty;

        public byte[] Payload { get; set; } = Array.Empty<byte>();

        public int BlockNumber { get; set; }

        public string AggregateID { get; set; } = string.Empty;

        public int EventNumber { get; set; }

        public int EventType { get; set; }

        public string EventHash { get; set; } = string.Empty;
    }
}