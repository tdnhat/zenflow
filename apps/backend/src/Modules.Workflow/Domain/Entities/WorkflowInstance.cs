using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Modules.Workflow.Domain.Entities
{
    public class WorkflowInstance
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid WorkflowDefinitionId { get; set; }
        public virtual WorkflowDefinition WorkflowDefinition { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
        
        public DateTime? StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public string Error { get; set; }
        
        [Column(TypeName = "json")]
        public string VariablesJson { get; set; }
        
        public virtual ICollection<NodeExecution> NodeExecutions { get; set; } = new List<NodeExecution>();
    }
    
    public class NodeExecution
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid WorkflowInstanceId { get; set; }
        
        [ForeignKey("WorkflowInstanceId")]
        public virtual WorkflowInstance WorkflowInstance { get; set; }
        
        [Required]
        public string NodeId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string ActivityType { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }
        
        public DateTime? StartedAt { get; set; }
        
        public DateTime? CompletedAt { get; set; }
        
        public string Error { get; set; }
        
        [Column(TypeName = "json")]
        public string InputDataJson { get; set; }
        
        [Column(TypeName = "json")]
        public string OutputDataJson { get; set; }
        
        [Column(TypeName = "json")]
        public string LogsJson { get; set; }
    }
}