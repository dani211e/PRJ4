using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Backend.DB.Models
{
    public class Game
    {
        public int GameId { get; set; }

        [StringLength(6)]
        public string GameCode { get; set; } = string.Empty;

        public int MaxPlayers { get; set; }

        public int CurrentPlayers { get; set; } = 1; // host counts as 1

        [StringLength(256)]
        public string HostName { get; set; } = string.Empty;

        // "Waiting" | "InProgress" | "Finished"
        [StringLength(32)]
        public string Status { get; set; } = "Waiting";
    }
}