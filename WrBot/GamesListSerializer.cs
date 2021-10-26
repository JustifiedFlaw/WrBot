using System;
using System.Collections.Generic;
using System.Text;
using SrcRestEase.Models;

public static class GamesListSerializer
{
    public static string Serialize(List<Game> games)
    {
        var sb = new StringBuilder(games.Count * 15);

        foreach (var game in games)
        {
            sb.Append(game.Id);
            sb.Append(' ');
            sb.AppendLine(game.Names.International);
        }

        return sb.ToString();
    }

    public static Game Deserialize(string line)
    {
        var spacePos = line.IndexOf(' ');
        if (spacePos > 0)
        {
            return new Game
            {
                Id = line.Substring(0, spacePos),
                Names = new GameNames
                {
                    International = line.Substring(spacePos + 1)
                }
            };
        }
        else
        {
            throw new FormatException($"No space found");
        }
    }
}