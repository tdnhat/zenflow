using Api.Endpoints;
using Modules.User.Infrastructure;
using ZenFlow.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add core services
builder.AddCoreServices();

// Register module services
builder.Services
    .AddUserModule(builder.Configuration);

var app = builder.Build();

// Use core middleware
app.UseCoreMiddleware();

// Use module middleware
app
    .UseUserModule();

// Map endpoints
app
    .MapSampleEndpoints()
    .MapUserEndpoints();

// Run the application
app.Run();