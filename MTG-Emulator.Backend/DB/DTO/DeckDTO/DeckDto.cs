using System.ComponentModel.DataAnnotations;
using MTG_Emulator.Backend.DB.DTO.CardDTO;

namespace MTG_Emulator.Backend.DB.DTO.DeckDTO
{
    public class DeckDto
    {
        [Required]
        public string DeckName { get; set; } = string.Empty;
        [Required]
        public string DeckCommander { get; set; } = string.Empty;
        [Required]
        public List<CardDto> Cards { get; set; } = [];
    }
}
