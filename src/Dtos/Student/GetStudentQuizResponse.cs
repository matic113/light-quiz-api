namespace light_quiz_api.Dtos.Student
{
    public class GetStudentQuizResponse
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public Guid QuizId { get; set; }
        public string QuizShortCode { get; set; }
        public int Grade { get; set; }
        public int PossiblePoints { get; set; } = 0;
        public DateTime GradedAt { get; set; }
        public int? CorrectQuestions { get; set; }
        public int? TotalQuestions { get; set; }
    }
}
