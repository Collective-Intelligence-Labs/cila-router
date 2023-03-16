
using Nethereum.Web3;
using Nethereum.Contracts;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;

namespace OmniChain
{
    interface IChainClient
    {
        void Send(OmniChainOperation op);
        void Push(OmniChainEvent ev);
        void Subscribe(Action<OmniChainEvent> action);
    }

    public class OmniChainOperation: CallInput
    {
        [Parameter("payload", "_payload", 1)]
        public byte[] Payload { get; set; }
    }

    public class EthChainClient : IChainClient
    {
        private Web3 _web3;
        private Contract _contract;

        public EthChainClient(string rpc, string contract, string abi)
        {
            _web3 = new Web3(rpc);
            _contract = _web3.Eth.GetContract(abi,contract);
        }

        public void Push(OmniChainEvent ev)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(Action<OmniChainEvent> action)
        {
            throw new NotImplementedException();
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

    public class OmniChainEvent
    {
        public string ChainID {get;set;}

        public byte[] Payload {get;set;}

        public int BlockNumber {get;set;}

        public string AggregateID {get;set;}
            
        public int EventNumber {get;set;}

        public int EventType {get;set;}

        public string EventHash {get;set;}
    }
}