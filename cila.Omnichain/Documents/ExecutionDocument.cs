namespace Cila
{
    public class ExecutionDocument
    {
        public string Id {get;set;}

        public string ChainId {get;set;}

        public string OperationId {get;set;}

        public string AggregateId {get;set;}

        public ulong ActualCost {get;set;}

        public ulong EstimatedCost {get;set;}

        public TimeSpan EstimatedDuration {get;set;}

        public TimeSpan ActualDuration {get;set;}

        public ulong SynchronizationCost {get;set;}

        public float EstiamtedSecurity {get;set;}

        public string Message {get;set;}

        public DateTime Timestamp {get;set;}
        public RoutingStrategy RouterStrategy { get; internal set; }
        public string RouterImplementation { get; internal set; }
    }
}