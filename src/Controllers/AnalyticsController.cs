using light_quiz_api.Dtos.Analytics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace light_quiz_api.Controllers
{
    [Route("api/analytics")]
    [ApiController]
    [Authorize(Roles = "teacher")]
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
                AvatarUrl = ur.User.AvatarUrl ?? string.Empty,
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
                AvatarUrl = ur.User.AvatarUrl ?? string.Empty,
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

            var numberOfStudents = 0;

            if(quiz.Group is not null)
            {
                numberOfStudents = quiz.Group.GroupMembers.Count();
            }

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

            var topTwo = questionsList.OrderByDescending(x => x.CorrectAnswers).Take(limit).ToList();
            var botTwo = questionsList.OrderByDescending(x => x.WrongAnswers).Take(limit).ToList();

            var response = new TopQuestionsResponse
            {
                QuizShortCode = shortCode,
                EasiestQuestions = topTwo,
                HardestQuestions = botTwo,
            };

            return Ok(response);
        }

        [HttpGet("group/{shortCode}/top-performers")]
        public async Task<ActionResult<IEnumerable<TopStudentsResponse>>> GetTopPerformersForGroup(string shortCode, [FromQuery] int limit = 3)
        {
            var group = await _context.Groups
                .Where(g => g.ShortCode == shortCode)
                .Include(g => g.Quizzes)
                    .ThenInclude(q => q.UserResults)
                        .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync();

            if (group is null)
            {
                return NotFound(new { Message = "Group not found." });
            }

            var allUserResults = group.Quizzes.SelectMany(q => q.UserResults).ToList();

            var studentPerformance = allUserResults
                .GroupBy(ur => ur.User)
                .Select(group => new TopPerformerResponse
                {
                    UserId = group.Key.Id,
                    Name = group.Key.FullName,
                    AvatarUrl = group.Key.AvatarUrl ?? string.Empty,
                    QuizzesTaken = group.Count(),
                    TotalScore = group.Sum(ur => ur.Grade),
                    AvgTimeSeconds = (int)group.Average(ur => ur.SecondsTaken)
                })
                .OrderByDescending(x => x.TotalScore)
                .ThenBy(x => x.AvgTimeSeconds)
                .Take(limit)
                .ToList();

            return Ok(studentPerformance);
        }
        [HttpGet("group/{shortCode}/bot-performers")]
        public async Task<ActionResult<IEnumerable<TopStudentsResponse>>> GetBottomPerformersForGroup(string shortCode, [FromQuery] int limit = 3)
        {
            var group = await _context.Groups
                .Where(g => g.ShortCode == shortCode)
                .Include(g => g.Quizzes)
                    .ThenInclude(q => q.UserResults)
                        .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync();

            if (group is null)
            {
                return NotFound(new { Message = "Group not found." });
            }

            var allUserResults = group.Quizzes.SelectMany(q => q.UserResults).ToList();

            var studentPerformance = allUserResults
                .GroupBy(ur => ur.User)
                .Select(group => new TopPerformerResponse
                {
                    UserId = group.Key.Id,
                    Name = group.Key.FullName,
                    AvatarUrl = group.Key.AvatarUrl ?? string.Empty,
                    QuizzesTaken = group.Count(),
                    TotalScore = group.Sum(ur => ur.Grade),
                    AvgTimeSeconds = (int)group.Average(ur => ur.SecondsTaken)
                })
                .OrderBy(x => x.TotalScore)
                .ThenByDescending(x => x.AvgTimeSeconds)
                .Take(limit)
                .ToList();

            return Ok(studentPerformance);
        }

        [HttpGet("groups/stats")]
        public async Task<ActionResult<TeacherStatsResponse>> GetTeacherStatsForGroup()
        {
            var userId = GetCurrentUserId();

            var groups = await _context.Groups
                .Where(g => g.CreatedBy == userId)
                .Include(g => g.GroupMembers)
                .Include(g => g.Quizzes)
                    .ThenInclude(q => q.UserResults)
                .ToListAsync();

            if (groups is null)
            {
                return NotFound(new { Message = "No Groups Found." });
            }

            var quizzesCreated = groups.SelectMany(g => g.Quizzes).Count();
            var totalStudents = groups.SelectMany(g => g.GroupMembers).Count();

            var totalQuestions = groups
                .Where(g => g.Quizzes != null)
                .SelectMany(g => g.Quizzes)
                .Sum(q => q.NumberOfQuestions);

            var upcomingQuizzesCount = groups.SelectMany(g => g.Quizzes).Count(q => q.StartsAt > DateTime.UtcNow);

            var response = new TeacherStatsResponse
            {
                UserId = userId,
                TotalGroups = groups.Count(),
                TotalStudents = totalStudents,
                QuizzesCreated = quizzesCreated,
                TotalQuestions = totalQuestions,
                UpcomingQuizzesCount = upcomingQuizzesCount
            };

            return Ok(response);
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
