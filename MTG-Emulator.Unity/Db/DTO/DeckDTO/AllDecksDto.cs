using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.DeckDTO
{
    public class AllDecksDto
    {
        [Required]
        public int DeckId { get; set; }
        [Required]
        public string DeckName { get; set; } = string.Empty;
        [Required]
        public string DeckCommander { get; set; } = string.Empty;
        [Required]
        public string DeckImageUri { get; set; } = string.Empty;
    }   
}