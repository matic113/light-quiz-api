using GenerativeAI;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Threading.RateLimiting;

namespace gemini_test.Services
{
  public class GeminiService : IGeminiService
  {
    private readonly GoogleAi _googleAI;
    private readonly GeminiSettings _settings;
    private readonly ILogger<GeminiService> _logger;
    private readonly RateLimiter _rateLimiter;

    public GeminiService(IOptions<GeminiSettings> settings, ILogger<GeminiService> logger, RateLimiter rateLimiter)
    {
      _settings = settings.Value;
      _logger = logger;
      _rateLimiter = rateLimiter;

      // Validate API Key 
      if (string.IsNullOrWhiteSpace(_settings.ApiKey))
      {
        _logger.LogError("Gemini API Key is missing or empty. Please configure it in appsettings or User Secrets under the 'Gemini' section.");
        // Throwing here ensures the service cannot be used without a key
        throw new InvalidOperationException("Gemini API Key is not configured.");
      }

      // Validate Model Name
      if (string.IsNullOrWhiteSpace(_settings.ModelName))
      {
        _logger.LogError("Gemini Model Name is missing or empty. Please configure it in appsettings under the 'Gemini' section.");
        throw new InvalidOperationException("Gemini Model Name is not configured.");
      }

      _googleAI = new GoogleAi(_settings.ApiKey);
    }

    public async Task<string?> GradeStudentAnswersAsync(string prompt, CancellationToken cancellationToken = default)
    {
      RateLimitLease? lease = null;
      try
      {
        _logger.LogDebug("Attempting to acquire rate limit permit.");

        lease = await _rateLimiter.AcquireAsync(1, cancellationToken);

        if (lease.IsAcquired)
        {

          using (lease)
          {
            var googleModel = _googleAI.CreateGenerativeModel(_settings.ModelName);

            var googleResponse = await googleModel.GenerateContentAsync(prompt, cancellationToken: cancellationToken);

            var responseText = googleResponse.Text();
            return responseText;
          }
        }
        else
        {
          // Lease was not acquired. This happens if the queue is full (QueueLimit reached).
          // Throw a specific exception that the controller can catch
          throw new Exception("Gemini API rate limit exceeded and queue is full.");
        }
      }
      catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
      {
        _logger.LogWarning("Operation cancelled while waiting for rate limit permit or calling Gemini API.");
        throw;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "An error occurred while acquiring permit or calling the Gemini API. Prompt: {Prompt}", prompt);
        // Dispose the lease manually if acquired but failed before the 'using' block completed fully
        lease?.Dispose();
        // Re-throw or handle as appropriate (e.g., return null or throw a custom service exception)
        throw new ApplicationException("An unexpected error occurred contacting the AI service.", ex);
      }
    }

    public async Task<string?> GetGeminiResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
      if (string.IsNullOrWhiteSpace(prompt))
      {
        _logger.LogWarning("Received an empty or whitespace prompt.");
        return null;
      }

      RateLimitLease? lease = null;
      var stopwatch = Stopwatch.StartNew(); // Optional: time the wait

      try
      {
        _logger.LogDebug("Attempting to acquire rate limit permit.");

        // Attempt to acquire a permit asynchronously.
        // This will wait if the limit is reached and the queue has space,
        // until a permit is available or cancellation is requested.
        // Pass permitCount: 1 as we need one permit per API call.
        lease = await _rateLimiter.AcquireAsync(1, cancellationToken);

        stopwatch.Stop(); // Stop timing the wait

        if (lease.IsAcquired)
        {
          _logger.LogInformation("Rate limit permit acquired after {WaitTimeMs}ms. Proceeding with Gemini API call.", stopwatch.ElapsedMilliseconds);

          // --- IMPORTANT: Release the permit when done ---
          // The using statement ensures lease.Dispose() is called even if exceptions occur below
          using (lease)
          {
            _logger.LogInformation("Generating content for prompt using model: {ModelName}", _settings.ModelName);
            var googleModel = _googleAI.CreateGenerativeModel(_settings.ModelName);

            var googleResponse = await googleModel.GenerateContentAsync(prompt, cancellationToken: cancellationToken);

            var responseText = googleResponse.Text();
            _logger.LogInformation("Successfully received response from Gemini.");
            _logger.LogDebug("Gemini Response Text: {ResponseText}", responseText);

            return responseText;
          }
        }
        else
        {
          // Lease was not acquired. This happens if the queue is full (QueueLimit reached).
          _logger.LogWarning("Failed to acquire rate limit permit after {WaitTimeMs}ms (queue likely full). Prompt: {Prompt}", stopwatch.ElapsedMilliseconds, prompt);
          // Throw a specific exception that the controller can catch
          throw new Exception("Gemini API rate limit exceeded and queue is full.");
        }
      }
      catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
      {
        stopwatch.Stop();
        _logger.LogWarning("Operation cancelled while waiting for rate limit permit or calling Gemini API after {WaitTimeMs}ms.", stopwatch.ElapsedMilliseconds);
        // Re-throw cancellation exception
        throw;
      }
      catch (Exception ex)
      {
        stopwatch.Stop();
        _logger.LogError(ex, "An error occurred while acquiring permit or calling the Gemini API after {WaitTimeMs}ms. Prompt: {Prompt}", stopwatch.ElapsedMilliseconds, prompt);
        // Dispose the lease manually if acquired but failed before the 'using' block completed fully
        lease?.Dispose();
        // Re-throw or handle as appropriate (e.g., return null or throw a custom service exception)
        throw new ApplicationException("An unexpected error occurred contacting the AI service.", ex);
      }
    }
  }
}
