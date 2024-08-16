namespace DexCexMevBot.Modules.Orberbooks.Pollers.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPollers(this IServiceCollection services)
    {
        services.AddSingleton<PollerService>();

        return services;
    }

}