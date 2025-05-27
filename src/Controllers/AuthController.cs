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
        private readonly IUserAvatarService _userAvatarService;
        private readonly UserManager<AppUser> _userManager;
        public AuthController(IAuthService authService, UserManager<AppUser> userManager, IUserAvatarService userAvatarService)
        {
            _authService = authService;
            _userManager = userManager;
            _userAvatarService = userAvatarService;
        }        /// <summary>
        /// Registers a new user account in the system.
        /// </summary>
        /// <remarks>
        /// Creates a new user account with the provided information and returns authentication token.
        /// The password must meet complexity requirements.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            var (registerResult, errors) = await _authService.RegisterAsync(request);

            if (!registerResult.IsAuthenticated)
            {
                return ValidationProblemDetailsHelper.CreateProblemDetailsFromErrorDetails(errors);
            }

            return Ok(new { token = registerResult.Token, expireOn = registerResult.ExpiresOn });
        }        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <remarks>
        /// Validates user credentials and returns a JWT token for authentication.
        /// The token should be included in the Authorization header for subsequent requests.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("token")]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
        {
            var (loginResult, errors) = await _authService.GetTokenAsync(model);

            if (!loginResult.IsAuthenticated)
            {
                return ValidationProblemDetailsHelper.CreateProblemDetailsFromErrorDetails(errors);
            }

            return Ok(new { token = loginResult.Token, expireOn = loginResult.ExpiresOn });
        }        /// <summary>
        /// Updates the device token for push notifications.
        /// </summary>
        /// <remarks>
        /// Updates the user's device token used for sending push notifications.
        /// Requires authentication.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("update-devicetoken/{deviceToken}")]
        [Authorize]
        public async Task<IActionResult> UpdateDeviceTokenAsync(string deviceToken)
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
            var currentUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid);
            if (currentUser is null)
            {
                return Unauthorized();
            }
            currentUser.DeviceToken = deviceToken;
            await _userManager.UpdateAsync(currentUser);
            return Ok("Device token updated successfully.");
        }        /// <summary>
        /// Validates the current user's authentication status.
        /// </summary>
        /// <remarks>
        /// Checks if the current user is authenticated and returns confirmation.
        /// Used for verifying token validity.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        }        /// <summary>
        /// Retrieves the current authenticated user's profile information.
        /// </summary>
        /// <remarks>
        /// Returns detailed information about the currently authenticated user including
        /// personal details, avatar, and assigned roles. Generates avatar if not already set.
        /// </remarks>
        [ProducesResponseType(typeof(GetUserInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<GetUserInfo>> GetMe()
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
            var currentUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userIdGuid);
            if (currentUser is null)
            {
                return Unauthorized();
            }

            if (currentUser.AvatarUrl is null)
            {
                var newAvatarUrl = _userAvatarService.GenerateAvatarUrl(currentUser.FullName);
                currentUser.AvatarUrl = newAvatarUrl;
                await _userManager.UpdateAsync(currentUser);
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var respose = new GetUserInfo
            {
                Id = currentUser.Id,
                Email = currentUser.Email ?? "",
                FullName = currentUser.FullName,
                AvatarUrl = currentUser.AvatarUrl,
                Roles = [.. userRoles]
            };

            return Ok(respose);
        }
    }
}
