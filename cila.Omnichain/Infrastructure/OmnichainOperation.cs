using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;

namespace cila.Omnichain.Infrastructure
{
    [Function("dispatch")]
    public class DispatchFunction : FunctionMessage
    {
        [Parameter("bytes", "opBytes")]
        public byte[] OpBytes { get; set; }
    }

    public class Command
    {
        public byte[] AggregateId { get; set; }
        public uint CmdType { get; set; }
        public byte[] CmdPayload { get; set; }
        public byte[] CmdSignature { get; set; }
    }

    public class Operation1
    {
        public byte[] RouterId { get; set; }
        public List<Command> Commands { get; set; }

        public Operation1()
        {
            Commands = new List<Command>();
        }

    }
}
