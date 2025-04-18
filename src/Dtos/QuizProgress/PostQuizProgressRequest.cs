namespace light_quiz_api.Dtos.QuizProgress
{
    public class PostQuizProgressRequest
    {
        public Guid AttemptId { get; set; }
        public List<QuestionAnswer> QuestionsAnswers { get; set; }
    }
}
