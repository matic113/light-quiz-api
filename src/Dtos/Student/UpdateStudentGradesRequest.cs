using light_quiz_api.Dtos.Question;

namespace light_quiz_api.Dtos.Student
{
    public class UpdateStudentGradesRequest
    {
        public Guid QuizId { get; set; }
        public Guid StudentId { get; set; }
        public List<UpdateQuestionGradeRequest> Questions { get; set; }
    }
}
