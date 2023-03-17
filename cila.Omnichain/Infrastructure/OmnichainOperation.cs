using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;

namespace cila.Omnichain.Infrastructure
{
    public class OmnichainOperation : CallInput
    {
        [Parameter("bytes", "opBytes")]
        public byte[] OpBytes { get; private set; }

        public OmnichainOperation(byte[] opBytes)
        {
            OpBytes = opBytes;
        }
    }
}