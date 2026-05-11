using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Transformers;

public class Calculation : ICalculate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "EMA";
    public string Description { get; set; } = "Exponential Moving Average";
    public Guid BelongToId { get; set; }

    public decimal Alpha { get; set; }
    public decimal CalculatedValue { get; set; }

    public ITransformer Transformer { get; set; }
    private EmaStream emaStream;
    private ISensor? sensorToReactOn;
    private readonly ILogger<Calculation> logger;

    public Calculation(ITransformer belongTo, decimal alpha, ISensor sensorToReactOn)
    {
        logger = AppService.GetRequiredService<ILogger<Calculation>>();

        Id = Guid.CreateVersion7();
        Alpha = alpha;
        Transformer = belongTo;
        this.sensorToReactOn = sensorToReactOn;
        emaStream = new EmaStream(5, Alpha);

        SubscribeToOnSensorDataChangedMessage();
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