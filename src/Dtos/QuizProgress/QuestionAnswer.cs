namespace light_quiz_api.Dtos.QuizProgress
{
    public class QuestionAnswer
    {
        public Guid QuestionId { get; set; }
        public string? AnswerText { get; set; }
        public char? AnswerOptionLetter { get; set; }
    }
}
