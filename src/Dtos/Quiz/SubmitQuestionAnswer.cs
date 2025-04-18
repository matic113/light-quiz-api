namespace light_quiz_api.Dtos.Quiz
{
    public class SubmitQuestionAnswer
    {
        public Guid QuestionId { get; set; }
        public char? OptionLetter { get; set; }
        public string? AnswerText { get; set; }
    }
}
