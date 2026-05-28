using UnityEngine;

public static class GameSession
{
    public static string PlayerName { get; set; } = PlayerPrefs.GetString("username");
    public static int PlayerId { get; set; }

    public static string GameCode { get; set; } = "------";
    public static int MaxPlayers { get; set; } = 4;
    public static bool IsHost { get; set; } = false;


    public static void ResetGame()
    {
        GameCode = string.Empty;
        MaxPlayers = 5;
        IsHost = false;
    }
}