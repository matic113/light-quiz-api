using System.ComponentModel.DataAnnotations;

namespace light_quiz_api.Dtos
{
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserType { get; set; }
    }
}
