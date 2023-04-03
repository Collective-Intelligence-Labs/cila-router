namespace cila.Omnichain.Infrastructure
{
    public interface IChainClient
    {
        Task SendAsync(Operation1 op);
    }
}