namespace light_quiz_api.Dtos.Quiz
{
    public class GetQuizMetadataResponse
    {
        public Guid QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int TimeAllowed { get; set; }
        public DateTime StartsAt { get; set; }
        public int NumberOfQuestions { get; set; }
        public bool DidStartQuiz { get; set; } = false;
        public Guid? GroupId { get; set; }
        public bool Anonymous { get; set; } = false;
    }
}
