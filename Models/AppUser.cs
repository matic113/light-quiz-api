using Microsoft.AspNetCore.Identity;

namespace light_quiz_api.Models
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }
    }
}
