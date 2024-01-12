using TemporalAirlinesConcept.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        .ConfigureServices(ctx =>
            ctx
                // Add the services
                .ConfigureServices()
                // Add the worker
                .ConfigureTemporalWorker())
        .Build();

    await host.RunAsync();
}