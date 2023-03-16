using Nethereum.ABI.FunctionEncoding.Attributes;

namespace cila.Omnichain.Infrastructure
{
    public class OmnichainEvent : IEventDTO
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