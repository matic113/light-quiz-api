namespace light_quiz_api.Dtos.Student
{
    public class GetStudentResultResponse
    {
        public Guid StudentId { get; set; }
        public Guid QuizId { get; set; }
        public int Grade { get; set; }
        public int? CorrectQuestions { get; set; }
        public int? TotalQuestions { get; set; }
    }
}
