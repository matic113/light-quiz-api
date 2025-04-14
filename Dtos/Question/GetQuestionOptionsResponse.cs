namespace light_quiz_api.Dtos.Question
{
    public class GetQuestionOptionsResponse
    {
        public Guid OptionId { get; set; }
        public string OptionText { get; set; }
        public char OptionLetter { get; set; }
    }
}
