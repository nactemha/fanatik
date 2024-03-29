
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fanatik.interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace fanatik.extention
{
    public class FrontRequirement : IAuthorizationRequirement
    {

    }

    public class FrontAuthHandler : AuthorizationHandler<FrontRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FrontAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FrontRequirement requirement)
        {
            var headers = _httpContextAccessor.HttpContext.Request.Headers;
            StringValues token;
            if (!headers.TryGetValue("Authorization", out token))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if(!context.User.HasClaim(it=>it.Type==System.Security.Claims.ClaimTypes.Name&&!String.IsNullOrEmpty(it.Value)))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }

    public class FrontAuth : AuthorizeAttribute
    {
        public FrontAuth() : base("front")
        {
        }
    }
    public static class AuthenticationExtensions
    {
        public static void AddAutherization(this IServiceCollection services,ISettings settings)
           {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
            {
            jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.getSecretKey())),
                ValidateIssuer = false,
                //ValidIssuer = "jwtSettings.Issuer",

                ValidateAudience = false,
                //ValidAudience = "jwtSettings.Audience",

                ValidateLifetime = true,
                ClockSkew = settings.getTokenExperationTime(),    
                 };
            });      
           services.AddAuthorization(options =>
            {
                options.AddPolicy("front", policy =>
                {                   
                    policy.AuthenticationSchemes.Add(JwtBearerDefaults.AuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new FrontRequirement());
                });
                
            });
            services.AddHttpContextAccessor();
            services.AddSingleton<IAuthorizationHandler, FrontAuthHandler>();
        }

    }
}