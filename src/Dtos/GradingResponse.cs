using System.Text.Json.Serialization;

namespace light_quiz_api.Dtos
{
    public class GradingResponse
    {
        [JsonPropertyName("results")]
        public List<GradingResult> Results { get; set; }
    }
}
