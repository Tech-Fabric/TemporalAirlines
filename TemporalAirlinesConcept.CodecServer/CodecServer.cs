using Google.Protobuf;
using Temporalio.Api.Common.V1;
using Temporalio.Converters;

namespace TemporalAirlinesConcept.CodecServer;

public static class CodecServer
{
    public static Task<IResult> EncodeAsync(
        HttpContext ctx, IPayloadCodec codec) => ApplyCodecFuncAsync(ctx, codec.EncodeAsync);

    public static Task<IResult> DecodeAsync(
        HttpContext ctx, IPayloadCodec codec) => ApplyCodecFuncAsync(ctx, codec.DecodeAsync);

    public static async Task<IResult> ApplyCodecFuncAsync(
        HttpContext ctx, Func<IReadOnlyCollection<Payload>, Task<IReadOnlyCollection<Payload>>> func)
    {
        // Read payloads as JSON
        if (ctx.Request.ContentType?.StartsWith("application/json") != true)
        {
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }
        Payloads inPayloads;
        using (var reader = new StreamReader(ctx.Request.Body))
        {
            inPayloads = JsonParser.Default.Parse<Payloads>(await reader.ReadToEndAsync());
        }

        // Apply codec func
        var outPayloads = new Payloads() { Payloads_ = { await func(inPayloads.Payloads_) } };

        // Return JSON
        return Results.Text(JsonFormatter.Default.Format(outPayloads), "application/json");
    }
}