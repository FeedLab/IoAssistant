using System.ComponentModel;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace IoAssistant.PnP.Interfaces;

public interface IModbusDevice
{
    Guid Id { get; set; }

    IModBusClient ModBusClient { get; set; }

    string Status { get; set; }

    string Name { get; set; }
    
    string FullName { get; set; }

    byte DeviceId { get; set; }

    ushort RegistersToRead { get; set; }

    ushort StartRegister { get; set; }

    string Description { get; set; }

    int PollingFrequency { get; set; }

    int DelayedStart { get; set; }

    bool IsExpanded { get; set; }

    bool IsEnabled { get; set; }

    ushort FunctionCode { get; set; }

    void Start();
    void Stop();
    void Dispose();
    void AddSensor(ISensor sensor);
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}