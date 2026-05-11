using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using IoAssistant.PnP;
using Microsoft.Extensions.Logging;
using NModbus;

namespace IoAssistant.Infrastructure.Services;

public partial class ModBusTcpClient : ModBusClient
{
    [ObservableProperty] private string host;

    [ObservableProperty] private ushort port;

    private TcpClient? tcpClient;
    private readonly ILogger<ModBusTcpClient> logger = AppService.GetRequiredService<ILogger<ModBusTcpClient>>();

    public ModBusTcpClient(string host, ushort port)
    {
        CommunicationType = CommunicationType.ModbusTcp;
        
        Host = host;
        Port = port;
    }

    public override void Start()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(Host, Port);

            logger.LogInformation("ModBus TCP client connected to {Host}:{Port}", Host, Port);
            
            IsInitialized = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error connecting ModBus TCP client to {Host}:{Port}", Host, Port);
            
            IsInitialized = false;
        }
    }

    public override void Stop()
    {
        if (tcpClient is not null)
        {
            tcpClient.Close();
            tcpClient.Dispose();
            tcpClient = null;

            logger.LogInformation("ModBus TCP client disconnected from {Host}:{Port}", Host, Port);
        }
        else
        {
            logger.LogWarning("Attempted to close ModBus TCP client when it was not connected");
        }
    }

    public override ushort[] Read(byte slaveId, ushort startAddress, ushort numRegisters, ushort functionCode)
    {
        try
        {
            if (tcpClient is null || !tcpClient.Connected || !IsInitialized)
            {
                logger.LogWarning("ModBus TCP client is not Connected or not proper initialized");
                return [];
            }
            
            var factory = new ModbusFactory();
            var master = factory.CreateMaster(tcpClient);
            var registerValues = ReadModbusRegisters(slaveId, startAddress, numRegisters, functionCode, master);

            logger.LogInformation("ModBus TCP read successful at register {Register} with {NumRegisters}", startAddress, numRegisters);

            return registerValues;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ModBus TCP read failed at register {Register} with {NumRegisters}", startAddress, numRegisters);
            throw;
        }
    }


}
