using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Transformers;

public class CalculationEngine : ICalculationEngine
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid BelongToId { get; set; }

    public decimal Alpha { get; set; }
    public int Period { get; set; } = 5;
    public bool IsEnabled { get; set; } = true;
    public Guid SensorId { get; set; }

    public decimal CalculatedValue { get; set; }

    public ITransformer Transformer { get; set; }
    public Guid ProjectId { get; set; }

    private EmaStream emaStream;
    private ISensor? sensorToReactOn;
    private readonly ILogger<CalculationEngine> logger;
    private List<ISensor> sensors = [];

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
            sensors.Add(m.Sensor);
            
            if(SensorId == m.Sensor.Id)
            {
                sensorToReactOn = m.Sensor;
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

    private record EmaData(decimal Alpha, int Period, bool IsEnabled, Guid SensorId);


    public string ToJson()
    {
        return JsonSerializer.Serialize(new { Alpha, Period, IsEnabled, SensorId });
    }

    private void SubscribeToOnSensorDataChangedMessage()
    {
        WeakReferenceMessenger.Default.Register<IOnSensorDataChangedMessage>(this, (recipient, m) =>
        {
            if (sensorToReactOn == null)
            {
                logger.LogError("Sensor to react on not set");
                return;
            }

            if (!m.HasChanged || m.Sensor.Id != sensorToReactOn.Id)
                return;

            CalculatedValue = emaStream.AddPoint(m.Sensor.Value);
            logger.LogDebug("EMA Value: {Value}", CalculatedValue);
        });
    }

}