using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class AltFace
    {
        public int AltFaceId { get; set; }

        [StringLength(1024)]
        public string OracleText { get; set; } = string.Empty;

        [StringLength(256)]
        public string ImageURI { get; set; } = string.Empty;

        public int CardId { get; set; }
        public Card Card { get; set; } = null!;
    }
}
