using Microsoft.AspNetCore.Mvc;
using Pulse.Api.Contracts.Responses;
using Pulse.Application.Services;

namespace Pulse.Api.Endpoints;

public static class RatesEndpoints
{
    public static RouteGroupBuilder GetRatesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rates").WithTags("Rates");
        
        group.MapGet("/", async ([FromQuery] string? codes, IRateService service, ILogger<IRateService> log) =>
            {
                var listCodes = string.IsNullOrWhiteSpace(codes)
                    ? []
                    : codes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                log.LogInformation("Rates: request codes=[{Codes}]", string.Join(',', listCodes));

                var result = await service.GetRates(listCodes);
                return result.Match(Results.Ok);
            })
            .Produces<RatesResponse>()
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable )
            .WithName("GetRates");

        return group;
    }
}