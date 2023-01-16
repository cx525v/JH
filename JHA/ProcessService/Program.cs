using ProcessService;
using ProcessService.Interfaces;
using ProcessService.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>()
        .AddScoped<IDataProcessService, DataProcessService>();
    })
    .Build();

await host.RunAsync();
