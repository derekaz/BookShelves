namespace BookShelves.Shared.Services
{
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

}
