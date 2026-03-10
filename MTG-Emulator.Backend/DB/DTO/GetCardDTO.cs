namespace MTG_Emulator.Backend.DB.DTO
{
    public class GetCardDTO
    {
        public int CardId { get; set; }
        public string Name { get; set; }
        public string OracleText { get; set; }
        public string ImageURI { get; set; }
    }
}
