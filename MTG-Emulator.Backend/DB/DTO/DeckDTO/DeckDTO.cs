using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.DTO
{
    public class DeckDTO
    {
        [Required]
        public string DeckName { get; set; } = string.Empty;
        [Required]
        public string DeckCommander { get; set; } = string.Empty;
        [Required]
        public List<CardDTO> Cards { get; set; } = [];
    }
}
