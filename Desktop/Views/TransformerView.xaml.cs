using IoAssistant.Infrastructure.Services;
using IoAssistant.Infrastructure.ViewModels;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.DataGrid;

namespace IoAssistant.Device.Desktop.Views;

public partial class TransformerView : ContentView
{
    private readonly ILogger<TransformerView> logger;
    private readonly TransformerViewModel viewModel;

    public TransformerView()
    {
        logger = AppService.GetRequiredService<ILogger<TransformerView>>();
        viewModel = AppService.GetRequiredService<TransformerViewModel>();

        InitializeComponent();

        BindingContext = viewModel;
        
        if (viewModel.SelectedTransformer is null)
        {
            logger.LogWarning("Selected transformer is null, cannot display configuration");
            return;
        }
        
        TransformerPlaceholder.Content = viewModel.SelectedTransformer.Configuration;
    }

    private void OnTransformerSelected(object sender, DataGridSelectionChangedEventArgs e)
    {
        if (e.AddedRows?.FirstOrDefault() is ITransformer transformer)
        {
            logger.LogInformation("Selected transformer: {Transformer}", transformer.Name);
            
            viewModel.SelectedTransformer = transformer;
            TransformerPlaceholder.Content = transformer.Configuration;
        }
        else
        {
            logger.LogWarning("No transformer selected");
        }
    }
}
