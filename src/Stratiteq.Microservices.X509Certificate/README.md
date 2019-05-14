# Stratiteq.Microservices.X509Certificate
Provides X509Certificate helper methods.

The X509Certificate2Extensions is completely based on work by Barry Dorrans (https://idunno.org/) from https://github.com/blowdart/idunno.Authentication/tree/master/src/idunno.Authentication.Certificate.
It contains some self explainatory methods like IsSelfSigned, SHA256Thumprint and CreateClaimsFromCertificate.

Also added the following helper that tries to find a certificate with specified subject name in the CurrentUser and the LocalMachine store locations. The certificate must be installed on the target machine before trying to find it. The datetime is used for removing expired certificates from the search result.
```
var certificate = CertificateFinder.FindBySubjectName(CertificateSubjectName, DateTime.UtcNow);
```