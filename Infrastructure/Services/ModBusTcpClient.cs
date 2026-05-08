using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using NModbus;

namespace IoAssistant.Infrastructure.Services;

public partial class ModBusTcpClient : ModBusClient
{
    [ObservableProperty] private string host = "192.168.68.75";

    [ObservableProperty] private int port = 8899;

    private TcpClient? tcpClient;
    private readonly ILogger<ModBusTcpClient> logger = AppService.GetRequiredService<ILogger<ModBusTcpClient>>();

    public override void Start()
    {
        try
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(Host, Port);

            logger.LogInformation("ModBus TCP client connected to {Host}:{Port}", Host, Port);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error connecting ModBus TCP client to {Host}:{Port}", Host, Port);
            throw;
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

    public override ushort[] Read(byte slaveId, ushort startAddress, ushort numRegisters)
    {
        // if (tcpClient is null || !tcpClient.Connected)
        // {
        //     logger.LogWarning("ModBus TCP client is not connected. Attempting reconnect to {Host}:{Port}", Host, Port);
        //     Start();
        // }

        try
        {
            var factory = new ModbusFactory();
            var master = factory.CreateMaster(tcpClient);

            var registers = master.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

            if (registers is null)
            {
                logger.LogError("ModBus TCP read failed at register {Register} with {NumRegisters}", startAddress, numRegisters);
                return [];
            }

            logger.LogInformation("ModBus TCP read successful at register {Register} with {NumRegisters}", startAddress, numRegisters);

            return registers;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "ModBus TCP read failed at register {Register} with {NumRegisters}", startAddress, numRegisters);
            throw;
        }
    }
}
