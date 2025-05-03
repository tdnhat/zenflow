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
        public WorkflowStatus Status { get; private set; } = WorkflowStatus.DRAFT;

        public List<WorkflowNode> Nodes { get; private set; } = new();
        public List<WorkflowEdge> Edges { get; private set; } = new();

        // Parameterless constructor for EF Core
        private Workflow() { }

        public static Workflow Create(string name, string description)
        {
            var workflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Status = WorkflowStatus.DRAFT
            };

            // Raise domain event
            workflow.AddDomainEvent(new WorkflowCreatedEvent(workflow.Id, workflow.Name));

            return workflow;
        }

        public void Update(string name, string description)
        {
            Name = name;
            Description = description;

            // Raise domain event
            AddDomainEvent(new WorkflowUpdatedEvent(Id, name));
        }

        public void Archive()
        {
            Status = WorkflowStatus.ARCHIVED;

            // Raise domain event for archiving workflow
            AddDomainEvent(new WorkflowArchivedEvent(Id));
        }

        public void Restore()
        {
            if (Status != WorkflowStatus.ARCHIVED)
            {
                return; // Only archived workflows can be restored
            }

            Status = WorkflowStatus.DRAFT;

            // Raise domain event for restoring workflow
            AddDomainEvent(new WorkflowRestoredEvent(Id));
        }

        // Node management methods
        public WorkflowNode AddNode(string nodeType, string nodeKind, float x, float y, string label, string configJson)
        {
            if (Status == WorkflowStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var node = WorkflowNode.Create(Id, nodeType, nodeKind, x, y, label, configJson);
            Nodes.Add(node);
            
            // Raise domain event
            AddDomainEvent(new WorkflowNodeCreatedEvent(node.Id, Id, nodeType));
            
            return node;
        }

        public void UpdateNode(Guid nodeId, float x, float y, string label, string configJson)
        {
            if (Status == WorkflowStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var node = Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
            {
                throw new InvalidOperationException($"Node with ID {nodeId} not found");
            }

            node.Update(x, y, label, configJson);
            
            // Raise domain event
            AddDomainEvent(new WorkflowNodeUpdatedEvent(nodeId, Id, node.NodeType));
        }

        public void RemoveNode(Guid nodeId)
        {
            if (Status == WorkflowStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var node = Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null)
            {
                throw new InvalidOperationException($"Node with ID {nodeId} not found");
            }

            // Remove connected edges first
            var edgesToRemove = Edges.Where(e => e.SourceNodeId == nodeId || e.TargetNodeId == nodeId).ToList();
            foreach (var edge in edgesToRemove)
            {
                Edges.Remove(edge);
            }

            Nodes.Remove(node);
            
            // Raise domain event
            AddDomainEvent(new WorkflowNodeDeletedEvent(nodeId, Id));
        }

        // Edge management methods
        public WorkflowEdge AddEdge(Guid sourceNodeId, Guid targetNodeId, string label, string edgeType, string conditionJson, string sourceHandle = "", string targetHandle = "")
        {
            if (Status == WorkflowStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            // Validate nodes exist
            if (!Nodes.Any(n => n.Id == sourceNodeId))
            {
                throw new InvalidOperationException($"Source node with ID {sourceNodeId} not found");
            }

            if (!Nodes.Any(n => n.Id == targetNodeId))
            {
                throw new InvalidOperationException($"Target node with ID {targetNodeId} not found");
            }

            var edge = WorkflowEdge.Create(Id, sourceNodeId, targetNodeId, label, edgeType, conditionJson, sourceHandle, targetHandle);
            Edges.Add(edge);
            
            // Raise domain event
            AddDomainEvent(new WorkflowEdgeCreatedEvent(edge.Id, Id, sourceNodeId, targetNodeId));
            
            return edge;
        }

        public void UpdateEdge(Guid edgeId, string label, string edgeType, string conditionJson, string sourceHandle, string targetHandle)
        {
            if (Status == WorkflowStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var edge = Edges.FirstOrDefault(e => e.Id == edgeId);
            if (edge == null)
            {
                throw new InvalidOperationException($"Edge with ID {edgeId} not found");
            }

            edge.Update(label, edgeType, conditionJson, sourceHandle, targetHandle);
            
            // Raise domain event
            AddDomainEvent(new WorkflowEdgeUpdatedEvent(edgeId, Id, edge.SourceNodeId, edge.TargetNodeId));
        }

        public void RemoveEdge(Guid edgeId)
        {
            if (Status == WorkflowStatus.ARCHIVED)
            {
                throw new InvalidOperationException("Cannot modify an archived workflow");
            }

            var edge = Edges.FirstOrDefault(e => e.Id == edgeId);
            if (edge == null)
            {
                throw new InvalidOperationException($"Edge with ID {edgeId} not found");
            }

            Edges.Remove(edge);
            
            // Raise domain event
            AddDomainEvent(new WorkflowEdgeDeletedEvent(edgeId, Id, edge.SourceNodeId, edge.TargetNodeId));
        }

        // Activate workflow - transition from DRAFT to ACTIVE
        public void Activate()
        {
            if (Status != WorkflowStatus.DRAFT)
            {
                throw new InvalidOperationException($"Cannot activate workflow in {Status} status");
            }

            // Perform validation before activation
            if (Nodes.Count == 0)
            {
                throw new InvalidOperationException("Cannot activate workflow without nodes");
            }

            Status = WorkflowStatus.ACTIVE;
            
            // Raise domain event
            AddDomainEvent(new WorkflowActivatedEvent(Id.ToString(), DateTime.UtcNow, Name));
        }
    }
}
