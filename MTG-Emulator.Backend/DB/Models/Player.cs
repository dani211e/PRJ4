using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        [StringLength(256)]
        public string Username { get; set; } = string.Empty;

        [StringLength(256)]
        public string Password { get; set; } = string.Empty;

        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDrawed { get; set; }
    }
}
