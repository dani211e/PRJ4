using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.DTO
{
    public class PlayerDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public int GamesWon { get; set; }
        [Required]
        public int GamesLost { get; set; }
        [Required]
        public int GamesDrawed { get; set; }
        [Required]
        public List<string> DeckNames { get; set; } = new();
    }
}
