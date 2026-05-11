using System.Text.Json;

namespace IoAssistant.PnP;

public class CalculationBase
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "EMA";
    public string Description { get; set; } = "Exponential Moving Average";
    private Guid BelongToId { get; set; }
    
    public void FromJson(string json)
    {
        var data = JsonSerializer.Deserialize<CalculationBase>(json);
        if (data != null)
        {
            Id = data.Id;
            Name = data.Name;
            Description = data.Description;
            BelongToId = data.BelongToId;
        }
    }
}