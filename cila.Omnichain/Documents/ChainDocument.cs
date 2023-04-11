namespace Cila
{
    public class ChainDocument
    {
        public string Id {get;set;}

        public ExecutionChainType ChainType {get;set;}

        public string RPC {get;set;}

        public string PrivateKey {get;set;}

        public string CQRSContract {get;set;}
    }

    public enum ExecutionChainType 
    {
        Ethereum
    }
}