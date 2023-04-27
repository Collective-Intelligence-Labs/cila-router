using System;
namespace cila.Omnichain.Documents
{
    public class ExecutionChainEventDocument
    {
        public string Id { get; set; }

        public string OriginChainId { get; set; }

        public string AggregateId { get; set; }

        public byte[] Serialized { get; set; }

        public byte[] Hash { get; set; }

        public int BlockNumber { get; set; }

        public ulong Version { get; set; }

    }
}

