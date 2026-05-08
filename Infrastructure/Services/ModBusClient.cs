using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.Services;

public abstract partial class ModBusClient : ObservableObject
{
    public readonly Lock BusLock = new();

    [ObservableProperty] private int readTimeout = 2000;
    
    private readonly ILogger<ModBusClient> logger = AppService.GetRequiredService<ILogger<ModBusClient>>();


    public abstract void Start();
    public abstract void Stop();
    public abstract ushort[] Read(byte slaveId, ushort startAddress, ushort numRegisters);
}