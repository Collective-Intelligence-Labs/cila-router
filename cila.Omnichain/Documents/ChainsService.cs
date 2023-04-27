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
            var chainsInSettings = settings.Chains;
            var chains = GetAll();
            var chainsToAdd = chainsInSettings.Where(x => !chains.Select(c => c.ChainId).Contains(x.ChainId)).ToList();

            var chainsCollection = database.GetChainsCollection();
            if (chainsToAdd.Any())
            {
                chainsCollection.InsertMany(chainsToAdd.Select(x => new ChainDocument
                {
                    Id = x.ChainId,
                    ChainId = x.ChainId,
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