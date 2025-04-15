namespace light_quiz_api.Models
{
    public class StudentQuizSubmission
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid QuizId { get; set; }
        public DateTime SubmittedAt { get; set; }

        // Navigational properties
        public AppUser Student { get; set; }
        public Quiz Quiz { get; set; }
    }
}
