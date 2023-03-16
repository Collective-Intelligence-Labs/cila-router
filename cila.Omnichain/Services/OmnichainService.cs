using Grpc.Core;

namespace cila.Omnichain.Services;

public class OmnichainService : Omnichain.OmnichainBase
{
    private readonly ILogger<OmnichainService> _logger;
    public OmnichainService(ILogger<OmnichainService> logger)
    {
        _logger = logger;
    }

    public override Task<OmnichainResponse> Mint(MintRequest request, ServerCallContext context)
    {
        return Task.FromResult(new OmnichainResponse
        {
            ChainId = 0,
            Success = true
        });
    }

    public override Task<OmnichainResponse> Transfer(TransferRequest request, ServerCallContext context)
    {
        return Task.FromResult(new OmnichainResponse
        {
            ChainId = 0,
            Success = true
        });
    }
}

