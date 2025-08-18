using System.Text.Json.Serialization;

namespace Pulse.Api.Сbr;

public sealed record CbrValuteResponse(
    [property: JsonPropertyName("ID")] string Id,
    string NumCode,
    string CharCode,
    int Nominal,
    string Name,
    decimal Value,
    decimal Previous
);