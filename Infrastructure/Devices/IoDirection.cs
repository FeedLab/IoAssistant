using CommunityToolkit.Mvvm.ComponentModel;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;

namespace IoAssistant.Infrastructure.Devices;

public partial class IoDirection : ObservableObject, IIoDirection
{
    [ObservableProperty] private IoDirectionType direction;

    public IoDirection(IoDirectionType direction)
    {
        Direction = direction;
    }

    public string Name
    {
        get
        {
            return Direction switch
            {
                IoDirectionType.Input => "In",
                IoDirectionType.Output => "Out",
                IoDirectionType.InAndOut => "In/Out",
                _ => "Unknown"
            };
        }
    }

    public Color Color
    {
        get
        {
            return Direction switch
            {
                IoDirectionType.Input => Colors.DarkGreen,
                IoDirectionType.Output => Colors.DarkRed,
                IoDirectionType.InAndOut => Colors.DarkOrange,
                _ => Colors.Gray
            };
        }
    }
}