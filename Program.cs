using CentralBankCurrencyService;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    }).UseWindowsService()
    .Build();

await host.RunAsync();