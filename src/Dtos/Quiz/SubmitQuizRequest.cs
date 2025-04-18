namespace light_quiz_api.Dtos.Quiz
{
    public class SubmitQuizRequest
    {
        public Guid QuizId { get; set; }
        public Guid StudentId { get; set; }
        public List<SubmitQuestionAnswer> Answers { get; set; }
    }
}
