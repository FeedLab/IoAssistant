using IoAssistant.Infrastructure.Devices;
using SQLite;
using Sensor = IoAssistant.Database.Models.SensorEntity;

namespace IoAssistant.Database.Repositories;

public class SensorRepository
{
    private readonly SQLiteAsyncConnection db;

    public SensorRepository(string dbPath)
    {
        db = new SQLiteAsyncConnection(dbPath);
    }

    public async Task InitializeAsync()
    {
        await db.CreateTableAsync<Sensor>().ConfigureAwait(false);
    }

    public Task AddAsync(Infrastructure.Devices.Sensor sensor, Guid deviceId) =>
        db.InsertAsync(ToEntity(sensor, deviceId));

    public async Task DeleteAsync(Infrastructure.Devices.Sensor sensor)
    {
        var entity = await db.Table<Sensor>()
            .Where(s => s.Id == sensor.Id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (entity is not null)
            await db.DeleteAsync(entity).ConfigureAwait(false);
    }

    public Task DeleteByIdAsync(Guid id) =>
        db.DeleteAsync<Sensor>(id);

    public Task DeleteAllAsync() =>
        db.DeleteAllAsync<Sensor>();

    public Task UpsertAsync(Infrastructure.Devices.Sensor sensor, Guid deviceId) =>
        db.InsertOrReplaceAsync(ToEntity(sensor, deviceId));

    public async Task<List<Infrastructure.Devices.Sensor>> GetByDeviceAsync(ModbusDevice device)
    {
        var entities = await db.Table<Sensor>()
            .Where(s => s.DeviceId == device.Id)
            .ToListAsync()
            .ConfigureAwait(false);

        return entities.Select(e => new Infrastructure.Devices.Sensor(
            device,
            e.Name,
            e.NumRegister,
            new IoDirection(e.Direction),
            e.Unit,
            e.NumberOfDecimals
        ) { Id = e.Id }).ToList();
    }
    
    public async Task<List<Infrastructure.Devices.Sensor>> GetAllAsync(ModbusDevice device)
    {
        var entities = await db.Table<Sensor>()
            .ToListAsync()
            .ConfigureAwait(false);

        return entities.Select(e => new Infrastructure.Devices.Sensor(
            device,
            e.Name,
            e.NumRegister,
            new IoDirection(e.Direction),
            e.Unit,
            e.NumberOfDecimals
        ) { Id = e.Id }).ToList();
    }

    private static Sensor ToEntity(Infrastructure.Devices.Sensor sensor, Guid deviceId) => new()
    {
        Id = sensor.Id,
        DeviceId = deviceId,
        Name = sensor.Name,
        Unit = sensor.Unit,
        NumRegister = sensor.NumRegister,
        Direction = sensor.Direction.Direction,
        NumberOfDecimals = sensor.NumberOfDecimals,
    };
}
