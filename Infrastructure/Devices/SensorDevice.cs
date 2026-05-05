using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;
using DecimalMath;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.Devices;

public enum DeviceDirection
{
    Input,
    Output,
    InAndOut
}

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0042:Prefer using [ObservableProperty] on partial properties")]
public abstract partial class DeviceBase : ObservableObject
{
    protected Timer? PollingTimer;

    private readonly ILogger<DeviceBase> logger = AppService.GetRequiredService<ILogger<DeviceBase>>();

    [ObservableProperty] private DeviceDirection direction;

    [ObservableProperty] private Guid id = Guid.CreateVersion7();

    [ObservableProperty] private ModBusRtuClient? modBusRtuClient;

    [ObservableProperty] private string name = "Device - Unknown";

    [ObservableProperty] private byte deviceId;

    [ObservableProperty] private byte registersToRead = 1;

    [ObservableProperty] private byte registerStart = 0;

    [ObservableProperty] private string description = "";

    [ObservableProperty] private int pollingFrequency = 1000; // Default to 1 second

    [ObservableProperty] private int delayedStart = 1000; // Default to 1 second

    [ObservableProperty] private bool isExpanded;

    /// <inheritdoc/>
    protected DeviceBase(ModBusRtuClient modBusRtuClient,
        string name,
        byte deviceId = 1,
        byte registerStart = 0,
        byte registersToRead = 1,
        DeviceDirection direction = DeviceDirection.InAndOut)
    {
        this.direction = direction;
        ModBusRtuClient = modBusRtuClient;
        Name = name;
        DeviceId = deviceId;
        RegisterStart = registerStart;
        RegistersToRead = registersToRead;
        Description = description;
    }

    public abstract void Start();
    public abstract void Stop();
}

public class SensorDevice : DeviceBase, IDisposable
{
    private readonly ILogger<SensorDevice> logger = AppService.GetRequiredService<ILogger<SensorDevice>>();
    private readonly Dictionary<int, Sensor> sensorInputs = new();

    public SensorDevice(ModBusRtuClient modBusRtuClient, string name, byte deviceId = 1, byte registerStart = 0,
        byte registersToRead = 1, DeviceDirection direction = DeviceDirection.InAndOut, string description = "")
        : base(modBusRtuClient, name, deviceId, registerStart, registersToRead, direction)
    {
    }

    private void OnTimerCallback(object? state)
    {
        logger.LogDebug("Polling sensor device {DeviceName}", Name);

        PollingTimer?.Change(Timeout.Infinite, Timeout.Infinite); // stop timer

        if (ModBusRtuClient is null)
        {
            logger.LogWarning("ModBus client is not initialized for sensor device {DeviceName}. Skipping polling.",
                Name);
            return;
        }

        try
        {
            WeakReferenceMessenger.Default.Send(new OnBeforeModbusReadMessage(this));

            var sensorReadings = ModBusRtuClient.Read(DeviceId, RegisterStart, RegistersToRead);

            WeakReferenceMessenger.Default.Send(new OnBeforeModbusReadMessage(this));

            foreach (var sensor in sensorInputs.Values)
            {
                sensor.ProcessSensorReading(sensorReadings);
            }

            WeakReferenceMessenger.Default.Send(new OnModbusReadDataTransformedMessage(this));
        }
        finally
        {
            logger.LogDebug("Polling sensor device {DeviceName} completed", Name);
            PollingTimer?.Change(PollingFrequency, PollingFrequency); // restart
        }
    }

    public void Dispose()
    {
        PollingTimer?.Dispose();
    }

    public void AddSensor(Sensor sensor)
    {
        if (!sensorInputs.TryAdd(sensor.RegisterAddress, sensor))
        {
            logger.LogWarning($"Sensor with index {sensor.RegisterAddress} already exists. Skipping addition.");
            return;
        }
        
        WeakReferenceMessenger.Default.Send(new OnSensorAddedMessage(sensor));
    }

    public List<Sensor> Sensors => sensorInputs.Values.ToList();

    public override void Start()
    {
        PollingTimer = new Timer(OnTimerCallback, null, PollingFrequency, PollingFrequency);
    }

    public override void Stop()
    {
        PollingTimer?.Dispose();
    }
}

public partial class Sensor : ObservableObject
{
    private readonly ILogger<Sensor> logger = AppService.GetRequiredService<ILogger<Sensor>>();

    [ObservableProperty] private SensorDevice sensorDevice;

    [ObservableProperty] private string name;

    [ObservableProperty] private string sensorType;

    [ObservableProperty] private int numberOfDecimals;

    [ObservableProperty] private int registerAddress;

    [ObservableProperty] private decimal registerValue;

    [ObservableProperty] private string unit = "%";

    public Sensor(SensorDevice sensorDevice, byte registerAddress)
    {
        RegisterAddress = registerAddress;
        SensorDevice = sensorDevice;
    }

    public void ProcessSensorReading(ushort[] sensorReadings)
    {
        var oldRegisterValue = RegisterValue;
        
        if (sensorReadings.Length == 0)
        {
            logger.LogWarning(
                "Invalid sensor readings provided for sensor {SensorName} ({SensorType}) on device {DeviceName} at register {RegisterAddress}. Skipping processing.",
                Name, SensorType, SensorDevice.Name, RegisterAddress);
            return;
        }

        var value = sensorReadings[RegisterAddress];

        if (value == 0)
        {
            logger.LogWarning(
                "Sensor {SensorName} ({SensorType}) on device {DeviceName} at register {RegisterAddress} value is zero. Skipping processing.",
                Name, SensorType, SensorDevice.Name, RegisterAddress);
            RegisterValue = 0;
        }
        else
        {
            var divider = DecimalEx.Pow(10.0m, NumberOfDecimals);
            RegisterValue = (value / divider);

            logger.LogInformation(
                "Processed sensor reading for {SensorName} ({SensorType}) on device {DeviceName} at register {RegisterAddress}: {RegisterValue} (decimals: {NumberOfDecimals})",
                Name, SensorType, SensorDevice.Name, RegisterAddress, RegisterValue, NumberOfDecimals);
        }

        WeakReferenceMessenger.Default.Send(new OnSensorDataChangedMessage(this, RegisterValue, oldRegisterValue));
    }
}

