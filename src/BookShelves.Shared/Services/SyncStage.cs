namespace BookShelves.Shared.Services
{
    /// <summary>
    /// Defines the stages of the synchronization process
    /// </summary>
    public enum SyncStage
    {
        None = 0,
        Pending = 1,
        Started = 2,
        Pushing = 3,           // Step 1: Pushing local changes to server
        Pulling = 4,           // Step 2: Pulling remote changes
        Processing = 5,        // Step 3: Processing/merging changes
        Completed = 6,
        Failed = 7
    }
}
