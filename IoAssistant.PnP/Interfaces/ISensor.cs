using System.ComponentModel;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace IoAssistant.PnP.Interfaces;

public interface ISensor
{
    Guid Id { get; set; }

    IModbusDevice ModbusDevice { get; set; }

    string Name { get; set; }
    string FullName { get; set; }

    string Unit { get; set; }

    ushort NumRegister { get; set; }

    decimal Value { get; set; }

    IIoDirection Direction { get; set; }

    int NumberOfDecimals { get; set; }

    void ProcessSensorReading(ushort[] sensorReadings);
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}