using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Stratiteq.Microservices.WebApiClient
{
    /// <summary>
    /// Base class for creating typed HTTP Client classes that handles all authenticated machine-to-machine communication to a specific Web API.
    /// Usage:
    /// Inherit from this base type and then register the typed HTTP Client during startup (where you do your service configuration) with Services.AddHttpClient().
    /// Also register a singleton instance of AzureADConfiguration, and add the typed HTTP client to its AADAppIdentifiers-collection.
    /// </summary>
    public abstract class WebApiClientBase
    {
        public const string DefaultScope = "/.default";

        public WebApiClientBase(HttpClient httpClient, AzureADConfiguration azureADConfiguration, IConfidentialClientApplication confidentialClientApplication)
        {
            HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            AzureADConfiguration = azureADConfiguration ?? throw new ArgumentNullException(nameof(azureADConfiguration));
            ConfidentialClientApplication = confidentialClientApplication ?? throw new ArgumentNullException(nameof(confidentialClientApplication));

            Scopes = new string[] { azureADConfiguration.AADAppIdentifiers[GetType()] + DefaultScope };
        }

        public HttpClient HttpClient { get; }

        public AzureADConfiguration AzureADConfiguration { get; }

        public IConfidentialClientApplication ConfidentialClientApplication { get; }

        public string[] Scopes { get; }

        protected async Task<AuthenticationHeaderValue> GetAuthHeaderValueAsync()
        {
            // TODO: Implement retries for when we get 429 som AAD:
            // https://github.com/azureAD/microsoft-authentication-library-for-dotnet/wiki/retry-after
            // Should use Polly instead for above example.
            var result = await ConfidentialClientApplication.AcquireTokenForClient(Scopes).ExecuteAsync().ConfigureAwait(false);
            return new AuthenticationHeaderValue("bearer", result.AccessToken);
        }
    }
}
