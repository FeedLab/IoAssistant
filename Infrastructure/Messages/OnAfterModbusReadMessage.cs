using IoAssistant.Infrastructure.Devices;

namespace IoAssistant.Infrastructure.Messages;

public class OnAfterModbusReadMessage(ModbusDevice modbusDevice)
{
    public ModbusDevice ModbusDevice { get; } = modbusDevice;
}