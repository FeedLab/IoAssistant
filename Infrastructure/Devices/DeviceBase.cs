using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using IoAssistant.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.Devices;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0042:Prefer using [ObservableProperty] on partial properties")]
public abstract partial class DeviceBase : ObservableObject
{
    private readonly ILogger<DeviceBase> logger = AppService.GetRequiredService<ILogger<DeviceBase>>();


    [ObservableProperty] private Guid id = Guid.CreateVersion7();

    [ObservableProperty] private ModBusClient modBusClient;

    [ObservableProperty] private string status = "Status: OK";

    [ObservableProperty] private string name = "Device - Unknown";

    [ObservableProperty] private byte deviceId;

    [ObservableProperty] private ushort registersToRead = 1;

    [ObservableProperty] private ushort registerStart = 0;

    [ObservableProperty] private string description = "";

    [ObservableProperty] private int pollingFrequency = 1000; // Default to 1 second

    [ObservableProperty] private int delayedStart = 1000; // Default to 1 second

    [ObservableProperty] private bool isExpanded;

    [ObservableProperty] private bool isEnabled;

    /// <inheritdoc/>
    protected DeviceBase(ModBusClient modBusClient,
        string name,
        byte deviceId = 1,
        ushort registerStart = 0,
        ushort registersToRead = 1,
        int pollingFrequency = 1000,
        int delayedStart = 1000,
        string description = "")
    {
        PollingFrequency = pollingFrequency;
        DelayedStart = delayedStart;
        ModBusClient = modBusClient;
        Name = name;
        DeviceId = deviceId;
        RegisterStart = registerStart;
        RegistersToRead = registersToRead;
        Description = description;
    }

    public abstract void Start();
    public abstract void Stop();
}