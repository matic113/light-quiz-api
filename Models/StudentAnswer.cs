namespace light_quiz_api.Models
{
    public class StudentAnswer
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid QuizId { get; set; }
        public Guid QuestionId { get; set; }
        public char? AnswerOptionLetter { get; set; }
        public string? AnswerText { get; set; }

        // Navigational properties
        public AppUser User { get; set; }
        public Quiz Quiz { get; set; }
        public Question Question { get; set; }
    }
}
