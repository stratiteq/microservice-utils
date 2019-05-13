using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Stratiteq.Microservices.ClientCertAuth
{
    public class CertificateValidationService : ICertificateValidationService
    {
        public CertificateValidationService(X509Certificate2[] validClientCertificates)
        {
            ValidClientCertificates = validClientCertificates ?? throw new ArgumentNullException(nameof(validClientCertificates));
        }

        public X509Certificate2[] ValidClientCertificates { get; }

        public bool ValidateCertificate(X509Certificate2 clientCertificate) =>
            ValidClientCertificates.Any(x => x.Thumbprint.Equals(clientCertificate.Thumbprint, StringComparison.OrdinalIgnoreCase));
    }
}
