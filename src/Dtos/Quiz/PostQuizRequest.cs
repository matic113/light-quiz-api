using light_quiz_api.Dtos.Question;

namespace light_quiz_api.Dtos.Quiz
{
    public class PostQuizRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartsAtUTC { get; set; }
        public int DurationMinutes { get; set; }
        public bool? Anonymous { get; set; }
        public Guid? GroupId { get; set; }
        public bool? Randomize { get; set; }
        public List<PostQuestionRequest> Questions { get; set; } = new List<PostQuestionRequest>();
    }
}
