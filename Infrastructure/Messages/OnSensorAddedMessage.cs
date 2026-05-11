using IoAssistant.Infrastructure.Devices;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;

namespace IoAssistant.Infrastructure.Messages;

public class OnSensorAddedMessage(ISensor sensor) : IOnSensorAddedMessage
{
    public ISensor Sensor { get; } = sensor;
}