namespace light_quiz_api.Services.Email.Models
{
    public class WelcomeEmailViewModel
    {
        public string UserName { get; set; } = null!;
        public string AppName { get; set; } = "Light-Quiz"; // Default app name
    }
}
