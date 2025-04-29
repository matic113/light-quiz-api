namespace light_quiz_api.Dtos.Analytics
{
    public class TopPerformerResponse
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string AvatarUrl { get; set; }
        public int QuizzesTaken { get; set; }
        public int AvgTimeSeconds { get; set; }
        public int TotalScore { get; set; }
    }
}
