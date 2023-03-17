using Grpc.Core;
using cila.Omnichain.Routers;
using cila.Omnichain.Infrastructure;
using System.Text;

namespace cila.Omnichain.Services;

public class OmnichainService : Omnichain.OmnichainBase
{
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
            var route = _router.Route().Result as EthChainClient;
            var random = new Random();

            var op = new OmnichainOperation(Encoding.Unicode.GetBytes(request.ToString()));

            //await route.SendAsync(op);

            return new OmnichainResponse
            {
                ChainId = random.Next(2).ToString(),
                Success = true,
                Sender = request.Sender
            };
        }
        catch (Exception ex)
        {
            var e = ex.Message;

            return new OmnichainResponse
            {
                ChainId = null,
                Success = false,
                Sender = request.Sender
            };
        }
        
    }

    public override Task<OmnichainResponse> Transfer(TransferRequest request, ServerCallContext context)
    {
        var route = _router.Route();
        var random = new Random();

        return Task.FromResult(new OmnichainResponse
        {
            ChainId = random.Next(2).ToString(),
            Success = true,
            Sender = request.Sender
        });
    }
}

