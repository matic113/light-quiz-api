using System.ComponentModel.DataAnnotations;

namespace light_quiz_api.Dtos.Group
{
    public class EmailEntry
    {
        [EmailAddress]
        public string Email { get; set; } = "";

        public string NormalizedEmail () => Email.ToUpperInvariant();
    }
}
