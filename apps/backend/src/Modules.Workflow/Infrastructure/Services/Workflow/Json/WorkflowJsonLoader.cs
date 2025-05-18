using System.Text.Json;
using Microsoft.Extensions.Logging;
using Modules.Workflow.Domain.Entities;

namespace Modules.Workflow.Infrastructure.Services.Workflow.Json
{
    public interface IWorkflowJsonLoader
    {
        WorkflowDefinition LoadFromJson(string json);
        string SaveToJson(WorkflowDefinition workflow);
    }

    public class WorkflowJsonLoader : IWorkflowJsonLoader
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<WorkflowJsonLoader> _logger;

        public WorkflowJsonLoader(ILogger<WorkflowJsonLoader> logger)
        {
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public WorkflowDefinition LoadFromJson(string json)
        {
            try
            {
                _logger.LogInformation("Loading workflow definition from JSON");

                // Deserialize the JSON to a dynamic object first to extract the basic structure
                var jsonDoc = JsonSerializer.Deserialize<JsonElement>(json);

                // Create the workflow definition
                var workflow = new WorkflowDefinition
                {
                    Id = jsonDoc.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String
                        ? Guid.Parse(idProp.GetString())
                        : Guid.NewGuid(),
                    Name = jsonDoc.GetProperty("name").GetString(),
                    Description = jsonDoc.TryGetProperty("description", out var descProp) && descProp.ValueKind == JsonValueKind.String
                        ? descProp.GetString()
                        : null,
                    Version = jsonDoc.TryGetProperty("version", out var versionProp) && versionProp.ValueKind == JsonValueKind.Number
                        ? versionProp.GetInt32()
                        : 1,
                    CreatedAt = jsonDoc.TryGetProperty("createdAt", out var createdAtProp) && createdAtProp.ValueKind == JsonValueKind.String
                        ? DateTime.Parse(createdAtProp.GetString())
                        : DateTime.UtcNow,
                    UpdatedAt = jsonDoc.TryGetProperty("updatedAt", out var updatedAtProp) && updatedAtProp.ValueKind == JsonValueKind.String
                        ? DateTime.Parse(updatedAtProp.GetString())
                        : null
                };

                // Process nodes
                if (jsonDoc.TryGetProperty("nodes", out var nodesProp) && nodesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var nodeProp in nodesProp.EnumerateArray())
                    {
                        var node = new WorkflowNode
                        {
                            Id = Guid.Parse(nodeProp.GetProperty("id").GetString()),
                            WorkflowId = workflow.Id,
                            Name = nodeProp.GetProperty("name").GetString(),
                            ActivityType = nodeProp.GetProperty("activityType").GetString()
                        };

                        // Process activity properties
                        if (nodeProp.TryGetProperty("activityProperties", out var activityPropsProp))
                        {
                            node.ActivityPropertiesJson = activityPropsProp.ToString();
                            node.ActivityProperties = JsonSerializer.Deserialize<Dictionary<string, object>>(
                                activityPropsProp.ToString(), _jsonOptions);
                        }

                        // Process position
                        if (nodeProp.TryGetProperty("position", out var positionProp))
                        {
                            var position = new NodePosition
                            {
                                X = positionProp.GetProperty("x").GetInt32(),
                                Y = positionProp.GetProperty("y").GetInt32()
                            };

                            node.Position = position;
                            node.PositionJson = JsonSerializer.Serialize(position, _jsonOptions);
                        }

                        // Process input mappings
                        if (nodeProp.TryGetProperty("inputMappings", out var inputMappingsProp) &&
                            inputMappingsProp.ValueKind == JsonValueKind.Array)
                        {
                            var inputMappings = new List<InputMapping>();

                            foreach (var mappingProp in inputMappingsProp.EnumerateArray())
                            {
                                inputMappings.Add(new InputMapping
                                {
                                    SourceNodeId = Guid.Parse(mappingProp.GetProperty("sourceNodeId").GetString()),
                                    SourceProperty = mappingProp.GetProperty("sourceProperty").GetString(),
                                    TargetProperty = mappingProp.GetProperty("targetProperty").GetString()
                                });
                            }

                            node.InputMappings = inputMappings;
                            node.InputMappingsJson = JsonSerializer.Serialize(inputMappings, _jsonOptions);
                        }

                        // Process output mappings
                        if (nodeProp.TryGetProperty("outputMappings", out var outputMappingsProp) &&
                            outputMappingsProp.ValueKind == JsonValueKind.Array)
                        {
                            var outputMappings = new List<OutputMapping>();

                            foreach (var mappingProp in outputMappingsProp.EnumerateArray())
                            {
                                outputMappings.Add(new OutputMapping
                                {
                                    SourceProperty = mappingProp.GetProperty("sourceProperty").GetString(),
                                    TargetProperty = mappingProp.GetProperty("targetProperty").GetString()
                                });
                            }

                            node.OutputMappings = outputMappings;
                            node.OutputMappingsJson = JsonSerializer.Serialize(outputMappings, _jsonOptions);
                        }

                        workflow.Nodes.Add(node);
                    }
                }

                // Process edges
                if (jsonDoc.TryGetProperty("edges", out var edgesProp) && edgesProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var edgeProp in edgesProp.EnumerateArray())
                    {
                        var edge = new WorkflowEdge
                        {
                            Id = Guid.Parse(edgeProp.GetProperty("id").GetString()),
                            WorkflowId = workflow.Id,
                            Source = Guid.Parse(edgeProp.GetProperty("source").GetString()),
                            Target = Guid.Parse(edgeProp.GetProperty("target").GetString())
                        };

                        // Process condition
                        if (edgeProp.TryGetProperty("condition", out var conditionProp) &&
                            conditionProp.ValueKind == JsonValueKind.Object)
                        {
                            var condition = new EdgeCondition
                            {
                                Expression = conditionProp.GetProperty("expression").GetString()
                            };

                            edge.Condition = condition;
                            edge.ConditionJson = JsonSerializer.Serialize(condition, _jsonOptions);
                        }

                        workflow.Edges.Add(edge);
                    }
                }

                _logger.LogInformation("Successfully loaded workflow definition with {NodeCount} nodes and {EdgeCount} edges",
                    workflow.Nodes.Count, workflow.Edges.Count);

                return workflow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading workflow definition from JSON");
                throw;
            }
        }

        public string SaveToJson(WorkflowDefinition workflow)
        {
            try
            {
                _logger.LogInformation("Saving workflow definition to JSON");

                // Create a dynamic object to represent the workflow
                var workflowJson = new
                {
                    id = workflow.Id,
                    name = workflow.Name,
                    description = workflow.Description,
                    version = workflow.Version,
                    createdAt = workflow.CreatedAt,
                    updatedAt = workflow.UpdatedAt,
                    nodes = workflow.Nodes.Select(n => new
                    {
                        id = n.Id,
                        name = n.Name,
                        activityType = n.ActivityType,
                        activityProperties = n.ActivityProperties,
                        position = n.Position,
                        inputMappings = n.InputMappings,
                        outputMappings = n.OutputMappings
                    }),
                    edges = workflow.Edges.Select(e => new
                    {
                        id = e.Id,
                        source = e.Source,
                        target = e.Target,
                        condition = e.Condition
                    })
                };

                // Serialize to JSON
                var json = JsonSerializer.Serialize(workflowJson, _jsonOptions);

                _logger.LogInformation("Successfully saved workflow definition to JSON");

                return json;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving workflow definition to JSON");
                throw;
            }
        }
    }
}