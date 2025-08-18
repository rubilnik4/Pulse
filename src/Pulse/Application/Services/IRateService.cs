using CSharpFunctionalExtensions;
using Pulse.Api.Contracts.Responses;
using Pulse.Infrastructure.Errors;

namespace Pulse.Application.Services;

public interface IRateService
{
    Task<Result<RatesResponse, IAppError>> GetRates(string[] codes);
}