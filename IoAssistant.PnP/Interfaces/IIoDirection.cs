using System.ComponentModel;
using PropertyChangingEventHandler = System.ComponentModel.PropertyChangingEventHandler;

namespace IoAssistant.PnP.Interfaces;

public interface IIoDirection
{
    /// <inheritdoc cref="IoDirection.direction"/>
    IoDirectionType Direction { get; set; }

    string Name { get; }
    Color Color { get; }
    event PropertyChangedEventHandler? PropertyChanged;
    event PropertyChangingEventHandler? PropertyChanging;
}