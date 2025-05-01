using Carter;
using Modules.User.Endpoints;
using Modules.User.Infrastructure;
using Modules.Workflow.Data;
using Modules.Workflow.Infrastructure;
using ZenFlow.Shared.Extensions;
using ZenFlow.Shared.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add core services
builder.AddCoreServices();

var userAssembly = typeof(UserModuleExtensions).Assembly;
var workflowAssembly = typeof(WorkflowModuleExtensions).Assembly;
builder.Services
    .AddCarterWithAssemblies(userAssembly, workflowAssembly)
    .AddMediatRWithAssemblies(userAssembly, workflowAssembly);

// Register module services
builder.Services
    .AddUserModule(builder.Configuration)
    .AddWorkflowModule(builder.Configuration);

var app = builder.Build();

app.MapCarter();

// Use core middleware
app.UseCoreMiddleware();

// Use module middleware
app
    .UseUserModule()
    .UseWorkflowModule();


// Run the application
app.Run();