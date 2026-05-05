using System.ComponentModel.DataAnnotations;
using MTG_Emulator.Backend.DB.DTO.CardFace;
using MTG_Emulator.Backend.DB.DTO.RelatedCardsDTO;

namespace MTG_Emulator.Backend.DB.DTO.CardDTO
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

        public CardFaceDto? AltFace { get; set; }
        [Required]
        public List<RelatedCardDto>  RelatedCards { get; set; } = [];
    }
}
