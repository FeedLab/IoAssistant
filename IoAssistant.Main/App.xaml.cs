using IoAssistant.Database;
using IoAssistant.Database.Repositories;
using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IoAssistant.Main;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        if (DeviceInfo.Platform == DevicePlatform.WinUI ||
            DeviceInfo.Platform == DevicePlatform.MacCatalyst)
        {
            var scale = 0.65;
            window.Width = 1920 * scale;
            window.Height = 1080 * scale;
        }

        return window;
    }

    private async Task InitializeSensorDevices()
    {
        // try
        // {
        //     var deviceService = AppService.GetRequiredService<DeviceService>();
        //     var modBusClientService = AppService.GetRequiredService<ModBusClientService>();
        //
        //     byte registerStart = 0;
        //     byte registersToRead = 12;
        //     byte deviceId23 = 23;
        //     byte deviceId52 = 52;
        //         
        //     modBusRtuClient = new ModBusRtuClient();
        //     modBusClientService.AddDevice(modBusRtuClient);
        //
        //     modBusTcpClient = new ModBusTcpClient();
        //     modBusClientService.AddDevice(modBusTcpClient);
        //
        //     ushort startVfd = 4096;
        //     var sensorDeviceVfd = new ModbusDevice(modBusTcpClient, "Vfd Fan System", 1, startVfd, registersToRead,
        //     var sensorDeviceVfd = new Sensor(modBusTcpClient, "Vfd Fan System", 1, startVfd, registersToRead,
        //         3000, 1000, "Temperature & Humidity & CO2"));
        //     deviceService.AddDevice(sensorDeviceVfd);
        //
        //     var frequencyRegister = (ushort)(startVfd + 1);
        //     var sensorFrequencyHz = new ModbusDevice(sensorDeviceVfd, new IoDirection(IoDirectionType.Input), frequencyRegister)
        //     {
        //         Name = "Frequency",
        //         SensorType = "Frequency",
        //         NumberOfDecimals = 2,
        //         Unit = "Hz"
        //     };
        //     sensorDeviceVfd.AddSensor(sensorFrequencyHz);
        //     
        //     var powerUsageKwRegister = (ushort)(startVfd + 5);
        //     var sensorPowerUsageKw = new ModbusDevice(sensorDeviceVfd, new IoDirection(IoDirectionType.Input), powerUsageKwRegister)
        //     {
        //         Name = "Power Usage",
        //         SensorType = "Power",
        //         NumberOfDecimals = 1,
        //         Unit = "kW"
        //     };
        //     sensorDeviceVfd.AddSensor(sensorPowerUsageKw);
        //     
        //     var sensorDevice23 = new Sensor(modBusRtuClient, "In Office 23", deviceId23, registerStart,
        //         registersToRead,  1000, 1000,
        //         "Temperature & Humidity & CO2");
        //     deviceService.AddDevice(sensorDevice23);
        //
        //     var sensorHumidity = new ModbusDevice(sensorDevice23, new IoDirection(IoDirectionType.Input), 0)
        //     {
        //         Name = "Humidity",
        //         SensorType = "Humidity",
        //         NumberOfDecimals = 1,
        //         Unit = "RH"
        //     };
        //     sensorDevice23.AddSensor(sensorHumidity);
        //
        //     var sensorTemp = new ModbusDevice(sensorDevice23, new IoDirection(IoDirectionType.Input),1)
        //     {
        //         Name = "Temperature",
        //         SensorType = "Temperature",
        //         NumberOfDecimals = 1,
        //         Unit = "C"
        //     };
        //     sensorDevice23.AddSensor(sensorTemp);
        //
        //     var sensorCo2 = new ModbusDevice(sensorDevice23, new IoDirection(IoDirectionType.Input),5)
        //     {
        //         Name = "CO2",
        //         SensorType = "CO2",
        //         NumberOfDecimals = 0,
        //         Unit = "ppm"
        //     };
        //     sensorDevice23.AddSensor(sensorCo2);
        //
        //
        //     var sensorDevice52 = new Sensor(modBusRtuClient, "Outdoor 52", deviceId52, registerStart, 6,
        //          1000, 1000,
        //         "Temperature & Humidity & CO2");
        //     deviceService.AddDevice(sensorDevice52);
        //
        //     sensorHumidity = new ModbusDevice(sensorDevice52, new IoDirection(IoDirectionType.Input),0)
        //     {
        //         Name = "Humidity",
        //         SensorType = "Humidity",
        //         NumberOfDecimals = 1,
        //         Unit = "RH"
        //     };
        //     sensorDevice52.AddSensor(sensorHumidity);
        //
        //     sensorTemp = new ModbusDevice(sensorDevice52, new IoDirection(IoDirectionType.Input),1)
        //     {
        //         Name = "Temperature",
        //         SensorType = "Temperature",
        //         NumberOfDecimals = 1,
        //         Unit = "C"
        //     };
        //     sensorDevice52.AddSensor(sensorTemp);
        //
        //     sensorCo2 = new ModbusDevice(sensorDevice52, new IoDirection(IoDirectionType.Input),5)
        //     {
        //         Name = "CO2",
        //         SensorType = "CO2",
        //         NumberOfDecimals = 0,
        //         Unit = "ppm"
        //     };
        //     sensorDevice52.AddSensor(sensorCo2);
        // }
        // catch (Exception e)
        // {
        // }
    }

    protected override async void OnStart()
    {
        base.OnStart();
        
        var seeder = AppService.GetRequiredService<DatabaseSeeder>();
        var projectRepository = AppService.GetRequiredService<ProjectRepository>();
        var deviceRepository = AppService.GetRequiredService<DeviceRepository>();
        var sensorRepository = AppService.GetRequiredService<SensorRepository>();
        var modBusClientRepository = AppService.GetRequiredService<ModBusClientRepository>();
        var deviceService = AppService.GetRequiredService<DeviceService>();
        await seeder.SeedAsync();

        var clients = await modBusClientRepository.GetAllAsync();
        var clientsById = clients.ToDictionary(c => c.Id);
        foreach (var client in clients)
            client.Start();

        var projectEntities = await projectRepository.GetAllAsync();
        var project = projectEntities.First();

        var listOfDevices = await deviceRepository.GetByProjectAsync(project.Id, clientsById);
        
        foreach (var device in listOfDevices)
        {
            deviceService.AddDevice(device);
            
            var listOfSensors = await sensorRepository.GetByDeviceAsync(device);
        
            foreach (var sensor in listOfSensors)
            {
                device.AddSensor(sensor);
            }
            
            device.Start();
        }
    }
    
    // protected override async void OnStart()
    // {
    //     base.OnStart();
    //
    //     if (_initTask is not null)
    //         await _initTask;
    //
    //     await AppService.GetRequiredService<DeviceRepository>().InitializeAsync();
    //     await AppService.GetRequiredService<ProjectRepository>().InitializeAsync();
    //
    //     var seeder = AppService.GetRequiredService<DatabaseSeeder>();
    //     await seeder.SeedAsync();
    //
    //     await InitializeSensorDevices();
    //
    //     var deviceService = AppService.GetRequiredService<DeviceService>();
    //
    //     try
    //     {
    //         {
    //             modBusRtuClient.Start();
    //
    //             foreach (var device in deviceService.GetDevices<ModBusRtuClient>())
    //             {
    //                 device.Start();
    //             }
    //         }
    //         
    //         {
    //             modBusTcpClient.Start();
    //
    //             foreach (var device in deviceService.GetDevices<ModBusTcpClient>())
    //             {
    //                 device.Start();
    //             }
    //         }
    //
    //         // if (modBusClient is not null)
    //         // {
    //         //     modBusClient.Start();
    //         //     var datas = modBusClient.Read(23, 0, 6);
    //         //     modBusClient.Stop();
    //         // }
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         throw;
    //     }
    // }
}