using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Shared.Db.DTO.GameDTO;

public class CreateGameDto
{
    [Required]
    [Range(1,5)]
    public int    MaxPlayers { get; set; }
    [Required]
    public string HostName   { get; set; } = string.Empty;
}