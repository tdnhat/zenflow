using Modules.User.Endpoints;
using Modules.User.Infrastructure;
using Modules.Workflow.Infrastructure;
using ZenFlow.Shared.Extensions;
using ZenFlow.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add core services
builder.AddCoreServices();

var userAssembly = typeof(CreateUser).Assembly;
builder.Services
    .AddCarterWithAssemblies(userAssembly)
    .AddMediatRWithAssemblies(userAssembly);

// Register module services
builder.Services
    .AddUserModule(builder.Configuration)
    .AddWorkflowModule(builder.Configuration);

var app = builder.Build();

// Use core middleware
app.UseCoreMiddleware();

// Use module middleware
app
    .UseUserModule()
    .UseWorkflowModule();

// Map endpoints
app
    .MapUserEndpoints()
    .MapWorkflowEndpoints();

// Run the application
app.Run();