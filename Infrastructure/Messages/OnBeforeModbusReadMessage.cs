using IoAssistant.Infrastructure.Devices;

namespace IoAssistant.Infrastructure.Messages;

public class OnBeforeModbusReadMessage(ModbusDevice modbusDevice)
{
    public ModbusDevice ModbusDevice { get; } = modbusDevice;
}