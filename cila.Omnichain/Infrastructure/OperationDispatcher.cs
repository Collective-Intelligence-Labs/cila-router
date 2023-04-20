using cila.Omnichain.Infrastructure;
using Cila;
using Google.Protobuf.WellKnownTypes;

namespace cila
{
    public class OperationDispatcher
    {
        private readonly RouterProvider provider;
        private readonly ChainClientsFactory clientsFactory;
        private readonly ExecutionsService executionsService;
        private readonly KafkaProducer producer;
        private readonly OmniChainSettings settings;
        private readonly AggregagtedEventsService aggregagtedEventsService;

        public OperationDispatcher(RouterProvider provider, ChainClientsFactory clientsFactory, ExecutionsService executionsService, KafkaProducer producer, OmniChainSettings settings, AggregagtedEventsService aggregagtedEventsService)
        {
            this.provider = provider;
            this.clientsFactory = clientsFactory;
            this.executionsService = executionsService;
            this.producer = producer;
            this.settings = settings;
            this.aggregagtedEventsService = aggregagtedEventsService;
        }

        public async Task<IEnumerable<ChainResponse>> Dispatch(Operation operation)
        {
            var context = RouterContext.Default;
            var router = provider.GetRouter(context);
            var routedOperations = operation.Commands.Select(x => new RoutedCommand
            {
                Command = x,
                Route = router.CalculateRoute(x)
            }).GroupBy(x => x.Route.ChainId).Select(x =>
            {
                var op = new Operation
                {
                    RouterId = operation.RouterId
                };
                foreach (var cmd in x)
                {
                    op.Commands.Add(cmd.Command);
                }
                return new RoutedOperation
                {
                    Operation = op,
                    ChainId = x.Key
                };
            });

            var result = new List<ChainResponse>();
            foreach (var rOp in routedOperations)
            {
                var versionNullable = aggregagtedEventsService.GetLastVersion(settings.SingletonAggregateID);
                var version = versionNullable.HasValue ? versionNullable + 1 : 0;
                var operationId = settings.SingletonAggregateID + version;
                try
                {
                    var client = clientsFactory.GetChainClient(rOp.ChainId);
                    var response = await client.SendAsync(rOp.Operation);

                    result.Add(response);

                    // Change with setting operation ID on the client
                    executionsService.Record(operationId, rOp.ChainId, response, context.Stretagy, router.GetType().Name);

                    //Send infrastructure event
                    /* TODO: replace with list of aggregates */
                    await ProduceInfrastructureEvent(
                        rOp.ChainId,
                        settings.SingletonAggregateID,
                        operationId.ToString(),
                        rOp.Operation.RouterId.ToString() ?? "Unspecified",
                        rOp.Operation.Commands.ToList(),
                        null);
                }
                catch (System.Exception e)
                {
                    /* TODO: replace with list of aggregates */
                    await ProduceInfrastructureEvent(
                        rOp.ChainId,
                        settings.SingletonAggregateID,
                        operationId.ToString(),
                        rOp.Operation.RouterId.ToString() ?? "Unspecified",
                        rOp.Operation.Commands.ToList(),
                        e.Message);
                }
            }

            return result;
        }

        private async Task ProduceInfrastructureEvent(string chainId, string aggregateId, string operationId, string routerId, List<Command> cmds, string errorMessage)
        {
            var infEvent = new InfrastructureEvent
            {
                Id = Guid.NewGuid().ToString(),
                EvntType = InfrastructureEventType.TransactionRoutedEvent,
                AggregatorId = aggregateId,
                //ChainId = chainId,
                OperationId = operationId,
                CoreId = errorMessage //TODO: replace with normal error handling
            };
            foreach (var cmd in cmds)
            {
                infEvent.Commands.Add(new DomainCommandDto
                {
                    AggregateId = cmd.AggregateId.ToString(),
                    Timespan = Timestamp.FromDateTime(DateTime.UtcNow),
                });
            }
            await producer.ProduceAsync("infr", infEvent);
        }
    }

    public class RoutedCommand
    {
        public Command Command { get; set; }

        public OmnichainRoute Route { get; set; }
    }

    public class RoutedOperation
    {
        public string ChainId { get; set; }

        public OmnichainRoute CombinedRoute { get; set; }

        public Operation Operation { get; set; }
    }
}