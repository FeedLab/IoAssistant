namespace IoAssistant.PnP.Interfaces;

public interface IOnTransformerDataChangedMessage
{
    ICalculationEngine CalculationEngine { get; }
    decimal OldValue { get; }
    decimal CalculatedValue { get; }
    IList<ISensor> SensorInputs { get; }
    bool HasChanged { get; }
}