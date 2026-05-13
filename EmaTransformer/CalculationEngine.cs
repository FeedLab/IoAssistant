using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Transformers;

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator", "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class CalculationEngine : ObservableObject, ICalculationEngine
{
    [ObservableProperty] private Guid id;
    [ObservableProperty] private string name;
    [ObservableProperty] private string description;
    [ObservableProperty] private Guid belongToId;

    [ObservableProperty] private decimal alpha;
    [ObservableProperty] private int period;
    [ObservableProperty] private bool isEnabled;
    [ObservableProperty] private Guid sensorId;
    [ObservableProperty] private ISensor? sensorToReactOn;

    [ObservableProperty] private decimal calculatedValue;
    [ObservableProperty] private decimal originalValue;

    public ITransformer Transformer { get; set; }
    public Guid ProjectId { get; set; }

    private readonly EmaStream emaStream;
    private readonly ILogger<CalculationEngine> logger;
    public ObservableCollection<ISensor> Sensors { get; } = [];

    public CalculationEngine(ITransformer transformer, Guid instanceId, Guid projectId,
        string name, string description, string data)
    {
        logger = AppServicePnP.GetRequiredService<ILogger<CalculationEngine>>();

        Transformer = transformer;
        Id = instanceId;
        BelongToId = transformer.Id;
        ProjectId = projectId;
        Description = description;
        Name = name;

        FromJson(data);

        emaStream = new EmaStream(Period, Alpha);

        SubscribeToOnSensorDataChangedMessage();
        SubscribeToOnSensorAddedMessage();
    }

    private void SubscribeToOnSensorAddedMessage()
    {
        WeakReferenceMessenger.Default.Register<IOnSensorAddedMessage>(this, async void (recipient, m) =>
        {
            Sensors.Add(m.Sensor);

            if (SensorId == m.Sensor.Id)
            {
                SensorToReactOn = m.Sensor;
            }
        });
    }

    public void FromJson(string json)
    {
        var data = JsonSerializer.Deserialize<EmaData>(json);

        if (data != null)
        {
            Alpha = data.Alpha;
            Period = data.Period;
            IsEnabled = data.IsEnabled;
            SensorId = data.SensorId;
        }
    }

    // partial void OnSensorIdChanged(Guid value)
    // {
    //     sensorToReactOn = Sensors.FirstOrDefault(s => s.Id == value);
    // }

    private record EmaData(decimal Alpha, int Period, bool IsEnabled, Guid SensorId);


    public string ToJson()
    {
        return JsonSerializer.Serialize(new { Alpha, Period, IsEnabled, SensorId });
    }

    public void SelectedSensorChanger(ISensor sensor)
    {
        SensorToReactOn = sensor;
    }

    private void SubscribeToOnSensorDataChangedMessage()
    {
        WeakReferenceMessenger.Default.Register<IOnSensorDataChangedMessage>(this, (recipient, m) =>
        {
            if (SensorToReactOn == null)
            {
                logger.LogError("Sensor to react on not set");
                return;
            }

            if (!m.HasChanged || m.Sensor.Id != sensorToReactOn.Id)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                OriginalValue = m.Sensor.Value;
                CalculatedValue = emaStream.AddPoint(m.Sensor.Value);
            });
            
            logger.LogDebug("EMA Value: {Value}", CalculatedValue);
        });
    }
}