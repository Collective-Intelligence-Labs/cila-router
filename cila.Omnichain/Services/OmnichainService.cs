using Grpc.Core;
using cila.Omnichain.Routers;
using cila.Omnichain.Infrastructure;
using System.Text;

namespace cila.Omnichain.Services;

public class OmnichainService : Omnichain.OmnichainBase
{
    private const string PRIVATE_KEY = "dfef8681aa52ab2ed9c4a9208531dbe27f7ba27be492bd9facb500fd8697196b";

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

            var op = new OmnichainOperation(Encoding.Unicode.GetBytes(request.ToString()), request.ToString());

            await chainClient.SendAsync(op);

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

            var op = new OmnichainOperation(Encoding.Unicode.GetBytes(request.ToString()), request.ToString());

            await chainClient.SendAsync(op);

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
}

