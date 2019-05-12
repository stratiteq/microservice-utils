using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Stratiteq.Microservices.Jwt
{
    /// <summary>
    /// Helps to disable token validation when calling ValidateToken on the JwtSecurityTokenHandler. 
    /// This can be helpful when only wanting to extract claims from a jwt, since this functionality is included in ValidateToken-method.
    /// </summary>
    public static class DisableTokenValidation
    {
        /// <summary>
        /// TokenValidationParameters instance where all validation settings are turned off. SignatureValidator will just return a jwt regardlessly.
        /// </summary>
        public static TokenValidationParameters TokenValidationParameters { get; } = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = false,
            ValidateLifetime = false,
            RequireSignedTokens = false,
            ValidateActor = false,
            ValidateTokenReplay = false,

            // Disable signature validation
            ValidateIssuerSigningKey = false,
            SignatureValidator = (string token, TokenValidationParameters parameters) => new JwtSecurityToken(token),
        };
    }
}
