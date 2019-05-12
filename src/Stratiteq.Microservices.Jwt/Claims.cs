using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Stratiteq.Microservices.Jwt
{
    public static class Claims
    {
        public static Claim[] CreateFromJwt(string token) =>
            new JwtSecurityTokenHandler()
            .ValidateToken(token, DisableTokenValidation.TokenValidationParameters, out _)
            .Claims.ToArray();
    }
}
