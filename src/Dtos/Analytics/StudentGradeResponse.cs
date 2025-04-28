namespace light_quiz_api.Dtos.Analytics
{
    public class StudentGradeResponse
    {
        public Guid StudentId { get; set; }
        public string FullName { get; set; }
        public int Score { get; set; }
        public int SecondsSpent { get; set; }
        public DateTime SubmissionDate { get; set; }
    }
}
