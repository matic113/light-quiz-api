namespace light_quiz_api.Services
{
    public interface IGradingService
    {
        Task GradeQuizAsync(Guid studentId, Guid quizId);
    }
}
