namespace IoAssistant.PnP.Interfaces;

public interface IOnSensorDataChangedMessage
{
    ISensor Sensor { get; }
    decimal RegisterValue { get; }
    decimal OldRegisterValue { get; }
    bool HasChanged { get; }
}