using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElaArquitetura.Application.Interfaces.Auth;
using ElaArquitetura.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ElaArquitetura.Infrastructure.Auth;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options) => _options = options.Value;

    public string GerarToken(Funcionario funcionario)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, funcionario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, funcionario.Email),
            new Claim(ClaimTypes.Name, funcionario.Nome),
            new Claim(ClaimTypes.Role, funcionario.Cargo)
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credenciais);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
