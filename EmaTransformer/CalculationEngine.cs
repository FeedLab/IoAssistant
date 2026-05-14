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

[SuppressMessage("CommunityToolkit.Mvvm.SourceGenerators.ObservablePropertyGenerator",
    "MVVMTK0045:Using [ObservableProperty] on fields is not AOT compatible for WinRT")]
public partial class CalculationEngine : ObservableObject, ICalculationEngine
{
    [ObservableProperty] private string name;
    [ObservableProperty] private string fullName;
    [ObservableProperty] private string description;
    [ObservableProperty] private Guid id;
    [ObservableProperty] private Guid belongToId;

    [ObservableProperty] private decimal alpha;
    [ObservableProperty] private int period = 5;
    [ObservableProperty] private bool isEnabled;
    [ObservableProperty] private Guid sensorId;
    [ObservableProperty] private ISensor? sensorToReactOn;

    [ObservableProperty] private decimal calculatedValue;
    [ObservableProperty] private decimal originalValue;

    public ITransformer Transformer { get; set; }
    public Guid ProjectId { get; set; }

    private readonly EmaStream emaStream;
    private readonly ILogger<CalculationEngine> logger;
    private bool doNotSave;
    public ObservableCollection<ISensor> Sensors { get; } = [];

    public CalculationEngine(ITransformer transformer, Guid instanceId, Guid projectId,
        string name, string description, string data)
    {
        logger = AppServicePnP.GetRequiredService<ILogger<CalculationEngine>>();

        doNotSave = true;

        Transformer = transformer;
        Id = instanceId;
        BelongToId = transformer.Id;
        ProjectId = projectId;
        Description = description;
        Name = name;
        FullName = name;

        FromJson(data);

        emaStream = new EmaStream(Period, Alpha);
        doNotSave = false;

        SubscribeToOnSensorDataChangedMessage();
        SubscribeToOnSensorAddedMessage();
    }

    public void FromJson(string json)
    {
        try
        {
            doNotSave = true;

            var data = JsonSerializer.Deserialize<EmaData>(json);

            if (data != null)
            {
                Alpha = data.Alpha;
                Period = data.Period;
                IsEnabled = data.IsEnabled;
                SensorId = data.SensorId;
            }
        }
        finally
        {
            doNotSave = false;
        }
    }

    private record EmaData(decimal Alpha, int Period, bool IsEnabled, Guid SensorId);

    public string ToJson()
    {
        return JsonSerializer.Serialize(new { Alpha, Period, IsEnabled, SensorId });
    }

    partial void OnSensorToReactOnChanged(ISensor? value)
    {
        if (value is null)
            return;

        if (doNotSave)
            return;

        SensorId = value.Id;

        WeakReferenceMessenger.Default.Send<IOnTransformerPropertyChangedMessage>(
            new OnTransformerPropertyChangedMessage(this));

        emaStream.Reset();

        CalculatedValue = 0.0m;
        OriginalValue = 0.0m;

        FullName = value.FullName;
    }

    partial void OnAlphaChanged(decimal value)
    {
        if (doNotSave)
            return;

        WeakReferenceMessenger.Default.Send<IOnTransformerPropertyChangedMessage>(
            new OnTransformerPropertyChangedMessage(this));
    }

    partial void OnNameChanged(string value)
    {
        if (doNotSave)
            return;

        WeakReferenceMessenger.Default.Send<IOnTransformerPropertyChangedMessage>(
            new OnTransformerPropertyChangedMessage(this));
    }

    private void SubscribeToOnSensorDataChangedMessage()
    {
        WeakReferenceMessenger.Default.Register<IOnSensorDataChangedMessage>(this, (_, m) =>
        {
            if (SensorToReactOn == null)
            {
                logger.LogError("Sensor to react on not set");
                return;
            }

            if (!m.HasChanged || m.Sensor.Id != SensorToReactOn.Id)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                OriginalValue = m.Sensor.Value;
                CalculatedValue = emaStream.AddPoint(m.Sensor.Value);
            });

            logger.LogDebug("EMA Value: {Value}", CalculatedValue);
        });
    }

    private void SubscribeToOnSensorAddedMessage()
    {
        WeakReferenceMessenger.Default.Register<IOnSensorAddedMessage>(this, (recipient, m) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Sensors.Add(m.Sensor);

                if (SensorId == m.Sensor.Id)
                {
                    doNotSave = true;
                    try
                    {
                        SensorToReactOn = m.Sensor;
                        FullName = m.Sensor.FullName;
                    }
                    finally
                    {
                        doNotSave = false;
                    }
                }
            });
        });
    }
}