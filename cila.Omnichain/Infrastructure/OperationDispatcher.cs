using System.Security.Cryptography;
using System.Text;
using cila.Omnichain.Documents;
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
        private readonly ExecutionChainEventService executionChainEventService;

        public OperationDispatcher(RouterProvider provider, ChainClientsFactory clientsFactory, ExecutionsService executionsService, KafkaProducer producer, OmniChainSettings settings, AggregagtedEventsService aggregagtedEventsService, ExecutionChainEventService executionChainEventService)
        {
            this.provider = provider;
            this.clientsFactory = clientsFactory;
            this.executionsService = executionsService;
            this.producer = producer;
            this.settings = settings;
            this.aggregagtedEventsService = aggregagtedEventsService;
            this.executionChainEventService = executionChainEventService;
        }

        public async Task<IEnumerable<ChainResponse>> Dispatch(Operation operation, string routerId)
        {
            var versionNullable = executionChainEventService.GetLastVersion(settings.SingletonAggregateID);
            var version = versionNullable.HasValue ? versionNullable + 1 : 0;
            var operationId = settings.SingletonAggregateID + version;

            await ProduceOperationInitiatedEvent(
                "Execution chain will be selected automatically",
                settings.SingletonAggregateID,
                operationId,
                routerId,
                operation.Commands.ToList());

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
                

                var chainId = rOp.ChainId;
                var commands = rOp.Operation.Commands.ToList();

                try
                {
                    await ProduceTransactionRoutedEvent(
                        chainId,
                        settings.SingletonAggregateID,
                        operationId,
                        routerId, //rOp.Operation.RouterId.ToStringUtf8() ?? "Unspecified",
                        commands);

                    var client = clientsFactory.GetChainClient(rOp.ChainId);
                    var response = await client.SendAsync(rOp.Operation);

                    result.Add(response);

                    // Change with setting operation ID on the client
                    executionsService.Record(operationId, rOp.ChainId, response, context.Stretagy, router.GetType().Name);

                    //Send infrastructure event
                    /* TODO: replace with list of aggregates */

                    await ProduceTransactionExecutedEvent(
                        response.ChainId,
                        settings.SingletonAggregateID,
                        operationId,
                        routerId,
                        response.TransactionHash,
                        commands);
                }
                catch (System.Exception e)
                {
                    await ProduceErrorEvent(
                        chainId,
                        settings.SingletonAggregateID,
                        operationId,
                        routerId, //rOp.Operation.RouterId.ToStringUtf8() ?? "Unspecified",
                        commands,
                        e.Message);
                }
            }

            return result;
        }

        private async Task ProduceTransactionRoutedEvent(string chainId, string aggregateId, string operationId, string routerId, List<Command> cmds)
        {
            var infEvent = new InfrastructureEvent
            {
                Id = Guid.NewGuid().ToString(),
                EvntType = InfrastructureEventType.TransactionRoutedEvent,
                AggregatorId = aggregateId,
                RouterId = routerId,
                ChainId = chainId,
                OperationId = operationId,
                CoreId = string.Empty,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            foreach (var cmd in cmds)
            {
                infEvent.Commands.Add(new DomainCommandDto
                {
                    Id = GetHashString(cmd.AggregateId.ToStringUtf8() + cmd.CmdPayload.ToStringUtf8() + cmd.CmdSignature.ToStringUtf8()),
                    AggregateId = aggregateId,
                    Timespan = Timestamp.FromDateTime(DateTime.UtcNow),
                });
            }
            await producer.ProduceAsync("infr", infEvent);
        }

        private async Task ProduceTransactionExecutedEvent(string chainId, string aggregateId, string operationId, string routerId, string txHash, List<Command> cmds)
        {
            var infEvent = new InfrastructureEvent
            {
                Id = Guid.NewGuid().ToString(),
                EvntType = InfrastructureEventType.TransactionExecutedEvent,
                AggregatorId = aggregateId,
                RouterId = routerId,
                ChainId = chainId,
                OperationId = operationId,
                // TODO: add trx hash
                CoreId = txHash,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            foreach (var cmd in cmds)
            {
                infEvent.Commands.Add(new DomainCommandDto
                {
                    Id = GetHashString(aggregateId + cmd.CmdPayload.ToBase64() + cmd.CmdSignature.ToBase64()),
                    AggregateId = aggregateId,
                    Timespan = Timestamp.FromDateTime(DateTime.UtcNow),
                });
            }
            await producer.ProduceAsync("infr", infEvent);
        }

        private async Task ProduceErrorEvent(string chainId, string aggregateId, string operationId, string routerId, List<Command> cmds, string errorMessage)
        {
            var infEvent = new InfrastructureEvent
            {
                Id = Guid.NewGuid().ToString(),
                EvntType = InfrastructureEventType.NotSpecifiedEvent,
                AggregatorId = aggregateId,
                RouterId = routerId,
                ChainId = chainId,
                OperationId = operationId,
                // TODO: add trx hash
                CoreId = errorMessage, //TODO: replace with normal error handling
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            foreach (var cmd in cmds)
            {
                infEvent.Commands.Add(new DomainCommandDto
                {
                    Id = GetHashString(aggregateId + cmd.CmdPayload.ToBase64() + cmd.CmdSignature.ToBase64()),
                    AggregateId = aggregateId,
                    Timespan = Timestamp.FromDateTime(DateTime.UtcNow),
                });
            }
            await producer.ProduceAsync("infr", infEvent);
        }

        private async Task ProduceOperationInitiatedEvent(string chainId, string aggregateId, string operationId, string routerId, List<Command> cmds)
        {
            var infEvent = new InfrastructureEvent
            {
                Id = Guid.NewGuid().ToString(),
                EvntType = InfrastructureEventType.ApplicationOperationInitiatedEvent,
                AggregatorId = aggregateId,
                RouterId = routerId,
                ChainId = chainId,
                OperationId = operationId,
                // TODO: add trx hash
                CoreId = string.Empty,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            foreach (var cmd in cmds)
            {
                infEvent.Commands.Add(new DomainCommandDto
                {
                    Id = GetHashString(aggregateId + cmd.CmdPayload.ToBase64() + cmd.CmdSignature.ToBase64()),
                    AggregateId = aggregateId,
                    Timespan = Timestamp.FromDateTime(DateTime.UtcNow),
                });
            }
            await producer.ProduceAsync("infr", infEvent);
        }

        private static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        private static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
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