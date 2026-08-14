using BookShelves.Shared.Components.Bases;
using Moq;
using MudBlazor;

namespace BookShelves.Shared.Tests.Components;

public sealed class DetailDialogBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_InitializesModel_WhenModelNotProvided()
    {
        var sut = new TestDetailDialog();

        await sut.InvokeOnInitializedAsync();

        Assert.NotNull(sut.ModelObject);
        Assert.Equal("initialized-id", sut.ModelObject.Id);
        Assert.Equal(1, sut.InitializeModelCallCount);
        Assert.Equal(1, sut.OnFormInitializedCallCount);
    }

    [Fact]
    public async Task OnInitializedAsync_DoesNotReinitialize_WhenModelProvided()
    {
        var existing = new TestViewModel { Id = "existing", Name = "Existing" };
        var sut = new TestDetailDialog { ModelObject = existing };

        await sut.InvokeOnInitializedAsync();

        Assert.Same(existing, sut.ModelObject);
        Assert.Equal(0, sut.InitializeModelCallCount);
        Assert.Equal(1, sut.OnFormInitializedCallCount);
    }

    [Fact]
    public async Task SubmitForm_NewRecord_Success_ClosesDialogWithSuccessMessage()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDetailDialog
        {
            ModelObject = new TestViewModel { Id = string.Empty, Name = "New Name" },
            CreateResult = true
        };
        sut.SetDialog(dialog.Object);

        await sut.InvokeSubmitForm();

        Assert.Equal(1, sut.OnBeforeSubmitCallCount);
        Assert.Equal(1, sut.ExecuteCreateCallCount);
        Assert.Equal(0, sut.ExecuteUpdateCallCount);
        dialog.Verify(x => x.Close(It.Is<DialogResult?>(r =>
            r != null &&
            !r.Canceled &&
            string.Equals(r.Data == null ? null : r.Data.ToString(), "created:New Name", StringComparison.Ordinal))), Times.Once);
    }

    [Fact]
    public async Task SubmitForm_ExistingRecord_Success_ClosesDialogWithSuccessMessage()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDetailDialog
        {
            ModelObject = new TestViewModel { Id = "id-10", Name = "Existing Name" },
            UpdateResult = true
        };
        sut.SetDialog(dialog.Object);

        await sut.InvokeSubmitForm();

        Assert.Equal(1, sut.OnBeforeSubmitCallCount);
        Assert.Equal(0, sut.ExecuteCreateCallCount);
        Assert.Equal(1, sut.ExecuteUpdateCallCount);
        dialog.Verify(x => x.Close(It.Is<DialogResult?>(r =>
            r != null &&
            !r.Canceled &&
            string.Equals(r.Data == null ? null : r.Data.ToString(), "updated:Existing Name", StringComparison.Ordinal))), Times.Once);
    }

    [Fact]
    public async Task SubmitForm_WhenOperationFails_DoesNotCloseDialog()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDetailDialog
        {
            ModelObject = new TestViewModel { Id = "id-20", Name = "Fail" },
            UpdateResult = false
        };
        sut.SetDialog(dialog.Object);

        await sut.InvokeSubmitForm();

        Assert.Equal(1, sut.OnBeforeSubmitCallCount);
        Assert.Equal(1, sut.ExecuteUpdateCallCount);
        dialog.Verify(x => x.Close(It.IsAny<DialogResult?>()), Times.Never);
    }

    [Fact]
    public async Task SubmitForm_WhenModelIsNull_DoesNothing()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDetailDialog
        {
            ModelObject = null
        };
        sut.SetDialog(dialog.Object);

        await sut.InvokeSubmitForm();

        Assert.Equal(0, sut.OnBeforeSubmitCallCount);
        Assert.Equal(0, sut.ExecuteCreateCallCount);
        Assert.Equal(0, sut.ExecuteUpdateCallCount);
        dialog.Verify(x => x.Close(It.IsAny<DialogResult?>()), Times.Never);
    }

    [Fact]
    public void Close_ClosesDialogAsCanceled()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDetailDialog();
        sut.SetDialog(dialog.Object);

        sut.InvokeClose();

        dialog.Verify(x => x.Close(It.Is<DialogResult?>(r => r != null && r.Canceled)), Times.Once);
    }

    [Fact]
    public void Cancel_CancelsDialog()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDetailDialog();
        sut.SetDialog(dialog.Object);

        sut.InvokeCancel();

        dialog.Verify(x => x.Cancel(), Times.Once);
    }

    private sealed class TestDetailDialog : DetailDialogBase<TestDetailDialog, TestViewModel>
    {
        public int InitializeModelCallCount { get; private set; }

        public int OnFormInitializedCallCount { get; private set; }

        public int OnBeforeSubmitCallCount { get; private set; }

        public int ExecuteCreateCallCount { get; private set; }

        public int ExecuteUpdateCallCount { get; private set; }

        public bool CreateResult { get; init; }

        public bool UpdateResult { get; init; }

        public void SetDialog(IMudDialogInstance dialog) => MudDialog = dialog;

        public Task InvokeOnInitializedAsync() => OnInitializedAsync();

        public Task InvokeSubmitForm() => SubmitForm();

        public void InvokeClose() => Close();

        public void InvokeCancel() => Cancel();

        protected override Task OnFormInitializedAsync()
        {
            OnFormInitializedCallCount++;
            return Task.CompletedTask;
        }

        protected override TestViewModel InitializeModel()
        {
            InitializeModelCallCount++;
            return new TestViewModel
            {
                Id = "initialized-id",
                Name = "Initialized"
            };
        }

        protected override bool IsNewRecord(TestViewModel model)
            => string.IsNullOrWhiteSpace(model.Id);

        protected override Task<bool> ExecuteCreateAsync(TestViewModel model)
        {
            ExecuteCreateCallCount++;
            return Task.FromResult(CreateResult);
        }

        protected override Task<bool> ExecuteUpdateAsync(TestViewModel model)
        {
            ExecuteUpdateCallCount++;
            return Task.FromResult(UpdateResult);
        }

        protected override Task OnBeforeSubmitAsync()
        {
            OnBeforeSubmitCallCount++;
            return Task.CompletedTask;
        }

        protected override string GetSuccessMessage(TestViewModel model, bool isNew)
            => isNew ? $"created:{model.Name}" : $"updated:{model.Name}";
    }

    private sealed class TestViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
