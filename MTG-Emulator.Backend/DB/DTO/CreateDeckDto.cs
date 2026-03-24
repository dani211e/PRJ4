namespace MTG_Emulator.Backend.DB.DTO
{
    public class CreateDeckDto
    {
        public string PlayerName { get; set; } = string.Empty;
        public string DeckName { get; set; } = string.Empty;
        public string Commander { get; set; } = string.Empty;
        public string CardList { get; set; } = string.Empty;
    }
}
