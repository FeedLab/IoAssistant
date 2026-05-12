using IoAssistant.Infrastructure.Devices;

namespace IoAssistant.Infrastructure.Messages;

public interface IOnProjectLoadedMessage
{
    IProject Project { get; }
}

public class OnProjectLoadedMessage(IProject project) : IOnProjectLoadedMessage
{
    public IProject Project { get; } = project;
}