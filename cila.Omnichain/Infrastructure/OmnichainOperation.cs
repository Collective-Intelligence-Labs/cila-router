using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;

namespace cila.Omnichain.Infrastructure
{
    [Function("dispatch2", "string")]
    public class DispatchFunction : FunctionMessage
    {
        [Parameter("bytes", "opBytes")]
        public byte[] OpBytes { get; set; }
    }

    [Function("pull")]
    public class PullFunction: FunctionMessage
    {
        [Parameter("uint", "_position")]
        public int Position { get; set; }
    }

    [FunctionOutput]
    public class PullEventsDTO : IFunctionOutputDTO
    {
        [Parameter("uint256", "position", 1)]
        public BigInteger Position { get; set; }

        [Parameter("DomainEvent[]", "events", 2)]
        public List<OmniChainEvent> Events { get; set; }
    }

    public class OmniChainEvent
    {
        public string ChainID { get; set; }

        public byte[] Payload { get; set; }

        public int BlockNumber { get; set; }

        public string AggregateID { get; set; }

        public int EventNumber { get; set; }

        public int EventType { get; set; }

        public string EventHash { get; set; }

    }
}
