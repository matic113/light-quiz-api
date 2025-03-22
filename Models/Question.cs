namespace light_quiz_api.Models
{
    public class Question
    {
        public Guid Id { get; set; }
        public Guid QuizId { get; set; }
        public required string QuestionText { get; set; }
        public int QuestionTypeId { get; set; }
        public int Points { get; set; }
        public string? CorrectAnswer { get; set; }

        // Navigational properties
        public Quiz Quiz { get; set; }
        public QuestionType QuestionType { get; set; }
        public ICollection<QuestionOption> QuestionOptions { get; set; }
        public ICollection<StudentAnswer> StudentAnswers { get; set; }
    }
}
