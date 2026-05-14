using IoAssistant.PnP.Interfaces;
using ValueChangedEventArgs = Syncfusion.Maui.Gauges.ValueChangedEventArgs;

namespace IoAssistant.Transformers;

public partial class EmaConfigurationComponent : ContentView
{
    public EmaConfigurationComponent()
    {
        InitializeComponent();
    }

    private void InteractiveMarker_ValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (BindingContext is not ICalculationEngine calculationEngine)
        {
            throw new InvalidOperationException("BindingContext is not of type ICalculationEngine");
        }

        calculationEngine.Alpha = (decimal)e.Value;
    }
}
