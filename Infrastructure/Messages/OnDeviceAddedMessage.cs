using IoAssistant.Infrastructure.Devices;

namespace IoAssistant.Infrastructure.Messages;

public class OnDeviceAddedMessage(ModbusDevice modbusDevice)
{
    public ModbusDevice ModbusDevice { get; } = modbusDevice;
}