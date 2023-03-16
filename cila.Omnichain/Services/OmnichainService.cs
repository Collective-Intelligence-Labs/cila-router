using Grpc.Core;
using cila.Omnichain.Routers;

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

    public override Task<OmnichainResponse> Mint(MintRequest request, ServerCallContext context)
    {
        var route = _router.Route();

        return Task.FromResult(new OmnichainResponse
        {
            ChainId = route.Result,
            Success = true
        });
    }

    public override Task<OmnichainResponse> Transfer(TransferRequest request, ServerCallContext context)
    {
        var route = _router.Route();

        return Task.FromResult(new OmnichainResponse
        {
            ChainId = route.Result,
            Success = true
        });
    }
}

