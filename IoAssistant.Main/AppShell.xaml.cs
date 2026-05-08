using IoAssistant.Infrastructure.Extensions;
using IoAssistant.Infrastructure.Services;

namespace IoAssistant.Main;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        var serviceProvider = AppService.Current ?? throw new InvalidOperationException();
        
        var pageName = serviceProvider.GetRequiredPage("MainPage", false, true);

        Items.Add(new ShellContent
        {
            Content = pageName,
            Route = "MainPage"
        });
    }
}