using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Stratiteq.Microservices.Jwt
{
    /// <summary>
    /// Provides helper methods for working with token claims.
    /// </summary>
    public static class Claims
    {
        /// <summary>
        /// Creates a list of Claim objects for each jwt claim in the specified token.
        /// </summary>
        /// <param name="token">The jwt token to extract claims from.</param>
        /// <returns>A list of Claim objects.</returns>
        public static Claim[] CreateFromJwt(string token) =>
            new JwtSecurityTokenHandler()
            .ValidateToken(token, DisableTokenValidation.TokenValidationParameters, out _)
            .Claims.ToArray();
    }
}
