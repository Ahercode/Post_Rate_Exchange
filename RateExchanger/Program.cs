using RateExchanger;
using RateExchanger.Processes;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<RateProcessor>();
    })
    .Build();

host.Run();