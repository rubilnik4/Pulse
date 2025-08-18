namespace Pulse.Api.Contracts.Responses;

public sealed record RateItemResponse(
    string Code, 
    string Name, 
    int Nominal, 
    decimal ValueRub, 
    decimal PreviousRub
);