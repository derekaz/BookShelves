using BookShelves.Shared.Services;

namespace BookShelves.Shared.Tests.Services;

public sealed class SyncProgressServiceTests
{
    private readonly SyncProgressService _sut = new();

    // ── Event raising ─────────────────────────────────────────────────────────

    [Fact]
    public void Report_RaisesProgressChangedEvent()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.Report(new SyncProgressEventArgs { SyncStage = SyncStage.None });

        Assert.NotNull(received);
    }

    [Fact]
    public void Report_PassesSameArgsToEvent()
    {
        var sent = new SyncProgressEventArgs { Message = "hello", SyncStage = SyncStage.None };
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.Report(sent);

        Assert.Same(sent, received);
    }

    // ── StageToStepMap mapping ────────────────────────────────────────────────

    [Theory]
    [InlineData(SyncStage.Pending, 0)]
    [InlineData(SyncStage.Started, 0)]
    [InlineData(SyncStage.Pushing, 1)]
    [InlineData(SyncStage.Pulling, 2)]
    [InlineData(SyncStage.Processing, 3)]
    [InlineData(SyncStage.Completed, 3)]
    [InlineData(SyncStage.Failed, 0)]
    public void Report_MapsStageToCorrectStep_WhenCurrentStepIsZero(SyncStage stage, int expectedStep)
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.Report(new SyncProgressEventArgs { SyncStage = stage });

        Assert.NotNull(received);
        Assert.Equal(expectedStep, received.CurrentStep);
    }

    [Fact]
    public void Report_DoesNotOverrideCurrentStep_WhenAlreadySet()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.Report(new SyncProgressEventArgs { SyncStage = SyncStage.Pushing, CurrentStep = 5 });

        Assert.Equal(5, received!.CurrentStep);
    }

    [Fact]
    public void Report_DoesNotSetStep_WhenStageIsNone()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.Report(new SyncProgressEventArgs { SyncStage = SyncStage.None });

        Assert.Equal(0, received!.CurrentStep);
    }

    // ── Progress percentage calculation ───────────────────────────────────────

    [Fact]
    public void Report_SetsProgressPercentage_WhenStepAndTotalStepsArePositive()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.None,
            CurrentStep = 1,
            TotalSteps = 4
        });

        // step 1 of 4: (0 + 0.5) / 4 * 100 = 12
        Assert.Equal(12, received!.ProgressPercentage);
    }

    [Fact]
    public void Report_DoesNotSetProgressPercentage_WhenStepIsZero()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.None,
            CurrentStep = 0,
            TotalSteps = 3
        });

        Assert.Equal(0, received!.ProgressPercentage);
    }

    [Fact]
    public void Report_IncorporatesItemLevelProgress_WhenCurrentAndTotalProvided()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        // Step 2 of 4, halfway through item list (5 of 10)
        _sut.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.None,
            CurrentStep = 2,
            TotalSteps = 4,
            Current = 5,
            Total = 10
        });

        // base: (1/4)*100 = 25; item: (5/10)*(100/4) = 12; total = 37
        Assert.Equal(37, received!.ProgressPercentage);
    }

    [Fact]
    public void Report_DoesNotIncorporateItemProgress_WhenTotalIsZero()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.None,
            CurrentStep = 2,
            TotalSteps = 4,
            Current = 5,
            Total = 0
        });

        // Falls back to stage-only percentage: (1 + 0.5) / 4 * 100 = 37
        Assert.Equal(37, received!.ProgressPercentage);
    }

    // ── ReportStage ───────────────────────────────────────────────────────────

    [Fact]
    public void ReportStage_RaisesProgressChangedEvent()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.ReportStage(SyncStage.Pushing, "Pushing changes");

        Assert.NotNull(received);
    }

    [Fact]
    public void ReportStage_SetsMessageAndStage()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.ReportStage(SyncStage.Pulling, "Pulling changes");

        Assert.Equal("Pulling changes", received!.Message);
        Assert.Equal(SyncStage.Pulling, received.SyncStage);
    }

    [Fact]
    public void ReportStage_ForwardsCurrent_WhenProvided()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.ReportStage(SyncStage.Processing, "Processing", current: 3, total: 10);

        Assert.Equal(3, received!.Current);
        Assert.Equal(10, received.Total);
    }

    [Fact]
    public void ReportStage_SetsTotalStepsToThree()
    {
        SyncProgressEventArgs? received = null;
        _sut.ProgressChanged += (_, args) => received = args;

        _sut.ReportStage(SyncStage.Pushing, "msg");

        Assert.Equal(3, received!.TotalSteps);
    }

    // ── No subscribers ────────────────────────────────────────────────────────

    [Fact]
    public void Report_DoesNotThrow_WhenNoSubscribers()
    {
        var exception = Record.Exception(() =>
            _sut.Report(new SyncProgressEventArgs { SyncStage = SyncStage.Pulling }));

        Assert.Null(exception);
    }
}
