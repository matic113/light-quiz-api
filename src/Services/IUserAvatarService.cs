namespace light_quiz_api.Services
{
    public interface IUserAvatarService
    {
        string GenerateAvatarUrl(string userFullName);
    }
}
