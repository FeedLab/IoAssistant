using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Transformers;

// All the code in this file is included in all platforms.
public class EmaConfiguration : ITransformer
{


    public Guid Id { get; }
    
    public int Inputs { get; } = 1;
    public string Name { get; } = "EMA";
    public string Description { get; } = "Exponential Moving Average";
    private decimal Alpha { get; set; }
    private decimal Value { get; set; }

    private EmaStream emaStream;
    public ContentView Configuration { get; } = new EmaConfigurationView();
    private ISensor? sensorToReactOn;
    private IServiceCollection? services;
    private readonly ILogger<EmaConfiguration> logger;

    public EmaConfiguration()
    {
        logger = AppService.GetRequiredService<ILogger<EmaConfiguration>>();
        
        Alpha = 0.6m;
        emaStream = new EmaStream(5, Alpha); // quick response 0.8 - smoother response 0.2
        Id = Guid.Parse("cb677e07-ef4d-4717-930a-420dac9ff961");
    }
    
    public void InitializeAndRegister(IServiceCollection serviceCollection)
    {
        services = serviceCollection;

        SubscribeToOnSensorDataChangedMessage();
    }

    public ICalculate CreateInstance(string transformerDbData)
    {
        var calculation = new Calculation(this, transformerDbData);
        
        return calculation;
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

            Value = emaStream.AddPoint(m.Sensor.Value);
            logger.LogDebug("EMA Value: {Value}", Value);
        });
    }


}