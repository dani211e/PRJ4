using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class Card
    {
        public int CardId { get; set; }

        public Guid ScryfallId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        // Length constraint is based on the max length found in a card at commit time
        [StringLength(2048)]
        public string OracleText { get; set; } = string.Empty;

        [StringLength(128)]
        public string ImageUri { get; set; } = string.Empty;

        public List<RelatedCard> RelatedCard { get; set; } = [];
        public List<Deck> Decks { get; set; } = [];
        public CardFace? AltFace { get; set; } = new();
    }
}
