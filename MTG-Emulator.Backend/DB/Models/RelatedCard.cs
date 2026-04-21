using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class RelatedCard
    {
        public int RelatedCardId { get; set; }

        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        [StringLength(256)]
        public string URI { get; set; } = string.Empty;

        public Card Card { get; set; } = new();
    }
}
