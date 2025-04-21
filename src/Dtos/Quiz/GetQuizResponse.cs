using light_quiz_api.Dtos.Question;

namespace light_quiz_api.Dtos.Quiz
{
    public class GetQuizResponse
    {
        public Guid QuizId { get; set; }
        public Guid AttemptId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartsAtUTC { get; set; }
        public DateTime EndsAtUTC { get; set; }
        public int DurationMinutes { get; set; }
        public List<GetQuestionResponse> Questions { get; set; }
    }
}
