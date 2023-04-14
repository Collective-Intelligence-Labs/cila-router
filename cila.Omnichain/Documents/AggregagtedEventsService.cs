using Cila;
using Cila.Database;
using MongoDB.Driver;

public class AggregagtedEventsService
{
    private readonly IMongoCollection<AggregatedEventDocument> _events;

    public AggregagtedEventsService(MongoDatabase database)
    {
        _events = database.GetAggregatedEventsCollection();;

        // Create an index on the AggregateId field
        var indexKeysDefinition = Builders<AggregatedEventDocument>.IndexKeys.Ascending(e => e.AggregateId);
        var indexModel = new CreateIndexModel<AggregatedEventDocument>(indexKeysDefinition);
        _events.Indexes.CreateOne(indexModel);
    }

    public AggregatedEventDocument GetUniqEvent(string aggregagteId, ulong version, string hash, string chainId)
    {
        return _events.Find(x=> x.Hash == hash && x.AggregateId == aggregagteId && x.Version == version && x.ChainId == chainId).FirstOrDefault();
    }

    public List<AggregatedEventDocument> GetEvents(string aggregagteId, ulong version, string hash)
    {
        return _events.Find(x=> x.Hash == hash && x.AggregateId == aggregagteId && x.Version == version).ToList();
    }

    public void AddEvent(AggregatedEventDocument aggregatedEventDocument)
    {
        _events.InsertOne(aggregatedEventDocument);
    }

    public ulong? GetLastVersion(string aggregateID)
    {
        var lastEvent = _events.Find(x=> x.AggregateId == aggregateID).SortByDescending(x=>x.Version).FirstOrDefault();
        return lastEvent != null ? lastEvent.Version : null;
    }
}