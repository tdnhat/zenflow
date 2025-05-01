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
            // Get all node types
            app.MapGet("/api/node-types", (INodeTypeRegistry nodeTypeRegistry) =>
            {
                var nodeTypes = nodeTypeRegistry.GetAllNodeTypes();
                var categories = nodeTypeRegistry.GetCategories();

                return Results.Ok(new
                {
                    nodeTypes,
                    categories
                });
            })
            .WithTags("Node Types")
            .WithName("NodeTypes_GetAll")
            .Produces<object>(StatusCodes.Status200OK);

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