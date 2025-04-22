using System.Text.Json;
using Hangfire;
using light_quiz_api.Dtos.Question;
using light_quiz_api.Dtos.Quiz;
using light_quiz_api.Dtos.QuizProgress;
using light_quiz_api.Dtos.Student;
using light_quiz_api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;

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
            var attempt = await _context.QuizAttempts
                .Where(qp => qp.Id == request.AttemptId)
                .FirstOrDefaultAsync();

            if (attempt is null)
            {
                return NotFound("Quiz attempt not found.");
            }

            attempt.LastSaved = DateTime.UtcNow;

            var studentAnswers = await _context.StudentAnswers
                .Where(x => x.QuizId == attempt.QuizId && x.UserId == attempt.StudentId)
                .ToListAsync();

            foreach (var answer in request.QuestionsAnswers)
            {
                if (answer.AnswerText is null && answer.AnswerOptionLetter is null)
                {
                    continue;
                }

                var existingAnswer = studentAnswers
                    .FirstOrDefault(x => x.QuestionId == answer.QuestionId);

                if (existingAnswer is null)
                {
                    var newAnswer = new StudentAnswer
                    {
                        Id = Guid.NewGuid(),
                        QuizId = attempt.QuizId,
                        UserId = attempt.StudentId,
                        QuestionId = answer.QuestionId,
                        AnswerOptionLetter = answer.AnswerOptionLetter,
                        AnswerText = answer.AnswerText ?? string.Empty
                    };
                    await _context.StudentAnswers.AddAsync(newAnswer);
                }
                else
                {
                    existingAnswer.AnswerOptionLetter = answer.AnswerOptionLetter;
                    existingAnswer.AnswerText = answer.AnswerText ?? string.Empty;
                }
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("progress/{quizId:guid}")]
        public async Task<ActionResult<GetQuizProgressResponse>> GetStudentProgress(Guid quizId)
        {
            var studentId = GetCurrentUserId();

            var attempt = await _context.QuizAttempts
                .Where(a => a.QuizId == quizId && a.StudentId == studentId)
                .FirstOrDefaultAsync();

            if (attempt is null)
            {
                return NotFound();
            }

            var studentAnswers = await _context.StudentAnswers
                .Where(x => x.QuizId == quizId && x.UserId == studentId)
                .ToListAsync();

            var questionsAnswered = studentAnswers
                .Select(x => new QuestionAnswer
                {
                    QuestionId = x.QuestionId,
                    AnswerOptionLetter = x.AnswerOptionLetter,
                    AnswerText = x.AnswerText
                })
                .ToList();

            var response = new GetQuizProgressResponse
            {
                AttemptId = attempt.Id,
                QuestionsAnswers = questionsAnswered,
                LastSaved = attempt.LastSaved,
                AttemptStartTimeUTC = attempt.AttemptStartTimeUTC,
                AttemptEndTimeUTC = attempt.AttemptEndTimeUTC
            };

            return Ok(response);
        }

        [HttpPost("submit/{quizId:guid}")]
        public async Task<ActionResult> SubmitQuiz(Guid quizId, [FromBody] SubmitQuizRequest request)
        {
            if (quizId != request.QuizId)
            {
                return BadRequest("quizId mismatch between sent request and route paramater");
            }

            var quiz = await _context.Quizzes.FindAsync(quizId);

            if (quiz is null)
            {
                return BadRequest($"quiz with Id: {quizId} wasn't found");
            }

            var studentId = GetCurrentUserId();

            var pastAttempt = await _context.QuizAttempts
                .Where(x => x.QuizId == quizId && x.StudentId == studentId)
                .FirstOrDefaultAsync();

            if (pastAttempt is null)
            {
                return BadRequest($"student with Id: {studentId} has not started this quiz yet.");
            }

            if (pastAttempt.State == AttemptState.Submitted)
            {
                return BadRequest($"student with Id: {studentId} has already submitted this quiz.");
            }

            var savedStudentAnswers = await _context.StudentAnswers
                .Where(x => x.QuizId == request.QuizId && x.UserId == studentId)
                .ToListAsync();

            var answersToBeAdded = new List<StudentAnswer>();

            foreach (var answer in request.Answers)
            {
                var existingAnswer = savedStudentAnswers
                    .FirstOrDefault(x => x.QuestionId == answer.QuestionId);

                if (existingAnswer is not null)
                {
                    existingAnswer.AnswerOptionLetter = answer.OptionLetter;
                    existingAnswer.AnswerText = answer.AnswerText ?? string.Empty;
                    continue;
                }

                // If the answer doesn't exist, create a new one
                var curr = new StudentAnswer
                {
                    Id = Guid.NewGuid(),
                    QuizId = quizId,
                    UserId = studentId,
                    QuestionId = answer.QuestionId,
                    AnswerOptionLetter = answer.OptionLetter,
                    AnswerText = answer.AnswerText ?? string.Empty
                };

                answersToBeAdded.Add(curr);
            }

            await _context.StudentAnswers.AddRangeAsync(answersToBeAdded);

            // Update the quiz attempt state to submitted
            pastAttempt.State = AttemptState.Submitted;

            var quizSubmission = new StudentQuizSubmission
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                StudentId = studentId,
                SubmittedAt = DateTime.UtcNow,
            };

            await _context.StudentQuizSubmissions.AddAsync(quizSubmission);
            await _context.SaveChangesAsync();

            // auto grade the quiz after submission
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
                                    QuizTitle = ur.QuizTitle ?? string.Empty,
                                    PossiblePoints = ur.PossiblePoints,
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
        [HttpGet("result/{shortCode}")]
        public async Task<ActionResult<GetStudentResultResponse>> GetQuizResultByQuizShortCode(string shortCode)
        {
            if (string.IsNullOrEmpty(shortCode))
            {
                return BadRequest("Invalid quiz shortcode.");
            }

            var studentId = GetCurrentUserId();

            var result = await _context.UserResults
                                .Where(ur => ur.UserId == studentId && ur.QuizShortCode == shortCode)
                                .Select(ur => new GetStudentResultResponse
                                {
                                    StudentId = ur.UserId,
                                    QuizId = ur.QuizId,
                                    Grade = ur.Grade,
                                    QuizTitle = ur.QuizTitle ?? string.Empty,
                                    PossiblePoints = ur.PossiblePoints,
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

        [HttpGet("results")]
        public async Task<ActionResult<IEnumerable<GetStudentResultResponse>>> GetStudentResults([FromQuery] int limit = 10)
        {
            var studentId = GetCurrentUserId();

            var results = await _context.UserResults
                                    .Where(ur => ur.UserId == studentId)
                                    .OrderByDescending(ur => ur.CreatedAt)
                                    .Select(ur => new GetStudentResultResponse
                                    {
                                        StudentId = ur.UserId,
                                        QuizId = ur.QuizId,
                                        Grade = ur.Grade,
                                        QuizTitle = ur.QuizTitle ?? string.Empty,
                                        PossiblePoints = ur.PossiblePoints,
                                        CorrectQuestions = ur.CorrectQuestions ?? 0,
                                        TotalQuestions = ur.TotalQuestion ?? 0
                                    })
                                    .Take(limit)
                                    .ToListAsync();

            if (!results.Any())
            {
                return Ok(Enumerable.Empty<GetStudentResultResponse>());
            }

            return Ok(results);
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
