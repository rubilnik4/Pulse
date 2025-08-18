namespace Pulse.Api.Contracts.Responses;

public sealed record RatesResponse(
    DateTime Date,
    IReadOnlyList<RateItemResponse> Items
);