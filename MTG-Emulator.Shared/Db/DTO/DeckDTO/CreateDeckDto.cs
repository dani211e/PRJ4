using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Shared.Db.DTO.DeckDTO
{
    public class CreateDeckDto
    {
        [Required]
        public string DeckName { get; set; } = string.Empty;
        public List<string> CommandZone { get; set; } = [];
        [Required]
        public string CardList { get; set; } = string.Empty;
    }
}
