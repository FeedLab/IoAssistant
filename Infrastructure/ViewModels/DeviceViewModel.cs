using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;

namespace IoAssistant.Infrastructure.ViewModels;

public class DeviceViewModel
{
    private readonly ILogger<DeviceViewModel> logger;
    private readonly ObservableCollection<DeviceSensorViewModel> sensorList = [];

    public DeviceViewModel(ILogger<DeviceViewModel> logger)
    {
        this.logger = logger;

        SubscribeOnDeviceAddedMessage();
    }

    public ObservableCollection<DeviceSensorViewModel> SensorList => sensorList;

    private void SubscribeOnDeviceAddedMessage()
    {
        logger.LogDebug("Subscribing to {Message}", nameof(OnDeviceAddedMessage));
        
        WeakReferenceMessenger.Default.Register<OnDeviceAddedMessage>(this,
            (_, m) =>
            {
                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    try
                    {
                        logger.LogDebug("Handling {Message} - {Name}", nameof(OnDeviceAddedMessage), m.SensorDevice.Name);
                        
                        SensorList.Add(new DeviceSensorViewModel(m.SensorDevice));
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "Error in {Method}: {Message}", nameof(DeviceViewModel), e.Message);
                    }
                });
            });
    }
}

public partial class DeviceSensorViewModel : ObservableObject
{
    private readonly Dictionary<Sensor, CancellationTokenSource> _highlightCts = new();

    [ObservableProperty] private SensorDevice sensorDevice;

    public DeviceSensorViewModel(SensorDevice sensorDevice)
    {
        SensorDevice = sensorDevice;

        WeakReferenceMessenger.Default.Register<OnSensorDataChangedMessage>(this, (_, m) =>
        {
            if (!m.HasChanged || !SensorDevice.Sensors.Contains(m.Sensor))
                return;

            if (_highlightCts.TryGetValue(m.Sensor, out var existing))
                existing.Cancel();

            var cts = new CancellationTokenSource();
            _highlightCts[m.Sensor] = cts;
            var token = cts.Token;

            MainThread.BeginInvokeOnMainThread(() => m.Sensor.IsRegisterValueHighlighted = true);

            Task.Delay(1000, token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                    MainThread.BeginInvokeOnMainThread(() => m.Sensor.IsRegisterValueHighlighted = false);
            }, TaskScheduler.Default);
        });
    }
}