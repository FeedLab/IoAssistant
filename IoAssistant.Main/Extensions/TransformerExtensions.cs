using System.Reflection;
using IoAssistant.PnP;
using IoAssistant.PnP.Interfaces;

namespace IoAssistant.Main.Extensions;

public static class TransformerExtensions
{
    public static MauiAppBuilder RegisterTransformers(this MauiAppBuilder builder)
    {
        foreach (var dll in Directory.GetFiles(AppContext.BaseDirectory, "*Transformer*.dll"))
            Assembly.LoadFrom(dll);

        var transformerType = typeof(ITransformer);
        var transformerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => transformerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        var transformers = new List<ITransformer>();
        foreach (var type in transformerTypes)
        {
            if (Activator.CreateInstance(type) is not ITransformer instance)
                throw new InvalidOperationException($"{type.Name} does not implement ITransformer");

            instance.InitializeAndRegister(builder.Services);
            transformers.Add(instance);
        }

        builder.Services.AddSingleton<IReadOnlyList<ITransformer>>(transformers);
        return builder;
    }
}
