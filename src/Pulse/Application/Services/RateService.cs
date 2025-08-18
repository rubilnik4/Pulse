using CSharpFunctionalExtensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Pulse.Api.Contracts.Responses;
using Pulse.Api.Сbr;
using Pulse.Application.Options;
using Pulse.Application.Services.Cbr;
using Pulse.Infrastructure.Errors;

namespace Pulse.Application.Services;

public sealed class RateService(
    ICbrClient cbrClient,
    IMemoryCache cache,
    IOptions<CbrOptions> options,
    ILogger<RateService> log) : IRateService
{
    private const string CacheKey = "rates:cbr:daily";

    public async Task<Result<RatesResponse, IAppError>> GetRates(string[] codes)
    {
        var normalizeCodes = (codes is { Length: > 0 } ? codes : ["USD", "EUR"])
            .Select(c => c.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();
        
        log.LogInformation("Requested rates for codes: {Codes}", string.Join(", ", normalizeCodes));
        
        var cbrDailyResponse = await GetDaily();
        if (cbrDailyResponse.IsFailure)
            return Result.Failure<RatesResponse, IAppError>(cbrDailyResponse.Error);

        var rates = GetRates(cbrDailyResponse.Value, normalizeCodes);
        return Result.Success<RatesResponse, IAppError>(rates);
    }

    private async Task<Result<CbrDailyResponse, IAppError>> GetDaily()
    {
        log.LogDebug("Checking cache for {CacheKey}", CacheKey);
        if (cache.TryGetValue<CbrDailyResponse>(CacheKey, out var cached))
        {
            log.LogInformation("Cache hit for {CacheKey}, date={Date}", CacheKey, cached!.Date);
            return Result.Success<CbrDailyResponse, IAppError>(cached!);
        }
           
        log.LogInformation("Cache miss for {CacheKey}, requesting CBR daily rates", CacheKey);
        var cbrDaily = await cbrClient.GetDaily();
        if (cbrDaily.IsFailure)
            return cbrDaily;
       
        var ttl = TimeSpan.FromMinutes(Math.Max(1, options.Value.CacheMinutes));
        cache.Set(CacheKey, cbrDaily.Value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });
        log.LogDebug("Cached CBR daily rates until {Expiry}", DateTime.UtcNow.Add(ttl));
        return cbrDaily;
    }

    private RatesResponse GetRates(CbrDailyResponse cbrDaily, string[] normalizeCodes)
    {
        var items = new List<RateItemResponse>(normalizeCodes.Length);
        foreach (var code in normalizeCodes)
        {
            if (cbrDaily.Valute.TryGetValue(code, out var valute))
            {
                items.Add(new RateItemResponse(
                    Code: code,
                    Name: valute.Name,
                    Nominal: valute.Nominal,
                    ValueRub: valute.Value,
                    PreviousRub: valute.Previous
                ));
            }
            else
            {
                log.LogWarning("CBR: code {Code} not found in snapshot", code);
            }
        }
        return new RatesResponse(cbrDaily.Date, items);
    }
}