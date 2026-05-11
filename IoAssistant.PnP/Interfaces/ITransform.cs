namespace IoAssistant.PnP.Interfaces;

public interface ITransformer
{
    public Guid Id { get; }
    
    public int Inputs { get; }

    public string Name { get; }

    public string Description { get; }

    public ContentView Configuration { get; }
    
    public void InitializeAndRegister(IServiceCollection services);
    
    ICalculate CreateInstance(string transformerDbData);
}

public interface ICalculate
{
    public ITransformer Transformer { get; }
    public decimal CalculatedValue { get; }
}
