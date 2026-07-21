using System;

namespace BookShelves.Shared.Services
{
    /// <summary>
    /// Defines the stages of the synchronization process
    /// </summary>
    public enum SyncStage
    {
        None = 0,
        Started = 1,
        Pushing = 2,           // Step 1: Pushing local changes to server
        Pulling = 3,           // Step 2: Pulling remote changes
        Processing = 4,        // Step 3: Processing/merging changes
        Completed = 5,
        Failed = 6
    }

    public class SyncProgressEventArgs : EventArgs
    {
        public string? Message { get; set; }
        public int? Current { get; set; }
        public int? Total { get; set; }
        public string? Stage { get; set; }

        /// <summary>
        /// The current sync stage (enum-based)
        /// </summary>
        public SyncStage SyncStage { get; set; } = SyncStage.None;

        /// <summary>
        /// Overall step number in the sync process (e.g., 1, 2, 3)
        /// </summary>
        public int CurrentStep { get; set; }

        /// <summary>
        /// Total number of steps in the sync process
        /// </summary>
        public int TotalSteps { get; set; } = 3;

        /// <summary>
        /// Overall progress percentage (0-100)
        /// </summary>
        public int ProgressPercentage { get; set; }
    }

    public interface ISyncProgressService
    {
        event EventHandler<SyncProgressEventArgs>? ProgressChanged;
        void Report(SyncProgressEventArgs args);
        void ReportStage(SyncStage stage, string message, int? current = null, int? total = null);
    }

    public class SyncProgressService : ISyncProgressService
    {
        public event EventHandler<SyncProgressEventArgs>? ProgressChanged;

        private static readonly Dictionary<SyncStage, int> StageToStepMap = new()
        {
            { SyncStage.Started, 0 },
            { SyncStage.Pushing, 1 },
            { SyncStage.Pulling, 2 },
            { SyncStage.Processing, 3 },
            { SyncStage.Completed, 3 },
            { SyncStage.Failed, 0 }
        };

        public void Report(SyncProgressEventArgs args)
        {
            try
            {
                // Calculate step information if not already set
                if (args.SyncStage != SyncStage.None && args.CurrentStep == 0)
                {
                    args.CurrentStep = StageToStepMap.GetValueOrDefault(args.SyncStage, 0);
                }

                // Calculate overall progress percentage
                if (args.CurrentStep > 0 && args.TotalSteps > 0)
                {
                    args.ProgressPercentage = (int)((args.CurrentStep - 1 + 0.5) / args.TotalSteps * 100);

                    // If we have item-level progress, factor that in
                    if (args.Current.HasValue && args.Total.HasValue && args.Total > 0)
                    {
                        int itemProgress = (int)((double)args.Current / args.Total * (100.0 / args.TotalSteps));
                        args.ProgressPercentage = (int)((args.CurrentStep - 1) / (double)args.TotalSteps * 100) + itemProgress;
                    }
                }

                // Raise the event to subscribers. UI layers should marshal to the UI thread if needed.
                ProgressChanged?.Invoke(this, args);
            }
            catch
            {
                // best-effort only
            }
        }

        public void ReportStage(SyncStage stage, string message, int? current = null, int? total = null)
        {
            Report(new SyncProgressEventArgs
            {
                SyncStage = stage,
                Stage = stage.ToString(),
                Message = message,
                Current = current,
                Total = total,
                TotalSteps = 3
            });
        }
    }
}
