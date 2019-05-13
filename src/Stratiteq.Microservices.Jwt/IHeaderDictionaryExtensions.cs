using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Stratiteq.Microservices.Jwt
{
    public static class IHeaderDictionaryExtensions
    {
        private const string TokenPrefix = "Bearer ";

        /// <summary>
        /// Extracts and returns the jwt from the Authorization HTTP header.
        /// </summary>
        /// <param name="headerDictionary">The header dictionary of the incoming request.</param>
        /// <returns>The jwt string from the Authorization HTTP header if found, otherwise null.</returns>
        public static string GetJwtFromAuthorizationHeader(this IHeaderDictionary headerDictionary)
        {
            var authorization = (string)headerDictionary[HeaderNames.Authorization];

            if (authorization != null && authorization.StartsWith(TokenPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return authorization.Substring(TokenPrefix.Length).Trim();
            }

            return null;
        }
    }
}
