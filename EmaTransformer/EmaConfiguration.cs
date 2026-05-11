using IoAssistant.PnP;

namespace IoAssistant.Transformers;

// All the code in this file is included in all platforms.
public class EmaConfiguration : ITransformer
{
    public EmaConfiguration()
    {
        Configuration = new ContentView();
    }

    public int Inputs { get; } = 1;
    public string Name { get; } = "EMA";
    public string Description { get; } = "Exponential Moving Average";
    public ContentView Configuration { get; }
    public void InitializeAndRegister(IServiceCollection services)
    {
        
    }
}