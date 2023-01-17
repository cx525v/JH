using ProcessService;
using ProcessService.Interfaces;
using ProcessService.Services;
using SharedLibrary.Handlers;
using SharedLibrary.Handlers.Interfaces;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>()
        .AddScoped<IDataProcessService, DataProcessService>()
        .AddScoped<IConsumerBuilderHandler, ConsumerBuilderHandler>()
        .AddScoped<IProducerBuilderHandler, ProducerBuilderHandler>()
        .AddScoped<IUpdateDataService, UpdateDataService>();
    
    })
    .Build();

await host.RunAsync();
