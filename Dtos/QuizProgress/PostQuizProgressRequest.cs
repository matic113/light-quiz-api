namespace light_quiz_api.Dtos.QuizProgress
{
    public class PostQuizProgressRequest
    {
        public Guid QuizId { get; set; }
        public Guid StudentId { get; set; }
        public string Answers { get; set; }
        public int RemainingTimeSeconds { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastSaved { get; set; }
    }
}
