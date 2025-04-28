namespace light_quiz_api.Dtos.Analytics
{
    public class GetQuizAnalyticsResponse
    {
        public Guid QuizId { get; set; }
        public string QuizShortCode { get; set; }
        public string QuizName { get; set; }
        public DateTime QuizDateTime{ get; set; }
        public int NumberOfQuestions{ get; set; }
        public int QuizDuration{ get; set; }
        public int NumberOfStudents{ get; set; }
        public int StudentsAttended{ get; set; }
        public int PossiblePoints { get; set; }
        public List<int> StudentGrades { get; set; }
        public List<int> StudentSecondsSpent{ get; set; }
    }
}
