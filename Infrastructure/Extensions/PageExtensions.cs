namespace IoAssistant.Infrastructure.Extensions;

public static class PageExtensions
{
    public static IServiceCollection RegisterPageNameScoped<T>(this IServiceCollection serviceCollection,
        string pageName) where T : Page
    {
        serviceCollection.AddKeyedScoped<Page, T>(pageName);

        return serviceCollection;
    }

    public static IServiceCollection RegisterPageNameScoped<T>(this IServiceCollection serviceCollection,
        string pageName,
        DevicePlatform platform) where T : Page
    {
        var fullPageName = $"{pageName}_{platform}";

        serviceCollection.AddKeyedScoped<Page, T>(fullPageName);

        return serviceCollection;
    }

    public static IServiceCollection RegisterPageNameScoped<T>(this IServiceCollection serviceCollection,
        string pageName,
        DeviceIdiom idiom) where T : Page
    {
        var fullPageName = $"{pageName}_{idiom}";

        serviceCollection.AddKeyedScoped<Page, T>(fullPageName);

        return serviceCollection;
    }

    public static IServiceCollection RegisterPageNameScoped<T>(this IServiceCollection serviceCollection,
        string pageName,
        DevicePlatform platform, DeviceIdiom idiom) where T : Page
    {
        var fullPageName = $"{pageName}_{platform}_{idiom}";

        serviceCollection.AddKeyedScoped<Page, T>(fullPageName);

        return serviceCollection;
    }

    public static IServiceCollection RegisterPageNameSingleton<T>(this IServiceCollection serviceCollection,
        string pageName, bool includePlatform, bool includeIdiom) where T : Page
    {
        var fullPageName = GetPageName(pageName, includePlatform, includeIdiom);

        serviceCollection.AddKeyedSingleton<Page, T>(fullPageName);

        return serviceCollection;
    }
    
    public static IServiceCollection RegisterPageNameScoped<T>(this IServiceCollection serviceCollection,
        string pageName, bool includePlatform, bool includeIdiom) where T : Page
    {
        var fullPageName = GetPageName(pageName, includePlatform, includeIdiom);

        serviceCollection.AddKeyedScoped<Page, T>(fullPageName);

        return serviceCollection;
    }
    
    public static IServiceCollection RegisterContentViewNameScoped<T>(this IServiceCollection serviceCollection,
        string pageName, bool includePlatform, bool includeIdiom) where T : ContentView
    {
        var fullPageName = GetPageName(pageName, includePlatform, includeIdiom);

        serviceCollection.AddKeyedScoped<ContentView, T>(fullPageName);

        return serviceCollection;
    }
    
    public static IServiceCollection RegisterContentViewNameTransient<T>(this IServiceCollection serviceCollection,
        string pageName, bool includePlatform, bool includeIdiom) where T : ContentView
    {
        var fullPageName = GetPageName(pageName, includePlatform, includeIdiom);

        serviceCollection.AddKeyedTransient<ContentView, T>(fullPageName);

        return serviceCollection;
    }
    
    public static IServiceCollection RegisterContentViewNameSingleton<T>(this IServiceCollection serviceCollection,
        string pageName, bool includePlatform, bool includeIdiom) where T : ContentView
    {
        var fullPageName = GetPageName(pageName, includePlatform, includeIdiom);

        serviceCollection.AddKeyedSingleton<ContentView, T>(fullPageName);

        return serviceCollection;
    }

    public static T? GetRequiredPage<T>(this IServiceProvider serviceProvider, string pageName,
        bool includePlatform = false, bool includeIdiom = false) where T : Page
    {
        var pageServiceByName = GetPageName(pageName, includePlatform, includeIdiom);

        var page = serviceProvider.GetKeyedService<Page>(pageServiceByName);

        return page as T ??
               throw new InvalidCastException(
                   $"Cannot cast page of type '{page?.GetType().Name}' to type '{typeof(T).Name}'");
    }

    public static Page GetRequiredPage(this IServiceProvider serviceProvider, string pageName,
        bool includePlatform = false, bool includeIdiom = false)
    {
        var pageServiceByName = GetPageName(pageName, includePlatform, includeIdiom);

        var page = serviceProvider.GetKeyedService<Page>(pageServiceByName);

        return page ??
               throw new InvalidOperationException($"Page '{pageServiceByName}' not found in service provider");
    }
    
    public static ContentView? GetRequiredView(this IServiceProvider serviceProvider, string pageName,
        bool includePlatform = false, bool includeIdiom = false)
    {
        var pageServiceByName = GetPageName(pageName, includePlatform, includeIdiom);

        var view = serviceProvider.GetKeyedService<ContentView>(pageServiceByName);

        return view;
    }

    private static string GetPageName(string pageName, bool includePlatform = false, bool includeIdiom = false)
    {
        if (!includePlatform && !includeIdiom)
        {
            var page = $"{pageName}";

            if (page is not null)
            {
                return page;
            }
        }
        else if (includePlatform && includeIdiom)
        {
            var currentPlatform = DeviceInfo.Current.Platform;
            var currentIdiom = DeviceInfo.Current.Idiom;

            var page = $"{pageName}_{currentPlatform}_{currentIdiom}";

            if (page is not null)
            {
                return page;
            }
        }
        else if (includePlatform)
        {
            var currentPlatform = DeviceInfo.Current.Platform;

            var page = $"{pageName}_{currentPlatform}";

            if (page is not null)
            {
                return page;
            }
        }
        else if (includeIdiom)
        {
            var currentIdiom = DeviceInfo.Current.Idiom;

            var page = $"{pageName}_{currentIdiom}";

            if (page is not null)
            {
                return page;
            }
        }

        throw new Exception("includeIdiom must be used with includePlatform");
    }
}