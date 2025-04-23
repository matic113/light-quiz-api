using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using light_quiz_api.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using light_quiz_api.Models;
using light_quiz_api.Helpers;

namespace light_quiz_api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserAvatarService _userAvatarService;
    private readonly JWT _jwt;

    public AuthService(UserManager<AppUser> userManager, IOptions<JWT> jwt, IUserAvatarService userAvatarService)
    {
        _userManager = userManager;
        _jwt = jwt.Value;
        _userAvatarService = userAvatarService;
    }


    public async Task<(AuthModel Auth, List<ErrorDetail> Errors)> RegisterAsync(RegisterRequest request)
    {
        var errors = new List<ErrorDetail>();

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            errors.Add(new ErrorDetail { PropertyName = "Email", ErrorMessage = "Email already used" });
            return (new AuthModel { IsAuthenticated = false }, errors);
        }

        var userName = request.Email;

        var avatarUrl = _userAvatarService.GenerateAvatarUrl(request.FullName);

        var user = new AppUser
        {
            Email = request.Email,
            UserName = userName,
            FullName = request.FullName,
            AvatarUrl = avatarUrl
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                errors.Add(new ErrorDetail { PropertyName = "Password", ErrorMessage = error.Description });
            }
            return (new AuthModel { IsAuthenticated = false }, errors);
        }

        // If we get here, the user was created successfully
        // role assignment

        var userRole = request.UserType.ToLower() switch
        {
            "admin" => "admin",
            "teacher" => "teacher",
            _ => "student"
        };

        await _userManager.AddToRoleAsync(user, userRole);

        var jwtSecurityToken = await CreateJwtToken(user);

        return (new AuthModel
        {
            IsAuthenticated = true,
            Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
            ExpiresOn = jwtSecurityToken.ValidTo,
            FullName = user.FullName,
            Email = user.Email,
            Roles = new List<string> { "student" }
        }, errors);
    }

    public async Task<(AuthModel Auth, List<ErrorDetail> Errors)> GetTokenAsync(TokenRequestModel model)
    {
        var authModel = new AuthModel();
        var errors = new List<ErrorDetail>();

        var user = await _userManager.FindByEmailAsync(model.Email);
        // User not found or password doesn't match
        if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            errors.Add(new ErrorDetail { PropertyName = "Credentials", ErrorMessage = "Email or Password is incorrect" });
            return (new AuthModel { IsAuthenticated = false }, errors);
        }

        var jwtSecurityToken = await CreateJwtToken(user);
        var rolesList = await _userManager.GetRolesAsync(user);

        authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        authModel.ExpiresOn = jwtSecurityToken.ValidTo;
        authModel.IsAuthenticated = true;
        authModel.Email = model.Email;
        authModel.Roles = rolesList.ToList();


        return (authModel, errors);
    }
    private async Task<JwtSecurityToken> CreateJwtToken(AppUser user)
    {
        var userClaims = await _userManager.GetClaimsAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var userId = await _userManager.GetUserIdAsync(user);
        var roleClaims = new List<Claim>();

        foreach (var role in roles)
            roleClaims.Add(new Claim("roles", role));

        var claims = new[]
        {
            new Claim("userId", userId),
            new Claim(JwtRegisteredClaimNames.Sub, user.FullName!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!)
        }
        .Union(userClaims)
        .Union(roleClaims);

        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.Now.AddDays(_jwt.ExpiresInDays),
            signingCredentials: signingCredentials);

        return jwtSecurityToken;
    }

}
