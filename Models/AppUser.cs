using Microsoft.AspNetCore.Identity;

namespace light_quiz_api.Models
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }

        // Navigational Properties
        public ICollection<GroupMember> GroupMemberships { get; set; }
        public ICollection<Quiz> CreatedQuizzes { get; set; }
        public ICollection<QuizProgress> QuizProgresses { get; set; }
        public ICollection<UserResult> Results { get; set; }
        public ICollection<StudentAnswer> StudentAnswers { get; set; }
        public ICollection<Invitation> SentInvitations { get; set; }
        public ICollection<Invitation> ReceivedInvitations { get; set; }
    }
}
