namespace light_quiz_api.Dtos.Question
{
    public class PostQuestionOptionRequest
    {
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public char OptionLetter { get; set; }
    }
}