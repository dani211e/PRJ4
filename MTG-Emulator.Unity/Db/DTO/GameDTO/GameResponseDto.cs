namespace MTG_Emulator.Unity.Db.DTO.GameDTO;

public class GameResponseDto
{
    public bool   Success        { get; set; }
    public string GameCode       { get; set; } = string.Empty;
    public int    MaxPlayers     { get; set; }
    public int    CurrentPlayers { get; set; }
    public string Message        { get; set; } = string.Empty;
    
    public List<string> PlayerNames { get; set; } = new();
    
    public string CurrentPlayerName  { get; set; } = string.Empty;
}