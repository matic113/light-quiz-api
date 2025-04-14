using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace light_quiz_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public QuizzesController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<ActionResult<List<Quiz>>> GetQuizzes()
        {
            var quizzes = await _context.Quizzes.ToListAsync();
            return Ok(quizzes);
        }
        private string GetCurrentUserId()
        {
            var jtiClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId");

            if (jtiClaim != null)
            {
                return jtiClaim.Value;
            }

            // Handle the case where the "jti" claim is not found
            return null;
        }

    }
}
