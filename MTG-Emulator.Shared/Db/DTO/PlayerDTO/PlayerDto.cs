using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.PlayerDTO
{
    public class PlayerDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public int GamesWon { get; set; }

        [Required]
        public int GamesLost { get; set; }

        [Required]
        public int GamesDrawed { get; set; }
    }
}
