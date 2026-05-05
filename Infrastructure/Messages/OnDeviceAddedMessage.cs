using IoAssistant.Infrastructure.Devices;

namespace IoAssistant.Infrastructure.Messages;

public class OnDeviceAddedMessage(SensorDevice sensorDevice)
{
    public SensorDevice SensorDevice { get; } = sensorDevice;
}