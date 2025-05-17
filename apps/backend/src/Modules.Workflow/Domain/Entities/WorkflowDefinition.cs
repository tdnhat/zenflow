using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Modules.Workflow.Domain.Entities
{
    public class WorkflowDefinition
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        [MaxLength(500)]
        public string Description { get; set; }
        
        public int Version { get; set; } = 1;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        [JsonIgnore]
        public virtual ICollection<WorkflowNode> Nodes { get; set; } = new List<WorkflowNode>();
        
        [JsonIgnore]
        public virtual ICollection<WorkflowEdge> Edges { get; set; } = new List<WorkflowEdge>();
    }
    
    public class WorkflowNode
    {
        [Key]
        public string Id { get; set; }
        
        public Guid WorkflowId { get; set; }
        
        [ForeignKey("WorkflowId")]
        [JsonIgnore]
        public virtual WorkflowDefinition Workflow { get; set; }
        
        [MaxLength(100)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string ActivityType { get; set; }
        
        [Column(TypeName = "json")]
        public string ActivityPropertiesJson { get; set; }
        
        [Column(TypeName = "json")]
        public string InputMappingsJson { get; set; }
        
        [Column(TypeName = "json")]
        public string OutputMappingsJson { get; set; }
        
        [Column(TypeName = "json")]
        public string PositionJson { get; set; }
        
        [NotMapped]
        public Dictionary<string, object> ActivityProperties { get; set; } = new();
        
        [NotMapped]
        public List<InputMapping> InputMappings { get; set; } = new();
        
        [NotMapped]
        public List<OutputMapping> OutputMappings { get; set; } = new();
        
        [NotMapped]
        public NodePosition Position { get; set; } = new();
    }
    
    public class WorkflowEdge
    {
        [Key]
        public string Id { get; set; }
        
        public Guid WorkflowId { get; set; }
        
        [ForeignKey("WorkflowId")]
        [JsonIgnore]
        public virtual WorkflowDefinition Workflow { get; set; }
        
        [Required]
        public string Source { get; set; }
        
        [Required]
        public string Target { get; set; }
        
        [Column(TypeName = "json")]
        public string ConditionJson { get; set; }
        
        [NotMapped]
        public EdgeCondition Condition { get; set; }
    }
    
    public class InputMapping
    {
        public string SourceNodeId { get; set; }
        public string SourceProperty { get; set; }
        public string TargetProperty { get; set; }
    }
    
    public class OutputMapping
    {
        public string SourceProperty { get; set; }
        public string TargetProperty { get; set; }
    }
    
    public class EdgeCondition
    {
        public string Expression { get; set; }
    }
    
    public class NodePosition
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}