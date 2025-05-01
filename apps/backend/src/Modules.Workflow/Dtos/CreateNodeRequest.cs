namespace Modules.Workflow.Dtos
{
    public record CreateNodeRequest(
        string NodeType,
        string NodeKind,
        float X,
        float Y,
        string Label,
        string ConfigJson
    );
}