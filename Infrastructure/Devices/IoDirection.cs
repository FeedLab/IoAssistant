using CommunityToolkit.Mvvm.ComponentModel;

namespace IoAssistant.Infrastructure.Devices;

public enum IoDirectionType
{
    Input,
    Output,
    InAndOut
}

public partial class IoDirection : ObservableObject
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