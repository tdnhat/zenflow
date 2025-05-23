using Carter;
using Modules.User.Infrastructure.Extensions;
using Modules.Workflow.Infrastructure.Extensions;
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

// Add MassTransit with RabbitMQ for message transport
builder.Services.AddMassTransitWithRabbitMq(builder.Configuration);

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