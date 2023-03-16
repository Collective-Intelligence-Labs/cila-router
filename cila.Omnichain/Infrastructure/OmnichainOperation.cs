using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.RPC.Eth.DTOs;

namespace cila.Omnichain.Infrastructure
{
    public class OmnichainOperation : CallInput
    {
        [Parameter("payload", "_payload", 1)]
        public byte[] Payload { get; private set; }

        public OmnichainOperation(byte[] payload)
        {
            Payload = payload;
        }
    }
}