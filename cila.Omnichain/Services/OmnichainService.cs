using Grpc.Core;
using cila.Omnichain.Infrastructure;
using System.Text;
using Google.Protobuf;
using System.Runtime.Serialization.Formatters.Binary;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;

namespace cila.Omnichain.Services;

public class OmnichainService : Omnichain.OmnichainBase
{ 
    private readonly ILogger<OmnichainService> _logger;
    private readonly KafkaProducer _producer;
    private readonly OperationDispatcher dispatcher;
    private readonly OmniChainSettings settings;

    public OmnichainService(ILogger<OmnichainService> logger, KafkaProducer producer, OperationDispatcher dispatcher, OmniChainSettings settings)
    {
        _logger = logger;
        _producer = producer;
        this.dispatcher = dispatcher;
        this.settings = settings;
    }

    public override async Task<OmnichainResponse> Mint(MintRequest request, ServerCallContext context)
    {
        try
        {
            var operation = new cila.Omnichain.Infrastructure.OperationDto
            {
                RouterId = FromHexString(settings.RouterId)
            };

            var payload = new MintNFTPayload
            {
                Hash = GetByteString(FromHexString(CalculateKeccak256(request.Hash))),
                Owner = GetByteString(FromHexString(request.Sender))
            };

            var cmd = new cila.Omnichain.Infrastructure.CommandDto
            {
                AggregateId = FromString(settings.SingletonAggregateID),
                CmdType = (uint)CommandType.MintNft,
                CmdPayload = payload.ToByteArray(),
                CmdSignature = FromHexString(request.Signature)
            };

            operation.Commands.Add(cmd);

            var result = await dispatcher.Dispatch(operation.ConvertToProtobuff());
            
            var response = new OmnichainResponse
            {
                //replace here with something
                Success = true,
                Sender = request.Sender
            };
            response.Logs.AddRange(result.Select(x => string.Format("Executed on chain {0}, tx: {1}", x.ChainId, x.TransactionHash)));
            return response;
        }
        catch (Exception ex)
        {
            var response = new OmnichainResponse
            {
                Success = false,
                Sender = request.Sender
            };
            response.Logs.Add(ex.Message);
            return response;
        }
    }

    public override async Task<OmnichainResponse> Transfer(TransferRequest request, ServerCallContext context)
    {
        try
        {
            //var chain = await _router.GetExecutionChain();
            //var chainClient = new EthChainClient(chain.Rpc, chain.Contract, PRIVATE_KEY);

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
                Success = true,
                Sender = request.Sender
            };
        }
        catch (Exception ex)
        {
            return new OmnichainResponse
            {
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
        str = str.StartsWith("0x") ? str.Substring(2) : str;
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

