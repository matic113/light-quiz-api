namespace light_quiz_api.Services
{
    public interface INotificationService
    {
        Task AddUserToTopic(string[] userTokens, string topic);
        Task NotifyGroupAsync(Guid groupId, string notificationTitle, string notificationBody);
        Task NotifyGroupForQuizAsync(Guid groupId, Guid quizId);
        Task RemoveUserFromTopic(string[] userTokens, string topic);
        Task SendNotificationToAll(string[] userTokens, string title, string body);
        Task SendNotificationToDevice(string deviceToken, string title, string body);
        Task SendNotificationToTopic(string topic, string title, string body);
    }
}
