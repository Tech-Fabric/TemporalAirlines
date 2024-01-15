using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TemporalAirlinesConcept.Configuration.ConfiguratoinExtensions;

// Run worker until cancelled
Console.WriteLine("Running worker");

try
{
    await RunWorkerAsync();
}
catch (OperationCanceledException)
{
    Console.WriteLine("Worker cancelled");
}

Console.ReadLine();
Console.WriteLine("Worker Finished");

async Task RunWorkerAsync()
{
    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureLogging(ctx =>
            ctx.AddSimpleConsole().SetMinimumLevel(LogLevel.Information))
        .ConfigureServices((hostContext, services) =>
            {
                services
                    // Add the services
                    .ConfigureServices(hostContext.Configuration)
                    // Add the worker
                    .ConfigureTemporalWorker();
            })
        .Build();

    await host.RunAsync();
}