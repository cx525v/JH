using SampleService;
using SampleService.Interfaces;
using SampleService.Services;
using SharedLibrary.Handlers;
using SharedLibrary.Handlers.Interfaces;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>()
        .AddScoped<IAppHttpClientHandler, AppHttpClientHandler>()
        .AddScoped<ITweetClient, TweetClient>()
        .AddScoped<IProducerBuilderHandler, ProducerBuilderHandler>()
        .AddScoped<IPublishService, PublishService>();

    })
    .Build();

await host.RunAsync();
