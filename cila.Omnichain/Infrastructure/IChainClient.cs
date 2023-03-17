namespace cila.Omnichain.Infrastructure
{
    public interface IChainClient
    {
        Task SendAsync(OmnichainOperation op);
    }
}