using Grpc.Core;
using cila.Omnichain;

namespace cila.Omnichain.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}

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

