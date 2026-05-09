using IoAssistant.Infrastructure.Services;
using IoAssistant.Infrastructure.ViewModels;
using Syncfusion.Maui.ListView;

namespace IoAssistant.Device.Desktop.Views;

public partial class DeviceView : ContentView
{
    private readonly DeviceViewModel viewModel;

    public DeviceView()
    {
        viewModel = AppService.GetRequiredService<DeviceViewModel>();

        InitializeComponent();

        BindingContext = viewModel;
    }

    private void OnExpandClicked(object sender, EventArgs e)
    {
        if (((Button)sender).BindingContext is ModbusDeviceViewModel vm)
            vm.ModbusDevice.IsExpanded = !vm.ModbusDevice.IsExpanded;
    }

    private void OnDeviceDoubleTapped(object sender, ItemDoubleTappedEventArgs e)
    {
        if (e.DataItem is ModbusDeviceViewModel vm)
            vm.ModbusDevice.IsExpanded = !vm.ModbusDevice.IsExpanded;
    }
}