using System.Text.Json.Serialization;

namespace Pulse.Api.Сbr;

public sealed record CbrDailyResponse(
    DateTime Date,
    DateTime PreviousDate,
    [property: JsonPropertyName("PreviousURL")] string PreviousUrl,
    DateTime Timestamp,
    IReadOnlyDictionary<string, CbrValuteResponse> Valute
);