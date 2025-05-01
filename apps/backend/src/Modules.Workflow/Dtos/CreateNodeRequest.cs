namespace Modules.Workflow.Dtos
{
    public record CreateNodeRequest(
        string NodeType,
        float X,
        float Y,
        string Label,
        string ConfigJson
    );
}