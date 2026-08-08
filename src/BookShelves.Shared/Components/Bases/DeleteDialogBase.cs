using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BookShelves.Shared.Components.Bases;

public abstract class DeleteDialogBase<TViewModel> : ComponentBase
{
    [CascadingParameter]
    protected IMudDialogInstance? MudDialog { get; set; }

    [Parameter]
    public TViewModel? ModelObject { get; set; }

    // Abstract methods to dynamically extract entity data on the child page level
    protected abstract string GetRecordIdentifier(TViewModel model);
    protected abstract string GetRecordName(TViewModel model);

    protected void Ok()
    {
        if (ModelObject is null)
        {
            MudDialog?.Cancel();
            return;
        }

        // Return the unique entity ID payload back to the underlying DataViewPageBase
        string id = GetRecordIdentifier(ModelObject);
        MudDialog?.Close(DialogResult.Ok(id));
    }

    protected void Close() => MudDialog?.Close(DialogResult.Cancel());
    protected void Cancel() => MudDialog?.Cancel();
}
