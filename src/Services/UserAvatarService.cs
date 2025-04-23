
namespace light_quiz_api.Services
{
    public class UserAvatarService : IUserAvatarService
    {
        private const string AvatarBaseUrl = "https://ui-avatars.com/api/";
        private readonly string[] _backgroundColors = ["0D8ABC", "FFBA00", "FF1654", "6A0572", "FFB300"];
        public string GenerateAvatarUrl(string userFullName)
        {
            if (string.IsNullOrWhiteSpace(userFullName))
            {
                userFullName = "NA";
            }

            var encodedName = Uri.EscapeDataString(userFullName);

            var random = new Random();
            var randomIndex = random.Next(_backgroundColors.Length);
            var backgroundColor = _backgroundColors[randomIndex];

            var avatarUrl = $"{AvatarBaseUrl}?name={encodedName}&background={backgroundColor}&color=fff&bold=true&size=128&font-size=0.5&rounded=true";

            return avatarUrl;
        }
    }
}
