using BookShelves.Shared.Components.Bases;
using Moq;
using MudBlazor;

namespace BookShelves.Shared.Tests.Components;

public sealed class DeleteDialogBaseTests
{
    [Fact]
    public void Ok_ModelIsNull_CancelsDialog()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDeleteDialog();
        sut.SetModelObject(null);
        sut.SetDialog(dialog.Object);

        sut.InvokeOk();

        dialog.Verify(x => x.Cancel(), Times.Once);
        dialog.Verify(x => x.Close(It.IsAny<DialogResult>()), Times.Never);
    }

    [Fact]
    public void Ok_ModelExists_ClosesDialogWithIdentifier()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDeleteDialog();
        sut.SetModelObject(new TestViewModel { Id = "id-123", Name = "Name" });
        sut.SetDialog(dialog.Object);

        sut.InvokeOk();

        dialog.Verify(x => x.Close(It.Is<DialogResult>(r =>
            !r.Canceled &&
            string.Equals(r.Data == null ? null : r.Data.ToString(), "id-123", StringComparison.Ordinal))), Times.Once);
        dialog.Verify(x => x.Cancel(), Times.Never);
    }

    [Fact]
    public void Close_ClosesDialogAsCanceled()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDeleteDialog();
        sut.SetModelObject(new TestViewModel { Id = "id-123", Name = "Name" });
        sut.SetDialog(dialog.Object);

        sut.InvokeClose();

        dialog.Verify(x => x.Close(It.Is<DialogResult>(r => r.Canceled)), Times.Once);
    }

    [Fact]
    public void Cancel_CancelsDialog()
    {
        var dialog = new Mock<IMudDialogInstance>();
        var sut = new TestDeleteDialog();
        sut.SetModelObject(new TestViewModel { Id = "id-123", Name = "Name" });
        sut.SetDialog(dialog.Object);

        sut.InvokeCancel();

        dialog.Verify(x => x.Cancel(), Times.Once);
    }

    private sealed class TestDeleteDialog : DeleteDialogBase<TestViewModel>
    {
        public void SetDialog(IMudDialogInstance dialog) => MudDialog = dialog;

        public void SetModelObject(TestViewModel? model) => ModelObject = model;

        public void InvokeOk() => Ok();

        public void InvokeClose() => Close();

        public void InvokeCancel() => Cancel();

        protected override string GetRecordIdentifier(TestViewModel model) => model.Id;

        protected override string GetRecordName(TestViewModel model) => model.Name;
    }

    private sealed class TestViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
