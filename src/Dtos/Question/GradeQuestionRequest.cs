namespace light_quiz_api.Dtos.Question
{
    public class GradeQuestionRequest
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; }
        public string StudentAnswer { get; set; } = string.Empty;

    }
}
