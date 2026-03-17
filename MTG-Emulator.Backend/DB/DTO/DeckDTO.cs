namespace MTG_Emulator.Backend.DB.DTO
{
    public class DeckDto
    {
        public string DeckName { get; set; } = string.Empty;
        public string DeckCommander { get; set; } = string.Empty;
        public List<CardDto> Cards { get; set; } = new();
    }
}
