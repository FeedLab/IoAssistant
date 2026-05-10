using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DecimalMath;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.Devices;

public partial class Sensor : ObservableObject
{
    private readonly ILogger<Sensor> logger = AppService.GetRequiredService<ILogger<Sensor>>();
    private readonly Dictionary<int, Sensor> sensorInputs = new();

    public Guid Id { get; set; } = Guid.CreateVersion7();

    [ObservableProperty] private ModbusDevice modbusDevice;
    [ObservableProperty] private string name;
    [ObservableProperty] private string unit;
    [ObservableProperty] private ushort numRegister;
    [ObservableProperty] private decimal value;
    [ObservableProperty] private IoDirection direction;
    [ObservableProperty] private int numberOfDecimals;

    public Sensor(ModbusDevice modbusDevice, string name, ushort numRegister, IoDirection direction, string unit,
        int numberOfDecimals = 0)
    {
        NumberOfDecimals = numberOfDecimals;
        ModbusDevice = modbusDevice;
        NumRegister = numRegister;
        Name = name;
        Unit = unit;
        Direction = direction;
    }


    // public void AddSensor(ModbusDevice modbusDevice)
    // {
    //     if (!sensorInputs.TryAdd(modbusDevice.RegisterAddress, modbusDevice))
    //     {
    //         logger.LogWarning($"Sensor with index {modbusDevice.RegisterAddress} already exists. Skipping addition.");
    //         return;
    //     }
    //
    //     WeakReferenceMessenger.Default.Send(new OnSensorAddedMessage(modbusDevice));
    // }

    // public List<ModbusDevice> Sensors => sensorInputs.Values.ToList();


    public void ProcessSensorReading(ushort[] sensorReadings)
    {
        var oldRegisterValue = Value;
        var sensorDeviceRegisterStart = NumRegister;

        var registerValue = sensorReadings[sensorDeviceRegisterStart];

        if (oldRegisterValue == registerValue)
        {
            WeakReferenceMessenger.Default.Send(new OnSensorDataChangedMessage(this, Value, oldRegisterValue));
            return;
        }

        var divider = DecimalEx.Pow(10.0m, NumberOfDecimals);
        Value = (registerValue / divider);

        logger.LogDebug("Sensor value: {Value}", Value);

        WeakReferenceMessenger.Default.Send(new OnSensorDataChangedMessage(this, Value, oldRegisterValue));
    }
}