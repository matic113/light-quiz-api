
using FirebaseAdmin.Messaging;

namespace light_quiz_api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task AddUserToTopic(string[] userTokens, string topic)
        {
            await FirebaseMessaging.DefaultInstance.SubscribeToTopicAsync(userTokens, topic);
        }
        public async Task RemoveUserFromTopic(string[] userTokens, string topic)
        {
            await FirebaseMessaging.DefaultInstance.UnsubscribeFromTopicAsync(userTokens, topic);
        }
        public async Task SendNotificationToAll(string[] userTokens,string title, string body)
        {
            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(new MulticastMessage()
            {
                Tokens = userTokens,
                Notification = new Notification()
                {
                    Title = title,
                    Body = body
                }
            });
            Console.WriteLine($"Successfully sent message to {response.SuccessCount} devices out of {userTokens.Length} tokens.");
        }
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
        public async Task NotifyGroupForQuizAsync(Guid groupId)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.Member)
                .Include(g => g.Quizzes)
                .SingleOrDefaultAsync(g => g.Id == groupId);

            if (group is null)
            {
                _logger.LogWarning($"Group with Id {groupId} not found but was supposed to be notified");
                return;
            }

            var members = group.GroupMembers.Select(g => g.Member).ToList();
            var tokens = members.Select(m => m.DeviceToken).ToList();

            if (tokens.Count == 0)
            {
                _logger.LogWarning($"No device tokens found for group {groupId}");
                return;
            }

            var latestQuiz = group.Quizzes
                .OrderBy(q => q.StartsAt)
                .FirstOrDefault();
            if (latestQuiz is null)
            {
                _logger.LogWarning($"No quizzes found for group {groupId}");
                return;
            }

            // Send notification to all members of the group
            var message = new MulticastMessage()
            {
                Tokens = tokens,
                Notification = new Notification()
                {
                    Title = $"Quiz: {latestQuiz.Title} is upcoming",
                    Body = $"A new quiz is available for group {group.Name}"
                }
            };

            var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            Console.WriteLine($"Successfully sent message to {response.SuccessCount} devices out of {tokens.Count} tokens.");
        }
    }
}
