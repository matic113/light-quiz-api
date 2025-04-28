namespace light_quiz_api.Dtos.Analytics
{
    public class TopStudentsResponse
    {
        public Guid QuizId { get; set; }
        public List<StudentGradeResponse> StudentsGrades { get; set; }
    }
}
