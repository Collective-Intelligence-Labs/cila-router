namespace cila.Omnichain.Infrastructure
{
    public interface IChainClient
    {
        void Send(OmnichainOperation op);
    }
}