
namespace MTG_Emulator.Backend.DB.Models
{
    public class Deck
    {
        public int DeckId { get; set; }
        public string DeckName { get; set; }
        public int DeckCommander {get; set;}
        public List<Card> Cards { get; set; }
        public Player Player { get; set; }
    }
}
