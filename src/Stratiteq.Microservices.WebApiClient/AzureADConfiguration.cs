using System;
using System.Collections.Generic;

namespace Stratiteq.Microservices.WebApiClient
{
    /// <summary>
    /// Contains the information needed to make authenticated requests to a web API protected with Azure Active Directory (and role based authentication).
    /// </summary>
    public class AzureADConfiguration
    {
        /// <summary>
        /// Gets or sets the tenant id of the Azure Active Directory (AAD) that hosts the application that is requesting access to another application.
        /// The tenant id is the id of the AAD-instance. This is always a GUID.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the client id (aka application id) of the Azure Active Directory-application that is requesting access to another application. This is always a GUID.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the subject name of the certificate that will be loaded and passed along the request to Azure Active Directory (AAD) to get an authentication token.
        /// The certificate (without the private key, .cer format) must be uploaded to the AAD application itself so that it can verify the certificate.
        /// The certificate (with the private key, pfx-format) must be uploaded to the web application host (App service or Azure Function).
        /// </summary>
        public string CertificateSubjectName { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory (AAD) application identifiers of the web APIs that the calling application needs authenticated access to.
        /// These identifiers can be found in the AAD application settings, and is separate from the client id / application id. It has the form of a URI.
        /// For the type parameter, use your typed HTTP client classes that inherit from WebApiClientBase.
        /// </summary>
        public Dictionary<Type, string> AADAppIdentifiers { get; set; } = new Dictionary<Type, string>();
    }
}
