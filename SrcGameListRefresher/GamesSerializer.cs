using System;
using System.Collections.Generic;
using System.Text;
using SrcRestEase.Models;

public static class GamesSerializer
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

    public static IEnumerable<Game> Deserialize(string text)
    {
        throw new NotImplementedException();
    }
}