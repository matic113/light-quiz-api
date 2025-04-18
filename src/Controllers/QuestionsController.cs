using light_quiz_api.Dtos.Question;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/questions")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("questiontypes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<GetQuestionTypesResponse>))]
        public async Task<ActionResult<IEnumerable<QuestionType>>> GetQuestionTypes()
        {
            var questionTypes = await _context.QuestionTypes.
                Select(qt => new GetQuestionTypesResponse
                {
                    Id = qt.Id,
                    Name = qt.Name,
                }).ToListAsync();

            return Ok(questionTypes);
        }
    }
}
