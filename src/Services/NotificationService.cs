
using FirebaseAdmin.Messaging;

namespace light_quiz_api.Services
{
    public class NotificationService : INotificationService
    {
        public async Task SendNotificationToDevice(string deviceToken, string title, string body)
        {
            var message = new Message()
            {
                Token = deviceToken,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                }
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            Console.WriteLine($"Successfully sent message to a device: {response}");
        }

        public async Task SendNotificationToTopic(string topic, string title, string body)
        {
            var message = new Message()
            {
                Topic = topic,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                }
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            Console.WriteLine($"Successfully sent message to topic: {response}");
        }
    }
}
