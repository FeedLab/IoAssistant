using IoAssistant.Database.Models;
using IoAssistant.Infrastructure.Services;
using IoAssistant.PnP;
using SQLite;

namespace IoAssistant.Database.Repositories;

public class ModBusClientRepository
{
    private readonly SQLiteAsyncConnection db;

    public ModBusClientRepository(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await db.CreateTableAsync<ModBusClientEntity>().ConfigureAwait(false);
    }

    public Task AddAsync(ModBusClient client) =>
        db.InsertAsync(ToEntity(client));

    public Task DeleteByIdAsync(Guid id) =>
        db.DeleteAsync<ModBusClientEntity>(id);

    public Task DeleteAllAsync() =>
        db.DeleteAllAsync<ModBusClientEntity>();

    public Task UpsertAsync(ModBusClient client) =>
        db.InsertOrReplaceAsync(ToEntity(client));

    public async Task<List<ModBusClient>> GetAllAsync()
    {
        var entities = await db.Table<ModBusClientEntity>()
            .ToListAsync()
            .ConfigureAwait(false);

        return entities.Select(ToDomain).ToList();
    }

    private static ModBusClient ToDomain(ModBusClientEntity entity) =>
        entity.Type switch
        {
            CommunicationType.ModbusTcp => new ModBusTcpClient(entity.Host!, entity.Port, entity.Name) { Id = entity.Id },
            CommunicationType.ModbusRtu => new ModBusRtuClient(entity.Name)
            {
                PortName = entity.PortName!,
                BaudRate = entity.BaudRate,
                DataBits = entity.DataBits,
                Parity = entity.Parity,
                StopBits = entity.StopBits,
                Id = entity.Id,
                Name = entity.Name
            },
            _ => throw new ArgumentOutOfRangeException(nameof(entity.Type), entity.Type, "Unknown client type")
        };

    private static ModBusClientEntity ToEntity(ModBusClient client) =>
        client switch
        {
            ModBusTcpClient tcp => new ModBusClientEntity
            {
                Id = tcp.Id,
                Host = tcp.Host,
                Port = tcp.Port,
                Type = CommunicationType.ModbusTcp,
            },
            ModBusRtuClient rtu => new ModBusClientEntity
            {
                Id = rtu.Id,
                Name = rtu.Name,
                Type = CommunicationType.ModbusRtu,
                PortName = rtu.PortName,
                BaudRate = rtu.BaudRate,
                DataBits = rtu.DataBits,
                Parity = rtu.Parity,
                StopBits = rtu.StopBits,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(client), client.GetType().Name, "Unknown client type")
        };
}
