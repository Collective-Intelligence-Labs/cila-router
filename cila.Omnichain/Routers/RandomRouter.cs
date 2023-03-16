using System;
using cila.Omnichain.Infrastructure;

namespace cila.Omnichain.Routers
{
	public enum Chains
	{
		Ethereum = 0,
		Gnosis = 1
	}

	public class RandomRouter
	{
		public RandomRouter()
		{
		}

		public async Task<IChainClient> Route()
		{
			var random = new Random();
			var route = random.Next() % 2 == 0 ? Chains.Ethereum : Chains.Gnosis;

            switch (route)
            {
                default:
                    return await Task.FromResult(new EthChainClient("rpc", "contract", "abi"));
            }

        }
    }
}

