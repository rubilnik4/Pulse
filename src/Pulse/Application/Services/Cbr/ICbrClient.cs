using CSharpFunctionalExtensions;
using Pulse.Api.Сbr;
using Pulse.Infrastructure.Errors;

namespace Pulse.Application.Services.Cbr;

public interface ICbrClient
{
    Task<Result<CbrDailyResponse, IAppError>> GetDaily();
}