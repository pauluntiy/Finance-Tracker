using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FinanceTracker.Application.Interfaces.Auth;
using FinanceTracker.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FinanceTracker.Infrastructure.Auth;

public class JwtProvider(IOptions<JwtOptions> options) : IJwtProvider
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}