using SQLite;

namespace IoAssistant.Database.Models;

[Table("Project")]
public class ProjectEntity
{
    [PrimaryKey]
    public Guid Id { get; set; } = Guid.CreateVersion7();

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
