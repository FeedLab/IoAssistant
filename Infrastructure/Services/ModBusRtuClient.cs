using System.IO.Ports;
using CommunityToolkit.Mvvm.ComponentModel;
using IoAssistant.PnP;
using Microsoft.Extensions.Logging;
using NModbus;
using NModbus.Serial;

namespace IoAssistant.Infrastructure.Services;

public partial class ModBusRtuClient : ModBusClient
{
    [ObservableProperty] private string portName = "COM4";

    [ObservableProperty] private int baudRate = 9600;

    [ObservableProperty] private int dataBits = 8;

    [ObservableProperty] private Parity parity = Parity.None;

    [ObservableProperty] private StopBits stopBits = StopBits.One;


    private SerialPort? serialPort;
    private readonly ILogger<ModBusRtuClient> logger = AppService.GetRequiredService<ILogger<ModBusRtuClient>>();

    public ModBusRtuClient(string name) : base()   
    {
        CommunicationType = CommunicationType.ModbusRtu;
        Name = name;
    }

    public override void Start()
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
            
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting ModBus RTU client on {PortName}", PortName);
            
            IsInitialized = false;
        }
    }


    public override void Stop()
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

    public override ushort[] Read(byte slaveId, ushort startAddress, ushort numRegisters, ushort functionCode)
    {
        try
        {
            if (serialPort is null || !serialPort.IsOpen || !IsInitialized)
            {
                logger.LogWarning("ModBus RTU client is not open or not proper initialized");
                return [];
            }
            
            var factory = new ModbusFactory();
            // serialPort = new SerialPort(PortName);
            var master = factory.CreateRtuMaster(serialPort);

            var registerValues = ReadModbusRegisters(slaveId, startAddress, numRegisters, functionCode, master);

            logger.LogInformation("ModBus read successful at register {Register} with {NumRegisters}", startAddress,
                numRegisters);

            return registerValues;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ModBus read failed at register {Register} with {NumRegisters}", startAddress,
                numRegisters);
            
            throw;
        }
    }
}