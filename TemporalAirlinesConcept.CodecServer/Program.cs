using TemporalAirlinesConcept.CodecServer;
using TemporalAirlinesConcept.DAL.Codec;
using Temporalio.Converters;

var builder = WebApplication.CreateBuilder(args);

// Setup console logging, codec, and cors
builder.Logging.AddSimpleConsole().SetMinimumLevel(LogLevel.Information);
builder.Services.AddSingleton<IPayloadCodec>(ctx => new EncryptionCodec());
builder.Services.AddCors();

var app = builder.Build();

// We need CORS so that the browser can access this endpoint from a
// different origin
app.UseCors(
    builder => builder.
        WithHeaders("content-type", "x-namespace").
        WithMethods("POST").
        WithOrigins("http://localhost:8080", "http://localhost:8233", "https://cloud.temporal.io"));

// These are the endpoints called for encrypt/decrypt
app.MapPost("/encode", CodecServer.EncodeAsync);
app.MapPost("/decode", CodecServer.DecodeAsync);

app.Run();