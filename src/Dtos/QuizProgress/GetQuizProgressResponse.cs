namespace light_quiz_api.Dtos.QuizProgress
{
    public class GetQuizProgressResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid QuizId { get; set; }
        public string Answers { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastSaved { get; set; }
    }
}
