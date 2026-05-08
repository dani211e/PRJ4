using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.CardFace
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
