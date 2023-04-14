using MongoDB.Driver;

namespace Cila.Database {

    public class MongoDatabase
    {
        private MongoClient _client;

        private string _dbname = "relay";

        private class Collections {
            public static string Events  = "events";
            public static string Subscriptions  = "subscriptions";
            public static string Chains  = "chains";
            public static string Executions  = "executions";
            public static string AggregatedEvents = "aggregated-events";
        }

        public MongoDatabase(OmniChainSettings settings)
        {
            _client = new MongoClient(settings.MongoDBConnectionString);
        }

        public IMongoCollection<SubscriptionDocument> GetSubscriptionsCollection()
        {
            return _client.GetDatabase(_dbname).GetCollection<SubscriptionDocument>(Collections.Subscriptions);
        }

        public IMongoCollection<ChainDocument> GetChainsCollection()
        {
            return _client.GetDatabase(_dbname).GetCollection<ChainDocument>(Collections.Chains);
        }

        public IMongoCollection<AggregatedEventDocument> GetAggregatedEventsCollection()
        {
            return _client.GetDatabase(_dbname).GetCollection<AggregatedEventDocument>(Collections.AggregatedEvents);
        }

        public IMongoCollection<ExecutionDocument> GetExecutionsCollection()
        {
            return _client.GetDatabase(_dbname).GetCollection<ExecutionDocument>(Collections.Executions);
        }
    }
}
