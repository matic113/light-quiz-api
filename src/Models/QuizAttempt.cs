namespace light_quiz_api.Models
{
    public class QuizAttempt
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public Guid QuizId { get; set; }
        public DateTime AttemptStartTimeUTC { get; set; }
        public DateTime AttemptEndTimeUTC { get; set; }
        public DateTime LastSaved { get; set; }
        public AttemptState State { get; set; }

        // Navigational properties
        public AppUser Student { get; set; }
        public Quiz Quiz { get; set; }
    }

    public enum AttemptState
    {
        InProgress,
        Submitted,
        AutomaticallySubmitted,
        TimedOut
    }
}
