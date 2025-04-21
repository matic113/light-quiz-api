namespace light_quiz_api.Dtos.Question
{
    public class PostQuestionRequest
    {
        public string QuestionText { get; set; }
        public int QuestionTypeId { get; set; }
        public int Points{ get; set; }
        public string? CorrectAnswer{ get; set; }
        public int QuestionNumber { get; set; }
        public List<PostQuestionOptionRequest> Options { get; set; }
    }
}
