using DexCexMevBot.Modules.Orberbooks.CexClients.Abstractions;

namespace DexCexMevBot.Modules.Orberbooks.CexClients.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCexClients(this IServiceCollection services)
    {
        services.AddSingleton<AbstractCexClient, BinanceClient>();

        return services;
    }
}