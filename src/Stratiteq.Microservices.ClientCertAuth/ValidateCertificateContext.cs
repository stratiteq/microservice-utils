// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Stratiteq.Microservices.ClientCertAuth
{
    public class ValidateCertificateContext : ResultContext<CertificateAuthenticationOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateCertificateContext"/> class.
        /// </summary>
        /// <param name="context">The HttpContext the validate context applies too.</param>
        /// <param name="scheme">The scheme used when the Basic Authentication handler was registered.</param>
        /// <param name="options">The <see cref="CertificateAuthenticationOptions"/> for the instance of
        /// <see cref="CertificateAuthenticationHandler"/> creating this instance.</param>
        public ValidateCertificateContext(
            HttpContext context,
            AuthenticationScheme scheme,
            CertificateAuthenticationOptions options)
            : base(context, scheme, options)
        {
        }

        /// <summary>
        /// Gets or sets the certificate to validate.
        /// </summary>
        public X509Certificate2 ClientCertificate { get; set; }
    }
}
