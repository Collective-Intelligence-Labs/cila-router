using Cila.Database;
using MongoDB.Driver;

namespace Cila
{
    public class SubscriptionsService
    {
        private readonly MongoDatabase database;

        public SubscriptionsService(MongoDatabase database)
        {
            this.database = database;
        }

        public SubscriptionDocument Get(string id)
        {
            var filter = Builders<SubscriptionDocument>.Filter.Eq(x=> x.Id, id);
            return database.GetSubscriptionsCollection().Find(filter).FirstOrDefault();
        }

        public List<SubscriptionDocument> GetAll()
        {
            var filter = Builders<SubscriptionDocument>.Filter.Empty;
            return database.GetSubscriptionsCollection().Find(filter).ToList();
        }
    }
}