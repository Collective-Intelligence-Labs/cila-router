using cila;
using cila.Omnichain.Services;
using Cila;
using Cila.Database;
using Cila.Omnichain.Routers;
using Confluent.Kafka;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Google.Protobuf;
using Swashbuckle.AspNetCore.SwaggerGen;
using cila.Omnichain.Documents;

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.

  var cnfg = new ConfigurationBuilder();
  var configuration = cnfg.AddJsonFile("settings.json")
            .Build();
  var settings = configuration.GetSection("Settings").Get<OmniChainSettings>();


 var configProducer = new ProducerConfig
        {
            BootstrapServers = "localhost:9092",
            ClientId = "dotnet-kafka-producer",
            Acks = Acks.All,
            MessageSendMaxRetries = 10,
            MessageTimeoutMs = 10000,
            EnableIdempotence = true,
            CompressionType = CompressionType.Snappy,
            BatchSize = 16384,
            LingerMs = 10,
            MaxInFlight = 5,
            EnableDeliveryReports = true,
            DeliveryReportFields = "all"
        };


builder.Services
    .AddScoped<MongoDatabase>()
    .AddSingleton(settings)
    .AddScoped<RouterProvider>()
    .AddScoped<ChainsService>()
    .AddScoped<OperationDispatcher>()
    .AddScoped<SubscriptionsService>()
    .AddScoped<ExecutionsService>()
    .AddScoped<ChainClientsFactory>()
    .AddScoped<AggregagtedEventsService>()
    .AddScoped<OperationGenerator>()
    .AddSingleton(configProducer)
    .AddSingleton<KafkaProducer>()
    .AddScoped<RandomRouter>()
    .AddScoped<EfficientRouter>()
    .AddScoped<ExecutionChainEventService>();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
//builder.Services.AddSwaggerGen();

var serviceProvider = builder.Services.BuildServiceProvider();

builder.Services.AddSingleton(serviceProvider);

//Initialize chains in database
serviceProvider.GetService<ChainsService>().InitializeFromSettings(settings);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});


builder.WebHost.ConfigureKestrel(options =>
{
    // Setup a HTTP/2 endpoint without TLS.
    options.ListenLocalhost(5025, o => o.Protocols =
        HttpProtocols.Http1AndHttp2);
});

var app = builder.Build();

IWebHostEnvironment env = app.Environment;

if (env.IsDevelopment())
{
    app.MapGrpcReflectionService();
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

app.UseGrpcWeb();
app.UseCors();

// Configure the HTTP request pipeline.
app.MapGrpcService<OmnichainService>().EnableGrpcWeb();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapPost("/route", async (HttpContext context) => 
{   
    using (var ms = new MemoryStream())
    {
        await context.Request.Body.CopyToAsync(ms);
        byte[] bytes = ms.ToArray();
        var operation = new Operation();
        operation.MergeFrom(bytes);
        var dispatcher = serviceProvider.GetService<OperationDispatcher>();
        await dispatcher.Dispatch(operation, settings.RouterId);
        context.Response.StatusCode = 200;
        await context.Response.WriteAsync("Success");
    }
});

app.MapGet("/generate/{number}", async (HttpContext context, int number) => 
{
    var opGen = serviceProvider.GetService<OperationGenerator>();
    await opGen.GenerateAndSend(number);
    await context.Response.WriteAsync("Success");
});
app.Run();

