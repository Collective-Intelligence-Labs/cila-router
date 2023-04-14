using cila;

namespace Cila
{
    public class RouterService
    {
        private readonly OperationDispatcher disparcher;
        private readonly RouterProvider provider;
        private readonly ChainClientsFactory clientsFactory;
        private readonly ExecutionsService executionsService;
        private readonly KafkaProducer producers;

        public RouterService(OperationDispatcher disparcher, RouterProvider provider, ChainClientsFactory clientsFactory, ExecutionsService executionsService, KafkaProducer producers)
        {
            this.disparcher = disparcher;
            this.provider = provider;
            this.clientsFactory = clientsFactory;
            this.executionsService = executionsService;
            this.producers = producers;
        }

        public async Task<RoutingResult> Route(Operation operation)
        {
            //Implement Router Service and more some logic from Dispatcher to here
            throw new NotImplementedException();
        }
    }

    public class RoutingResult
    {
        public bool Success {get;set;}
    }
}