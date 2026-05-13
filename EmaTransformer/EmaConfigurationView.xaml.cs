using IoAssistant.PnP.Interfaces;
using Syncfusion.Maui.Inputs;
using SelectionChangedEventArgs = Syncfusion.Maui.Inputs.SelectionChangedEventArgs;
using ValueChangedEventArgs = Syncfusion.Maui.Gauges.ValueChangedEventArgs;

namespace IoAssistant.Transformers;

public partial class EmaConfigurationView : ContentView
{
    public EmaConfigurationView()
    {
        InitializeComponent();
    }

    // private void OnSensorSelectionChanged(object? sender, SelectionChangedEventArgs e)
    // {
    //     if (BindingContext is not ICalculationEngine calculationEngine)
    //     {
    //         throw new InvalidOperationException("BindingContext is not of type ICalculationEngine");
    //     }
    //
    //     if (e.AddedItems?.FirstOrDefault() is ISensor sensor)
    //     {
    //         calculationEngine.SensorId = sensor.Id;
    //     }
    // }

    private void InteractiveMarker_ValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (BindingContext is not ICalculationEngine calculationEngine)
        {
            throw new InvalidOperationException("BindingContext is not of type ICalculationEngine");
        }
        
        calculationEngine.Alpha = (decimal)e.Value;
    }

    private void OnSensorSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (BindingContext is not ICalculationEngine calculationEngineVm)
        {
            throw new InvalidOperationException("BindingContext is not of type ICalculationEngine");
        }
        
        if(e.AddedItems is null)
            return;

        var selectedSensor = e.AddedItems.First() as ISensor;
        
        if(selectedSensor is null)
            return;
        
        calculationEngineVm.SelectedSensorChanger(selectedSensor);
    }
}