using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.PlayerDTO
{
    public class LoginResponseDto
    {
        public string Username { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }
}