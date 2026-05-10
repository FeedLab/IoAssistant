using IoAssistant.Infrastructure.ViewModels;

namespace IoAssistant.Infrastructure.Messages;

public class OnDeviceEnabledChangedMessage(ModbusDeviceViewModel deviceViewModel, bool isEnabled)
{
    public ModbusDeviceViewModel DeviceViewModel { get; } = deviceViewModel;
    public bool IsEnabled { get; } = isEnabled;
}
