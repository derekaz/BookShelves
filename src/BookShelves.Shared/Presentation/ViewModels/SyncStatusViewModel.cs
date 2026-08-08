using BookShelves.Shared.Services;

namespace BookShelves.Shared.Presentation.ViewModels;

public sealed class SyncStatusViewModel
{
    public bool ShowWhenIdle { get; set; }

    public bool ShowProgress { get; set; } = true;

    public SyncStage CurrentStage { get; set; } = SyncStage.None;

    public string? Message { get; set; }

    public int ProgressPercentage { get; set; }

    public int CurrentStep { get; set; }

    public int TotalSteps { get; set; } = 3;
}
