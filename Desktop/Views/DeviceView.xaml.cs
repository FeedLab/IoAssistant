using System.Collections.Specialized;
using IoAssistant.Infrastructure.Services;
using IoAssistant.Infrastructure.ViewModels;
using Syncfusion.Maui.Charts;
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.TabView;

namespace IoAssistant.Device.Desktop.Views;

public partial class DeviceView : ContentView
{
    private readonly DeviceViewModel viewModel;

    private readonly Dictionary<string, (SfTabItem Tab, SfCartesianChart Chart, Dictionary<DeviceSensorViewModel, LineSeries> SeriesMap)>
        sensorTabs = new();

    private readonly Dictionary<DeviceSensorViewModel, NotifyCollectionChangedEventHandler>
        resetHandlers = new();

    public DeviceView()
    {
        viewModel = AppService.GetRequiredService<DeviceViewModel>();

        InitializeComponent();

        BindingContext = viewModel;

        viewModel.SelectedSensors.CollectionChanged += OnSelectedSensorsChanged;
    }

    private void OnSelectedSensorsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (DeviceSensorViewModel sensor in e.NewItems)
                AddSensorToChart(sensor);

        if (e.OldItems != null)
            foreach (DeviceSensorViewModel sensor in e.OldItems)
                RemoveSensorFromChart(sensor);
    }

    private void AddSensorToChart(DeviceSensorViewModel sensor)
    {
        var sensorName = sensor.Sensor.Name;

        if (!sensorTabs.TryGetValue(sensorName, out var entry))
        {
            var chart = new SfCartesianChart { Margin = new Thickness(12, 24) };
            chart.XAxes.Add(new DateTimeAxis());
            chart.YAxes.Add(new NumericalAxis());
            chart.Legend = new ChartLegend();

            var tab = new SfTabItem { Header = sensorName, Content = chart };
            SensorTabs.Items.Add(tab);
            entry = (tab, chart, new Dictionary<DeviceSensorViewModel, LineSeries>());
            sensorTabs[sensorName] = entry;
        }

        var series = new LineSeries
        {
            ItemsSource = sensor.DataPoints,
            XBindingPath = nameof(SensorDataPoint.Timestamp),
            YBindingPath = nameof(SensorDataPoint.Value),
            Label = sensor.Sensor.ModbusDevice.Name,
            EnableAnimation = false
        };
        entry.SeriesMap[sensor] = series;
        entry.Chart.Series.Add(series);

        var xAxis = (DateTimeAxis)entry.Chart.XAxes[0];
        NotifyCollectionChangedEventHandler handler = (_, e) =>
        {
            if (e.Action != NotifyCollectionChangedAction.Reset) return;
            UpdateAxisMinimum(xAxis, sensorTabs[sensorName].SeriesMap.Keys);
        };
        sensor.DataPoints.CollectionChanged += handler;
        resetHandlers[sensor] = handler;
    }

    private void RemoveSensorFromChart(DeviceSensorViewModel sensor)
    {
        var sensorName = sensor.Sensor.Name;
        if (!sensorTabs.TryGetValue(sensorName, out var entry)) return;

        if (resetHandlers.Remove(sensor, out var handler))
            sensor.DataPoints.CollectionChanged -= handler;

        if (entry.SeriesMap.Remove(sensor, out var series))
            entry.Chart.Series.Remove(series);

        if (entry.SeriesMap.Count == 0)
        {
            SensorTabs.Items.Remove(entry.Tab);
            sensorTabs.Remove(sensorName);
        }
    }

    private static void UpdateAxisMinimum(DateTimeAxis xAxis, IEnumerable<DeviceSensorViewModel> sensors)
    {
        var oldest = sensors
            .Select(s => s.DataPoints.OldestTimestamp)
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .DefaultIfEmpty(DateTime.Now)
            .Min();

        xAxis.Minimum = oldest;
    }

    private void OnDeviceDoubleTapped(object sender, ItemDoubleTappedEventArgs e)
    {
        if (e.DataItem is ModbusDeviceViewModel vm)
            vm.ModbusDevice.IsExpanded = !vm.ModbusDevice.IsExpanded;
    }
}
