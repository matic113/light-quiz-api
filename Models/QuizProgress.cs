namespace light_quiz_api.Models
{
    public class QuizProgress
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid QuizId { get; set; }
        public string Answers { get; set; } // JSONB
        public long RemainingTimeSeconds { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastSaved { get; set; }
        
        // Navigational properties
        public AppUser User { get; set; }
        public Quiz Quiz { get; set; }
    }
}
