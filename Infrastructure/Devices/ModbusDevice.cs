using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DecimalMath;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.Devices;



[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0034:Direct field reference to [ObservableProperty] backing field")]
public partial class ModbusDevice : ObservableObject
{
    private readonly ILogger<ModbusDevice> logger = AppService.GetRequiredService<ILogger<ModbusDevice>>();

    protected Timer? PollingTimer;

    [ObservableProperty] private Guid id = Guid.CreateVersion7();

    [ObservableProperty] private ModBusClient modBusClient;

    [ObservableProperty] private string status = "Status: OK";

    [ObservableProperty] private string name = "Device - Unknown";

    [ObservableProperty] private byte deviceId;

    [ObservableProperty] private ushort registersToRead = 1;

    [ObservableProperty] private ushort startRegister = 0;

    [ObservableProperty] private string description = "";

    [ObservableProperty] private int pollingFrequency = 1000; // Default to 1 second

    [ObservableProperty] private int delayedStart = 1000; // Default to 1 second

    [ObservableProperty] private bool isExpanded;

    [ObservableProperty] private bool isEnabled;
    
    [ObservableProperty] private ushort functionCode;
    
    private readonly List<Sensor> sensors = [];


    public ModbusDevice(
        ModBusClient modBusClient,
        string name, 
        byte deviceId = 1, 
        ushort startRegister = 0,
        ushort registersToRead = 1, 
        ushort functionCode = 4, 
        int pollingFrequency = 1000,
        int delayedStart = 1000, 
        string description = "")
    {
        ModBusClient = modBusClient;
        Name = name;
        DeviceId = deviceId;
        StartRegister = startRegister;
        RegistersToRead = registersToRead;
        FunctionCode = functionCode;
        PollingFrequency = pollingFrequency;
        DelayedStart = delayedStart;
        Description = string.IsNullOrEmpty(description) ? name : description;
    }
    
    private void OnTimerCallback(object? state)
    {
        if (!IsEnabled)
            return;

        if (IsExpanded)
            logger.LogDebug("Polling sensor device {DeviceName}", Name);

        Status = "Status: Queued...";

        lock (modBusClient.BusLock)
        {
            PollingTimer?.Change(Timeout.Infinite, Timeout.Infinite); // stop timer

            try
            {
                Status = "Status: Reading...";
                WeakReferenceMessenger.Default.Send(new OnBeforeModbusReadMessage(this));

                var sensorReadings = modBusClient.Read(DeviceId, startRegister, RegistersToRead, FunctionCode);

                WeakReferenceMessenger.Default.Send(new OnAfterModbusReadMessage(this));

                foreach (var sensor in sensors)
                {
                    sensor.ProcessSensorReading(sensorReadings);
                }

                Status = "Status: Reading... Ok";
                WeakReferenceMessenger.Default.Send(new OnModbusReadDataTransformedMessage(this));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error reading ModBus data for sensor device {DeviceName}", Name);
                Status = $"Status: Error: {ex.Message}";
            }
            finally
            {
                logger.LogDebug("Polling sensor device {DeviceName} completed", Name);
                PollingTimer?.Change(PollingFrequency, PollingFrequency); // restart
            }
        }
    }

    public void Start()
    {
        PollingTimer = new Timer(OnTimerCallback, null, PollingFrequency, PollingFrequency);
    }

    public void Stop()
    {
        PollingTimer?.Dispose();
    }
    
    public void Dispose()
    {
        PollingTimer?.Dispose();
    }

    public void AddSensor(Sensor sensor)
    {
        sensors.Add(sensor);

        WeakReferenceMessenger.Default.Send(new OnSensorAddedMessage(sensor));
    }
    

}