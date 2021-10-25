using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Serilog;
using SrcRestEase;
using SrcRestEase.Models;

public class GamesListRefresher
{
    public GamesListRefresherSettings Settings { get; private set; }
    private Timer Timer;

    public GamesListRefresher(GamesListRefresherSettings settings)
    {
        this.Settings = settings;

        this.Timer = new Timer(settings.DelayInMilliseconds);
        this.Timer.Elapsed += Timer_Elapsed;
        this.Timer.Start();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            Log.Information("Getting all games from speedrun.com");
            
            var games = GetAllGames();

            Log.Debug($"Serializing {games.Count} games");
            var text = GamesSerializer.Serialize(games);

            var filePath = Directory.GetCurrentDirectory() 
                + Path.DirectorySeparatorChar 
                + "games.txt";
            Log.Debug("Saving to " + filePath);
            File.WriteAllText(filePath, text);
            
            Log.Information($"Got {games.Count} games from speedrun.com");
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message + "\n" + ex.StackTrace);
        }
    }

    private static List<Game> GetAllGames()
    {        
        const int pageSize = 1000;
        int offset = 0;

        var srcApi = SrcApi.Connect();

        var allGames = new List<Game>(30000);
        Game[] page;
        do
        {
            Log.Debug($"Querying games {offset} to {offset + pageSize}...");
            page = srcApi.GetGames(pageSize, offset).Result.Data;
            Log.Debug($"Got {page.Length} games");

            allGames.AddRange(page);

            offset += pageSize;

        } while (page.Length >= pageSize);
        
        return allGames;
    }
}
