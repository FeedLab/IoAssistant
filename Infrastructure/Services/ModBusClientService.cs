using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using NModbus;
using NModbus.Serial;

namespace IoAssistant.Infrastructure.Services;

public partial class ModBusRtuClient : ObservableObject
{
    [ObservableProperty] private string portName = "COM4";

    [ObservableProperty] private int baudRate = 9600;

    [ObservableProperty] private int dataBits = 8;

    [ObservableProperty] private Parity parity = Parity.None;

    [ObservableProperty] private StopBits stopBits = StopBits.One;

    [ObservableProperty] private int readTimeout = 2000;

    private readonly Lock modbusLock = new();
    private SerialPort? serialPort;
    private readonly ILogger<ModBusRtuClient> logger = AppService.GetRequiredService<ILogger<ModBusRtuClient>>();

    public void Start()
    {
        try
        {
            serialPort = new SerialPort(PortName)
            {
                BaudRate = BaudRate,
                DataBits = DataBits,
                Parity = Parity,
                StopBits = StopBits,
                ReadTimeout = ReadTimeout
            };

            serialPort.Open();

            logger.LogInformation(
                "ModBus RTU client started on {PortName} with BaudRate={BaudRate}, DataBits={DataBits}, Parity={Parity}, StopBits={StopBits}, ReadTimeout={ReadTimeout}",
                PortName, BaudRate, DataBits, Parity, StopBits, ReadTimeout
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting ModBus RTU client on {PortName}", PortName);
            throw;
        }
    }


    public void Stop()
    {
        if (serialPort is not null && serialPort.IsOpen)
        {
            var port = serialPort.PortName;

            serialPort.Close();
            serialPort.Dispose();
            serialPort = null;

            logger.LogInformation("ModBus RTU client on {PortName} closed successfully", port);
        }
        else
        {
            logger.LogWarning("Attempted to close ModBus RTU client when it was not open");
        }
    }

    // public Task<ushort[]> ReadAsync(byte deviceId, ushort startAddress, ushort numRegisters)
    // {
    //     return Task.Run(() => return Read(deviceId, startAddress, numRegisters));
    // }

    public ushort[] Read(byte slaveId, byte startAddress, byte numRegisters)
    {
        lock (modbusLock)
        {
            try
            {
                var factory = new ModbusFactory();
                var master = factory.CreateRtuMaster(new SerialPortAdapter(serialPort));

                var registers = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

                if (registers is null)
                {
                    logger.LogError("ModBus read failed at register {Register} with {NumRegisters}", startAddress,
                        numRegisters);
                    return [];
                }

                logger.LogInformation("ModBus read successful at register {Register} with {NumRegisters}", startAddress,
                    numRegisters);

                return registers;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ModBus read failed at register {Register} with {NumRegisters}", startAddress,
                    numRegisters);
            }
        }

        return [];
    }
}

public class ModBusClientService
{
    private readonly List<ModBusRtuClient> items = [];

    public void AddDevice(ModBusRtuClient client)
    {
        items.Add(client);
    }

    public List<ModBusRtuClient> GetDevices()
    {
        return items;
    }
}