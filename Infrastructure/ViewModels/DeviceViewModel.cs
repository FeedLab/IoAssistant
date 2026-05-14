using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.Infrastructure.Services;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.ViewModels;

public partial class DeviceViewModel
{
    private readonly ILogger<DeviceViewModel> logger;
    private readonly ObservableCollection<ModbusDeviceViewModel> deviceList = [];
    private readonly ObservableCollection<DeviceSensorViewModel> selectedSensors = [];

    public DeviceViewModel(ILogger<DeviceViewModel> logger)
    {
        this.logger = logger;

        SubscribeOnDeviceAddedMessage();
        SubscribeOnDeviceEnabledChangedMessage();
    }

    public ObservableCollection<ModbusDeviceViewModel> DeviceList => deviceList;
    public ObservableCollection<DeviceSensorViewModel> SelectedSensors => selectedSensors;

    private void SubscribeOnDeviceEnabledChangedMessage()
    {
        WeakReferenceMessenger.Default.Register<OnDeviceEnabledChangedMessage>(this, (recipient, m) =>
        {
            if (m.IsEnabled)
            {
                foreach (var sensor in m.DeviceViewModel.SensorList)
                    selectedSensors.Add(sensor);
            }
            else
            {
                foreach (var sensor in m.DeviceViewModel.SensorList)
                    selectedSensors.Remove(sensor);
            }
        });
    }

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
    private CancellationTokenSource? _highlightCts;

    [ObservableProperty] private ModbusDevice modbusDevice;
    [ObservableProperty] private Color nameTextColor = DefaultNameTextColor();
    public ObservableCollection<DeviceSensorViewModel> SensorList { get; } = [];

    private static Color DefaultNameTextColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

    public ModbusDeviceViewModel(ModbusDevice modbusDevice)
    {
        this.modbusDevice = modbusDevice;

        logger = AppService.GetRequiredService<ILogger<ModbusDeviceViewModel>>();

        WeakReferenceMessenger.Default.Register<IOnSensorAddedMessage>(this, async void (recipient, m) =>
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

                    throw new InvalidOperationException("Error in DeviceSensorViewModel", e);
                }
            });
        });

        WeakReferenceMessenger.Default.Register<IOnSensorDataChangedMessage>(this, (recipient, m) =>
        {
            if (!m.HasChanged || m.Sensor.ModbusDevice.Id != modbusDevice.Id) return;
            _ = AnimateNameHighlight();
        });
    }

    private async Task AnimateNameHighlight()
    {
        _highlightCts?.Cancel();
        _highlightCts = new CancellationTokenSource();
        var token = _highlightCts.Token;
        var toColor = DefaultNameTextColor();
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

public record SensorDataPoint(DateTime Timestamp, double Value);

/// <summary>
/// 15-minute rolling chart data source.
/// Fires Add while no trimming occurs, Reset when old points are dropped,
/// to avoid Syncfusion's SetIndividualPoint index-out-of-range bug caused by RemoveAt.
/// </summary>
public class BoundedChartData : IList<SensorDataPoint>, INotifyCollectionChanged
{
    private static readonly TimeSpan RetentionWindow = TimeSpan.FromMinutes(15);
    private readonly List<SensorDataPoint> items = [];

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public int Count => items.Count;
    public bool IsReadOnly => true;
    public DateTime? OldestTimestamp => items.Count > 0 ? items[0].Timestamp : null;

    public SensorDataPoint this[int index]
    {
        get => items[index];
        set => throw new NotSupportedException();
    }

    public void Add(SensorDataPoint item)
    {
        var cutoff = DateTime.Now - RetentionWindow;
        var removeCount = 0;
        while (removeCount < items.Count && items[removeCount].Timestamp < cutoff)
            removeCount++;

        if (removeCount > 0)
        {
            items.RemoveRange(0, removeCount);
            CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        items.Add(item);
        CollectionChanged?.Invoke(this,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, items.Count - 1));
    }

    public int IndexOf(SensorDataPoint item) => items.IndexOf(item);
    public bool Contains(SensorDataPoint item) => items.Contains(item);
    public void CopyTo(SensorDataPoint[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);
    public IEnumerator<SensorDataPoint> GetEnumerator() => items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

    void IList<SensorDataPoint>.Insert(int index, SensorDataPoint item) => throw new NotSupportedException();
    void IList<SensorDataPoint>.RemoveAt(int index) => throw new NotSupportedException();
    void ICollection<SensorDataPoint>.Clear() => throw new NotSupportedException();
    bool ICollection<SensorDataPoint>.Remove(SensorDataPoint item) => throw new NotSupportedException();
}

public partial class DeviceSensorViewModel : ObservableObject
{
    public ISensor Sensor { get; }

    private readonly ILogger<DeviceSensorViewModel> logger =
        AppService.GetRequiredService<ILogger<DeviceSensorViewModel>>();

    public ObservableCollection<SensorViewModel> SensorList { get; } = [];
    public BoundedChartData DataPoints { get; } = new();
    [ObservableProperty] private decimal nameColumnWidth;
    [ObservableProperty] private Color registerValueTextColor;
    [ObservableProperty] private Color nameTextColor = DefaultTextColor();
    private CancellationTokenSource? _highlightCts;

    private static Color DefaultTextColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

    public DeviceSensorViewModel(ISensor sensor)
    {
        Sensor = sensor;

        WeakReferenceMessenger.Default.Register<IOnSensorDataChangedMessage>(this, (recipient, m) =>
        {
            if (!m.HasChanged || m.Sensor != sensor)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
                DataPoints.Add(new SensorDataPoint(DateTime.Now, (double)m.RegisterValue)));

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

    [ObservableProperty] private ISensor sensor;
    [ObservableProperty] private double nameColumnWidth;
    [ObservableProperty] private Color registerValueTextColor = DefaultTextColor();

    private static Color DefaultTextColor() =>
        Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;

    public SensorViewModel(ISensor sensor)   
    {
        Sensor = sensor;

        WeakReferenceMessenger.Default.Register<IOnSensorDataChangedMessage>(this, (recipient, m) =>
        {
            if (!m.HasChanged || m.Sensor != sensor)
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