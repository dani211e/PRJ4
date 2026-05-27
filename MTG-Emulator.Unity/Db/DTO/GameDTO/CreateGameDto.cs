using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.GameDTO;

public class CreateGameDto
{
    [Required]
    [Range(1,5)]
    public int MaxPlayers { get; set; }
}