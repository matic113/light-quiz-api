using System.Text.Json.Serialization;

namespace light_quiz_api.Dtos
{
    public class GradingResult
    {
        [JsonPropertyName("questionId")]
        public Guid QuestionId { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("confidence")]
        public int Confidence { get; set; }

        [JsonPropertyName("feedback")]
        public string Feedback { get; set; }
    }
}
