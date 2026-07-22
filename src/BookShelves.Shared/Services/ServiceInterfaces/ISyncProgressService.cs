namespace BookShelves.Shared.Services.ServiceInterfaces
{
    public interface ISyncProgressService
    {
        event EventHandler<SyncProgressEventArgs>? ProgressChanged;
        void Report(SyncProgressEventArgs args);
        void ReportStage(SyncStage stage, string message, int? current = null, int? total = null);
    }
}
