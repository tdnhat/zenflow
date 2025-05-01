namespace Modules.Workflow.Dtos
{
    public record NodeTypeDto(
        string Type,
        string Label,
        string Category,
        string Kind,
        string Description,
        string Icon,
        IEnumerable<NodePropertyDto> Properties);

    public record NodePropertyDto(
        string Name,
        string Label,
        string Type,
        object DefaultValue,
        bool Required,
        IEnumerable<OptionValueDto> Options);

    public record OptionValueDto(
        string Label,
        string Value);
}