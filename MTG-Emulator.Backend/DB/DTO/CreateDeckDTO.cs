using MTG_Emulator.Backend.DB.Models;

namespace MTG_Emulator.Backend.DB.DTO
{
    public class CreateDeckDTO
    {
        public string PlayerName { get; set; }
        public string DeckName { get; set; }
        public string Commander {get; set;}
        public string CardList {get; set;}
        public List<Card> Cards {get; set;}
    }
}
