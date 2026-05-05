using IoAssistant.Infrastructure.Devices;

namespace IoAssistant.Infrastructure.Messages;

public class OnSensorDataChangedMessage(Sensor sensor, decimal registerValue, decimal oldRegisterValue)
{
    public Sensor Sensor { get; } = sensor;
    public decimal RegisterValue { get; } = registerValue;
    public decimal OldRegisterValue { get; } = oldRegisterValue;
    public bool HasChanged { get; } = registerValue != oldRegisterValue;
}