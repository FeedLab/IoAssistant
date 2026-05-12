using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using IoAssistant.Infrastructure.Messages;
using IoAssistant.Infrastructure.Services;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;

namespace IoAssistant.Infrastructure.ViewModels;

public class TransformerViewModel
{
    public ObservableCollection<ICalculationEngine> CalculationEngines { get; set; } = new();
    public ITransformer? SelectedTransformer { get; set; }

    public TransformerViewModel(IReadOnlyList<ITransformer> transformers, TransformerService transformerService)
    {
        WeakReferenceMessenger.Default.Register<IOnProjectLoadedMessage>(this, (recipient, m) =>
        {
            foreach (var transformer in transformerService.GetTransformers())
            {
                CalculationEngines.Add(transformer);
            }
        });
        

    }
}