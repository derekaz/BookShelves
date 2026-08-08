using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BookShelves.Shared.Components.Bases;

public abstract class DetailDialogBase<TDialog, TViewModel> : ComponentBase
{
    [CascadingParameter]
    protected IMudDialogInstance? MudDialog { get; set; }

    [Parameter]
    public TViewModel? ModelObject { get; set; }

    [Parameter]
    public string TitleText { get; set; } = "Details";

    protected string message = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        // Ensure an object context exists if none was supplied by parameter pipelines
        ModelObject ??= InitializeModel();
        await OnFormInitializedAsync();
    }

    // Lifecycle Hook overrides for descendant page custom loads
    protected virtual Task OnFormInitializedAsync() => Task.CompletedTask;
    protected abstract TViewModel InitializeModel();

    // Core Identity checks (Determines if an object is newly initialized vs existing persistence layer key)
    protected abstract bool IsNewRecord(TViewModel model);
    protected abstract Task<bool> ExecuteCreateAsync(TViewModel model);
    protected abstract Task<bool> ExecuteUpdateAsync(TViewModel model);

    // Hook to modify form variables or apply child properties right before submitting
    protected virtual Task OnBeforeSubmitAsync() => Task.CompletedTask;
    protected abstract string GetSuccessMessage(TViewModel model, bool isNew);

    protected async Task SubmitForm()
    {
        if (ModelObject is null) return;

        await OnBeforeSubmitAsync();

        bool isNew = IsNewRecord(ModelObject);
        bool success = isNew
            ? await ExecuteCreateAsync(ModelObject)
            : await ExecuteUpdateAsync(ModelObject);

        if (success)
        {
            string successMsg = GetSuccessMessage(ModelObject, isNew);
            MudDialog?.Close(DialogResult.Ok(successMsg));
        }
    }

    protected void Close() => MudDialog?.Close(DialogResult.Cancel());
    protected void Cancel() => MudDialog?.Cancel();
}
