using SQLite;

namespace IoAssistant.Database.Models;

[Table("Transformer")]
public class TransformerEntity
{
    [PrimaryKey] public Guid Id { get; set; } = Guid.CreateVersion7();

    [NotNull] public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [NotNull] public Guid BelongToId { get; set; }

    [NotNull] public string Data { get; set; } = string.Empty;

    [NotNull] public Guid ProjectId { get; set; }
}