using light_quiz_api.Dtos.Question;

namespace light_quiz_api.Dtos.Quiz
{
    public class GetQuizReviewResponse
    {
        public Guid QuizId { get; set; }
        public string ShortCode { get; set; }
        public string Title{ get; set; }
        public string Description{ get; set; }
        public int Grade{ get; set; }
        public int PossiblePoints { get; set; }
        public int CorrectQuestions { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmissionDate{ get; set; }
        public DateTime GradingDate{ get; set; }
        public List<GetQuestionReviewResponse> Questions { get; set; }
    }
}
