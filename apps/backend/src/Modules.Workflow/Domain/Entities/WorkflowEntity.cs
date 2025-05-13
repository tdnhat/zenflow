using System;
using System.Collections.Generic;
using Modules.Workflow.DDD.ValueObjects;

namespace Modules.Workflow.DDD.Entities
{
    public class WorkflowEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public WorkflowStatus Status { get; private set; }
        public byte[] Version { get; private set; }
        public List<object> DomainEvents { get; private set; } = new List<object>();

        private WorkflowEntity() { }

        public static WorkflowEntity Create(string name, string description)
        {
            return new WorkflowEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                Status = WorkflowStatus.Draft
            };
        }

        public void Update(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public void ChangeStatus(WorkflowStatus newStatus)
        {
            Status = newStatus;
        }
    }
} 