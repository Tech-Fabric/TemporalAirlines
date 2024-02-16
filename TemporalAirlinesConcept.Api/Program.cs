using OpenTelemetry.Exporter;
using TemporalAirlinesConcept.Api.Configuration;
using TemporalAirlinesConcept.Configuration.ConfigurationExtensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.AddTelemetryLogger("DI API");

builder.Services.ConfigureServices(builder.Configuration, ConsoleExporterOutputTargets.Debug);
builder.Services.ConfigureTemporalClient();

// Worker for testing
builder.Services.ConfigureTemporalWorker();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await app.CheckCosmosDb();
    await app.InitializeDb();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
