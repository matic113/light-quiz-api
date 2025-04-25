namespace light_quiz_api.Dtos.Student
{
    public class GetStudentResultResponse
    {
        public Guid StudentId { get; set; }
        public Guid QuizId { get; set; }
        public string QuizShortCode { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int Grade { get; set; }
        public int PossiblePoints { get; set; } = 0;
        public int? CorrectQuestions { get; set; }
        public int? TotalQuestions { get; set; }
    }
}
