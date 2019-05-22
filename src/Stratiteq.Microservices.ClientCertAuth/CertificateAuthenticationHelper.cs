// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Stratiteq.Microservices.X509Certificate;

namespace Stratiteq.Microservices.ClientCertAuth
{
    public static class CertificateAuthenticationHelper
    {
        private static readonly Oid ClientCertificateOid = new Oid("1.3.6.1.5.5.7.3.2");

        public static async Task<AuthenticateResult> AuthenticateCertificateAsync(
            HttpContext context,
            ILogger logger,
            CertificateAuthenticationOptions options,
            CertificateAuthenticationEvents events,
            AuthenticationScheme scheme)
        {
            // You only get client certificates over HTTPS
            if (!context.Request.IsHttps)
            {
                return AuthenticateResult.NoResult();
            }

            var clientCertificate = await context.Connection.GetClientCertificateAsync();

            // This should never be the case, as cert authentication happens long before ASP.NET kicks in.
            if (clientCertificate == null)
            {
                logger.LogDebug("No client certificate found.");
                return AuthenticateResult.NoResult();
            }

            // If we have a self signed cert, and they're not allowed, exit early and not bother with
            // any other validations.
            if (clientCertificate.IsSelfSigned() &&
                !options.AllowedCertificateTypes.HasFlag(CertificateTypes.SelfSigned))
            {
                logger.LogWarning("Self signed certificate rejected, subject was {0}", clientCertificate.Subject);

                return AuthenticateResult.Fail("Options do not allow self signed certificates.");
            }

            // If we have a chained cert, and they're not allowed, exit early and not bother with
            // any other validations.
            if (!clientCertificate.IsSelfSigned() &&
                !options.AllowedCertificateTypes.HasFlag(CertificateTypes.Chained))
            {
                logger.LogWarning("Chained certificate rejected, subject was {0}", clientCertificate.Subject);

                return AuthenticateResult.Fail("Options do not allow chained certificates.");
            }

            var chainPolicy = BuildChainPolicy(clientCertificate, options);

            try
            {
                var chain = new X509Chain { ChainPolicy = chainPolicy };

                var certificateIsValid = chain.Build(clientCertificate);

                if (!certificateIsValid)
                {
                    using (logger.BeginScope(clientCertificate.SHA256Thumprint()))
                    {
                        logger.LogWarning("Client certificate failed validation, subject was {0}", clientCertificate.Subject);
                        foreach (var validationFailure in chain.ChainStatus)
                        {
                            logger.LogWarning("{0} {1}", validationFailure.Status, validationFailure.StatusInformation);
                        }
                    }

                    return AuthenticateResult.Fail("Client certificate failed validation.");
                }

                var validateCertificateContext = new ValidateCertificateContext(context, scheme, options) { ClientCertificate = clientCertificate };

                await events.ValidateCertificate(validateCertificateContext);

                if (validateCertificateContext.Result != null &&
                    validateCertificateContext.Result.Succeeded)
                {
                    return Success(validateCertificateContext.Principal, clientCertificate, scheme);
                }

                if (validateCertificateContext.Result != null &&
                    validateCertificateContext.Result.Failure != null)
                {
                    return AuthenticateResult.Fail(validateCertificateContext.Result.Failure);
                }

                var certClaims = clientCertificate.CreateClaimsFromCertificate(options.ClaimsIssuer);
                return Success(new ClaimsPrincipal(new ClaimsIdentity(certClaims, CertificateAuthenticationDefaults.AuthenticationScheme)), clientCertificate, scheme);
            }
            catch (Exception ex)
            {
                var authenticationFailedContext = new CertificateAuthenticationFailedContext(context, scheme, options)
                {
                    Exception = ex,
                };

                await events.AuthenticationFailed(authenticationFailedContext);

                if (authenticationFailedContext.Result != null)
                {
                    return authenticationFailedContext.Result;
                }

                throw;
            }
        }

        private static X509ChainPolicy BuildChainPolicy(X509Certificate2 certificate, CertificateAuthenticationOptions options)
        {
            // Now build the chain validation options.
            var revocationFlag = options.RevocationFlag;
            var revocationMode = options.RevocationMode;

            if (certificate.IsSelfSigned())
            {
                // Turn off chain validation, because we have a self signed certificate.
                revocationFlag = X509RevocationFlag.EntireChain;
                revocationMode = X509RevocationMode.NoCheck;
            }

            var chainPolicy = new X509ChainPolicy
            {
                RevocationFlag = revocationFlag,
                RevocationMode = revocationMode,
            };

            if (options.ValidateCertificateUse)
            {
                chainPolicy.ApplicationPolicy.Add(ClientCertificateOid);
            }

            if (certificate.IsSelfSigned())
            {
                chainPolicy.VerificationFlags |= X509VerificationFlags.AllowUnknownCertificateAuthority;
                chainPolicy.VerificationFlags |= X509VerificationFlags.IgnoreEndRevocationUnknown;
                chainPolicy.ExtraStore.Add(certificate);
            }

            if (!options.ValidateValidityPeriod)
            {
                chainPolicy.VerificationFlags |= X509VerificationFlags.IgnoreNotTimeValid;
            }

            return chainPolicy;
        }

        private static AuthenticateResult Success(ClaimsPrincipal principal, X509Certificate2 certificate, AuthenticationScheme scheme)
        {
            var props = new AuthenticationProperties
            {
                Items =
                {
                    { CertificateAuthenticationDefaults.CertificateItemsKey, certificate.GetRawCertDataString() },
                },
            };

            var ticket = new AuthenticationTicket(principal, props, scheme.Name);
            return AuthenticateResult.Success(ticket);
        }
    }
}
