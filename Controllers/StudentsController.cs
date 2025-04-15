using light_quiz_api.Dtos.Question;
using light_quiz_api.Dtos.QuizProgress;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("/progress/{quizId:guid}")]
        public async Task<ActionResult> SaveStudentProgress(Guid quizId, [FromBody] PostQuizProgressRequest request)
        {
            if (quizId != request.QuizId)
            {
                return BadRequest("quizId mismatch between sent request and route paramater");
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

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("/progress/{quizId:guid}")]
        public async Task<ActionResult<GetQuizProgressResponse>> GetStudentProgress(Guid quizId)
        {
            var latestProgress = await _context.QuizProgresses
                .Where(qp => qp.QuizId == quizId)
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
    }
}
