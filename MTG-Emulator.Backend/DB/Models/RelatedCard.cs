namespace MTG_Emulator.Backend.DB.Models
{
    public class RelatedCard
    {
        public int RelatedCardId { get; set; }
        public string Name { get; set; }
        public string URI { get; set; }
        public Card Card { get; set; }
    }
}
