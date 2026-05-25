using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.GameDTO;

public class JoinGameDto
{
    [Required]
    public string GameCode   { get; set; } = string.Empty;
}