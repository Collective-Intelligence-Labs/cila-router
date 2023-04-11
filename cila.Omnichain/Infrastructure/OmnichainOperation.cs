using System.Numerics;
using Google.Protobuf;
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

    public class CommandDto
    {
        public byte[] AggregateId { get; set; }
        public uint CmdType { get; set; }
        public byte[] CmdPayload { get; set; }
        public byte[] CmdSignature { get; set; }
    }

    public class OperationDto
    {
        public byte[] RouterId { get; set; }
        public List<CommandDto> Commands { get; set; }

        public OperationDto()
        {
            Commands = new List<CommandDto>();
        }

        public Operation ConvertToProtobuff()
        {
            var pbOperation = new Operation()
            {
                RouterId = ByteString.CopyFrom(RouterId)
            };

            foreach (var c in Commands)
            {
                pbOperation.Commands.Add(new global::Command()
                {
                    AggregateId = ByteString.CopyFrom(c.AggregateId),
                    CmdPayload = ByteString.CopyFrom(c.CmdPayload),
                    CmdSignature = ByteString.CopyFrom(c.CmdSignature),
                    CmdType = (CommandType)c.CmdType
                });
            }
            return pbOperation;
        }
    }
}
