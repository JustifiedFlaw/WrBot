using System;
using System.Collections.Generic;
using System.IO;
using SrcRestEase;
using SrcRestEase.Models;

namespace SrcGameListRefresher
{
    class Program
    {
        static void Main(string[] args)
        {
            var games = GetAllGames();

            Console.WriteLine($"Serializing {games.Count} games");
            var text = GamesSerializer.Serialize(games);

            Console.WriteLine("Saving to games.txt");
            File.WriteAllText(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar +  "games.txt", text);
        }

        private static List<Game> GetAllGames()
        {
            const int pageSize = 1000;
            int offset = 0;

            var srcApi = SrcApi.Connect();

            Game[] page;
            var allGames = new List<Game>(50000);
            do
            {
                Console.Write($"Querying games {offset} to {offset + pageSize}...");
                page = srcApi.GetGames(pageSize, offset).Result.Data;
                Console.WriteLine($"got {page.Length} games");

                allGames.AddRange(page);

                offset += pageSize;

            } while (page.Length >= pageSize);

            return allGames;
        }
    }
}
