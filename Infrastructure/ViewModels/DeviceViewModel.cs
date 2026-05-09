using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.ViewModels;

public partial class DeviceViewModel
{
    private readonly ILogger<DeviceViewModel> logger;
    private readonly ObservableCollection<ModbusDeviceViewModel> deviceList = [];

    public DeviceViewModel(ILogger<DeviceViewModel> logger)
    {
        this.logger = logger;

        SubscribeOnDeviceAddedMessage();
    }

    public ObservableCollection<ModbusDeviceViewModel> DeviceList => deviceList;

    private void SubscribeOnDeviceAddedMessage()
    {
        logger.LogDebug("Subscribing to {Message}", nameof(OnDeviceAddedMessage));

        WeakReferenceMessenger.Default.Register<OnDeviceAddedMessage>(this, async void (recipient, m) =>
            {
             await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        logger.LogDebug("Handling {Message} - {Name}", nameof(OnDeviceAddedMessage),
                            m.ModbusDevice.Name);

                        deviceList.Add(new ModbusDeviceViewModel(m.ModbusDevice));
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(SubscribeOnDeviceAddedMessage),
                            e.Message);
                    }
                });
            });
    }
}

public partial class ModbusDeviceViewModel : ObservableObject
{
    private readonly ILogger<ModbusDeviceViewModel> logger;
    
    [ObservableProperty]private ModbusDevice modbusDevice;
    public ObservableCollection<DeviceSensorViewModel> SensorList { get; } = [];

    public ModbusDeviceViewModel(ModbusDevice modbusDevice)
    {
        this.modbusDevice = modbusDevice;
        
        logger = AppService.GetRequiredService<ILogger<ModbusDeviceViewModel>>();
        
        WeakReferenceMessenger.Default.Register<OnSensorAddedMessage>(this, async (recipient, m) =>
        {
            if (m.Sensor.ModbusDevice.Id != modbusDevice.Id) return;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    logger.LogDebug("Handling {Message} - {Name}", nameof(OnSensorAddedMessage), m.Sensor.Name);

                    {
                        SensorList.Add(new DeviceSensorViewModel(m.Sensor));
                        var maxWidth = SensorList.Max(s => s.Sensor.Name.Length * 7.5);
                        foreach (var vm in SensorList)
                        {
                            vm.NameColumnWidth = (decimal)maxWidth;
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error in {Method}: {Message}", nameof(DeviceSensorViewModel),
                        e.Message);
                    
                    throw;
                }
            });
        });
    }
}

public partial class DeviceSensorViewModel : ObservableObject
{
    public Sensor Sensor { get; }

    private readonly ILogger<DeviceSensorViewModel> logger =
        AppService.GetRequiredService<ILogger<DeviceSensorViewModel>>();

    public ObservableCollection<SensorViewModel> SensorList { get; } = [];
    [ObservableProperty] private decimal nameColumnWidth;
    [ObservableProperty] private Color registerValueTextColor;
    [ObservableProperty] private Color nameTextColor = DefaultTextColor();
    private CancellationTokenSource? _highlightCts;

    private static Color DefaultTextColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

    public DeviceSensorViewModel(Sensor sensor)
    {
        Sensor = sensor;

        WeakReferenceMessenger.Default.Register<OnSensorDataChangedMessage>(this, (recipient, m) =>
        {
            if (!m.HasChanged || m.Sensor.NumRegister != sensor.NumRegister)
                return;

            _ = AnimateNameHighlight();
            _ = AnimateHighlight();
        });

        
    }

    private async Task AnimateHighlight()
    {
        _highlightCts?.Cancel();
        _highlightCts = new CancellationTokenSource();
        var token = _highlightCts.Token;
        var toColor = DefaultTextColor();
        try
        {
            MainThread.BeginInvokeOnMainThread(() => RegisterValueTextColor = Colors.Orange);
            await Task.Delay(250, token);
            for (var i = 0; i <= 20; i++)
            {
                var t = (float)i / 20;
                var color = Color.FromRgba(
                    Colors.Orange.Red + (toColor.Red - Colors.Orange.Red) * t,
                    Colors.Orange.Green + (toColor.Green - Colors.Orange.Green) * t,
                    Colors.Orange.Blue + (toColor.Blue - Colors.Orange.Blue) * t,
                    1.0f);
                MainThread.BeginInvokeOnMainThread(() => RegisterValueTextColor = color);
                if (i < 20) await Task.Delay(50, token);
            }
        }
        catch (OperationCanceledException) { }
    }


    private async Task AnimateNameHighlight()
    {
        _highlightCts?.Cancel();
        _highlightCts = new CancellationTokenSource();
        var token = _highlightCts.Token;
        var toColor = DefaultTextColor();
        try
        {
            MainThread.BeginInvokeOnMainThread(() => NameTextColor = Colors.Orange);
            await Task.Delay(250, token);
            for (var i = 0; i <= 20; i++)
            {
                var t = (float)i / 20;
                var color = Color.FromRgba(
                    Colors.Orange.Red + (toColor.Red - Colors.Orange.Red) * t,
                    Colors.Orange.Green + (toColor.Green - Colors.Orange.Green) * t,
                    Colors.Orange.Blue + (toColor.Blue - Colors.Orange.Blue) * t,
                    1.0f);
                MainThread.BeginInvokeOnMainThread(() => NameTextColor = color);
                if (i < 20) await Task.Delay(50, token);
            }
        }
        catch (OperationCanceledException) { }
    }
}

public partial class SensorViewModel : ObservableObject
{
    private CancellationTokenSource? _highlightCts;

    [ObservableProperty] private Sensor sensor;
    [ObservableProperty] private double nameColumnWidth;
    [ObservableProperty] private Color registerValueTextColor = DefaultTextColor();

    private static Color DefaultTextColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

    public SensorViewModel(Sensor sensor)   
    {
        Sensor = sensor;

        WeakReferenceMessenger.Default.Register<OnSensorDataChangedMessage>(this, (recipient, m) =>
        {
            if (!m.HasChanged || m.Sensor.NumRegister != sensor.NumRegister)
                return;

            _ = AnimateHighlight();
        });
    }

    private async Task AnimateHighlight()
    {
        _highlightCts?.Cancel();
        _highlightCts = new CancellationTokenSource();
        var token = _highlightCts.Token;
        var toColor = DefaultTextColor();
        try
        {
            MainThread.BeginInvokeOnMainThread(() => RegisterValueTextColor = Colors.Orange);
            await Task.Delay(250, token);
            for (var i = 0; i <= 20; i++)
            {
                var t = (float)i / 20;
                var color = Color.FromRgba(
                    Colors.Orange.Red + (toColor.Red - Colors.Orange.Red) * t,
                    Colors.Orange.Green + (toColor.Green - Colors.Orange.Green) * t,
                    Colors.Orange.Blue + (toColor.Blue - Colors.Orange.Blue) * t,
                    1.0f);
                MainThread.BeginInvokeOnMainThread(() => RegisterValueTextColor = color);
                if (i < 20) await Task.Delay(50, token);
            }
        }
        catch (OperationCanceledException) { }
    }
}