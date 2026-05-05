using IoAssistant.Infrastructure.Devices;
using IoAssistant.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IoAssistant.Main;

public partial class App : Application
{
    private ModBusRtuClient? modBusClient;
    private Task? _initTask;

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
        try
        {
            var deviceService = AppService.GetRequiredService<DeviceService>();
            var modBusClientService = AppService.GetRequiredService<ModBusClientService>();

            modBusClient = new ModBusRtuClient();
            byte registerStart = 0;
            byte registersToRead = 12;
            byte deviceId23 = 23;
            byte deviceId52 = 52;

            modBusClientService.AddDevice(modBusClient);

        
            var sensorDevice23 = new SensorDevice(modBusClient, "In Office 23", deviceId23,  registerStart, registersToRead, DeviceDirection.Input,
                "Temperature & Humidity & CO2");
            deviceService.AddDevice(sensorDevice23);

            var sensorHumidity = new Sensor(sensorDevice23, 0)
            {
                Name = "Humidity",
                SensorType = "Humidity",
                NumberOfDecimals = 1,
                Unit = "RH"
            };
            sensorDevice23.AddSensor(sensorHumidity);

            var sensorTemp = new Sensor(sensorDevice23, 1)
            {
                Name = "Temperature",
                SensorType = "Temperature",
                NumberOfDecimals = 1,
                Unit = "C"
            };
            sensorDevice23.AddSensor(sensorTemp);

            var sensorCo2 = new Sensor(sensorDevice23, 5)
            {
                Name = "CO2",
                SensorType = "CO2",
                NumberOfDecimals = 0,
                Unit = "ppm"
            };
            sensorDevice23.AddSensor(sensorCo2);

        
            var sensorDevice52 = new SensorDevice(modBusClient, "Outdoor 52", deviceId52,  registerStart, 6, DeviceDirection.Input,
                "Temperature & Humidity & CO2");
            deviceService.AddDevice(sensorDevice52);

            sensorHumidity = new Sensor(sensorDevice52, 0)
            {
                Name = "Humidity",
                SensorType = "Humidity",
                NumberOfDecimals = 1,
                Unit = "RH"
            };
            sensorDevice52.AddSensor(sensorHumidity);

            sensorTemp = new Sensor(sensorDevice52, 1)
            {
                Name = "Temperature",
                SensorType = "Temperature",
                NumberOfDecimals = 1,
                Unit = "C"
            };
            sensorDevice52.AddSensor(sensorTemp);

            sensorCo2 = new Sensor(sensorDevice52, 5)
            {
                Name = "CO2",
                SensorType = "CO2",
                NumberOfDecimals = 0,
                Unit = "ppm"
            };
            sensorDevice52.AddSensor(sensorCo2);
        }
        catch (Exception e)
        {
            
        }
    }

    protected override async void OnStart()
    {
        base.OnStart();

        if (_initTask is not null)
            await _initTask;

        await InitializeSensorDevices();

        var deviceService = AppService.GetRequiredService<DeviceService>();

        try
        {
            if (modBusClient is not null)
            {
                modBusClient.Start();

                foreach (var device in deviceService.GetDevices())
                {
                    device.Start();
                }

                //     modBusClient.Stop();
            }

            // if (modBusClient is not null)
            // {
            //     modBusClient.Start();
            //     var datas = modBusClient.Read(23, 0, 6);
            //     modBusClient.Stop();
            // }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}