namespace MTG_Emulator.Backend.DB.DTO
{
    public class CardDto
    {
        public int CardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OracleText { get; set; } = string.Empty;
        public string ImageUri { get; set; } = string.Empty;
    }
}
