namespace BookShelves.Shared.Services.ServiceInterfaces;

public interface IPageSyncCoordinator : IDisposable
{
    bool IsSupported { get; }

    Task QueueAutomaticSyncAsync(
        Func<Task> syncAction,
        Func<Task> refreshAction,
        Action<string>? setMessage = null,
        Func<Task>? refreshUi = null,
        bool isAutomaticTrigger = true);

    Task TriggerSyncAsync(
        Func<Task> syncAction,
        Func<Task> refreshAction,
        Action<string>? setMessage = null,
        Func<Task>? refreshUi = null,
        bool isAutomaticTrigger = true);
}
