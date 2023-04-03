using System;
using cila.Omnichain.Infrastructure;
using Nethereum.Web3;

namespace cila.Omnichain.Routers
{
	public enum Chains
	{
		Ethereum = 0,
		Gnosis = 1
	}

	public class RandomRouter
	{
        public async Task<ExecutionChain> GetExecutionChain()
        {
            var random = new Random();
            var chain = random.Next() % 2 == 0 ? Chains.Ethereum : Chains.Gnosis;

			var rpc = "http://127.0.0.1:7545";
			var contract = "0x9b89499cE43Fb6D95ABe0557362E4FAa02B2aeC0";

            //var rpc = "https://eth-goerli.public.blastapi.io";
            //var contract = "0xedC3Cc09dF964ddf939eCDc137F4833c96a62A2A";


            return await Task.FromResult(new ExecutionChain((int)chain, rpc, contract));
        }
    }

	public class ExecutionChain
	{
		public int ChainId { get; private set; }
		public string Rpc { get; private set; }
		public string Contract { get; private set; }

		public ExecutionChain(int id, string rpc, string contract)
		{
			ChainId = id;
			Rpc = rpc;
			Contract = contract;

		}

	}
}