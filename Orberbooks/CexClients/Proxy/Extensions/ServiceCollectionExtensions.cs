namespace DexCexMevBot.Modules.Orberbooks.CexClients.Proxy.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProxy(this IServiceCollection services)
    {
        services.AddSingleton<ProxyProvider>();

        return services;
    }
}