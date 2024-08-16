namespace DexCexMevBot.Config;

public class AppSettings
{
    public string NetworkName { get; set; }
    public Telegram Telegram { get; set; }
}

public class Telegram
{
    public string BotToken { get; set; }
    public string ChatId { get; set; }
}