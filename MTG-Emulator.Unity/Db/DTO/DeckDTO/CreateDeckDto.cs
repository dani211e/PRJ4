using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.DeckDTO
{
    public class CreateDeckDto
    {
        [Required]
        public string DeckName { get; set; } = string.Empty;
        [Required]
        public string Commander { get; set; } = string.Empty;
        [Required]
        public string CardList { get; set; } = string.Empty;
    }
}
