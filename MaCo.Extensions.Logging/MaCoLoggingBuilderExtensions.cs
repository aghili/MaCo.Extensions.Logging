using Microsoft.Extensions.Logging;

namespace MaCo.Extensions.Logging;

public static class MaCoLoggingBuilderExtensions
{
    public static ILoggingBuilder AddMaCoLogging(this ILoggingBuilder builder)
    {
        builder.AddProvider(new MaCoLoggerProvider(() => new MaCoLoggerConfiguration()));
        return builder;
    }

    public static ILoggingBuilder AddMaCoLogging(this ILoggingBuilder builder, MaCoLoggerConfiguration configuration)
    {
        builder.AddProvider(new MaCoLoggerProvider(() => configuration));
        return builder;
    }
}
