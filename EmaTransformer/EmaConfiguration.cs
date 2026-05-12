﻿using CommunityToolkit.Mvvm.Messaging;
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
    public ContentView Configuration { get; } = new EmaConfigurationView();
    private IServiceCollection? services;
    private ILogger<EmaConfiguration> logger;

    public EmaConfiguration()
    {
        Id = Guid.Parse("cb677e07-ef4d-4717-930a-420dac9ff961");
    }
    
    public void Register(IServiceCollection serviceCollection)
    {
        services = serviceCollection;
    }

    public void Initialize()
    {
        logger = AppServicePnP.GetRequiredService<ILogger<EmaConfiguration>>();
    }

    public ICalculationEngine CreateInstance(Guid transformerInstanceId, Guid belongToId, Guid projectId,
        string name, string description, string data)
    {
        ICalculationEngine calculation = new CalculationEngine(this, transformerInstanceId, projectId, name, description, data);
        
        return calculation;
    }





}