using System.ComponentModel;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace IoAssistant.PnP.Interfaces;

public interface IModBusClient
{
    Guid Id { get; set; }
    Lock BusLock { get; set; }

    bool IsInitialized { get; set; }
    
    string Name { get; set; }

    int ReadTimeout { get; set; }

    CommunicationType CommunicationType { get; set; }

    void Start();
    void Stop();
    ushort[] Read(byte slaveId, ushort startAddress, ushort numRegisters, ushort functionCode);
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}