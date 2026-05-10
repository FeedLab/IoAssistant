using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.Infrastructure.ViewModels;
using Syncfusion.Maui.Buttons;

namespace IoAssistant.Device.Desktop.Views;

public partial class ModbusDeviceItemView : ContentView
{
    public ModbusDeviceItemView()
    {
        InitializeComponent();
    }

    private void OnExpandClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is ModbusDeviceViewModel vm)
            vm.ModbusDevice.IsExpanded = !vm.ModbusDevice.IsExpanded;
    }

    private void OnDeviceEnabledChanged(object sender, StateChangedEventArgs e)
    {
        if (((SfCheckBox)sender).BindingContext is ModbusDeviceViewModel vm)
            WeakReferenceMessenger.Default.Send(new OnDeviceEnabledChangedMessage(vm, e.IsChecked == true));
    }
}
