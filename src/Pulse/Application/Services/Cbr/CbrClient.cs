using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using Pulse.Api.Сbr;
using Pulse.Application.Options;
using Pulse.Infrastructure.Errors;

namespace Pulse.Application.Services.Cbr;

public sealed class CbrClient(
    HttpClient http,
    IOptions<CbrOptions> options,
    ILogger<ICbrClient> log) : ICbrClient
{
    public async Task<Result<CbrDailyResponse, IAppError>> GetDaily()
    {
        try
        {
            log.LogDebug("Getting  CBR daily valute");
            
            var resp = await http.GetAsync(options.Value.DailyPath);
            if (!resp.IsSuccessStatusCode)
            {
                var msg = $"CBR service api error {(int)resp.StatusCode} {resp.ReasonPhrase}";
                log.LogError("CBR non-success: {Msg}", msg);
                return Result.Failure<CbrDailyResponse, IAppError>(new ExternalApiError("CBR", ((int)resp.StatusCode).ToString(), 
                    resp.ReasonPhrase ?? "CBR service api error"));
            }
            
            var response = await resp.Content.ReadFromJsonAsync<CbrDailyResponse>();
            return response is not null 
                ? Result.Success<CbrDailyResponse, IAppError>(response)  
                : Result.Failure<CbrDailyResponse, IAppError>(new ExternalApiError("CBR", 500.ToString(), "Empty or invalid JSON")) ;
        }
        catch (TaskCanceledException ex)
        {
            log.LogError(ex, "CBR service timeout");
            return Result.Failure<CbrDailyResponse, IAppError>(new ExternalApiError("CBR", "Timeout", ex.Message));
        }
        catch (Exception ex)
        {
            log.LogError(ex, "CBR error");
            return Result.Failure<CbrDailyResponse, IAppError>(new ExternalApiError("CBR", "Unknown", ex.Message));
        }
    }
}