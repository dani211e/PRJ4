using Microsoft.AspNetCore.Identity;

namespace MTG_Emulator.Backend.DB.Models
{
    public class ApiUser : IdentityUser
    {
        public Player? Player { get; set; }
    }   
}