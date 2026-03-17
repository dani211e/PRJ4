namespace MTG_Emulator.Backend.DB.DTO
{
    public class RelatedCardDto
    {
        public int RelatedCardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Uri { get; set; } = string.Empty;
    }
}
