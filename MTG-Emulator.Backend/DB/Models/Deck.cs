using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class Deck
    {
        public int DeckId { get; set; }

        [StringLength(256)]
        public string DeckName { get; set; } = string.Empty;

        public List<Card> CommandZone { get; set; } = [];

        public List<DeckCard> DeckCards { get; set; } = [];
        public Player Player { get; set; } = new();
    }
}
