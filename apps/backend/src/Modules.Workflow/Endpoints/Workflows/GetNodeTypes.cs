using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Workflow.Services.NodeManagement;

namespace Modules.Workflow.Endpoints.Workflows
{
    public class GetNodeTypes : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Get all node types in a format matching the frontend task-library
            app.MapGet("/api/node-types", (INodeTypeRegistry nodeTypeRegistry) =>
            {
                var categories = nodeTypeRegistry.GetCategories();
                var result = new List<object>();

                foreach (var category in categories)
                {
                    var tasksInCategory = nodeTypeRegistry.GetNodeTypesByCategory(category)
                        .Select(nodeType => new
                        {
                            title = nodeType.Label,
                            description = nodeType.Description,
                            type = nodeType.Type
                        });

                    result.Add(new
                    {
                        name = category,
                        tasks = tasksInCategory
                    });
                }

                return Results.Ok(result);
            })
            .WithTags("Node Types")
            .WithName("NodeTypes_GetAll")
            .Produces<List<object>>(StatusCodes.Status200OK);

            // Get node types by category
            app.MapGet("/api/node-types/categories/{category}", (string category, INodeTypeRegistry nodeTypeRegistry) =>
            {
                var nodeTypes = nodeTypeRegistry.GetNodeTypesByCategory(category);
                return Results.Ok(nodeTypes);
            })
            .WithTags("Node Types")
            .WithName("NodeTypes_GetByCategory")
            .Produces<IEnumerable<object>>(StatusCodes.Status200OK);

            // Get specific node type
            app.MapGet("/api/node-types/{type}", (string type, INodeTypeRegistry nodeTypeRegistry) =>
            {
                var nodeType = nodeTypeRegistry.GetNodeTypeByType(type);

                if (nodeType == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(nodeType);
            })
            .WithTags("Node Types")
            .WithName("NodeTypes_GetByType")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}