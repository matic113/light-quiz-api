namespace light_quiz_api.Dtos.Analytics
{
    public class TeacherStatsResponse
    {
        public Guid UserId { get; set; }
        public int TotalGroups { get; set; }
        public int QuizzesCreated { get; set; }
        public int TotalStudents { get; set; }
        public int TotalQuestions { get; set; }
        public int UpcomingQuizzesCount { get; set; }
    }
}
