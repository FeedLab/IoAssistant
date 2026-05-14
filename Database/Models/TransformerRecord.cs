namespace IoAssistant.Database.Models;

public record TransformerRecord
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid BelongToId { get; init; }
    public string Data { get; init; } = string.Empty;
    public Guid ProjectId { get; init; }
}
