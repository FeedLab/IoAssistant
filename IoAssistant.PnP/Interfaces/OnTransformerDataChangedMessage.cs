namespace IoAssistant.PnP.Interfaces;

public class OnTransformerDataChangedMessage(
    ICalculationEngine calculationEngine,
    decimal oldValue,
    decimal calculatedValue,
    IList<ISensor> sensorInputs)
    : IOnTransformerDataChangedMessage
{
    public ICalculationEngine CalculationEngine { get; } = calculationEngine;
    public decimal OldValue { get; } = oldValue;
    public decimal CalculatedValue { get; } = calculatedValue;
    public IList<ISensor> SensorInputs { get; } = sensorInputs;
    public bool HasChanged => OldValue != CalculatedValue;
}