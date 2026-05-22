using System.ComponentModel.DataAnnotations;

namespace MTG_Emulator.Unity.Db.DTO.AuthenticationDTO
{
    public class ResetPasswordDto
    {
        public string? TargetUserId { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}