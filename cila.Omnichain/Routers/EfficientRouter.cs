using Cila;
using Cila.Database;

namespace Cila
{
    public class EfficientRouter : IOmnichainRouter
    {
        private readonly ChainsService chainsService;
        private readonly SubscriptionsService subscriptionsService;
        private readonly ExecutionsService executionsService;
        private readonly MongoDatabase database;

        public EfficientRouter(ChainsService chainsService, SubscriptionsService subscriptionsService, ExecutionsService executionsService)
        {
            this.chainsService = chainsService;
            this.subscriptionsService = subscriptionsService;
            this.executionsService = executionsService;
        }

        public OmnichainRoute CalculateRoute(Command command)
        {
    
            var routes = new List<OmnichainRoute>();
            var chains = chainsService.GetAll();
            
            foreach (var chain in chains)
            {
                var score = 0.0;
                
                // Check if this chain is synced with the latest state of the aggregate
                var lastSync = executionsService.GetLastFor(chain.Id, command.AggregateId.ToString());
                
                if (lastSync == null)
                {
                    // Chain has never synced with the aggregate, give it a low score
                    score += 0.1;
                }
                else if (lastSync.ActualCost == 0)
                {
                    // Chain has synced with the aggregate and no changes were made, give it a high score
                    score += 1.0;
                }
                else
                {
                    // Chain has synced with the aggregate and some changes were made, give it a score based on estimated cost, duration and security
                    score += (1.0 / (double)lastSync.ActualCost) * (1.0 / (double)lastSync.ActualDuration.TotalSeconds) * (double)lastSync.EstiamtedSecurity;
                }
                
                routes.Add(new OmnichainRoute
                {
                    ChainId = chain.Id,
                    Confidence = score
                });
            }
            
            // Pick the route with the highest score
            var bestRoute = routes.OrderByDescending(route => route.Confidence).First();

            return bestRoute;
        }
    }
}