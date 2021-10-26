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
    private string FilePath = Directory.GetCurrentDirectory() 
                + Path.DirectorySeparatorChar 
                + "games.txt";

    public GamesListRefresher(GamesListRefresherSettings settings)
    {
        this.Settings = settings;
    }

    public void Start()
    {
        LoadSavedData();

        this.Timer = new Timer(this.Settings.DelayInMilliseconds);
        this.Timer.Elapsed += Timer_Elapsed;
        this.Timer.Start();
    }

    private void LoadSavedData()
    {
        if (File.Exists(this.FilePath))
        {
            var file = File.OpenText(this.FilePath);
            var lineNumber = 0;
            var gamesList = new List<Game>(25000);
            while (!file.EndOfStream)
            {
                 var line = file.ReadLine();

                 if (!string.IsNullOrWhiteSpace(line))
                 {
                    var spacePos = line.IndexOf(' ');
                    if (spacePos > 0)
                    {
                        var game = new Game
                        {
                            Id = line.Substring(0, spacePos),
                            Names = new GameNames
                            {
                                International = line.Substring(spacePos)
                            }
                        };

                        gamesList.Add(game);
                    }
                    else
                    {
                        Log.Error($"Unexpected format on line {lineNumber}, no space found");
                    }
                 }

                 lineNumber++;
            }

            GamesList.Data = gamesList;
        }
        else
        {
            DownloadData();
        }
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        DownloadData();
    }

    private void DownloadData()
    {
        try
        {
            Log.Information("Getting all games from speedrun.com");
            
            GamesList.Data = GetAllGames();

            Log.Debug($"Serializing {GamesList.Data.Count} games");
            var text = GamesSerializer.Serialize(GamesList.Data);

            Log.Debug("Saving to " + this.FilePath);
            File.WriteAllText(this.FilePath, text);
            
            Log.Information($"Got {GamesList.Data.Count} games from speedrun.com");
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
