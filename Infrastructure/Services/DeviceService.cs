using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;

namespace IoAssistant.Infrastructure.Services;

public class DeviceService
{
    readonly List<ModbusDevice> items = new List<ModbusDevice>();
    private readonly ILogger<DeviceService> logger = AppService.GetRequiredService<ILogger<DeviceService>>();

    public void AddDevice(ModbusDevice modbusDevice)
    {
        items.Add(modbusDevice);
        logger.LogDebug("Device {Device} added", modbusDevice.Name);
        
        logger.LogDebug("Sending {Message} for device {Device} (Id={DeviceId})", nameof(OnDeviceAddedMessage), modbusDevice.Name, modbusDevice.DeviceId);
        WeakReferenceMessenger.Default.Send(new OnDeviceAddedMessage(modbusDevice));
        logger.LogDebug("{Message} sent successfully for device {Device}", nameof(OnDeviceAddedMessage), modbusDevice.Name);
    }

   
    public List<ModbusDevice> GetDevices()
    {
        return items.ToList();
    }
    
    public List<ModbusDevice> GetDevices<T>() where T : ModBusClient
    {
        return items.Where(w => w.ModBusClient is T).ToList();
    }
    
    public List<ISensor> GetAllSensors()
    {
        return items.SelectMany(m => m.Sensors).ToList();
    }
}