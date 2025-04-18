using System.ComponentModel.DataAnnotations;

namespace light_quiz_api.Dtos
{
    public class AuthModel
    {
        public bool IsAuthenticated { get; set; }
        public string FullName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
    }
}
