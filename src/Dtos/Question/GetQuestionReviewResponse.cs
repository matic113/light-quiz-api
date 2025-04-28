namespace light_quiz_api.Dtos.Question
{
    public class GetQuestionReviewResponse
    {
        public string QuestionText { get; set; }
        public string? ImageUrl { get; set; }
        public List<GetQuestionOptionsResponse>? Options{ get; set; }
        public int Points { get; set; }
        public string? StudentAnsweredText { get; set; }
        public char? StudentAnsweredOption{ get; set; }
        public char? CorrectOption { get; set; }
        public bool IsCorrect { get; set; }
        public string? FeedbackMessage { get; set; }
    }
}
