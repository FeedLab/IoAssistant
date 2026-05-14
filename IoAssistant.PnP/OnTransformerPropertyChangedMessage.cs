using IoAssistant.PnP.Interfaces;

namespace IoAssistant.PnP;

public class OnTransformerPropertyChangedMessage(ICalculationEngine calculationEngine)
    : IOnTransformerPropertyChangedMessage
{
    public ICalculationEngine CalculationEngine { get; } = calculationEngine;
}