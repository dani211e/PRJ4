namespace MTG_Emulator.Unity.Db.DTO.GameDTO;

public class GameResponseDto
{
    public bool   success        { get; set; }
    public string gameCode       { get; set; } = string.Empty;
    public int    maxPlayers     { get; set; }
    public int    currentPlayers { get; set; }
    public string message        { get; set; } = string.Empty;
}