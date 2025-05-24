namespace light_quiz_api.Services.Email.Models
{
    public class PasswordRecoveryViewModel
    {
        public string UserName { get; set; } = null!;
        public string RecoveryLink { get; set; } = null!;
        public string AppName { get; set; } = "Light-Quiz"; // Default app name
    }
}
