using Cila.Database;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Cila
{
    public class ChainsService
    {
        private readonly MongoDatabase database;

        public ChainsService(MongoDatabase database)
        {
            this.database = database;
        }

        public void InitializeFromSettings(OmniChainSettings settings)
        {  
            var chains = GetAll();

            if (!chains.Any())
            {
                 var chainsCollection = database.GetChainsCollection();
                chainsCollection.InsertMany(settings.Chains.Select(x=> new ChainDocument{
                    Id = ObjectId.GenerateNewId().ToString(),
                    PrivateKey = x.PrivateKey,
                    CQRSContract = x.Contract,
                    RPC = x.Rpc,
                    ChainType = ExecutionChainType.Ethereum
                }));
            }
        }

        public ChainDocument Get(string id)
        {
            var filter = Builders<ChainDocument>.Filter.Eq(x => x.Id, id);
            return database.GetChainsCollection().Find(filter).FirstOrDefault();
        }

        public List<ChainDocument> GetAll()
        {
            var filter = Builders<ChainDocument>.Filter.Empty;
            return database.GetChainsCollection().Find(filter).ToList();
        }
    }
}