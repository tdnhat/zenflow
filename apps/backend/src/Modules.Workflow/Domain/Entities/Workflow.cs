using System;
using System.Collections.Generic;
using System.Text.Json;
using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.Events.WorkflowEdgeEvents;
using Modules.Workflow.DDD.ValueObjects;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class Workflow : Aggregate<Guid>
    {
        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = string.Empty;
        public WorkflowStatus Status { get; private set; } = WorkflowStatus.Draft;
        public byte[] Version { get; private set; } = new byte[0]; // Concurrency token
        public string GlobalVariablesJson { get; private set; } = "{}"; // JSON string for global variables
        public string MetadataJson { get; private set; } = "{}"; // JSON string for metadata
        public string ExecutionSettingsJson { get; private set; } = "{}"; // JSON string for execution settings

        public List<WorkflowNode> Nodes { get; private set; } = new();
        public List<WorkflowEdge> Edges { get; private set; } = new();

        // Parameterless constructor for EF Core
        private Workflow() { }

        public static Workflow Create(string name, string description, Dictionary<string, object>? globalVariables = null, object? metadata = null, object? executionSettings = null)
        {
            var workflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Status = WorkflowStatus.Draft,
                Version = new byte[0],
                GlobalVariablesJson = globalVariables != null ? JsonSerializer.Serialize(globalVariables) : "{}",
                MetadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : "{}",
                ExecutionSettingsJson = executionSettings != null ? JsonSerializer.Serialize(executionSettings) : "{}"
            };

            workflow.AddDomainEvent(new WorkflowCreatedEvent(workflow.Id, workflow.Name));
            return workflow;
        }

        public void Update(string name, string description, Dictionary<string, object>? globalVariables = null, object? metadata = null, object? executionSettings = null)
        {
            Name = name;
            Description = description;
            GlobalVariablesJson = globalVariables != null ? JsonSerializer.Serialize(globalVariables) : GlobalVariablesJson;
            MetadataJson = metadata != null ? JsonSerializer.Serialize(metadata) : MetadataJson;
            ExecutionSettingsJson = executionSettings != null ? JsonSerializer.Serialize(executionSettings) : ExecutionSettingsJson;
            Version = new byte[0];

            AddDomainEvent(new WorkflowUpdatedEvent(Id, name));
        }

        public void UpdateBasicInfo(string name, string description)
        {
            if (Status == WorkflowStatus.Archived)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            Name = name;
            Description = description;
            Version = new byte[0];

            AddDomainEvent(new WorkflowUpdatedEvent(Id, name));
        }

        public void Archive()
        {
            Status = WorkflowStatus.Archived;
            Version = new byte[0];
            AddDomainEvent(new WorkflowArchivedEvent(Id));
        }

        public void Restore()
        {
            if (Status != WorkflowStatus.Archived) return;
            Status = WorkflowStatus.Draft;
            Version = new byte[0];
            AddDomainEvent(new WorkflowRestoredEvent(Id));
        }

        public WorkflowNode AddNode(string nodeType, string nodeKind, float x, float y, string label, string configJson)
        {
            if (Status == WorkflowStatus.Archived)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var node = WorkflowNode.Create(Id, nodeType, nodeKind, x, y, label, configJson);
            Nodes.Add(node);
            Version = new byte[0];
            AddDomainEvent(new WorkflowNodeCreatedEvent(node.Id, Id, nodeType));
            return node;
        }

        public void UpdateNode(Guid nodeId, float x, float y, string label, string configJson)
        {
            if (Status == WorkflowStatus.Archived)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var node = Nodes.FirstOrDefault(n => n.Id == nodeId) ?? throw new InvalidOperationException($"Node with ID {nodeId} not found");
            node.Update(x, y, label, configJson);
            Version = new byte[0];
            AddDomainEvent(new WorkflowNodeUpdatedEvent(nodeId, Id, node.NodeType));
        }

        public void RemoveNode(Guid nodeId)
        {
            if (Status == WorkflowStatus.Archived)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var node = Nodes.FirstOrDefault(n => n.Id == nodeId) ?? throw new InvalidOperationException($"Node with ID {nodeId} not found");
            var edgesToRemove = Edges.Where(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId).ToList();
            foreach (var edge in edgesToRemove) Edges.Remove(edge);
            Nodes.Remove(node);
            Version = new byte[0];
            AddDomainEvent(new WorkflowNodeDeletedEvent(nodeId, Id));
        }

        public WorkflowEdge AddEdge(Guid sourceNodeId, Guid targetNodeId, string label, string edgeType, string conditionJson, string sourceHandle = "", string targetHandle = "")
        {
            if (Status == WorkflowStatus.Archived)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            if (!Nodes.Any(n => n.Id == sourceNodeId)) throw new InvalidOperationException($"Source node with ID {sourceNodeId} not found");
            if (!Nodes.Any(n => n.Id == targetNodeId)) throw new InvalidOperationException($"Target node with ID {targetNodeId} not found");

            var edge = WorkflowEdge.Create(Id, sourceNodeId, targetNodeId, label, edgeType, conditionJson, sourceHandle, targetHandle);
            Edges.Add(edge);
            Version = new byte[0];
            AddDomainEvent(new WorkflowEdgeCreatedEvent(edge.Id, Id, sourceNodeId, targetNodeId));
            return edge;
        }

        public void UpdateEdge(Guid edgeId, string label, string edgeType, string conditionJson, string sourceHandle, string targetHandle)
        {
            if (Status == WorkflowStatus.Archived)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var edge = Edges.FirstOrDefault(e => e.Id == edgeId) ?? throw new InvalidOperationException($"Edge with ID {edgeId} not found");
            edge.Update(label, edgeType, conditionJson, sourceHandle, targetHandle);
            Version = new byte[0];
            AddDomainEvent(new WorkflowEdgeUpdatedEvent(edgeId, Id, edge.SourceNodeId, edge.TargetNodeId));
        }

        public void RemoveEdge(Guid edgeId)
        {
            if (Status == WorkflowStatus.Archived)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var edge = Edges.FirstOrDefault(e => e.Id == edgeId) ?? throw new InvalidOperationException($"Edge with ID {edgeId} not found");
            Edges.Remove(edge);
            Version = new byte[0];
            AddDomainEvent(new WorkflowEdgeDeletedEvent(edgeId, Id, edge.SourceNodeId, edge.TargetNodeId));
        }

        public void Activate()
        {
            if (Status != WorkflowStatus.Draft) throw new InvalidOperationException($"Cannot activate workflow in {Status} status");
            if (Nodes.Count == 0) throw new InvalidOperationException("Cannot activate workflow without nodes");
            Status = WorkflowStatus.Active;
            Version = new byte[0];
            AddDomainEvent(new WorkflowActivatedEvent(Id.ToString(), DateTime.UtcNow, Name));
        }
    }
}
