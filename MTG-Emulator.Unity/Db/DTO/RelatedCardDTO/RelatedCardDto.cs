using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.RelatedCardDTO
{
    public class RelatedCardDto
    {
        [Required]
        public int RelatedCardId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string ImageUri { get; set; } = string.Empty;
    }
}
