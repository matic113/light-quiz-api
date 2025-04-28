namespace light_quiz_api.Dtos.Analytics
{
    public class TopQuestionsResponse
    {
        public string QuizShortCode { get; set; }
        public List<AnalyticsQuestionResponse> EasiestQuestions { get; set; }
        public List<AnalyticsQuestionResponse> HardestQuestions { get; set; }
    }
}
