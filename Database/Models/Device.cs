using IoAssistant.Infrastructure.Devices;
using SQLite;

namespace IoAssistant.Database.Models;

[Table("Device")]
public class DeviceEntity
{
    [PrimaryKey] public Guid Id { get; set; } = Guid.CreateVersion7();

    [NotNull] public Guid ProjectId { get; set; }

    public Guid ModBusClientId { get; set; }

    public string Name { get; set; } = "Device - Unknown";

    public byte DeviceId { get; set; }

    public ushort RegistersToRead { get; set; } = 1;

    public ushort StartRegister  { get; set; }
    
    public ushort FunctionCode  { get; set; }
    
    public string Description  { get; set; } = String.Empty;

    public int PollingFrequency { get; set; } = 1000; // Default to 1 second

    public int DelayedStart { get; set; } = 1000; // Default to 1 second
}