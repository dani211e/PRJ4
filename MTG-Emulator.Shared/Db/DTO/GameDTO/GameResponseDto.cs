namespace MTG_Emulator.Shared.Db.DTO.GameDTO;

public class GameResponseDto
{
    public bool   success        { get; set; }
    public string gameCode       { get; set; } = string.Empty;
    public int    maxPlayers     { get; set; }
    public int    currentPlayers { get; set; }
    public string message        { get; set; } = string.Empty;
    
    public List<string> playerNames { get; set; } = new();
    
    public string currentPlayerName  { get; set; } = string.Empty;
}