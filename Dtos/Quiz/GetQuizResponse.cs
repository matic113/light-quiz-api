using light_quiz_api.Dtos.Question;

namespace light_quiz_api.Dtos.Quiz
{
    public class GetQuizResponse
    {
        public Guid QuizId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartsAtUTC { get; set; }
        public int DurationMinutes { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GetQuestionResponse> Questions { get; set; }
    }
}
