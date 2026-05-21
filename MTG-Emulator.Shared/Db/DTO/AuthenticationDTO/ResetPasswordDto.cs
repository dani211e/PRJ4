namespace MTG_Emulator.Unity.Db.DTO.AuthenticationDTO
{
    public class ResetPasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}