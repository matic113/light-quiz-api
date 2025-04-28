using light_quiz_api.Dtos.Analytics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace light_quiz_api.Controllers
{
    [Route("api/analytics")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly ILogger<AnalyticsController> _logger;
        private readonly ApplicationDbContext _context;
        public AnalyticsController(ILogger<AnalyticsController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("quiz/{shortCode}/top-students")]
        public async Task<ActionResult<TopStudentsResponse>> GetTopStudentsForQuizByShortCode(string shortCode, [FromQuery] int limit = 3)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizAttempts)
                .Include(q => q.UserResults)
                    .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(q => q.ShortCode == shortCode);

            if (quiz is null)
            {
                return NotFound(new { Message = "Quiz not found." });
            }

            var studentsResponse = quiz.UserResults.Select(ur => new StudentGradeResponse
            {
                StudentId = ur.User.Id,
                FullName = ur.User.FullName,
                Score = ur.Grade,
            }).ToList();

            var attemps = quiz.QuizAttempts;

            foreach (var attempt in attemps)
            {
                var currentStudent = studentsResponse.FirstOrDefault(s => s.StudentId == attempt.StudentId);
                if (currentStudent != null)
                {
                    // Seconds
                    currentStudent.SecondsSpent = (int)(attempt.SubmissionDate - attempt.AttemptStartTimeUTC).TotalSeconds;
                    currentStudent.SubmissionDate = attempt.SubmissionDate;
                }
            }
            
            var topStudents = studentsResponse
                .OrderByDescending(s => s.Score)
                .ThenBy(s => s.SecondsSpent)
                .Take(limit)
                .ToList();

            var response = new TopStudentsResponse
            {
                QuizId = quiz.Id,
                StudentsGrades = topStudents
            };

            return Ok(response);
        }
        [HttpGet("quiz/{shortCode}/bot-students")]
        public async Task<ActionResult<TopStudentsResponse>> GetBottomStudentsForQuizByShortCode(string shortCode, [FromQuery] int limit = 3)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.QuizAttempts)
                .Include(q => q.UserResults)
                    .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(q => q.ShortCode == shortCode);

            if (quiz is null)
            {
                return NotFound(new { Message = "Quiz not found." });
            }

            var studentsResponse = quiz.UserResults.Select(ur => new StudentGradeResponse
            {
                StudentId = ur.User.Id,
                FullName = ur.User.FullName,
                Score = ur.Grade,
            }).ToList();

            var attemps = quiz.QuizAttempts;

            foreach (var attempt in attemps)
            {
                var currentStudent = studentsResponse.FirstOrDefault(s => s.StudentId == attempt.StudentId);
                if (currentStudent != null)
                {
                    // Seconds
                    currentStudent.SecondsSpent = (int)(attempt.SubmissionDate - attempt.AttemptStartTimeUTC).TotalSeconds;
                    currentStudent.SubmissionDate = attempt.SubmissionDate;
                }
            }

            var topStudents = studentsResponse
                .OrderBy(s => s.Score)
                .ThenByDescending(s => s.SecondsSpent)
                .Take(limit)
                .ToList();

            var response = new TopStudentsResponse
            {
                QuizId = quiz.Id,
                StudentsGrades = topStudents
            };

            return Ok(response);
        }
        [HttpGet("quiz/{shortCode}")]
        public async Task<ActionResult<GetQuizAnalyticsResponse>> GetQuizAnalytics(string shortCode)
        {
            var quiz = await _context.Quizzes.Where(q => q.ShortCode == shortCode)
                .Include(q => q.QuizAttempts)
                .Include(q => q.UserResults)
                .Include(q => q.Group)
                   .ThenInclude(g => g.GroupMembers)
                .FirstOrDefaultAsync();

            if (quiz is null)
            {
                return NotFound(new { Message = "Quiz not found." });
            }

            var secondsSpent = new List<int>();
            var studentGrades = new List<int>();

            foreach (var attempt in quiz.QuizAttempts)
            {
                // for each attempt, get the student who took the quiz
                var currentStudent = quiz.UserResults.FirstOrDefault(s => s.UserId == attempt.StudentId);
                if (currentStudent != null)
                {
                    // Seconds
                    secondsSpent.Add((int)(attempt.SubmissionDate - attempt.AttemptStartTimeUTC).TotalSeconds);
                    studentGrades.Add(currentStudent.Grade);
                }
            }

            var numberOfStudents = quiz.Group.GroupMembers.Count();

            var response = new GetQuizAnalyticsResponse
            {
                QuizId = quiz.Id,
                QuizShortCode = quiz.ShortCode ?? string.Empty,
                QuizName = quiz.Title,
                QuizDateTime = quiz.StartsAt,
                NumberOfQuestions = quiz.NumberOfQuestions,
                QuizDuration = quiz.DurationMinutes,
                NumberOfStudents = numberOfStudents,
                StudentsAttended = quiz.QuizAttempts.Count(),
                PossiblePoints = quiz.PossiblePoints,
                StudentGrades = studentGrades,
                StudentSecondsSpent = secondsSpent
            };

            return Ok(response);
        }
        [HttpGet("quiz/{shortCode}/top-questions")]
        public async Task<ActionResult<TopQuestionsResponse>> GetTopQuestionsAnswered(string shortCode, [FromQuery] int limit = 2)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.StudentAnswers)
                .FirstOrDefaultAsync(q => q.ShortCode == shortCode);

            if (quiz is null)
            {
                return NotFound(new { Message = "Quiz not found." });
            }

            var questionsList = quiz.Questions.Select(q => new AnalyticsQuestionResponse
            {
                QuestionText = q.QuestionText,
                CorrectAnswers = q.StudentAnswers.Count(a => a.GradingRating > 5),
                WrongAnswers = q.StudentAnswers.Count(a => a.GradingRating <= 5),
                TotalAnswers = q.StudentAnswers.Count()
            }).ToList();

            var topTwo = questionsList.OrderBy(x => x.CorrectAnswers).Take(limit).ToList();
            var botTwo = questionsList.OrderBy(x => x.WrongAnswers).Take(limit).ToList();

            var response = new TopQuestionsResponse
            {
                QuizShortCode = shortCode,
                EasiestQuestions = topTwo,
                HardestQuestions = botTwo,
            };

            return Ok(response);
        }
    }
}
