using ReadYourWritesConsistency.API.JSONSerialization;

namespace ReadYourWritesConsistency.API.JsonSerialization;

public static class HttpJsonOptionConfigurator
{
    public static IServiceCollection AddExtendedJsonSerializationContext(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolver = new ExtendedJsonSerializationContext();
        });

        return services;
    }
}
