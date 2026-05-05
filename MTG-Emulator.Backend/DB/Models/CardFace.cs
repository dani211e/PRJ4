using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class CardFace
    {
        public int AltFaceId { get; set; }

        // Same as Card.OracleText
        [StringLength(2048)]
        public string OracleText { get; set; } = string.Empty;

        [StringLength(256)]
        public string ImageUri { get; set; } = string.Empty;

        public int CardId { get; set; }
        public Card Card { get; set; } = null!;
    }
}
