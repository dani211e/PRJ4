using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.DTO
{
    public class CardDTO
    {
        [Required]
        public int CardId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string OracleText { get; set; } = string.Empty;
        [Required]
        public string ImageUri { get; set; } = string.Empty;
    }
}
