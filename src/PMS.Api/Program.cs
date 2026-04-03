using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PMS.Api;
using PMS.Api.Logging;
using PMS.Api.Middleware;
using PMS.Api.Swagger;
using PMS.Application;
using PMS.Application.Common.Interfaces;
using PMS.Infrastructure;
using PMS.Infrastructure.Persistence;
using PMS.Infrastructure.Security;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("Application", "PMS.Api");
});

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection.Secret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection.Issuer,
            ValidAudience = jwtSection.Audience,
            IssuerSigningKey = signingKey,
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddPmsSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PMS API v1");
        options.DocumentTitle = "PMS API";
    });

    await using (var scope = app.Services.CreateAsyncScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}

app.UseExceptionHandling();
app.UseHttpsRedirection();
app.UsePmsSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
