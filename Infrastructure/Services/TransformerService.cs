using IoAssistant.PnP.Interfaces;

namespace IoAssistant.Infrastructure.Services;

public class TransformerService
{
    private readonly List<ICalculationEngine> items = [];

    public void AddTransformer(ICalculationEngine transformer)
    {
        items.Add(transformer);
    }

    public List<ICalculationEngine> GetTransformers()
    {
        return items;
    }
}