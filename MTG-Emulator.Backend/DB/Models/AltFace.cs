namespace MTG_Emulator.Backend.DB.Models
{
    public class AltFace
    {
        public int AltFaceId { get; set; }
        public string OracleText { get; set; }
        public string ImageURI { get; set; }
        public int CardId { get; set; }
        public Card Card { get; set; } = null!;
    }
}
