using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Shared.Services
{

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
