using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class Card
    {
        public int CardId { get; set; }
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;
        [StringLength(1024)]
        public string OracleText { get; set; } = string.Empty;
        [StringLength(128)]
        public string ImageUri { get; set; } = string.Empty;
        public List<RelatedCard> RelatedCard { get; set; } = [];
        public List<Deck> Decks { get; set; } = [];
        public AltFace? AltFace { get; set; } = new AltFace();
    }
}
