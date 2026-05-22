using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.AuthenticationDTO;

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}