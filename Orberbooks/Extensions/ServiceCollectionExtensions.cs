using DexCexMevBot.Modules.Orberbooks.CexClients.Extensions;
using DexCexMevBot.Modules.Orberbooks.CexClients.Proxy.Extensions;
using DexCexMevBot.Modules.Orberbooks.Pollers.Extensions;

namespace DexCexMevBot.Modules.Orberbooks.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderbooks(this IServiceCollection services)
    {
        services.AddCexClients();
        services.AddProxy();
        services.AddPollers();

        return services;
    }

}