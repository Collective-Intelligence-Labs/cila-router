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

        public OperationDispatcher(RouterProvider provider, ChainClientsFactory clientsFactory, ExecutionsService executionsService, KafkaProducer producer)
        {
            this.provider = provider;
            this.clientsFactory = clientsFactory;
            this.executionsService = executionsService;
            this.producer = producer;
        }

        public async Task Dispatch(Operation operation)
        {
            var context = RouterContext.Default;
            var router = provider.GetRouter(context);
            var routedOperations = operation.Commands.Select(x => new RoutedCommand {
                Command = x,
                Route = router.CalculateRoute(x)
            }).GroupBy(x=> x.Route.ChainId).Select(x=> 
            { 
                var op = new Operation {
                    RouterId = operation.RouterId
                };
                foreach (var cmd in x)
                {
                    op.Commands.Add(cmd.Command);
                }
                return new RoutedOperation{
                    Operation = op,
                    ChainId = x.Key
                };
            });

            foreach (var rOp in routedOperations)
            {
                var client = clientsFactory.GetChainClient(rOp.ChainId);
                var response = await client.SendAsync(rOp.Operation);
                var operationId = rOp.Operation.GetHashCode();
                executionsService.Record(operationId, rOp.ChainId, response, context.Stretagy, router.GetType().Name);
                
                //Send infrastructure event
                await ProduceInfrastructureEvent(rOp.ChainId, 
                /* TODO: replace with list of aggregates */ rOp.Operation.Commands.First().AggregateId.ToString(),
                operationId.ToString(),
                rOp.Operation.RouterId.ToString(),
                rOp.Operation.Commands.ToList(),
                null);
            }
        }

        private async Task ProduceInfrastructureEvent(string chainId, string aggregateId, string operationId, string routerId, List<Command> cmds, string errorMessage)
        {
             var infEvent = new InfrastructureEvent{
                        Id = Guid.NewGuid().ToString(),
                        EvntType = InfrastructureEventType.TransactionRoutedEvent,
                        AggregatorId = aggregateId,
                        //ChainId = chainId,
                        OperationId = operationId,
                        CoreId = errorMessage //TODO: replace with normal error handling
                    };
                    foreach (var cmd in cmds)
                    {
                        infEvent.Commands.Add( new DomainCommandDto{
                                AggregateId = cmd.AggregateId.ToString(),
                                Timespan = Timestamp.FromDateTime(DateTime.UtcNow),
                        });
                    }
                    await producer.ProduceAsync("infr", infEvent);
        }
    }

    public class RoutedCommand
    {
        public Command Command {get;set;}

        public OmnichainRoute Route {get;set;}
    }

    public class RoutedOperation
    {
        public string ChainId {get;set;}

        public OmnichainRoute CombinedRoute {get;set;}

        public Operation Operation {get;set;}
    }
}