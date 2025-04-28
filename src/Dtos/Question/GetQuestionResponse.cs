namespace light_quiz_api.Dtos.Question
{
    public class GetQuestionResponse
    {
        public Guid QuestionId { get; set; }
        public Guid QuizId { get; set; }
        public string Text { get; set; }
        public string? ImageUrl { get; set; }
        public int TypeId { get; set; }
        public int Points { get; set; }
        public List<GetQuestionOptionsResponse> Options { get; set; }
    }
}
