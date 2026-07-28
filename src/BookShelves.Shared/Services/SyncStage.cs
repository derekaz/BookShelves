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
}
