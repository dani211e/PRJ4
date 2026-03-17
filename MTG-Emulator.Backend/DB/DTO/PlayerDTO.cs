namespace MTG_Emulator.Backend.DB.DTO
{
    public class PlayerDto
    {
        public string Username { get; set; }
        public int GamesWon { get; set; }
        public int GamesLost { get; set; }
        public int GamesDrawed { get; set; }
        public List<string> DeckNames { get; set; } = new();
    }
}
