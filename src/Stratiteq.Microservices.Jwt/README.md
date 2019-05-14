# Stratiteq.Microservices.Jwt
Extends some common classes with helpful methods for working with Jwt in .NET Core / Standard.

```
// In order to populate claims from jwt to the claims list in the .NET ClaimsPrincipal to hook up the Authorize-attribute and User.IsInRole etc, use the following configuration during startup (in the ConfigureServices-method):
// This is helpful if doing jwt authentication in a API gateway or similar, but you still need to do authorization with the roles embedded as claims in the jwt.
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => 
{
	options.TokenValidationParameters = DisableTokenValidation.TokenValidationParameters; 
});

// Sometimes you just need to get the raw token from the incoming request header object.
var token = context.Request.Headers.GetJwtFromAuthorizationHeader();

// This little helper creates a list of Claim objects for each jwt claim in the specified token. Can be used to create a ClaimsPrincipal-object based on a jwt.
var claims = Claims.CreateFromJwt(token)
```