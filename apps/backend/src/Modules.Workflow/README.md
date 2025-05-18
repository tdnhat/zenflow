# ZenFlow Workflow Module

## Overview
The Workflow Module enables automation through a visual workflow engine. It allows users to define and execute workflows comprised of connected activity nodes.

## Architecture

### Domain Layer
- **Entities**: Core business objects (WorkflowDefinition, WorkflowInstance, etc.)
- **Interfaces**: Abstractions for repositories and services
- **Enums**: Status and result enumerations
- **ValueObjects**: Immutable objects representing concepts within the domain

### Infrastructure Layer
- **Persistence**: Database contexts, migrations, and repositories
- **Services**: Implementation of domain interfaces
- **Extensions**: Module registration and configuration

### Features Layer
- **Workflows**: Use cases for creating, updating, and executing workflows

## Activity System

The activity system follows a plugin architecture:
1. **IActivityExecutor**: Core interface for all activity executors
2. **BaseActivityExecutor**: Abstract base implementation with common functionality
3. **Concrete Executors**: Type-specific implementations (Playwright, Email, AI)

## Recommendations for Improvement

### 1. Activity Registration System
Create a more flexible registration system for activities:

```csharp
public interface IActivityDescriptor
{
    string ActivityType { get; }
    string DisplayName { get; }
    string Description { get; }
    IEnumerable<PropertyDescriptor> InputProperties { get; }
    IEnumerable<PropertyDescriptor> OutputProperties { get; }
}

public class ActivityRegistration
{
    public static void RegisterActivity<TExecutor>(IServiceCollection services, 
        IActivityDescriptor descriptor) where TExecutor : class, IActivityExecutor
    {
        services.AddScoped<TExecutor>();
        services.AddScoped<IActivityExecutor, TExecutor>();
        
        // Register the activity descriptor for discovery and UI
        ActivityCatalog.RegisterActivityDescriptor(descriptor);
    }
}
```

### 2. Standardized Error Handling

Implement a consistent error handling pattern across all activities:

```csharp
public class ActivityError
{
    public string Code { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
    public Exception Exception { get; set; }
}

public static class ActivityErrorExtensions
{
    public static ActivityError ToActivityError(this Exception ex)
    {
        return new ActivityError
        {
            Code = "EXCEPTION",
            Message = ex.Message,
            Exception = ex
        };
    }
}
```

### 3. Optimize Persistence Layer

1. **Performance**: Optimize database queries in repositories by:
   - Using projections for specific query needs
   - Limiting eager loading to necessary related entities
   - Adding appropriate indexes

2. **Consistency**: Add a unit of work pattern for transactional integrity:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class WorkflowUnitOfWork : IUnitOfWork
{
    private readonly WorkflowDbContext _dbContext;
    
    public WorkflowUnitOfWork(WorkflowDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
```

### 4. Improved Testing Support

Add testing utilities for workflow execution:

```csharp
public class WorkflowTestRunner
{
    public static async Task<WorkflowExecutionContext> ExecuteWorkflowAsync(
        WorkflowDefinition workflow,
        Dictionary<string, object> initialVariables = null)
    {
        // Create services with test doubles
        var services = new ServiceCollection();
        // Register test versions of services
        var serviceProvider = services.BuildServiceProvider();
        
        // Execute the workflow
        var engine = serviceProvider.GetRequiredService<IWorkflowEngine>();
        var workflowId = await engine.StartWorkflowAsync(
            workflow.Id, initialVariables);
            
        return await engine.GetWorkflowStateAsync(workflowId);
    }
}
```

### 5. Activity Development Guidelines

1. **Input Validation**: Each activity should validate inputs before execution
2. **Idempotency**: Activities should be designed to be safely re-executable
3. **Progress Reporting**: Long-running activities should report progress
4. **Resource Cleanup**: Activities should properly dispose of resources
5. **Documentation**: Each activity should document its inputs, outputs, and behavior

### 6. Workflow Engine Improvements

1. **Persistent Execution**: Support for long-running workflows that can survive process restarts
2. **Versioning**: Better support for workflow definition versioning
3. **Monitoring**: Enhanced logging and metrics for workflow execution
4. **Debugging**: Tools for inspecting workflow state during execution
5. **Parallel Execution**: Support for parallel branches in workflows

## Next Steps

1. Implement activity registration system
2. Refactor repositories for better performance
3. Add comprehensive testing framework
4. Create documentation for activity development
5. Implement workflow versioning system 