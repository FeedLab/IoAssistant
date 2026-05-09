using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using NModbus;

namespace IoAssistant.Infrastructure.Services;

public enum CommunicationType
{
    ModbusTcp,
    ModbusRtu
}

public abstract partial class ModBusClient : ObservableObject
{
    public readonly Lock BusLock = new();

    public Guid Id { get; set; } = Guid.CreateVersion7();

    [ObservableProperty] private int readTimeout = 2000;
    [ObservableProperty] private CommunicationType communicationType = CommunicationType.ModbusTcp;
    
    private readonly ILogger<ModBusClient> logger = AppService.GetRequiredService<ILogger<ModBusClient>>();


    public abstract void Start();
    public abstract void Stop();
    public abstract ushort[] Read(byte slaveId, ushort startAddress, ushort numRegisters, ushort functionCode);
    
    protected ushort[] ReadModbusRegisters(byte slaveId, ushort startAddress, ushort numRegisters, ushort functionCode,
        IModbusMaster master)
    {
        ushort[] registerValues;

        switch (functionCode)
        {
            case 1:
                throw new NotImplementedException();
            //registerValues = master.ReadCoils(slaveId, startAddress, numRegisters);
            case 2:
                throw new NotImplementedException();
            // registerValues = master.ReadInputs(slaveId, startAddress, numRegisters);
            case 3:
                registerValues = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);
                break;
            case 4:
                registerValues = master.ReadInputRegisters(slaveId, startAddress, numRegisters);
                break;
            case 5:
                throw new NotImplementedException();
            default:
                logger.LogError("Unsupported ModBus function code {FunctionCode}", functionCode);
                throw new ArgumentException("Unsupported ModBus function code");
        }

        return registerValues;
    }
}