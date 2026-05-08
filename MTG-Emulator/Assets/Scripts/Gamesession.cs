public static class GameSession
{
    // Set this after successful login
    public static string PlayerName    { get; set; } = string.Empty;
 
    // Set after CreateGame or JoinGame succeeds
    public static string GameCode      { get; set; } = string.Empty;
    public static int    MaxPlayers    { get; set; } = 4;
    public static bool   IsHost        { get; set; } = false;
 
    public static void ResetGame()
    {
        GameCode   = string.Empty;
        MaxPlayers = 4;
        IsHost     = false;
    }
}