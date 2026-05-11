using System.ComponentModel.DataAnnotations;
using MTG_Emulator.Unity.Db.DTO.CardDTO;

namespace MTG_Emulator.Unity.Db.DTO.DeckDTO
{
    public class DeckDto
    {
        [Required]
        public int DeckId { get; set; }
        [Required]
        public string DeckName { get; set; } = string.Empty;
        [Required]
        public string DeckCommander { get; set; } = string.Empty;
        [Required]
        public List<CardDto> Cards { get; set; } = [];
    }
}
