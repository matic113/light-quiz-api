namespace light_quiz_api.Dtos.Quiz
{
    public class GetQuizMetadataResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeAllowed { get; set; }
        public DateTime StartsAt { get; set; }
        public int NumberOfQuestions { get; set; }
    }
}
