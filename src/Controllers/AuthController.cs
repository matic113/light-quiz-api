using light_quiz_api.Services;
using light_quiz_api.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace light_quiz_api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<AppUser> _userManager;
        public AuthController(IAuthService authService, UserManager<AppUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var (registerResult, errors) = await _authService.RegisterAsync(request);

            if (!registerResult.IsAuthenticated)
            {
                return ValidationProblemDetailsHelper.CreateProblemDetailsFromErrorDetails(errors);
            }

            return Ok(new { token = registerResult.Token, expireOn = registerResult.ExpiresOn });
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
        {
            var (loginResult, errors) = await _authService.GetTokenAsync(model);

            if (!loginResult.IsAuthenticated)
            {
                return ValidationProblemDetailsHelper.CreateProblemDetailsFromErrorDetails(errors);
            }

            return Ok(new { token = loginResult.Token, expireOn = loginResult.ExpiresOn });
        }

        [HttpPost("info")]
        [Authorize]
        public IActionResult GetInfo()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            var userIdStringValue = userIdClaim.Value;

            if (!Guid.TryParse(userIdStringValue, out Guid userIdGuid))
            {
                return Unauthorized();
            }

            var currentUser = _userManager.Users.FirstOrDefault(u => u.Id == userIdGuid);

            if (currentUser is null)
            {
                return Unauthorized();
            }

            return Ok("Authenticated");
        }
    }
}
