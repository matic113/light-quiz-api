using Hangfire;
using light_quiz_api.Dtos.Quiz;
using light_quiz_api.Dtos.QuizProgress;
using light_quiz_api.Dtos.Student;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/student")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IGradingService _gradingService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public StudentsController(ApplicationDbContext context, IBackgroundJobClient backgroundJobClient, IGradingService gradingService)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _gradingService = gradingService;
        }        
        /// <summary>
        /// Saves student progress for a quiz attempt.
        /// </summary>
        /// <remarks>
        /// Allows students to save their current progress on a quiz, including answers
        /// to questions. This enables students to resume their quiz later.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
        /// <summary>
        /// Retrieves the current progress of a student's quiz attempt.
        /// </summary>
        /// <remarks>
        /// Returns the student's saved progress for a specific quiz, including
        /// previously answered questions and timing information.
        /// </remarks>
        [ProducesResponseType(typeof(GetQuizProgressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        /// <summary>
        /// Submits a completed quiz for grading.
        /// </summary>
        /// <remarks>
        /// Finalizes a student's quiz attempt by submitting all answers for grading.
        /// Once submitted, the quiz cannot be modified and will be processed for scoring.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
            pastAttempt.SubmissionDate = DateTime.UtcNow;

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
        /// <summary>
        /// Retrieves the result of a student's quiz attempt by quiz ID.
        /// </summary>
        /// <remarks>
        /// Returns the graded result of a student's quiz attempt, including
        /// the score, possible points, and quiz metadata.
        /// </remarks>
        [ProducesResponseType(typeof(GetStudentResultResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                                    QuizShortCode = ur.QuizShortCode ?? string.Empty,
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
        /// <summary>
        /// Retrieves the result of a student's quiz attempt by quiz short code.
        /// </summary>
        /// <remarks>
        /// Returns the graded result of a student's quiz attempt using the quiz's short code,
        /// including the score, possible points, and quiz metadata.
        /// </remarks>
        [ProducesResponseType(typeof(GetStudentResultResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                                    QuizShortCode = ur.QuizShortCode,
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
        /// <summary>
        /// Retrieves all quiz results for the authenticated student.
        /// </summary>
        /// <remarks>
        /// Returns a list of all quiz results for the current student, with an optional
        /// limit parameter to control the number of results returned.
        /// </remarks>
        [ProducesResponseType(typeof(IEnumerable<GetStudentResultResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                                        QuizShortCode = ur.QuizShortCode ?? string.Empty,
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
            var userIdClaim = User?.FindFirst("userId");

            if (userIdClaim != null)
            {
                return Guid.Parse(userIdClaim.Value);
            }

            return Guid.Empty;
        }
    }
}
