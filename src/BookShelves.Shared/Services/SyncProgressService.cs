using System;

namespace BookShelves.Shared.Services
{
    public class SyncProgressEventArgs : EventArgs
    {
        public string? Message { get; set; }
        public int? Current { get; set; }
        public int? Total { get; set; }
        public string? Stage { get; set; }
    }

    public interface ISyncProgressService
    {
        event EventHandler<SyncProgressEventArgs>? ProgressChanged;
        void Report(SyncProgressEventArgs args);
    }

    public class SyncProgressService : ISyncProgressService
    {
        public event EventHandler<SyncProgressEventArgs>? ProgressChanged;

        public void Report(SyncProgressEventArgs args)
        {
            try
            {
                // Raise the event to subscribers. UI layers should marshal to the UI thread if needed.
                ProgressChanged?.Invoke(this, args);
            }
            catch
            {
                // best-effort only
            }
        }
    }
}
