namespace MTG_Emulator.Backend.DB.Models
{
    public class Card
    {
        public int CardId { get; set; }
        public string Name { get; set; }
        public string OracleText { get; set; }
        public string ImageURI { get; set; }
        public List<RelatedCard> RelatedCard { get; set; }
        public List<Deck> Decks { get; set; }
        public AltFace? AltFace { get; set; }
    }
}
