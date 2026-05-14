using System.Text.Json;
using IoAssistant.Database.Models;
using IoAssistant.Database.Repositories;
using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Services;
using IoAssistant.PnP.Interfaces;

namespace IoAssistant.Database;

public class DatabaseSeeder(
    ProjectRepository projectRepository,
    DeviceRepository deviceRepository,
    SensorRepository sensorRepository,
    ModBusClientRepository modBusClientRepository,
    TransformerRepository transformerRepository)
{
    public async Task SeedAsync()
    {
        return ;
        await modBusClientRepository.InitializeAsync();
        await projectRepository.InitializeAsync();
        await deviceRepository.InitializeAsync();
        await sensorRepository.InitializeAsync();
        await transformerRepository.InitializeAsync();

        // Clean all rows in all 4 tables
        await sensorRepository.DeleteAllAsync();
        await deviceRepository.DeleteAllAsync();
        await projectRepository.DeleteAllAsync();
        await modBusClientRepository.DeleteAllAsync();
        await transformerRepository.DeleteAllAsync();

        var projects = await projectRepository.GetAllAsync();
        if (projects.Count > 0)
            return;

        var modBusTcpClient = new ModBusTcpClient("192.168.68.75", 8899);
        var modBusRtuClient = new ModBusRtuClient();

        await modBusClientRepository.AddAsync(modBusTcpClient);
        await modBusClientRepository.AddAsync(modBusRtuClient);

        var project = new ProjectEntity { Name = "Default Project", Description = "Auto-created on first run" };
        await projectRepository.AddAsync(project);

        ushort startVfd = 4096;
        ushort registersToRead = 10;
        var modbusDeviceVfd = new ModbusDevice(modBusTcpClient, "Vfd Fan System", 1, startVfd, registersToRead, 3, 3000,
            1000, "Vfd Fan System");
        await deviceRepository.AddAsync(modbusDeviceVfd, project.Id, modBusTcpClient.Id);
        await sensorRepository.AddAsync(
            new Sensor(modbusDeviceVfd, "Frequency", 1, new IoDirection(IoDirectionType.Input), "Hz", 2),
            modbusDeviceVfd.Id);
        await sensorRepository.AddAsync(
            new Sensor(modbusDeviceVfd, "Power Usage", 5, new IoDirection(IoDirectionType.Input), "Kw", 1),
            modbusDeviceVfd.Id);

        var modbusDeviceSensor23 = new ModbusDevice(modBusRtuClient, "Sensor 23", 23, 0, 6, 3, 1000, 1000, "");
        await deviceRepository.AddAsync(modbusDeviceSensor23, project.Id, modBusRtuClient.Id);
        await sensorRepository.AddAsync(
            new Sensor(modbusDeviceSensor23, "Temperature", 1, new IoDirection(IoDirectionType.Input), "C", 1),
            modbusDeviceSensor23.Id);
        await sensorRepository.AddAsync(
            new Sensor(modbusDeviceSensor23, "Humidity (RH)", 0, new IoDirection(IoDirectionType.Input), "%", 1),
            modbusDeviceSensor23.Id);
        await sensorRepository.AddAsync(
            new Sensor(modbusDeviceSensor23, "CO2", 5, new IoDirection(IoDirectionType.Input), "ppm", 0),
            modbusDeviceSensor23.Id);

        var modbusDeviceSensor52 = new ModbusDevice(modBusRtuClient, "Sensor 52", 52, 0, 6, 3, 1000, 1000, "");
        await deviceRepository.AddAsync(modbusDeviceSensor52, project.Id, modBusRtuClient.Id);
        await sensorRepository.AddAsync(
            new Sensor(modbusDeviceSensor52, "Temperature", 1, new IoDirection(IoDirectionType.Input), "C", 1),
            modbusDeviceSensor52.Id);
        var sensor52Humidity = new Sensor(modbusDeviceSensor52, "Humidity (RH)", 0,
            new IoDirection(IoDirectionType.Input), "%", 1);
        await sensorRepository.AddAsync(sensor52Humidity, modbusDeviceSensor52.Id);
        await sensorRepository.AddAsync(
            new Sensor(modbusDeviceSensor52, "CO2", 5, new IoDirection(IoDirectionType.Input), "ppm", 0),
            modbusDeviceSensor52.Id);

        var transformerEmaRecord = new TransformerRecord
        {
            Id = new Guid("8b2b7cfe-a09f-4e00-872c-9496aee89692"),
            Name = "EMA Transformer",
            Description = "Exponential Moving Average Transformer",
            BelongToId = new Guid("cb677e07-ef4d-4717-930a-420dac9ff961"),
            Data = JsonSerializer.Serialize(new
                { Alpha = 0.5m, Period = 5, IsEnabled = false, SensorId = sensor52Humidity.Id }),
            ProjectId = project.Id
        };

        await transformerRepository.AddAsync(transformerEmaRecord);
    }
}