namespace light_quiz_api.Models
{
    public class UserResult
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid QuizId { get; set; }
        public string QuizShortCode { get; set; } = string.Empty;
        public string? QuizTitle { get; set; }
        public int Grade { get; set; }
        public int PossiblePoints { get; set; } = 0;
        public int? CorrectQuestions { get; set; }
        public int? TotalQuestion { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        // Navigational properties
        public AppUser User { get; set; }
        public Quiz Quiz { get; set; }
    }
}
