using System;
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

		public Task<int> Route()
		{
			var random = new Random();
			var route = random.Next() % 2 == 0 ? Chains.Ethereum : Chains.Gnosis;
			return Task.FromResult((int) route);
		}
	}
}

