namespace light_quiz_api.Models
{
    public class UserResult
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid QuizId { get; set; }
        public int Grade { get; set; }
        public int? CorrectQuestions { get; set; }
        public int? TotalQuestion { get; set; }

        // Navigational properties
        public AppUser User { get; set; }
        public Quiz Quiz { get; set; }
    }
}
