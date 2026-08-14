using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Shared.Services;

namespace BookShelves.Shared.Tests.Presentation.ViewModels;

public sealed class ViewModelDefaultsTests
{
    [Fact]
    public void AuthorViewModel_Defaults_AreInitialized()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-2);
        var sut = new AuthorViewModel();
        var after = DateTimeOffset.UtcNow.AddSeconds(2);

        Assert.Equal("**UNIQUEID**", AuthorViewModel.AUTHORITEM_UNIQUEID_RECORD_ID);
        Assert.Equal(string.Empty, sut.Id);
        Assert.Equal(string.Empty, sut.Name);
        Assert.Equal(string.Empty, sut.Biography);
        Assert.NotNull(sut.LastUpdateTime);
        Assert.InRange(sut.LastUpdateTime!.Value, before, after);
        Assert.Null(sut.Version);
    }

    [Fact]
    public void BookViewModel_Defaults_AreInitialized()
    {
        var before = DateTimeOffset.UtcNow.AddSeconds(-2);
        var sut = new BookViewModel();
        var after = DateTimeOffset.UtcNow.AddSeconds(2);

        Assert.Equal("**UNIQUEID**", BookViewModel.AUTHORITEM_UNIQUEID_RECORD_ID);
        Assert.Equal(string.Empty, sut.Id);
        Assert.Equal(string.Empty, sut.Title);
        Assert.Null(sut.Author);
        Assert.Null(sut.Description);
        Assert.Null(sut.PublishDate);
        Assert.NotNull(sut.LastUpdateTime);
        Assert.InRange(sut.LastUpdateTime!.Value, before, after);
        Assert.Null(sut.Version);
    }

    [Fact]
    public void SyncStatusViewModel_Defaults_AreInitialized()
    {
        var sut = new SyncStatusViewModel();

        Assert.False(sut.ShowWhenIdle);
        Assert.True(sut.ShowProgress);
        Assert.Equal(SyncStage.None, sut.CurrentStage);
        Assert.Null(sut.Message);
        Assert.Equal(0, sut.ProgressPercentage);
        Assert.Equal(0, sut.CurrentStep);
        Assert.Equal(3, sut.TotalSteps);
    }
}
