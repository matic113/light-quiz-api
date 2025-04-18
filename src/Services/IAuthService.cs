using light_quiz_api.Dtos;

namespace light_quiz_api.Services;

public interface IAuthService
{
    Task<(AuthModel Auth, List<ErrorDetail> Errors)> RegisterAsync(RegisterRequest model);
    Task<(AuthModel Auth, List<ErrorDetail> Errors)> GetTokenAsync(TokenRequestModel model);
}
