using OpenTelemetry.Exporter;
using System.Text.Json;
using System.Text.Json.Serialization;
using TemporalAirlinesConcept.Api.Configuration;
using TemporalAirlinesConcept.Api.Profiles;
using TemporalAirlinesConcept.Configuration.ConfigurationExtensions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(x => new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Logging.AddTelemetryLogger("DI API");

builder.Services.ConfigureServices(builder.Configuration, ConsoleExporterOutputTargets.Debug);
builder.Services.ConfigureTemporalClient();

builder.Services.AddAutoMapper(typeof(UserApiProfile));
builder.Services.AddAutoMapper(typeof(SeatApiProfilee));
builder.Services.AddAutoMapper(typeof(TicketApiProfile));
builder.Services.AddAutoMapper(typeof(FlightApiProfile));

// Worker for testing
builder.Services.ConfigureTemporalWorker();

var app = builder.Build();

app.MigrateDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    //await app.CheckCosmosDb();
    await app.InitializeDb();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
