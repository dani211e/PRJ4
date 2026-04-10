using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.DTO
{
    public class CreateDeckDTO
    {
        [Required]
        public string PlayerName { get; set; } = string.Empty;
        [Required]
        public string DeckName { get; set; } = string.Empty;
        [Required]
        public string Commander { get; set; } = string.Empty;
        [Required]
        public string CardList { get; set; } = string.Empty;
    }
}
