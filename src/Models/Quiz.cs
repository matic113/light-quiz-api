namespace light_quiz_api.Models
{
    public class Quiz
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public DateTime StartsAt { get; set; }
        public int DurationMinutes { get; set; }
        public Guid? GroupId { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int NumberOfQuestions { get; set; }
        public int PossiblePoints { get; set; } = 0;
        public bool Anonymous { get; set; }
        public bool Randomize { get; set; } = false;
        public string? ShortCode { get; set; }

        // Navigation properties
        public Group Group { get; set; }
        public AppUser CreatedByUser { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<QuizAttempt> QuizAttempts { get; set; }
        public ICollection<UserResult> UserResults { get; set; }
        public ICollection<StudentQuizSubmission> StudentSubmissions { get; set; }
        public ICollection<StudentAnswer> StudentAnswers { get; set; }
    }
}
