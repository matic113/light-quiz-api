using System.Text.Json;
using Hangfire;
using light_quiz_api.Dtos.Question;
using light_quiz_api.Dtos.Quiz;
using light_quiz_api.Dtos.QuizProgress;
using light_quiz_api.Dtos.Student;
using light_quiz_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGradingService _gradingService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public StudentsController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IBackgroundJobClient backgroundJobClient, IGradingService gradingService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _backgroundJobClient = backgroundJobClient;
            _gradingService = gradingService;
        }

        [HttpPost("progress/{quizId:guid}")]
        public async Task<ActionResult> SaveStudentProgress(Guid quizId, [FromBody] PostQuizProgressRequest request)
        {
            if (quizId != request.QuizId)
            {
                return BadRequest("quizId mismatch between sent request and route paramater");
            }

            if (!IsValidJson(request.Answers))
            {
                return BadRequest("The 'answers' field in the request is not a valid JSON string.");
            }

            var progress = new QuizProgress
            {
                Id = Guid.NewGuid(),
                UserId = request.StudentId,
                QuizId = quizId,
                Answers = request.Answers,
                RemainingTimeSeconds = request.RemainingTimeSeconds,
                StartTime = request.StartTime,
                LastSaved = request.LastSaved
            };

            _context.QuizProgresses.Add(progress);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("progress/{quizId:guid}")]
        public async Task<ActionResult<GetQuizProgressResponse>> GetStudentProgress(Guid quizId)
        {
            var studentId = GetCurrentUserId();

            var latestProgress = await _context.QuizProgresses
                .Where(qp => qp.QuizId == quizId && qp.UserId == studentId)
                .OrderByDescending(qp => qp.LastSaved)
                .Select(qp => new GetQuizProgressResponse
                {
                    Id = qp.Id,
                    UserId = qp.UserId,
                    QuizId = qp.QuizId,
                    Answers = qp.Answers,
                    StartTime = qp.StartTime,
                    LastSaved = qp.LastSaved
                })
                .FirstOrDefaultAsync();

            if (latestProgress is null)
            {
                return NotFound();
            }

            return Ok(latestProgress);
        }

        [HttpPost("submit/{quizId:guid}")]
        public async Task<ActionResult> SubmitQuiz(Guid quizId, [FromBody] SubmitQuizRequest request)
        {
            if (quizId != request.QuizId)
            {
                return BadRequest("quizId mismatch between sent request and route paramater");
            }

            var pastSubmission = await _context.StudentQuizSubmissions
                .Where(x => x.QuizId == quizId && x.StudentId == request.StudentId)
                .FirstOrDefaultAsync();

            if (pastSubmission is not null)
            {
                return BadRequest("You have already submitted this quiz.");
            }

            var studentAnswers = new List<StudentAnswer>();

            foreach (var answer in request.Answers)
            {
                var curr = new StudentAnswer
                {
                    Id = Guid.NewGuid(),
                    QuizId = quizId,
                    UserId = request.StudentId,
                    QuestionId = answer.QuestionId,
                    AnswerOptionLetter = answer.OptionLetter,
                    AnswerText = answer.AnswerText ?? string.Empty
                };

                studentAnswers.Add(curr);
            }

            await _context.StudentAnswers.AddRangeAsync(studentAnswers);

            var quizSubmission = new StudentQuizSubmission
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                StudentId = request.StudentId,
                SubmittedAt = DateTime.UtcNow,
            };

            await _context.StudentQuizSubmissions.AddAsync(quizSubmission);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("grade/{quizId:guid}")]
        public async Task<ActionResult> GradeQuiz(Guid quizId)
        {
            var studentId = GetCurrentUserId();

            // Enqueue the grading job
            _backgroundJobClient.Enqueue(() => _gradingService.GradeQuizAsync(studentId, quizId));
            return Ok();
        }

        [HttpGet("result/{quizId:guid}")]
        public async Task<ActionResult<GetStudentResultResponse>> GetQuizResult(Guid quizId)
        {
            if (quizId == Guid.Empty)
            {
                return BadRequest("Invalid quiz ID.");
            }

            var studentId = GetCurrentUserId();
            
            var result = await _context.UserResults
                                .Where(ur => ur.UserId == studentId && ur.QuizId == quizId)
                                .Select(ur => new GetStudentResultResponse
                                {
                                    StudentId = ur.UserId,
                                    QuizId = ur.QuizId,
                                    Grade = ur.Grade,
                                    CorrectQuestions = ur.CorrectQuestions ?? 0,
                                    TotalQuestions = ur.TotalQuestion ?? 0
                                })
                                .FirstOrDefaultAsync();

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
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
        private bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return true; } // Empty or null can be considered valid depending on your requirements
            try
            {
                JsonDocument.Parse(strInput);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
