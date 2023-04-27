using System.Text;
using cila;
using Google.Protobuf;
using Nethereum.Web3;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Signer;
using System.Security.Cryptography;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Cila
{
    public class OperationGenerator
    {
        private readonly OperationDispatcher dispatcher;
        private readonly OmniChainSettings settings;

        private const string PRIVATE_KEY = "0x6203901f60ff32c60f7ffc98ba2449979615bf9eac90c470f52fc09b09a09b0b";

        public OperationGenerator(OperationDispatcher dispatcher, OmniChainSettings settings)
        {
            this.dispatcher = dispatcher;
            this.settings = settings;
        }

        public async Task GenerateAndSend(int number)
        {
            var operations = Enumerable.Range(1, number)
                                 .Select(i => 
                                 {
                                    return GenerateMintOpertaion(GenerateHexString(16), GenerateHexString(16), PRIVATE_KEY);
                                 });

            foreach (var op in operations)
            {
                await dispatcher.Dispatch(op, settings.RouterId);
            }
        }

        public Operation GenerateMintOpertaion(string sender, string hash, string privateKey)
        {
            var ecKey = new EthECKey(privateKey);
         
            var operation = new cila.Omnichain.Infrastructure.OperationDto
            {
                RouterId = FromHexString(settings.RouterId)
            };

            var payload = new MintNFTPayload
            {
                Hash = GetByteString(FromHexString(CalculateKeccak256(hash))), //GetByteString(FromHexString(request.Hash)),
                Owner = GetByteString(FromHexString(sender))
            };

            var commandHash = CalculateKeccak256(payload.ToByteArray());
            var signature = ecKey.Sign(commandHash).ToDER();

            var cmd = new cila.Omnichain.Infrastructure.CommandDto
            {
                AggregateId = FromString(settings.SingletonAggregateID),
                CmdType = (uint)CommandType.MintNft,
                CmdPayload = payload.ToByteArray(),
                CmdSignature = signature
            };

            operation.Commands.Add(cmd);
            return operation.ConvertToProtobuff();
        }


    private string CalculateKeccak256(string str)
    {
        var keccak = new Sha3Keccack();
        return keccak.CalculateHash(str);
    }

    private byte[] CalculateKeccak256(byte[] hash)
    {
        var keccak = new Sha3Keccack();
        return keccak.CalculateHash(hash);
    }

    private byte[] FromHexString(string str)
    {
        return Enumerable.Range(0, str.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(str.Substring(x, 2), 16)).ToArray();
    }

    private byte[] FromString(string str)
    {
        var bytes = Encoding.Default.GetBytes(str);
        return FromHexString(Convert.ToHexString(bytes));
    }

    private ByteString GetByteString(byte[] bytes)
    {
        return ByteString.CopyFrom(bytes);
    }

    private string GenerateHexString(int length)
    {
        Random random = new Random();
        byte[] bytes = new byte[length];
        random.NextBytes(bytes);
        string hexString = BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        return hexString;
    }
    }
}