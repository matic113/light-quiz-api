namespace light_quiz_api.Dtos.Question
{
    public class UpdateQuestionGradeRequest
    {
        public Guid QuestionId { get; set; }
        public int NewGrade { get; set; }
        public bool IsCorrect { get; set; }
    }
}
