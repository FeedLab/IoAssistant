using IoAssistant.Infrastructure.Devices;

namespace IoAssistant.Infrastructure.Messages;

public class OnSensorAddedMessage(Sensor sensor)
{
    public Sensor Sensor { get; } = sensor;
}