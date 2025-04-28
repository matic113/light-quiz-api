namespace light_quiz_api.Dtos.Analytics
{
    public class AnalyticsQuestionResponse
    {
        public string QuestionText { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int TotalAnswers { get; set; }
    }
}
