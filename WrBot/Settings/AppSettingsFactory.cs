using System;

public static class AppSettingsFactory
{
    public static AppSettings Load()
    {
        var appSettings = new AppSettings();

        appSettings.DatabaseSettings = LoadDatabaseSettings();
        appSettings.BotSettings = LoadBotSettings();

        return appSettings;
    }

    private static DatabaseSettings LoadDatabaseSettings()
    {
        var databaseSettings = new DatabaseSettings();
        databaseSettings.Host = GetEnvironmentString("WRBOT_DB_HOST");
        databaseSettings.Port = GetEnvironmentInteger("WRBOT_DB_PORT");
        databaseSettings.Database = GetEnvironmentString("WRBOT_DB_DB");
        databaseSettings.User = GetEnvironmentString("WRBOT_DB_USER");
        databaseSettings.Password = GetEnvironmentString("WRBOT_DB_PWRD");

        return databaseSettings;
    }

    private static BotSettings LoadBotSettings()
    {
        var botSettings = new BotSettings();
        botSettings.BotName = GetEnvironmentString("WRBOT_BOT_NAME");
        botSettings.ClientId = GetEnvironmentString("WRBOT_BOT_CID");
        botSettings.AccessToken = GetEnvironmentString("WRBOT_BOT_TKN");
        botSettings.KeepAlive = GetEnvironmentInteger("WRBOT_BOT_KA");
        botSettings.KeepChannelsConnected = GetEnvironmentInteger("WRBOT_BOT_KC");

        return botSettings;
    }

    private static string GetEnvironmentString(string name)
    {
        var str = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(str))
        {
            throw new Exception($"Environment variable {name} is empty or missing");
        }

        return str;
    }

    private static int GetEnvironmentInteger(string name)
    {
        var str = Environment.GetEnvironmentVariable(name);
        if (int.TryParse(str, out int value))
        {
            return value;
        }

        throw new Exception($"Expected an integer in environment variable {name}, but was \"{str}\"");
    }
}