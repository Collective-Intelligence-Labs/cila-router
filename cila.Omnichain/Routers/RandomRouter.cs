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

            var rpc = "https://eth-goerli.public.blastapi.io";
            var contract = "0xCa1E5C04d9410B8cF075BbfD777f8F9246829150";


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