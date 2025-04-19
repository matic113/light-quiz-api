using Hangfire;
using light_quiz_api.Dtos.Question;
using light_quiz_api.Dtos.Quiz;
using light_quiz_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace light_quiz_api.Controllers
{
    [Route("api/quiz")]
    [Authorize]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<QuizzesController> _logger;
        private readonly ShortCodeGeneratorService _shortCodeGenerator;
        public QuizzesController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILogger<QuizzesController> logger, IBackgroundJobClient backgroundJobClient, ShortCodeGeneratorService shortCodeGenerator)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _shortCodeGenerator = shortCodeGenerator;
        }

        [HttpGet("metadata/{quizId:guid}")]
        public async Task<ActionResult<GetQuizMetadataResponse>> GetQuizMetadata(Guid quizId)
        {
            var response = await _context.Quizzes
                .Where(q => q.Id == quizId)
                .Select(q => new GetQuizMetadataResponse
                {
                    Title = q.Title,
                    Description = q.Description ?? string.Empty,
                    StartsAt = q.StartsAt,
                    TimeAllowed = q.DurationMinutes,
                    NumberOfQuestions = _context.Questions.Count(x => x.QuizId == quizId)
                })
                .FirstOrDefaultAsync();

            if (response is null)
            {
                return BadRequest($"quiz with Id: {quizId} doesn't exist");
            }

            return Ok(response);
        }

        [HttpGet("metadata/{shortCode}")]
        public async Task<ActionResult<GetQuizMetadataResponse>> GetQuizMetadataByShortCode(string shortCode)
        {
            var response = await _context.Quizzes
                .Where(q => q.ShortCode == shortCode)
                .Select(q => new GetQuizMetadataResponse
                {
                    Title = q.Title,
                    Description = q.Description ?? string.Empty,
                    StartsAt = q.StartsAt,
                    TimeAllowed = q.DurationMinutes,
                    NumberOfQuestions = _context.Questions.Count(x => x.QuizId == q.Id)
                })
                .FirstOrDefaultAsync();

            if (response is null)
            {
                return BadRequest($"quiz with shortCode: {shortCode} doesn't exist");
            }

            return Ok(response);
        }

        [HttpPost("start/{quizId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetQuizResponse))]
        public async Task<ActionResult<Quiz>> StartAndGetQuiz(Guid quizId)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }
           
            var studentId = GetCurrentUserId();

            var quizStartTime = DateTime.UtcNow;
            var quizEndTime = DateTime.UtcNow.AddMinutes(quiz.DurationMinutes);

            var quizAttempt = new QuizAttempt
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                StudentId = studentId,
                AttemptStartTimeUTC = quizStartTime,
                AttemptEndTimeUTC = quizEndTime,
                LastSaved = DateTime.UtcNow,
                State = AttemptState.InProgress
            };

            await _context.QuizAttempts.AddAsync(quizAttempt);
            await _context.SaveChangesAsync();

            // Schedule the auto quiz submit
            _backgroundJobClient.Schedule<StudentSubmissionService>(
                service => service.AutoSubmitQuizAsync(quizAttempt.Id),
                TimeSpan.FromMinutes(quiz.DurationMinutes)
                );

            _logger.LogInformation($"Quiz attempt created for quiz {quizId} with attempt ID {quizAttempt.Id}");
            _logger.LogInformation($"Scheduled auto submit for student {studentId}, attempt {quizAttempt.Id}");

            var questions =
                await _context.Questions
                .Where(q => q.QuizId == quizId)
                .Include(q => q.QuestionOptions)
                .Select(q => new GetQuestionResponse
                {
                    QuizId = quizId,
                    QuestionId = q.Id,
                    Text = q.QuestionText,
                    TypeId = q.QuestionTypeId,
                    Points = q.Points,
                    Options = q.QuestionOptions.Select(o => new GetQuestionOptionsResponse
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText,
                        OptionLetter = o.OptionLetter
                    }).ToList()
                }).ToListAsync();

            var response = new GetQuizResponse
            {
                QuizId = quiz.Id,
                AttemptId = quizAttempt.Id,
                Title = quiz.Title,
                Description = quiz.Description ?? string.Empty,
                StartsAtUTC = quiz.StartsAt,
                DurationMinutes = quiz.DurationMinutes,
                Questions = questions
            };

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] PostQuizRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var shortCode = await _shortCodeGenerator.GenerateUniqueCodeAsync();

            var newQuiz = new Quiz{
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartsAt = request.StartsAtUTC,
                DurationMinutes = request.DurationMinutes,
                Anonymous = request.Anonymous ?? false,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ShortCode = shortCode,
            };
            _context.Quizzes.Add(newQuiz);

            var newQuestions = request.Questions.ToList();
            //var questionTypes = _context.QuestionTypes.ToList();
            foreach (var question in newQuestions)
            {
                var newQuestion = new Question
                {
                    Id = Guid.NewGuid(),
                    QuizId = newQuiz.Id,
                    QuestionText = question.QuestionText,
                    QuestionTypeId = question.QuestionTypeId,
                    Points = question.Points,
                    CorrectAnswer = question.CorrectAnswer
                };
                _context.Questions.Add(newQuestion);
                if (question.Options is not null)
                {
                    foreach (var option in question.Options)
                    {
                        var newOption = new QuestionOption
                        {
                            Id = Guid.NewGuid(),
                            QuestionId = newQuestion.Id,
                            OptionText = option.OptionText,
                            IsCorrect = option.IsCorrect,
                            OptionLetter = option.OptionLetter
                        };
                        _context.QuestionOptions.Add(newOption);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQuizMetadataByShortCode), new { shortCode }, null);
        }
        private Guid GetCurrentUserId()
        {
            var jtiClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId");

            if (jtiClaim != null)
            {
                return Guid.Parse(jtiClaim.Value);
            }

            // Handle the case where the "jti" claim is not found
            return Guid.Empty;
        }
    }
}
