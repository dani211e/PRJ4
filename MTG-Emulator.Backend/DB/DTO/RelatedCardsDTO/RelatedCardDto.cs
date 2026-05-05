using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.DTO.RelatedCardsDTO
{
    public class RelatedCardDto
    {
        [Required]
        public int RelatedCardId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Uri { get; set; } = string.Empty;
    }
}
