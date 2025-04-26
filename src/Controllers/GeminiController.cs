using Hangfire;
using light_quiz_api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeminiController : ControllerBase
    {
        private readonly IGeminiService _geminiService;
        private readonly ILogger<GeminiController> _logger;
        public GeminiController(IGeminiService geminiService, ILogger<GeminiController> logger, IBackgroundJobClient backgroundJobClient, IGradingService gradingService)
        {
            _geminiService = geminiService;
            _logger = logger;
        }

        public record PromptRequest(string Prompt);

        [HttpPost("prompt")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateContent([FromBody] PromptRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                _logger.LogWarning("Received invalid prompt request.");
                return BadRequest("Prompt cannot be empty.");
            }

            _logger.LogInformation("Received request to generate content for prompt.");

            try
            {
                var response = await _geminiService.GetGeminiResponseAsync(request.Prompt, cancellationToken);

                if (response == null)
                {
                    // This could happen if the service handled an error internally and returned null
                    _logger.LogError("Gemini service returned null response for prompt: {Prompt}", request.Prompt);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get response from AI service.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions bubbled up from the service
                _logger.LogError(ex, "An unexpected error occurred while processing the prompt: {Prompt}", request.Prompt);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
