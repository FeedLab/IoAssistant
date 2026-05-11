namespace IoAssistant.PnP;

public interface ITransformer
{
    public int Inputs { get; }

    public string Name { get; }

    public string Description { get; }

    public ContentView Configuration { get; }
    
    public void InitializeAndRegister(IServiceCollection services);
}