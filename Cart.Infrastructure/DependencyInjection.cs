using Cart.Application.Abstractions;
using Cart.Infrastructure.Messaging;
using Cart.Infrastructure.Options;
using Cart.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Cart.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure options
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<CartOptions>(configuration.GetSection(CartOptions.SectionName));
        services.Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.SectionName));

        // Redis connection
        var redisConnectionString = configuration.GetSection($"{RedisOptions.SectionName}:ConnectionString").Value
                                    ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configOptions = ConfigurationOptions.Parse(redisConnectionString);
            configOptions.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(configOptions);
        });

        // Repositories
        services.AddScoped<ICartRepository, RedisCartRepository>();

        // Event Publisher
        services.AddSingleton<IEventPublisher, KafkaEventPublisher>();

        return services;
    }
}

