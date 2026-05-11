using System.Collections.ObjectModel;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;

namespace IoAssistant.Infrastructure.ViewModels;

public class TransformerViewModel
{
    public ObservableCollection<ITransformer> Transformers { get; set; } = new();
    public ITransformer? SelectedTransformer { get; set; }

    public TransformerViewModel(IReadOnlyList<ITransformer> transformers)
    {
        foreach (var transformer in transformers)
        {
            Transformers.Add(transformer);
        }
    }
}