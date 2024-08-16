using DexCexMevBot.Constant;
using Telegram.Bot;

namespace DexCexMevBot.Config.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddNetworkName(this WebApplicationBuilder builder)
    {
        var options = new AppSettings();
        builder.Configuration.GetSection(nameof(AppSettings)).Bind(options);

        var networkId = Enum.Parse<NetworkChainId>(options.NetworkName);
        var networkNameEnum = Enum.Parse<NetworkName>(options.NetworkName);
        
        Common.NETWORK_NAME_ENUM = networkNameEnum;
        Common.NETWORK_NAME = options.NetworkName;
        Common.NETWORK_ID = networkId;
        Common.BuyExchangeId = options.NetworkName;

        return builder;
    }
    
    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{builder.Environment.EnvironmentName}.json",
                true,
                true
            );
        builder.Services.Configure<AppSettings>(
            builder.Configuration.GetSection(nameof(AppSettings))
        );

        return builder;
    }
    
    public static WebApplicationBuilder AddTelegramBot(this WebApplicationBuilder builder)
    {
        var options = new AppSettings();
        builder.Configuration.GetSection(nameof(AppSettings)).Bind(options);

        // var botClient = new TelegramBotClient(options.Telegram.BotToken);
        // var me2 = botClient.SendTextMessageAsync(options.Telegram.ChatId,"Test text").GetAwaiter().GetResult();

        return builder;
    }

}