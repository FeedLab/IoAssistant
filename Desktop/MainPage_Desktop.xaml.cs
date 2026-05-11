using IoAssistant.Infrastructure.Services;
using IoAssistant.Infrastructure.ViewModels;
using Syncfusion.Maui.TabView;

namespace IoAssistant.Device.Desktop;

public partial class MainPage_Desktop : ContentPage
{
    private readonly MainPageViewModel viewModel;
    
    public MainPage_Desktop()
    {
        viewModel = AppService.GetRequiredService<MainPageViewModel>();
        
        InitializeComponent();
        
        BindingContext = viewModel;
    }

    private void TabView_OnSelectionChanged(object? sender, TabSelectionChangedEventArgs e)
    {
    }
}