using System.IO.Ports;
using IoAssistant.Infrastructure.Services;
using SQLite;

namespace IoAssistant.Database.Models;

[Table("ModBusClient")]
public class ModBusClientEntity
{
    [PrimaryKey] public Guid Id { get; set; } = Guid.CreateVersion7();

    [NotNull] public CommunicationType Type { get; set; }

    // TCP fields
    public string? Host { get; set; }
    public ushort Port { get; set; }

    // RTU fields
    public string? PortName { get; set; }
    public int BaudRate { get; set; } = 9600;
    public int DataBits { get; set; } = 8;
    public Parity Parity { get; set; } = Parity.None;
    public StopBits StopBits { get; set; } = StopBits.One;
}
