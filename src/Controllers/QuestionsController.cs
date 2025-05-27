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
        }        /// <summary>
        /// Retrieves all available question types.
        /// </summary>
        /// <remarks>
        /// Returns a list of all question types that can be used when creating quiz questions,
        /// such as multiple choice, true/false, short answer, etc.
        /// </remarks>
        [ProducesResponseType(typeof(IEnumerable<GetQuestionTypesResponse>), StatusCodes.Status200OK)]
        [HttpGet("questiontypes")]
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
