using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.CardDTO
{
    public class CardDto
    {
        [Required]
        public int CardId { get; set; }
        [Required]
        public Guid ScryfallId {get; set;}
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string OracleText { get; set; } = string.Empty;
        [Required]
        public string ImageUri { get; set; } = string.Empty;
    }
}
