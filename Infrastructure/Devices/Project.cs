namespace IoAssistant.Infrastructure.Devices;

public interface IProject
{
}

public class Project(Guid projectId, string projectName, string projectDescription) : IProject
{
    
}