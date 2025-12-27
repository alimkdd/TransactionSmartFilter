using Mapster;
using MapsterMapper;
using TransactSmartFilter.Application.Mapper;

namespace TransactSmartFilter.API.Common;

public static class MapperRegistration
{
    public static IServiceCollection RegisterMapper(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(MapsterConfig).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, Mapper>();

        return services;
    }
}