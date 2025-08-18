using Microsoft.Extensions.Options;

namespace Pulse.Application.Options;

public static class OptionsExtensions
{
    public static OptionsBuilder<T> AddValidatedOptions<T>(
        this IServiceCollection services,
        IConfiguration config,
        string sectionName,
        Action<OptionsBuilder<T>>? extraValidation = null)
        where T : class
    {
        var section = config.GetSection(sectionName);
        var builder = services
            .AddOptions<T>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        extraValidation?.Invoke(builder);
        return builder;
    }
}