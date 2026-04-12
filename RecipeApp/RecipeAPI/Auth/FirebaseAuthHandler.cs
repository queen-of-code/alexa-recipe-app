using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using FirebaseAdmin.Auth;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RecipeAPI.Auth
{
    public class FirebaseAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public FirebaseAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();

            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Invalid Authorization header format.");

            var bearerToken = authHeader["Bearer ".Length..].Trim();
            if (string.IsNullOrEmpty(bearerToken))
                return AuthenticateResult.Fail("Empty bearer token.");

            try
            {
                var decoded = await FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(bearerToken);

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, decoded.Uid),
                    new Claim(ClaimTypes.Email,
                        decoded.Claims.TryGetValue("email", out var email) ? email.ToString() : string.Empty)
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Firebase token verification failed: {Message}", ex.Message);
                return AuthenticateResult.Fail(ex.Message);
            }
        }
    }
}
