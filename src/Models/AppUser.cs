namespace light_quiz_api.Models
{
    public class AppUser : IdentityUser<Guid>
    {
        public string FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? DeviceToken { get; set; }

        // Navigational Properties
        public ICollection<GroupMember> GroupMemberships { get; set; }
        public ICollection<Quiz> CreatedQuizzes { get; set; }
        public ICollection<QuizAttempt> QuizAttempts { get; set; }
        public ICollection<UserResult> Results { get; set; }
        public ICollection<StudentQuizSubmission> QuizSubmissions { get; set; }
        public ICollection<StudentAnswer> StudentAnswers { get; set; }
        public ICollection<Invitation> SentInvitations { get; set; }
        public ICollection<Invitation> ReceivedInvitations { get; set; }
    }
}
