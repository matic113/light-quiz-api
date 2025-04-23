namespace light_quiz_api.Dtos
{
    public class GetUserInfo
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string AvatarUrl { get; set; }
    }
}
