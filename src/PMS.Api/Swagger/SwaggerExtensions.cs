using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace PMS.Api.Swagger;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddPmsSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Property Management System API",
                Version = "v1",
                Description =
                    "REST API for registration, login (JWT), lease contracts, and webhook subscriptions. " +
                    "Authorize with a Bearer token from POST /api/v1/auth/login."
            });

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT from POST /api/v1/auth/login. Example: Bearer {token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
        });

        return services;
    }
}
