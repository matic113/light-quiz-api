namespace light_quiz_api.Services
{
  public interface IGeminiService
  {
    Task<string?> GetGeminiResponseAsync(string prompt, CancellationToken cancellationToken = default);
    Task<string?> GradeStudentAnswersAsync(string prompt, CancellationToken cancellationToken = default);
  }
}