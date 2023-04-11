namespace cila.Omnichain.Infrastructure
{
    public interface IChainClient
    {
        Task<ChainResponse> SendAsync(Operation op);
    }
}