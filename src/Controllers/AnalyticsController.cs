using light_quiz_api.Dtos.Analytics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
                    // Minutes
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
                    // Minutes
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
    }
}
