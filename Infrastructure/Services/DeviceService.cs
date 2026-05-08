using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Messages;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.Services;

public class DeviceService
{
    readonly List<SensorDevice> items = new List<SensorDevice>();
    private readonly ILogger<DeviceService> logger = AppService.GetRequiredService<ILogger<DeviceService>>();

    public void AddDevice(SensorDevice device)
    {
        items.Add(device);
        logger.LogDebug("Device {Device} added", device.Name);
        
        logger.LogDebug("Sending {Message} for device {Device} (Id={DeviceId})", nameof(OnDeviceAddedMessage), device.Name, device.DeviceId);
        WeakReferenceMessenger.Default.Send(new OnDeviceAddedMessage(device));
        logger.LogDebug("{Message} sent successfully for device {Device}", nameof(OnDeviceAddedMessage), device.Name);
    }

   
    public List<SensorDevice> GetDevices()
    {
        return items.ToList();
    }
    
    public List<SensorDevice> GetDevices<T>() where T : ModBusClient
    {
        return items.Where(w => w.ModBusClient is T).ToList();
    }
}