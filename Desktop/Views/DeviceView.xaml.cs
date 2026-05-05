using IoAssistant.Infrastructure.Services;
using IoAssistant.Infrastructure.ViewModels;

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
        if (((Button)sender).BindingContext is DeviceSensorViewModel vm)
            vm.SensorDevice.IsExpanded = !vm.SensorDevice.IsExpanded;
    }
}