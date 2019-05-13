// Copyright (c) Barry Dorrans. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Stratiteq.Microservices.ClientCertAuth
{
    public class CertificateForwarderMiddleware
    {
        private readonly RequestDelegate next;
        private readonly CertificateForwarderOptions options;
        private readonly ILogger logger;

        public CertificateForwarderMiddleware(
                RequestDelegate next,
                ILoggerFactory loggerFactory,
                IOptions<CertificateForwarderOptions> options)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.options = options.Value;
            logger = loggerFactory.CreateLogger<CertificateForwarderMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!string.IsNullOrWhiteSpace(options.CertificateHeader))
            {
                var clientCertificate = await httpContext.Connection.GetClientCertificateAsync();

                if (clientCertificate == null)
                {
                    // Check for forwarding header
                    string certificateHeader = httpContext.Request.Headers[options.CertificateHeader];
                    if (!string.IsNullOrEmpty(certificateHeader))
                    {
                        try
                        {
                            httpContext.Connection.ClientCertificate = options.HeaderConverter(certificateHeader);
                        }
                        catch
                        {
                            logger.LogError("Could not read certificate from header.");
                        }
                    }
                }
            }

            await next(httpContext);
        }
    }
}
