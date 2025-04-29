using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
<<<<<<< Updated upstream
=======
using Microsoft.OpenApi.Models;
using ZenFlow.Shared.Application.Auth;
using ZenFlow.Shared.Infrastructure.Auth;
>>>>>>> Stashed changes

namespace ZenFlow.Shared.Infrastructure;

public static class AppBuilderExtensions
{
    public static WebApplicationBuilder AddCoreServices(this WebApplicationBuilder builder)
    {
        // Add HTTP context accessor for dependency injection
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Common services used across modules
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configure CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" })
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        // Add authentication
        builder.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = builder.Configuration["JwtSettings:Authority"];
                options.Audience = builder.Configuration["JwtSettings:Audience"];

                if (builder.Environment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = false
                    };
                }
            });

        builder.Services.AddAuthorization();

        return builder;
    }

    public static WebApplication UseCoreMiddleware(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowFrontend");
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}