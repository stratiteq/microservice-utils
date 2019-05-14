# microservice-utils
A collection of Nuget packages that simplifies building micro services on the .NET platform

# Stratiteq.Microservices.WebApiClient
Contains code to more easily make authenticated requests from one Web Api to another, when the APIs require jwt tokens issued from Azure Active Directory (and role based authentication / RBAC).
Optimized for use in .NET Core 2.2 or later.

Usage:

1. Add the Nuget `Stratiteq.Microservices.WebApiClient` from nuget.org.
2. Create a client class that handles all communication with a specific Web Api (preferably one class for every Web Api you need to call). E.g. if ServiceA calls ServiceB, create a class in ServiceA called ServiceBClient. Inherit from the base class `WebApiClientBase` that is accessible through the Nuget package. The following example shows one system making a call to another:


```
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Stratiteq.Microservices.WebApiClient;

namespace ServiceA
{
    public class ServiceBClient : WebApiClientBase
    {
        public ServiceBClient(HttpClient httpClient, AzureADConfiguration azureADConfiguration)
            : base(httpClient, azureADConfiguration)
        {
        }

        public async Task<CreateServiceBResourceResponse> CreateResourceAsync(CreateServiceBResourceRequest createServiceBResourceRequest)
        {
            HttpClient.DefaultRequestHeaders.Authorization = await GetAuthHeaderValueAsync();

            var response = await HttpClient.PostAsJsonAsync("/v1/resource", createServiceBResourceRequest);

            return JsonConvert.DeserializeObject<CreateServiceBResourceResponse>(await response.Content.ReadAsStringAsync());
        }
    }
}
```
The base class `WebApiClientBase` will help you get the token you need with `GetAuthHeaderValueAsync()` in order to successfully authenticate with ServiceB.

3. In order to hook up your clients, first create a singleton instance of the `AzureADConfiguration` class during startup (typically in Startup.cs where you configure your services), e.g:
```
Services.AddSingleton<AzureADConfiguration>(new AzureADConfiguration
{
    TenantId = config["Security:AzureAD:TenantId"],
    ClientId = config["Security:AzureAD:ClientId"],
    CertificateSubjectName = config["Security:AzureAD:CertificateSubjectName"],
    AADAppIdentifiers =
    {
        { typeof(ServiceBClient), config["ServiceBClient:AADAppIdentifier"] }
    }
});
```
(The configuration naming is an example only)
Note that AADAppIdentifiers is a list, this is because a given service might need to call several different Web Apis. The `WebApiClientBase` base class knows how to pick the correct identifier based on your typed client class.

4. The last step is to register your typed client class. Place this code close to (e.g. right under) the above code, e.g.:

```
Services.AddHttpClient<ServiceBClient>(c =>
{
    c.BaseAddress = new Uri(config["ServiceBClient:BaseUrl"]);
    c.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
});
```
Here you specify both the base address and accept header, this is to avoid having to repeat this configuration in every method in your client class. This makes it as clean and simple as in the above ServiceBClient example.
