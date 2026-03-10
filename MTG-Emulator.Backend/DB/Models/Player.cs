namespace MTG_Emulator.Backend.DB.Models
{
    public class Player
    {
        public int PlayerId { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDrawed { get; set; }
    }
}
