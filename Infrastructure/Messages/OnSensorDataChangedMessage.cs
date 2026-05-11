using IoAssistant.Infrastructure.Devices;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;

namespace IoAssistant.Infrastructure.Messages;

public class OnSensorDataChangedMessage(ISensor sensor, decimal registerValue, decimal oldRegisterValue) : IOnSensorDataChangedMessage
{
    public ISensor Sensor { get; } = sensor;
    public decimal RegisterValue { get; } = registerValue;
    public decimal OldRegisterValue { get; } = oldRegisterValue;
    public bool HasChanged { get; } = registerValue != oldRegisterValue;
}