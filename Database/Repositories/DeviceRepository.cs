using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Services;
using SQLite;
using Device = IoAssistant.Database.Models.DeviceEntity;

namespace IoAssistant.Database.Repositories;

public class DeviceRepository
{
    private readonly SQLiteAsyncConnection db;

    public DeviceRepository(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await db.CreateTableAsync<Device>().ConfigureAwait(false);
    }

    public Task AddAsync(ModbusDevice device, Guid projectId, Guid modBusClientId) =>
        db.InsertAsync(ToEntity(device, projectId, modBusClientId));

    public async Task DeleteAsync(ModbusDevice device)
    {
        var entity = await db.Table<Device>()
            .Where(d => d.Id == device.Id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (entity is not null)
            await db.DeleteAsync(entity).ConfigureAwait(false);
    }

    public Task DeleteByIdAsync(Guid id) =>
        db.DeleteAsync<Device>(id);

    public Task DeleteAllAsync() =>
        db.DeleteAllAsync<Device>();

    public Task UpsertAsync(ModbusDevice device, Guid projectId, Guid modBusClientId) =>
        db.InsertOrReplaceAsync(ToEntity(device, projectId, modBusClientId));

    public async Task<List<ModbusDevice>> GetByProjectAsync(Guid projectId, IReadOnlyDictionary<Guid, ModBusClient> clientsById)
    {
        var entities = await db.Table<Device>()
            .Where(d => d.ProjectId == projectId)
            .ToListAsync()
            .ConfigureAwait(false);

        return entities.Select(e => new ModbusDevice(
            clientsById[e.ModBusClientId],
            e.Name,
            e.DeviceId,
            e.StartRegister,
            e.RegistersToRead,
            e.FunctionCode,
            e.PollingFrequency,
            e.DelayedStart,
            e.Description
        ) { Id = e.Id }).ToList();
    }

    public async Task<List<ModbusDevice>> GetAllAsync(IReadOnlyDictionary<Guid, ModBusClient> clientsById)
    {
        var entities = await db.Table<Device>()
            .ToListAsync()
            .ConfigureAwait(false);

        return entities.Select(e => new ModbusDevice(
            clientsById[e.ModBusClientId],
            e.Name,
            e.DeviceId,
            e.StartRegister,
            e.RegistersToRead,
            e.FunctionCode,
            e.PollingFrequency,
            e.DelayedStart,
            e.Description
        ) { Id = e.Id }).ToList();
    }

    private static Device ToEntity(ModbusDevice device, Guid projectId, Guid modBusClientId) => new()
    {
        Id = device.Id,
        ProjectId = projectId,
        ModBusClientId = modBusClientId,
        Name = device.Name,
        Description = device.Description,
        DeviceId = device.DeviceId,
        StartRegister = device.StartRegister,
        RegistersToRead = device.RegistersToRead,
        FunctionCode = device.FunctionCode,
        PollingFrequency = device.PollingFrequency,
        DelayedStart = device.DelayedStart,
    };
}
