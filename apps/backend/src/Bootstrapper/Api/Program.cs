using Api.Endpoints;
using Modules.Identity.Infrastructure;
using Modules.User.Infrastructure;
using ZenFlow.Core.Infrastructure;
using ZenFlow.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add core services
builder.AddCoreServices();

// Add database
builder.Services.AddApplicationDatabase(builder.Configuration);

// Register module services
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddUserModule();

var app = builder.Build();

// Use core middleware
app.UseCoreMiddleware();

// Map endpoints
app.MapSampleEndpoints();
app.MapIdentityEndpoints();
app.MapUserEndpoints();

// Seed the database
await app.SeedDatabaseAsync();

// Run the application
app.Run();