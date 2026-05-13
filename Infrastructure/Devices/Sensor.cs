using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DecimalMath;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.Infrastructure.Services;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.Devices;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class Sensor : ObservableObject, ISensor
{
    private readonly ILogger<ISensor> logger = AppService.GetRequiredService<ILogger<ISensor>>();
    private readonly Dictionary<int, Sensor> sensorInputs = new();

    public Guid Id { get; set; } = Guid.CreateVersion7();

    [ObservableProperty] private IModbusDevice modbusDevice;
    [ObservableProperty] private string name;
    [ObservableProperty] private string fullName;
    [ObservableProperty] private string unit;
    [ObservableProperty] private ushort numRegister;
    [ObservableProperty] private decimal value;
    [ObservableProperty] private IIoDirection direction;
    [ObservableProperty] private int numberOfDecimals;

    public Sensor(IModbusDevice modbusDevice, string name, ushort numRegister, IIoDirection direction, string unit,
        int numberOfDecimals = 0)
    {
        NumberOfDecimals = numberOfDecimals;
        ModbusDevice = modbusDevice;
        NumRegister = numRegister;
        Name = name;
        FullName = $"{modbusDevice.Name} - {name}";
        Unit = unit;
        Direction = direction;
    }

    public void ProcessSensorReading(ushort[] sensorReadings)
    {
        var oldRegisterValue = Value;
        var sensorDeviceRegisterStart = NumRegister;

        var registerValue = sensorReadings[sensorDeviceRegisterStart];

        if (oldRegisterValue == registerValue)
        {
            WeakReferenceMessenger.Default.Send<IOnSensorDataChangedMessage>(new OnSensorDataChangedMessage(this, Value, oldRegisterValue));
            return;
        }

        var divider = DecimalEx.Pow(10.0m, NumberOfDecimals);
        Value = (registerValue / divider);

        logger.LogDebug("Sensor value: {Value}", Value);

        WeakReferenceMessenger.Default.Send<IOnSensorDataChangedMessage>(new OnSensorDataChangedMessage(this, Value, oldRegisterValue));
    }
}