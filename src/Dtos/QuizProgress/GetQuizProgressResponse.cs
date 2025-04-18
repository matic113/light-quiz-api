namespace light_quiz_api.Dtos.QuizProgress
{
    public class GetQuizProgressResponse
    {
        public Guid AttemptId { get; set; }
        public List<QuestionAnswer> QuestionsAnswers { get; set; }
        public DateTime LastSaved { get; set; }
        public DateTime AttemptStartTimeUTC { get; set; }
        public DateTime AttemptEndTimeUTC { get; set; }
    }
}
