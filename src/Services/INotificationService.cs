namespace light_quiz_api.Services
{
    public interface INotificationService
    {
        Task SendNotificationToDevice(string deviceToken, string title, string body);
        Task SendNotificationToTopic(string topic, string title, string body);
        Task AddUserToTopic(string[] userTokens, string topic);
    }
}
