using IoAssistant.Infrastructure.Devices;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;
using SQLite;

namespace IoAssistant.Database.Models;

[Table("Sensor")]
public class SensorEntity
{
    [PrimaryKey] public Guid Id { get; set; } = Guid.CreateVersion7();

    [NotNull] public Guid DeviceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public ushort NumRegister { get; set; }

    public IoDirectionType Direction { get; set; }

    public int NumberOfDecimals { get; set; }
}
