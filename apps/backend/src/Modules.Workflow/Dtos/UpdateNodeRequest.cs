namespace Modules.Workflow.Dtos
{
    public record UpdateNodeRequest(
        float X,
        float Y,
        string Label,
        string ConfigJson
    );
}