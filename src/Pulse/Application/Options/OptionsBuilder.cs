namespace Pulse.Application.Options;

public static class OptionsBuilder
{
    public static WebApplicationBuilder AddOptions(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedOptions<PaginationOptions>(
            builder.Configuration, "Pagination",
            b => b.Validate(o => o.DefaultSize <= o.MaxSize, "DefaultSize must be <= MaxSize")
        );

        builder.Services.AddValidatedOptions<CbrOptions>(
            builder.Configuration, "Cbr"
        );

        builder.Services.AddValidatedOptions<PostgresOptions>(
            builder.Configuration, "Postgres"
        );

        builder.Services.AddValidatedOptions<OverdueWorkerOptions>(
            builder.Configuration, "Workers:Overdue"
        );
        return builder;
    }
}