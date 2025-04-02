namespace light_quiz_api.Helpers
{
  public class GeminiSettings
  {
    public const string SectionName = "Gemini";
    public required string ApiKey { get; set; }
    public string ModelName { get; set; } = "models/gemini-2.0-flash-lite";
    public int MaxRequestsPerMinute { get; set; } = 15;
  }

}
