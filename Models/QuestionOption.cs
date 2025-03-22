namespace light_quiz_api.Models
{
    public class QuestionOption
    {
        public Guid Id { get; set; }
        public Guid QuestionId { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrect { get; set; }
        public char OptionLetter { get; set; }

        // Navigational properties
        public Question Question { get; set; }
    }
}
