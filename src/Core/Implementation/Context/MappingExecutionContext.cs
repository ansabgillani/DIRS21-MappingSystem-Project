using MappingSystem.Core;

namespace MappingSystem.Implementation;

public static class MappingExecutionContext
{
    private static readonly AsyncLocal<IMapHandler?> CurrentHandlerSlot = new();

    public static IMapHandler CurrentHandler
    {
        get => CurrentHandlerSlot.Value ?? throw new InvalidOperationException(
            "MapHandler not initialized. Ensure MapHandler is constructed before mapping.");
        set => CurrentHandlerSlot.Value = value;
    }

    public static bool TryGetCurrentHandler(out IMapHandler? currentHandler)
    {
        currentHandler = CurrentHandlerSlot.Value;
        return currentHandler != null;
    }

    public static void SetCurrentHandler(IMapHandler? currentHandler)
    {
        CurrentHandlerSlot.Value = currentHandler;
    }
}
