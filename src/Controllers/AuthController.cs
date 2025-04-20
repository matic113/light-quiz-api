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
        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
            return Ok("Authenticated");
        }
    }
}
