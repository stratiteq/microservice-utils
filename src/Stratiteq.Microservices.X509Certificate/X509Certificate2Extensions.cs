// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Stratiteq.Microservices.X509Certificate
{
    public static class X509Certificate2Extensions
    {
        public static bool IsSelfSigned(this X509Certificate2 certificate)
        {
            return certificate.SubjectName.RawData.SequenceEqual(certificate.IssuerName.RawData);
        }

        public static string SHA256Thumprint(this X509Certificate2 certificate)
        {
            using (var hasher = SHA256.Create())
            {
                var certificateHash = hasher.ComputeHash(certificate.RawData);
                string hashAsString = string.Empty;
                foreach (byte hashByte in certificateHash)
                {
                    hashAsString += hashByte.ToString("x2", CultureInfo.InvariantCulture);
                }

                return hashAsString;
            }
        }

        public static Claim[] CreateClaimsFromCertificate(this X509Certificate2 certificate, string claimsIssuer)
        {
            var claims = new List<Claim>();

            var issuer = certificate.Issuer;
            claims.Add(new Claim("clientcertissuer", issuer, ClaimValueTypes.String, claimsIssuer));

            var thumbprint = certificate.Thumbprint;
            claims.Add(new Claim(ClaimTypes.Thumbprint, thumbprint, ClaimValueTypes.Base64Binary, claimsIssuer));

            var value = certificate.SubjectName.Name;
            if (!string.IsNullOrWhiteSpace(value))
            {
                claims.Add(new Claim(ClaimTypes.X500DistinguishedName, value, ClaimValueTypes.String, claimsIssuer));
            }

            value = certificate.SerialNumber;
            if (!string.IsNullOrWhiteSpace(value))
            {
                claims.Add(new Claim(ClaimTypes.SerialNumber, value, ClaimValueTypes.String, claimsIssuer));
            }

            value = certificate.GetNameInfo(X509NameType.DnsName, false);
            if (!string.IsNullOrWhiteSpace(value))
            {
                claims.Add(new Claim(ClaimTypes.Dns, value, ClaimValueTypes.String, claimsIssuer));
            }

            value = certificate.GetNameInfo(X509NameType.SimpleName, false);
            if (!string.IsNullOrWhiteSpace(value))
            {
                claims.Add(new Claim(ClaimTypes.Name, value, ClaimValueTypes.String, claimsIssuer));
            }

            value = certificate.GetNameInfo(X509NameType.EmailName, false);
            if (!string.IsNullOrWhiteSpace(value))
            {
                claims.Add(new Claim(ClaimTypes.Email, value, ClaimValueTypes.String, claimsIssuer));
            }

            value = certificate.GetNameInfo(X509NameType.UpnName, false);
            if (!string.IsNullOrWhiteSpace(value))
            {
                claims.Add(new Claim(ClaimTypes.Upn, value, ClaimValueTypes.String, claimsIssuer));
            }

            value = certificate.GetNameInfo(X509NameType.UrlName, false);
            if (!string.IsNullOrWhiteSpace(value))
            {
                claims.Add(new Claim(ClaimTypes.Uri, value, ClaimValueTypes.String, claimsIssuer));
            }

            return claims.ToArray();
        }
    }
}
