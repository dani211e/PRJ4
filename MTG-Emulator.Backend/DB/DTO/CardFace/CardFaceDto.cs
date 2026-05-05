using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.DTO.CardFace
{
    public class CardFaceDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string OracleText { get; set; } = string.Empty;
        [Required]
        public string ImageUri { get; set; } = string.Empty;
    }
}
