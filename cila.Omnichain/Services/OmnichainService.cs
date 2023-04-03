using Grpc.Core;
using cila.Omnichain.Routers;
using cila.Omnichain.Infrastructure;
using System.Text;
using Google.Protobuf;
using System.Runtime.Serialization.Formatters.Binary;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;

namespace cila.Omnichain.Services;

public class OmnichainService : Omnichain.OmnichainBase
{
    //private const string PRIVATE_KEY = "dfef8681aa52ab2ed9c4a9208531dbe27f7ba27be492bd9facb500fd8697196b";
    //private const string PRIVATE_KEY = "11bcf8712793289e8bba856c63ac991256eda9c9e53e9d93cb360a313a1a2cda";
    private const string PRIVATE_KEY = "0x4647b4c2ca4a070d72fe9c9c633c1f1f23ac5375d0da30ece0db6765aed8dac5";
    private const string AGGREGATE_ID = "E512D6FCf560eb300d264f4260D08F0ef3Fef1A8";

    private readonly RandomRouter _router;

    private readonly ILogger<OmnichainService> _logger;

    public OmnichainService(ILogger<OmnichainService> logger)
    {
        _router = new RandomRouter();
        _logger = logger;
    }

    

    public override async Task<OmnichainResponse> Mint(MintRequest request, ServerCallContext context)
    {
        try
        {
            var chain = await _router.GetExecutionChain();
            var chainClient = new EthChainClient(chain.Rpc, chain.Contract, PRIVATE_KEY);

            var signature = FromHexString(CalculateKeccak256("some sig"));
            var operation = new cila.Omnichain.Infrastructure.Operation1
            {
                RouterId = FromHexString("B4B7f66d146613B1Cf4524cf47DE28db2b466567")
            };

            var payload = new MintNFTPayload
            {
                Hash = GetByteString(FromHexString(CalculateKeccak256(request.Hash))), //GetByteString(FromHexString(request.Hash)),
                Owner = GetByteString(FromHexString(request.Sender))
            };

            var cmd = new cila.Omnichain.Infrastructure.Command
            {
                AggregateId = FromHexString(AGGREGATE_ID),
                CmdType = (uint)CommandType.MintNft,
                CmdPayload = payload.ToByteArray(),
                CmdSignature = signature
            };

            operation.Commands.Add(cmd);

            await chainClient.SendAsync(operation);

            return new OmnichainResponse
            {
                ChainId = chain.ChainId.ToString(),
                Success = true,
                Sender = request.Sender
            };
        }
        catch (Exception ex)
        {
            return new OmnichainResponse
            {
                ChainId = "-1",
                Success = false,
                Sender = ex.Message
            };
        }
    }

    public override async Task<OmnichainResponse> Transfer(TransferRequest request, ServerCallContext context)
    {
        try
        {
            var chain = await _router.GetExecutionChain();
            var chainClient = new EthChainClient(chain.Rpc, chain.Contract, PRIVATE_KEY);

            //var operation = new Operation
            //{
            //    RouterId = GetByteString(FromHexString("B4B7f66d146613B1Cf4524cf47DE28db2b466567"))
            //};

            //var payload = new TransferNFTPayload
            //{
            //    Hash = GetByteString(FromHexString(CalculateKeccak256("Some NFT"))), //GetByteString(FromHexString(request.Hash)),
            //    To = GetByteString(FromHexString(request.Recipient))
            //};

            //var cmd = new Command
            //{
            //    AggregateId = ByteString.CopyFrom("cila", Encoding.Unicode),
            //    CmdType = CommandType.MintNft,
            //    CmdPayload = payload.ToByteString(),
            //    CmdSignature = ByteString.CopyFrom(request.Signature, Encoding.Unicode)
            //};

            //operation.Commands.Add(cmd);

            //await chainClient.SendAsync(operation);

            return new OmnichainResponse
            {
                ChainId = chain.ChainId.ToString(),
                Success = true,
                Sender = request.Sender
            };
        }
        catch (Exception ex)
        {
            return new OmnichainResponse
            {
                ChainId = "-1",
                Success = false,
                Sender = ex.Message
            };
        }
    }

    private string CalculateKeccak256(string str)
    {
        var keccak = new Sha3Keccack();
        return keccak.CalculateHash(str);
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
}

