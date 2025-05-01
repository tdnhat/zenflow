using Modules.Workflow.DDD.Events;
using Modules.Workflow.DDD.ValueObjects;
using ZenFlow.Shared.Domain;

namespace Modules.Workflow.DDD.Entities
{
    public class Workflow : Aggregate<Guid>
    {
        public string Name { get; private set; } = default!;
        public string Description { get; private set; } = string.Empty;
        public string Status { get; private set; } = WorkflowStatus.Draft;

        public List<WorkflowNode> Nodes { get; private set; } = new();
        public List<WorkflowEdge> Edges { get; private set; } = new();


        // Parameterless constructor for EF Core
        public Workflow() { }

        public static Workflow Create(string name, string description)
        {
            var workflow =  new Workflow
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Status = WorkflowStatus.Draft
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
            Status = WorkflowStatus.Archived;
        }
    }
}
