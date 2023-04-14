using cila;
using cila.Omnichain.Infrastructure;
using Cila.Omnichain.Routers;

namespace Cila
{
    public class RouterProvider
    {
        private readonly IServiceProvider serviceProvider;

        public RouterProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IOmnichainRouter GetRouter(RouterContext context)
        {
            switch (context.Stretagy)
            {
                case RoutingStrategy.Random:
                    return serviceProvider.GetService<RandomRouter>();
                default:
                    return serviceProvider.GetService<EfficientRouter>();
            }
        }
    }

    public interface IOmnichainRouter
    {
        OmnichainRoute CalculateRoute(Command operation);
    }

    public class OmnichainRoute
    {
        public double? Confidence { get; set; }

        public int? EstimatedCost { get; set; }

        public TimeSpan? EstimatedDuration {get;set;}

        public double? EstiamtedSecurity {get;set;}

        public string ChainId {get;set;}

        public List<string> OtherChainsCalls {get;set;}
    }

    public class RouterContext
    {
        public RoutingStrategy Stretagy {get;set;}

        public static RouterContext Default = new RouterContext{ Stretagy = RoutingStrategy.Optimal};
    }

    public enum RoutingStrategy
    {
        Chepest,
        Optimal,
        Fastest,
        Random
    }
}