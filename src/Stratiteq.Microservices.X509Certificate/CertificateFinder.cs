using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Stratiteq.Microservices.X509Certificate
{
    /// <summary>
    /// Helps to find certificates from the current machine.
    /// </summary>
    public static class CertificateFinder
    {
        /// <summary>
        /// Tries to find a certificate with specified subject name in the CurrentUser and the LocalMachine store locations.
        /// The certificate must be installed on the target machine before trying to find it.
        /// </summary>
        /// <param name="certificateSubjectName">A string representing the subject name of the certificate.</param>
        /// <param name="utcNow">Timestamp to filter out unexpired certificates.</param>
        /// <returns>X509Certificate2 if certificate was found, otherwise null.</returns>
        public static X509Certificate2 FindBySubjectName(string certificateSubjectName, DateTime utcNow) =>
            FindBySubjectName(certificateSubjectName, StoreLocation.CurrentUser, utcNow) ??
            FindBySubjectName(certificateSubjectName, StoreLocation.LocalMachine, utcNow);

        /// <summary>
        /// Tries to find a certificate with specified subject name in the specified store location.
        /// The certificate must be installed on the target machine before trying to find it.
        /// </summary>
        /// <param name="certificateSubjectName">A string representing the subject name of the certificate.</param>
        /// <param name="storeLocation">Specifies the location of the X.509 certificate store.</param>
        /// <param name="utcNow">Timestamp to filter out unexpired certificates.</param>
        /// <returns>X509Certificate2 if certificate was found, otherwise null.</returns>
        public static X509Certificate2 FindBySubjectName(string certificateSubjectName, StoreLocation storeLocation, DateTime utcNow)
        {
            if (string.IsNullOrWhiteSpace(certificateSubjectName))
            {
                throw new ArgumentException("subjectName is missing", nameof(certificateSubjectName));
            }

            using (var store = new X509Store(StoreName.My, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates;

                // Find unexpired certificates.
                var currentCerts = certCollection.Find(X509FindType.FindByTimeValid, utcNow, false);

                // From the collection of unexpired certificates, find the ones with the correct name.
                var signingCert = currentCerts.Find(X509FindType.FindBySubjectName, certificateSubjectName, false);

                // Return the first certificate in the collection, has the right name and is current.
                return signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
            }
        }
    }
}
