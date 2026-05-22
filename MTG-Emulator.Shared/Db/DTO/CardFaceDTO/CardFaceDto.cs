using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Shared.Db.DTO.CardFaceDTO
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
