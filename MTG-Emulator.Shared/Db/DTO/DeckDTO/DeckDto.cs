using System.ComponentModel.DataAnnotations;
using MTG_Emulator.Shared.Db.DTO.CardDTO;

namespace MTG_Emulator.Shared.Db.DTO.DeckDTO
{
    public class DeckDto
    {
        [Required]
        public int DeckId { get; set; }
        [Required]
        public string DeckName { get; set; } = string.Empty;
        [Required]
        public List<CardDto> CommandZone { get; set; } = new();
        [Required]
        public List<CardDto> Cards { get; set; } = [];
    }
}
