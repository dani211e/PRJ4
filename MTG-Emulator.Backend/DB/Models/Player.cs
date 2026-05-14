using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class Player
    {
        public int PlayerId { get; set; }

        [StringLength(256)]
        public string Username { get; set; } = string.Empty;
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDrawn { get; set; }
        
        // Link to Identity user (It needs to be 450 since the foreign key is a nvarchar(450)
        [StringLength(450)]
        public string ApiUserId { get; set; } = string.Empty;
        public ApiUser ApiUser { get; set; } = null!;
        
        public List<Deck> Decks { get; set; } = [];
    }
}
