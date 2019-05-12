using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Stratiteq.Microservices.Jwt
{
    public static class IHeaderDictionaryExtensions
    {
        private const string TokenPrefix = "Bearer ";

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
