using System.Linq;
using System.Reflection;
using CommunityToolkit.Maui;
using IoAssistant.Database;
using IoAssistant.Database.Repositories;
using IoAssistant.Device.Desktop;
using IoAssistant.Device.Phone;
using IoAssistant.Infrastructure.Extensions;
using IoAssistant.Infrastructure.Services;
using IoAssistant.Infrastructure.ViewModels;
using IoAssistant.Main.Services;
using IoAssistant.PnP;
using IoAssistant.Transformers;
using Microsoft.Extensions.Logging;
using Serilog;
using Syncfusion.Maui.Core.Hosting;

namespace IoAssistant.Main;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWdCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWXxccXZVQ2FeVEV/XUdWYEo=");
        
        var builder = MauiApp.CreateBuilder();
        
        // Configure Serilog
        var memorySink = new MemoryLogSink();
        var logPath = Path.Combine(FileSystem.AppDataDirectory, "logs", "IoAssistant.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .WriteTo.Sink(memorySink)
            .CreateLogger();
        
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Logging.AddSerilog(dispose: true);
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        
        builder.Services.AddSingleton<Storage>();
        builder.Services.AddSingleton<DeviceService>();
        builder.Services.AddSingleton<ModBusClientService>();
        
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "ioassistant.db");
        builder.Services.AddSingleton(_ => new DeviceRepository(dbPath));
        builder.Services.AddSingleton(_ => new ProjectRepository(dbPath));
        builder.Services.AddSingleton(_ => new SensorRepository(dbPath));
        builder.Services.AddSingleton(_ => new ModBusClientRepository(dbPath));
        builder.Services.AddTransient<DatabaseSeeder>();

        builder.Services.AddSingleton<DeviceViewModel>();
        
        RegisterPages(builder.Services);
        
        RegisterTransformers(builder);


#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        return app;
    }

    private static void RegisterTransformers(MauiAppBuilder builder)
    {
        // Load all transformer assemblies from the base directory
        foreach (var dll in Directory.GetFiles(AppContext.BaseDirectory, "*Transformer*.dll"))
            Assembly.LoadFrom(dll);

        // Find all implementations of ITransformer and call Register method
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var transformerType = typeof(ITransformer);
        
        var transformerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => transformerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        var transformers = new List<ITransformer>();
        foreach (var type in transformerTypes)
        {
            if (Activator.CreateInstance(type) is not ITransformer instance)
                throw new InvalidOperationException($"{type.Name} does not implement ITransformer");

            instance.InitializeAndRegister(builder.Services);
            transformers.Add(instance);
        }
        
        builder.Services.AddSingleton<IReadOnlyList<ITransformer>>(transformers);
    }

    private static void RegisterPages(IServiceCollection serviceCollection)
    {
        var currentIdiom = DeviceInfo.Current.Idiom;
        serviceCollection.AddSingleton<MainPageViewModel>();

        if (currentIdiom.Equals(DeviceIdiom.Tablet) || currentIdiom.Equals(DeviceIdiom.Desktop))
        {
            serviceCollection.RegisterPageNameSingleton<MainPage_Desktop>("MainPage", false, true);
            // serviceCollection.RegisterContentViewNameTransient<MainPage_Tablet>("MainPage", false, true);
            // serviceCollection.RegisterContentViewNameTransient<HomePage_Tablet>("HomePage", false, true);
            // serviceCollection.RegisterContentViewNameTransient<Baskets_Tablet>("Baskets", false, true);
            // serviceCollection.RegisterContentViewNameTransient<UserExchangesPage_Tablet>("UserExchange", false,
            //    true);
        }
        else if (currentIdiom.Equals(DeviceIdiom.Phone))
        {
            serviceCollection.RegisterPageNameSingleton<MainPage_Phone>("MainPage", false, true);
            // serviceCollection.RegisterPageNameScoped<MainPage_Phone>("MainPage", false, true);
            // serviceCollection.RegisterPageNameScoped<Authentication_Phone>("AuthenticationPopup", false, true);
        }
        else
        {
            //  serviceCollection.RegisterPageNameScoped<MainPage_NotSupported>("MainPage_NotSupported");
        }
    }
}