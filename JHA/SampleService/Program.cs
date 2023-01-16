using SampleService;
using SampleService.Interfaces;
using SampleService.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>()
        .AddScoped<IAppHttpClientHandler, AppHttpClientHandler>()
        .AddScoped<ITweetClient, TweetClient>();

    })
    .Build();

await host.RunAsync();
