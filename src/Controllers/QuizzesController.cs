﻿using Hangfire;
using light_quiz_api.Dtos.Question;
using light_quiz_api.Dtos.Quiz;
using light_quiz_api.Dtos.QuizProgress;
using light_quiz_api.Dtos.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/quiz")]
    [Authorize]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<QuizzesController> _logger;
        private readonly ShortCodeGeneratorService _shortCodeGenerator;
        private readonly ReportService _reportService;
        public QuizzesController(ApplicationDbContext context, ILogger<QuizzesController> logger, IBackgroundJobClient backgroundJobClient, ShortCodeGeneratorService shortCodeGenerator, ReportService reportService)
        {
            _context = context;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _shortCodeGenerator = shortCodeGenerator;
            _reportService = reportService;
        }        
        /// <summary>
        /// Retrieves quiz metadata by quiz ID.
        /// </summary>
        /// <remarks>
        /// Returns basic quiz information including title, description, timing,
        /// and configuration details for a specific quiz identified by its GUID.
        /// </remarks>
        [ProducesResponseType(typeof(GetQuizMetadataResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("metadata/{quizId:guid}")]
        public async Task<ActionResult<GetQuizMetadataResponse>> GetQuizMetadata(Guid quizId)
        {
            var response = await _context.Quizzes
                .Where(q => q.Id == quizId)
                .Select(q => new GetQuizMetadataResponse
                {
                    QuizId = q.Id,
                    ShortCode = q.ShortCode ?? string.Empty,
                    Title = q.Title,
                    Description = q.Description ?? string.Empty,
                    StartsAt = q.StartsAt,
                    TimeAllowed = q.DurationMinutes,
                    NumberOfQuestions = q.NumberOfQuestions,
                    PossiblePoints = q.PossiblePoints,
                    GroupId = q.GroupId ?? null,
                    Anonymous = q.Anonymous
                })
                .FirstOrDefaultAsync();

            if (response is null)
            {
                return BadRequest($"quiz with Id: {quizId} doesn't exist");
            }

            var studentId = GetCurrentUserId();

            // Check if the student is in the quiz's group
            var groupId = response.GroupId;
            var allowsAnonymous = response.Anonymous;

            // if the quiz doesn't allow anonymous access, check if the student is in the group
            if (!allowsAnonymous && groupId != Guid.Empty)
            {
                var isStudentInGroup = await _context.GroupMembers
                    .AnyAsync(g => g.GroupId == groupId && g.MemberId == studentId);

                if (!isStudentInGroup)
                {
                    return new ObjectResult(new ProblemDetails
                    {
                        Title = "Access Denied",
                        Detail = $"Student with ID: {studentId} is not in the quiz's group.",
                        Status = StatusCodes.Status403Forbidden,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }

            var pastAttempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(x => x.QuizId == response.QuizId && x.StudentId == studentId);

            if (pastAttempt?.State == AttemptState.Submitted ||
                pastAttempt?.State == AttemptState.AutomaticallySubmitted)
            {
                return BadRequest($"student with Id: {studentId} has taken this quiz before.");
            }

            response.DidStartQuiz = pastAttempt?.State == AttemptState.InProgress;


            return Ok(response);
        }       
        /// <summary>
        /// Retrieves quiz metadata by quiz short code.
        /// </summary>
        /// <remarks>
        /// Returns basic quiz information including title, description, timing,
        /// and configuration details for a specific quiz identified by its short code.
        /// </remarks>
        [ProducesResponseType(typeof(GetQuizMetadataResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("metadata/{shortCode}")]
        public async Task<ActionResult<GetQuizMetadataResponse>> GetQuizMetadataByShortCode(string shortCode)
        {
            var response = await _context.Quizzes
                .Where(q => q.ShortCode == shortCode)
                .Select(q => new GetQuizMetadataResponse
                {
                    QuizId = q.Id,
                    ShortCode = q.ShortCode ?? string.Empty,
                    Title = q.Title,
                    Description = q.Description ?? string.Empty,
                    StartsAt = q.StartsAt,
                    TimeAllowed = q.DurationMinutes,
                    NumberOfQuestions = q.NumberOfQuestions,
                    PossiblePoints = q.PossiblePoints,
                    GroupId = q.GroupId ?? null,
                    Anonymous = q.Anonymous
                })
                .FirstOrDefaultAsync();

            if (response is null)
            {
                return NotFound($"quiz with shortCode: {shortCode} doesn't exist");
            }

            var studentId = GetCurrentUserId();

            // Check if the student is in the quiz's group
            var groupId = response.GroupId;
            var allowsAnonymous = response.Anonymous;

            // if the quiz doesn't allow anonymous access, check if the student is in the group
            if (!allowsAnonymous && groupId != Guid.Empty)
            {
                var isStudentInGroup = await _context.GroupMembers
                    .AnyAsync(g => g.GroupId == groupId && g.MemberId == studentId);

                if (!isStudentInGroup)
                {
                    return new ObjectResult(new ProblemDetails
                    {
                        Title = "Access Denied",
                        Detail = $"Student with ID: {studentId} is not in the quiz's group.",
                        Status = StatusCodes.Status403Forbidden,
                        Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                    })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }
            }

            var pastAttempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(x => x.QuizId == response.QuizId && x.StudentId == studentId);

            if (pastAttempt?.State == AttemptState.Submitted ||
                pastAttempt?.State == AttemptState.AutomaticallySubmitted)
            {
                return BadRequest($"student with Id: {studentId} has taken this quiz before.");
            }

            response.DidStartQuiz = pastAttempt?.State == AttemptState.InProgress;

            return Ok(response);
        }
        /// <summary>
        /// Retrieves quiz metadata for quizzes within a specific group.
        /// </summary>
        /// <remarks>
        /// Returns quiz metadata for all quizzes belonging to a group identified by its short code.
        /// Useful for displaying group-specific quiz information.
        /// </remarks>
        [ProducesResponseType(typeof(GetQuizMetadataResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("metadata/group/{shortCode}")]
        public async Task<ActionResult<GetQuizMetadataResponse>> GetQuizMetadataByGroupShortCode(string shortCode)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.ShortCode == shortCode);

            if (group is null)
            {
                return NotFound($"group with shortCode: {shortCode} doesn't exist");
            }

            var studentId = GetCurrentUserId();

            var isStudentInGroup = group.GroupMembers
                .Any(g => g.MemberId == studentId);

            if (!isStudentInGroup)
            {
                return new ObjectResult(new ProblemDetails
                {
                    Title = "Access Denied",
                    Detail = $"Student with ID: {studentId} is not in the quiz's group.",
                    Status = StatusCodes.Status403Forbidden,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            var response = await _context.Quizzes
                .Where(q => q.GroupId == group.Id)
                .Select(q => new GetQuizMetadataResponse
                {
                    QuizId = q.Id,
                    ShortCode = q.ShortCode ?? string.Empty,
                    Title = q.Title,
                    Description = q.Description ?? string.Empty,
                    StartsAt = q.StartsAt,
                    TimeAllowed = q.DurationMinutes,
                    NumberOfQuestions = q.NumberOfQuestions,
                    PossiblePoints = q.PossiblePoints,
                    GroupId = q.GroupId ?? null,
                    Anonymous = q.Anonymous
                })
                .ToListAsync();

            if (response is null)
            {
                return NotFound($"No quizzes found for group with shortcode: {shortCode}");
            }
            return Ok(response);
        }        
        /// <summary>
        /// Retrieves all quizzes created by the authenticated user.
        /// </summary>
        /// <remarks>
        /// Returns metadata for all quizzes created by the current user,
        /// ordered by start date with an optional limit parameter.
        /// </remarks>
        [ProducesResponseType(typeof(IEnumerable<GetQuizMetadataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<GetQuizMetadataResponse>>> GetQuizzesCreatedMetadata(int limit = 10)
        {
            var userId = GetCurrentUserId();

            var response = await _context.Quizzes
                .Where(q => q.CreatedBy == userId)
                .OrderByDescending(q => q.StartsAt)
                .Select(q => new GetQuizMetadataResponse
                {
                    QuizId = q.Id,
                    ShortCode = q.ShortCode ?? string.Empty,
                    Title = q.Title,
                    Description = q.Description ?? string.Empty,
                    StartsAt = q.StartsAt,
                    TimeAllowed = q.DurationMinutes,
                    NumberOfQuestions = q.NumberOfQuestions,
                    PossiblePoints = q.PossiblePoints,
                    GroupId = q.GroupId ?? null,
                    Anonymous = q.Anonymous
                })
                .Take(limit)
                .ToListAsync();

            if (response is null)
            {
                return NotFound($"No quizzes found for user with Id: {userId}");
            }
            return Ok(response);
        }
        /// <summary>
        /// Starts a new quiz attempt and retrieves quiz questions.
        /// </summary>
        /// <remarks>
        /// Initiates a new quiz attempt for the student and returns the quiz questions.
        /// Creates a new attempt record and validates quiz availability and timing.
        /// </remarks>
        [ProducesResponseType(typeof(GetQuizResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("start/{quizId:guid}")]
        public async Task<ActionResult<Quiz>> StartAndGetQuiz(Guid quizId)
        {
            var studentId = GetCurrentUserId();

            var pastAttempt = await _context.QuizAttempts
                .Where(x => x.QuizId == quizId && x.StudentId == studentId)
                .FirstOrDefaultAsync();

            if (pastAttempt is not null ||
                pastAttempt?.State == AttemptState.Submitted ||
                pastAttempt?.State == AttemptState.AutomaticallySubmitted)
            {
                return BadRequest($"student with Id: {studentId} has taken this quiz before.");
            }

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            var allowedMinutesDifference = 1;

            var studentStartTime = DateTime.UtcNow;
            var globalQuizEndTime = quiz.StartsAt.AddMinutes(quiz.DurationMinutes + 1);

            var studentEndTime = studentStartTime.AddMinutes(quiz.DurationMinutes);

            if (studentStartTime > quiz.StartsAt.AddMinutes(quiz.DurationMinutes))
            {
                return BadRequest($"Quiz with Id: {quizId} has already ended.");
            }

            // Student started the quiz after the allowed time difference
            if (!(studentStartTime < quiz.StartsAt.AddMinutes(allowedMinutesDifference)))
            {
                // Adjust the quiz end time to account for the time difference
                var studentDuration = globalQuizEndTime - studentStartTime;
                studentEndTime = studentStartTime + studentDuration;
            }

            var quizAttempt = new QuizAttempt
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                StudentId = studentId,
                AttemptStartTimeUTC = studentStartTime,
                AttemptEndTimeUTC = studentEndTime,
                LastSaved = DateTime.UtcNow,
                State = AttemptState.InProgress
            };

            await _context.QuizAttempts.AddAsync(quizAttempt);
            await _context.SaveChangesAsync();

            // Schedule the auto quiz submit
            _backgroundJobClient.Schedule<StudentSubmissionService>(
                service => service.AutoSubmitQuizAsync(quizAttempt.Id),
                TimeSpan.FromMinutes(quiz.DurationMinutes + allowedMinutesDifference)
                );

            _logger.LogInformation($"Quiz attempt created for quiz {quizId} with attempt ID {quizAttempt.Id}");
            _logger.LogInformation($"Scheduled auto submit for student {studentId}, attempt {quizAttempt.Id}");

            var questions =
                await _context.Questions
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.QuestionNumber)
                .Include(q => q.QuestionOptions)
                .Select(q => new GetQuestionResponse
                {
                    QuizId = quizId,
                    QuestionId = q.Id,
                    Text = q.QuestionText,
                    ImageUrl = q.ImageUrl,
                    TypeId = q.QuestionTypeId,
                    Points = q.Points,
                    Options = q.QuestionOptions.Select(o => new GetQuestionOptionsResponse
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText,
                        OptionLetter = o.OptionLetter
                    }).ToList()
                }).ToListAsync();

            if (quiz.Randomize)
            {
                questions = questions.OrderBy(q => Guid.NewGuid()).ToList();
            }

            var response = new GetQuizResponse
            {
                QuizId = quiz.Id,
                AttemptId = quizAttempt.Id,
                Title = quiz.Title,
                Description = quiz.Description ?? string.Empty,
                StartsAtUTC = quiz.StartsAt,
                EndsAtUTC = studentEndTime,
                DurationMinutes = quiz.DurationMinutes,
                Questions = questions
            };

            return Ok(response);
        }

        /// <summary>
        /// Resumes a previously started quiz attempt.
        /// </summary>
        /// <remarks>
        /// Allows a student to resume a quiz that was previously started but not completed.
        /// Returns the quiz questions and restores the previous attempt state.
        /// </remarks>
        [ProducesResponseType(typeof(GetQuizResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("resume/{quizId:guid}")]
        public async Task<ActionResult<Quiz>> ResumeQuiz(Guid quizId)
        {
            var studentId = GetCurrentUserId();

            var pastAttempt = await _context.QuizAttempts
                .Where(x => x.QuizId == quizId && x.StudentId == studentId)
                .FirstOrDefaultAsync();

            if (pastAttempt is null)
            {
                return BadRequest($"student with Id: {studentId} hasn't started this quiz yet.");
            }

            if (pastAttempt?.State == AttemptState.Submitted ||
                pastAttempt?.State == AttemptState.AutomaticallySubmitted)
            {
                return BadRequest($"student with Id: {studentId} has taken this quiz before.");
            }

            var quiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            var questions =
                await _context.Questions
                .Where(q => q.QuizId == quizId)
                .OrderBy(q => q.QuestionNumber)
                .Include(q => q.QuestionOptions)
                .Select(q => new GetQuestionResponse
                {
                    QuizId = quizId,
                    QuestionId = q.Id,
                    Text = q.QuestionText,
                    ImageUrl = q.ImageUrl,
                    TypeId = q.QuestionTypeId,
                    Points = q.Points,
                    Options = q.QuestionOptions.Select(o => new GetQuestionOptionsResponse
                    {
                        OptionId = o.Id,
                        OptionText = o.OptionText,
                        OptionLetter = o.OptionLetter
                    }).ToList()
                }).ToListAsync();

            if (quiz.Randomize)
            {
                questions = questions.OrderBy(q => Guid.NewGuid()).ToList();
            }

            var quizResponse = new GetQuizResponse
            {
                QuizId = quiz.Id,
                AttemptId = pastAttempt.Id,
                Title = quiz.Title,
                Description = quiz.Description ?? string.Empty,
                StartsAtUTC = quiz.StartsAt,
                EndsAtUTC = pastAttempt.AttemptEndTimeUTC,
                DurationMinutes = quiz.DurationMinutes,
                Questions = questions
            };

            // get student progress

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

            var progressResponse = new GetQuizProgressResponse
            {
                AttemptId = pastAttempt.Id,
                QuestionsAnswers = questionsAnswered,
                LastSaved = pastAttempt.LastSaved,
                AttemptStartTimeUTC = pastAttempt.AttemptStartTimeUTC,
                AttemptEndTimeUTC = pastAttempt.AttemptEndTimeUTC
            };

            var response = new
            {
                Quiz = quizResponse,
                Progress = progressResponse
            };

            return Ok(response);
        }        
        /// <summary>
        /// Retrieves a student's quiz review by quiz short code.
        /// </summary>
        /// <remarks>
        /// Returns detailed review information for a student's completed quiz attempt,
        /// including answers, correct responses, and grading details.
        /// </remarks>
        [ProducesResponseType(typeof(GetQuizReviewResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("review/{shortCode}")]
        public async Task<IActionResult> GetStudentReviewByQuizShortCode(string shortCode, int limit = 10)
        {
            var studentId = GetCurrentUserId();
            var quizAttempt = await _context.QuizAttempts
                .Include(x => x.Quiz)
                .FirstOrDefaultAsync(x => x.Quiz.ShortCode == shortCode && x.StudentId == studentId);

            if (quizAttempt is null)
            {
                _logger.LogWarning("Student with Id: {studentId} didn't take quiz with Id: {shortCode}.", studentId, shortCode);
                return NotFound($"Student didn't take this quiz yet.");
            }

            if (quizAttempt.State != AttemptState.Graded)
            {
                return BadRequest($"Student with Id: {studentId} is still taking this quiz.");
            }

            var answers = await _context.StudentAnswers
                .Where(x => x.QuizId == quizAttempt.QuizId && x.UserId == studentId)
                .Include(x => x.Question)
                    .ThenInclude(q => q.QuestionOptions)
                .ToListAsync();

            var reviewQuestions = new List<GetQuestionReviewResponse>();

            foreach (var answer in answers)
            {
                var question = answer.Question;
                var questionOptions = question.QuestionOptions.Select(x => new GetQuestionOptionsResponse
                {
                    OptionId = x.Id,
                    OptionText = x.OptionText,
                    OptionLetter = x.OptionLetter
                }).ToList();

                char? correctOption = question.QuestionOptions.FirstOrDefault(x => x.IsCorrect)?.OptionLetter ?? null;

                var answerToAdd = new GetQuestionReviewResponse
                {
                    QuestionText = question.QuestionText,
                    ImageUrl = question.ImageUrl,
                    Options = questionOptions,
                    StudentAnsweredText = answer.AnswerText ?? "",
                    StudentAnsweredOption = answer.AnswerOptionLetter ?? null,
                    CorrectOption = correctOption,
                    Points = answer.Question.Points,
                    IsCorrect = answer.GradingRating > 5,
                    FeedbackMessage = answer.GradingFeedback
                };

                reviewQuestions.Add(answerToAdd);
            }

            var result = await _context.UserResults
                            .Where(x => x.UserId == studentId && x.QuizShortCode == shortCode)
                            .FirstOrDefaultAsync();

            var response = new GetQuizReviewResponse
            {
                QuizId = quizAttempt.QuizId,
                ShortCode = shortCode,
                Title = quizAttempt.Quiz.Title,
                Description = quizAttempt.Quiz.Description ?? string.Empty,
                Grade = result.Grade,
                PossiblePoints = result.PossiblePoints,
                CorrectQuestions = result.CorrectQuestions ?? 0,
                TotalQuestions = result.TotalQuestion ?? 0,
                SubmissionDate = quizAttempt.SubmissionDate,
                GradingDate = result.CreatedAt,
                Questions = reviewQuestions
            };

            return Ok(response);
        }       
        /// <summary>
        /// Retrieves all student responses for a specific quiz by short code.
        /// </summary>
        /// <remarks>
        /// Returns a list of all student submissions and their responses for a quiz,
        /// useful for teachers to review student performance and answers.
        /// </remarks>
        [ProducesResponseType(typeof(IEnumerable<GetStudentQuizResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("responses/{quizShortCode}")]
        public async Task<ActionResult<IEnumerable<GetStudentQuizResponse>>> GetStudentResponses(string quizShortCode)
        {
            var results = await _context.UserResults
                        .Include(ur => ur.User)
                        .Where(ur => ur.QuizShortCode == quizShortCode)
                        .OrderByDescending(ur => ur.CreatedAt)
                        .Select(ur => new GetStudentQuizResponse
                        {
                            StudentId = ur.UserId,
                            StudentName = ur.User.FullName,
                            StudentEmail = ur.User.Email ?? string.Empty,
                            QuizId = ur.QuizId,
                            QuizShortCode = ur.QuizShortCode ?? string.Empty,
                            Grade = ur.Grade,
                            GradedAt = ur.CreatedAt,
                            PossiblePoints = ur.PossiblePoints,
                            CorrectQuestions = ur.CorrectQuestions ?? 0,
                            TotalQuestions = ur.TotalQuestion ?? 0
                        })
                        .ToListAsync();

            if (!results.Any())
            {
                return Ok(Enumerable.Empty<GetStudentQuizResponse>());
            }

            return Ok(results);
        }        
        /// <summary>
        /// Retrieves quiz data for manual grading of a student's submission.
        /// </summary>
        /// <remarks>
        /// Returns detailed quiz and student answer information for manual grading purposes,
        /// allowing teachers to review and score individual student responses.
        /// </remarks>
        [ProducesResponseType(typeof(GetQuizGradingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("manual-grading")]
        public async Task<ActionResult<GetQuizGradingResponse>> GradeStudentResultManually(string quizShortCode, Guid studentId)
        {
            var quizAttempt = await _context.QuizAttempts
                .Include(x => x.Quiz)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x => x.Quiz.ShortCode == quizShortCode && x.StudentId == studentId);

            if (quizAttempt is null)
            {
                _logger.LogWarning("Student with Id: {studentId} didn't take quiz with Id: {shortCode}.", studentId, quizShortCode);
                return NotFound($"Student didn't take this quiz yet.");
            }

            if (quizAttempt.State != AttemptState.Graded)
            {
                return BadRequest($"Student with Id: {studentId} is still taking this quiz or their results is not ready.");
            }

            var answers = await _context.StudentAnswers
                .Where(x => x.QuizId == quizAttempt.QuizId && x.UserId == studentId)
                .Include(x => x.Question)
                    .ThenInclude(q => q.QuestionOptions)
                .ToListAsync();

            var gradingQuestions = new List<GetQuestionGradingResponse>();

            foreach (var answer in answers)
            {
                var question = answer.Question;
                var questionOptions = question.QuestionOptions.Select(x => new GetQuestionOptionsResponse
                {
                    OptionId = x.Id,
                    OptionText = x.OptionText,
                    OptionLetter = x.OptionLetter
                })
                .OrderBy(x => x.OptionLetter)
                .ToList();

                char? correctOption = question.QuestionOptions.FirstOrDefault(x => x.IsCorrect)?.OptionLetter ?? null;

                var answerToAdd = new GetQuestionGradingResponse
                {
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    ImageUrl = question.ImageUrl,
                    Options = questionOptions,
                    StudentAnsweredText = answer.AnswerText ?? "",
                    StudentAnsweredOption = answer.AnswerOptionLetter ?? null,
                    CorrectOption = correctOption,
                    Points = answer.Question.Points,
                    IsCorrect = answer.GradingRating > 5,
                    AiMessage = answer.GradingFeedback,
                    AiConfidence = answer.GradingConfidence ?? 0,
                    AiRating = answer.GradingRating ?? 0,
                };

                gradingQuestions.Add(answerToAdd);
            }

            var result = await _context.UserResults
                            .Where(x => x.UserId == studentId && x.QuizShortCode == quizShortCode)
                            .FirstOrDefaultAsync();

            var response = new GetQuizGradingResponse
            {
                QuizId = quizAttempt.QuizId,
                ShortCode = quizShortCode,
                Title = quizAttempt.Quiz.Title,
                Description = quizAttempt.Quiz.Description ?? string.Empty,
                StudentId = studentId,
                StudentName = quizAttempt.Student.FullName,
                StudentEmail = quizAttempt.Student.Email ?? string.Empty,
                Grade = result.Grade,
                PossiblePoints = result.PossiblePoints,
                CorrectQuestions = result.CorrectQuestions ?? 0,
                TotalQuestions = result.TotalQuestion ?? 0,
                SubmissionDate = quizAttempt.SubmissionDate,
                GradingDate = result.CreatedAt,
                Questions = gradingQuestions
            };

            return Ok(response);
        }        
        /// <summary>
        /// Updates student grades after manual grading.
        /// </summary>
        /// <remarks>
        /// Allows teachers to update student grades and question scores after manual review.
        /// Updates both individual question grades and overall quiz scores.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("update-grades")]
        public async Task<IActionResult> UpdateStudentGrades([FromBody] UpdateStudentGradesRequest request)
        {
            var oldResult = await _context.UserResults
                .Where(x => x.UserId == request.StudentId && x.QuizId == request.QuizId)
                .FirstOrDefaultAsync();

            if (oldResult is null)
            {
                return NotFound($"Student with Id {request.StudentId} haven't takes this quiz yet.");
            }

            var oldAnswers = await _context.StudentAnswers
                .Where(x => x.QuizId == request.QuizId && x.UserId == request.StudentId)
                .Include(x => x.Question)
                .ToListAsync();

            // Looping over the question to check if each of them was in the mentioned quiz
            foreach (var question in request.Questions)
            {
                var oldQuestion = oldAnswers.FirstOrDefault(x => x.QuestionId == question.QuestionId);
                if (oldQuestion is null)
                {
                    return NotFound($"Question with Id {question.QuestionId} doesn't exist.");
                }

                // Update the question with the new grading
                oldQuestion.GradingRating = question.IsCorrect ? 10 : 0;
            }

            int correctQuestions = request.Questions.Where(x => x.IsCorrect).Count();

            //int newTotal = request.Questions.Sum(x => x.NewGrade);
            int newTotal = (correctQuestions / (oldResult.TotalQuestion) ?? 1) * oldResult.PossiblePoints;

            if (newTotal > oldResult.PossiblePoints)
            {
                return BadRequest($"New total points {newTotal} is greater than possible points {oldResult.PossiblePoints}");
            }

            oldResult.CorrectQuestions = correctQuestions;
            oldResult.Grade = newTotal;

            await _context.SaveChangesAsync();
            return Ok();
        }        
        /// <summary>
        /// Creates a new quiz with questions and options.
        /// </summary>
        /// <remarks>
        /// Creates a new quiz including all questions, options, and configuration settings.
        /// Automatically generates a unique short code for the quiz.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost]
        public async Task<IActionResult> CreateQuiz([FromBody] PostQuizRequest request)
        {
            var userId = GetCurrentUserId();
            var shortCode = await _shortCodeGenerator.GenerateQuizShortCodeAsync();

            var numeberOfQuestions = request.Questions.Count;

            if (numeberOfQuestions == 0)
            {
                return BadRequest("Quiz must have at least one question.");
            }

            var newQuiz = new Quiz
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartsAt = request.StartsAtUTC,
                DurationMinutes = request.DurationMinutes,
                NumberOfQuestions = numeberOfQuestions,
                Anonymous = request.Anonymous ?? false,
                Randomize = request.Randomize ?? false,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ShortCode = shortCode,
            };
            _context.Quizzes.Add(newQuiz);

            var possiblePoints = 0;

            var newQuestions = request.Questions.ToList();
            foreach (var question in newQuestions)
            {
                var newQuestion = new Question
                {
                    Id = Guid.NewGuid(),
                    QuizId = newQuiz.Id,
                    QuestionText = question.QuestionText,
                    QuestionTypeId = question.QuestionTypeId,
                    ImageUrl = question.ImageUrl,
                    Points = question.Points,
                    CorrectAnswer = question.CorrectAnswer,
                    QuestionNumber = question.QuestionNumber
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

                possiblePoints += question.Points;
            }

            newQuiz.PossiblePoints = possiblePoints;

            await _context.SaveChangesAsync();

            var isValidGroupId = request.GroupId is not null || request.GroupId != Guid.Empty;

            if (isValidGroupId && request.Anonymous == false)
            {
                // add the group to quiz
                var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == request.GroupId);

                if (group is null)
                {
                    return BadRequest("Group doesn't exist.");
                }

                newQuiz.GroupId = group.Id;
                await _context.SaveChangesAsync();

                // Send immediate Notification
                var notificationTitle = $"Quiz: {newQuiz.Title} is upcoming";
                var notificationBody = $"a new quiz for group {group.Name} on {newQuiz.StartsAt}";

                _backgroundJobClient.Enqueue<NotificationService>(service => service.NotifyGroupAsync((Guid)newQuiz.GroupId, notificationTitle, notificationBody));

                // Schedule notification 10 minutes before the quiz starts
                var notificationTime = newQuiz.StartsAt.AddMinutes(-10);

                _logger.LogInformation($"Scheduling notification for group {request.GroupId} in {notificationTime}");

                _backgroundJobClient.Schedule<NotificationService>(
                    service => service.NotifyGroupForQuizAsync((Guid)newQuiz.GroupId, newQuiz.Id),
                    notificationTime
                    );
            }

            return CreatedAtAction(nameof(GetQuizMetadataByShortCode), new { shortCode }, null);
        }        
        /// <summary>
        /// Deletes a quiz by its unique identifier.
        /// </summary>
        /// <remarks>
        /// Permanently removes a quiz and all associated data from the system.
        /// Only the quiz creator can delete a quiz.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpDelete("{quizId:guid}")]
        public async Task<IActionResult> DeleteQuiz(Guid quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null)
            {
                return NotFound();
            }
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return Ok();
        }        
        /// <summary>
        /// Deletes a quiz by its short code.
        /// </summary>
        /// <remarks>
        /// Permanently removes a quiz and all associated data from the system using its short code.
        /// Only the quiz creator can delete a quiz.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpDelete("{shortCode}")]
        public async Task<IActionResult> DeleteQuiz(string shortCode)
        {
            var quiz = await _context.Quizzes.FirstOrDefaultAsync(x => x.ShortCode == shortCode);
            if (quiz == null)
            {
                return NotFound();
            }
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
            return Ok();
        }        
        /// <summary>
        /// Generates a comprehensive report for a quiz.
        /// </summary>
        /// <remarks>
        /// Creates a detailed PDF report containing quiz analytics, student performance,
        /// and statistical analysis for a quiz identified by its short code.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("report/{shortCode}")]
        public async Task<IActionResult> GenerateReportForQuiz(string shortCode)
        {
            var reportUrl = await _reportService.GenerateQuizReport(shortCode);
            return Ok(reportUrl);
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
