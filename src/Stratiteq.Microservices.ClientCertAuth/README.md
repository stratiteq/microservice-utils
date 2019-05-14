# Stratiteq.Microservices.ClientCertAuth
Validates incoming client certificate, the .NET Core way.

Completely based on work by Barry Dorrans (https://idunno.org/) from https://github.com/blowdart/idunno.Authentication/tree/master/src/idunno.Authentication.Certificate.
Some minor code comment fixes, and extracted the certificate bits only, as well as refactored out the X509Certificate helpers to separate nuget. Also added a default certificate validation service based on thumbprint matching.

Example Startup.cs code:

```
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stratiteq.Microservices.ClientCertAuth;
using Stratiteq.Microservices.Jwt;
using Stratiteq.Microservices.X509Certificate;

namespace My.WebApplication
{
    public class Startup
    {
        public const string ClaimsIssuer = "https://mycompany.com";
        public const string CertificateSubjectName = "ServiceA";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var certificate = CertificateFinder.FindBySubjectName(CertificateSubjectName, DateTime.UtcNow);

            if (certificate == null)
            {
                throw new Exception($"Could not find the certificate with subject {CertificateSubjectName} in either the CurrentUser or LocalMachine store locations. Please install this certificate on target machine before trying to use it.");
            }

            services.AddSingleton<ICertificateValidationService, CertificateValidationService>(_ => new CertificateValidationService(new[] { certificate }));

            services
                .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    options.ClaimsIssuer = ClaimsIssuer;
                    options.AllowedCertificateTypes = CertificateTypes.All;
                    options.Events = new CertificateAuthenticationEvents
                    {
                        OnValidateCertificate = context =>
                        {
                            var validationService = context.HttpContext.RequestServices.GetService<ICertificateValidationService>();

                            if (validationService.ValidateCertificate(context.ClientCertificate) == false)
                            {
                                context.Fail("Client certificate thumbprint didn't match any registered trusted certificates.");
                                return Task.CompletedTask;
                            }

							// Create a ClaimsIdentiy based on the claims in the certificate. This code uses the CreateClaimsFromCertificate() helper method in the Stratiteq.Microservices.X509Certificate nuget. 
							// It also uses the GetJwtFromAuthorizationHeader() helper method from the Stratiteq.Microservices.Jwt nuget.
                            var token = context.Request.Headers.GetJwtFromAuthorizationHeader();
                            var certClaimsIdentity = new ClaimsIdentity(
                                context.ClientCertificate.CreateClaimsFromCertificate(context.Options.ClaimsIssuer),
                                CertificateAuthenticationDefaults.AuthenticationScheme);

                            // Optionally, if also doing authorization based on incoming jwt token, populate claims from jwt to the claims list in the .NET ClaimsPrincipal to hook up the Authorize-attribute and User.IsInRole etc. 
							// These helpers are in the Stratiteq.Microservices.Jwt nuget. Alternatively, just assign certClaimsIdentity to context.Principal directly.
                            context.Principal = string.IsNullOrEmpty(token) ?
                                new ClaimsPrincipal(certClaimsIdentity) :
                                new ClaimsPrincipal(new[]
                                {
                                    new ClaimsIdentity(Claims.CreateFromJwt(token), JwtBearerDefaults.AuthenticationScheme),
                                    certClaimsIdentity
                                });

                            context.Success();
                            return Task.CompletedTask;
                        }
                    };
                });

			// Strongly consider enforcing authorization globally for all endpoints in the API by using the following configuration:
            services
                .AddMvc(o =>
                {
                    o.Filters.Add(
                        new AuthorizeFilter(
                            new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build()));
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCertificateHeaderForwarding();
            app.UseAuthentication();

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
```