
    public class OmniChainSettings
    {
        public  string MongoDBConnectionString {get;set;}

        public string RouterId {get;set;}

        public List<ExecutionChainSettings> Chains {get;set;}

        public string SingletonAggregateID { get; set; }

        public OmniChainSettings()
        {
            Chains = new List<ExecutionChainSettings>();
        }
    }

    public class ExecutionChainSettings
    {
        public string ChainId { get; set; }

        public string Rpc { get; set; } 

        public string PrivateKey { get; set; }  

        public string Contract { get; set; }

        public string AbiFile {get;set;}

        private string _abi;
        public string Abi {get {
                _abi = _abi ?? File.ReadAllText(AbiFile);
            return _abi;
        }}
    }