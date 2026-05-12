namespace IoAssistant.PnP.Interfaces;

public interface ITransformer
{
    public Guid Id { get; }
    
    public int Inputs { get; }

    public string Name { get; }

    public string Description { get; }

    public ContentView Configuration { get; }
    
    public void Register(IServiceCollection services);
    public void Initialize();
    
    ICalculationEngine CreateInstance(Guid transformerInstanceId, Guid belongToId, Guid projectId, string name, string description, string data);
}

public interface ICalculationEngine
{
    public ITransformer Transformer { get; }
    public decimal CalculatedValue { get; }

    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid BelongToId { get; set; }
    public Guid ProjectId { get; set; }
    
    public void FromJson(string json);
    public string ToJson();

}
