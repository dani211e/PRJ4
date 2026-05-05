using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class CardFace
    {
        public int CardFaceId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        // Same as Card.OracleText
        [StringLength(2048)]
        public string OracleText { get; set; } = string.Empty;

        [StringLength(256)]
        public string ImageUri { get; set; } = string.Empty;

        public int CardId { get; set; }
        public Card Card { get; set; } = null!;
    }
}
