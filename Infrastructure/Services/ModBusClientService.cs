namespace IoAssistant.Infrastructure.Services;

public class ModBusClientService
{
    private readonly List<ModBusClient> items = [];

    public void AddDevice(ModBusClient client)
    {
        items.Add(client);
    }

    public List<ModBusClient> GetDevices()
    {
        return items;
    }
}