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
        DeviceDirection direction = DeviceDirection.InAndOut,
        int pollingFrequency = 1000,
        int delayedStart = 1000,
        string description = "")
    {
        PollingFrequency = pollingFrequency;
        DelayedStart = delayedStart;
        Direction = direction;
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

public class SensorDevice : DeviceBase, IDisposable
{
    private readonly ILogger<SensorDevice> logger = AppService.GetRequiredService<ILogger<SensorDevice>>();
    private readonly Dictionary<int, Sensor> sensorInputs = new();

    public SensorDevice(ModBusClient modBusClient, string name, byte deviceId = 1, ushort registerStart = 0,
        ushort registersToRead = 1, DeviceDirection direction = DeviceDirection.InAndOut,
        int pollingFrequency = 1000,
        int delayedStart = 1000, string description = "")
        : base(modBusClient, name, deviceId, registerStart, registersToRead, direction, pollingFrequency, delayedStart,
            string.IsNullOrEmpty(description) ? name : description)
    {
    }

    private void OnTimerCallback(object? state)
    {
        if (!IsEnabled)
            return;

        if (IsExpanded)
            logger.LogDebug("Polling sensor device {DeviceName}", Name);

        Status = "Status: Queued...";

        lock (ModBusClient.BusLock)
        {
            PollingTimer?.Change(Timeout.Infinite, Timeout.Infinite); // stop timer

            try
            {
                Status = "Status: Reading...";
                WeakReferenceMessenger.Default.Send(new OnBeforeModbusReadMessage(this));

                var sensorReadings = ModBusClient.Read(DeviceId, RegisterStart, RegistersToRead);

                WeakReferenceMessenger.Default.Send(new OnBeforeModbusReadMessage(this));

                foreach (var sensor in sensorInputs.Values)
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

    [ObservableProperty] private ushort registerAddress;

    [ObservableProperty] private decimal registerValue;

    [ObservableProperty] private string unit = "%";

    public Sensor(SensorDevice sensorDevice, ushort registerAddress, string name = "Sensor", string sensorType = "Temp",
        int numberOfDecimals = 1, string unit = "C")
    {
        RegisterAddress = registerAddress;
        SensorDevice = sensorDevice;
        Name = name;
        SensorType = sensorType;
        NumberOfDecimals = numberOfDecimals;
        Unit = unit;
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

        var sensorDeviceRegisterStart = RegisterAddress - SensorDevice.RegisterStart;
        var value = sensorReadings[sensorDeviceRegisterStart];

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