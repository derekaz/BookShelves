# Sync Progress Display in Authors Page

## What Was Changed

The existing `BookShelves.Shared/Components/Pages/Authors/Index.razor` page now displays real-time sync progress with:

- **Step tracking**: "Step 1 of 3", "Step 2 of 3", etc.
- **Progress bar**: Visual indicator showing sync completion percentage
- **Stage emoji**: 📤 Uploading, 📥 Downloading, ⚙️ Processing
- **Item progress**: Shows current item being processed (e.g., "Uploading changes (5/100)")

## How It Works

### 1. UI Display (HTML Section)

Added a progress display section that appears when syncing is in progress:

```html
@if (isSyncing)
{
	<div class="sync-progress-container alert alert-info">
		<strong>Step @currentStep of @totalSteps - @GetStageDisplay(syncStage)</strong>
		<span class="badge bg-info">@progressPercentage%</span>

		<div class="progress">
			<div class="progress-bar" style="width: @progressPercentage%;"></div>
		</div>

		<small>@syncMessage</small>
	</div>
}
```

### 2. Properties (Code Section)

New properties track the sync state:

```csharp
private bool isSyncing = false;              // Is sync currently running?
private int currentStep = 0;                 // Current step (1-3)
private int totalSteps = 3;                  // Total steps (always 3)
private int progressPercentage = 0;          // 0-100%
private SyncStage syncStage = SyncStage.None; // Enum for stage type
private string syncMessage = string.Empty;   // "Uploading item 5 of 100"
```

### 3. Event Handling

The `OnSyncProgressChanged` event handler updates all properties:

```csharp
private void OnSyncProgressChanged(object? sender, SyncProgressEventArgs e)
{
	// Update properties from event args
	currentStep = e.CurrentStep;
	totalSteps = e.TotalSteps;
	progressPercentage = e.ProgressPercentage;
	syncStage = e.SyncStage;
	syncMessage = BuildSyncMessage(e);
	isSyncing = (e.SyncStage is Started or Pushing or Pulling or Processing);

	// Trigger UI refresh
	StateHasChanged();
}
```

### 4. Display Helpers

**`BuildSyncMessage()`** - Shows item-level progress:
```
"Uploading changes (5/100)"
```

**`GetStageDisplay()`** - Converts SyncStage enum to readable text with emoji:
```
📤 Uploading    (Pushing stage)
📥 Downloading  (Pulling stage)
⚙️ Processing    (Processing stage)
✅ Complete     (Completed)
❌ Failed       (Failed)
```

## Live Example

When you click "Server Sync" on the Authors page:

```
Before sync:
├─ [Server Sync] button is enabled
└─ No progress display

During sync (Step 1):
├─ [Server Sync] button is disabled
├─ Progress display shows:
│  ├─ "Step 1 of 3 - 📤 Uploading"
│  ├─ Progress bar: [████░░░░░░] 25%
│  └─ "Uploading changes (50/100)"
└─ (User sees it's working)

During sync (Step 2):
├─ Progress display updates to:
│  ├─ "Step 2 of 3 - 📥 Downloading"
│  ├─ Progress bar: [████████░░] 58%
│  └─ "Downloading updates (30/50)"

During sync (Step 3):
├─ Progress display updates to:
│  ├─ "Step 3 of 3 - ⚙️ Processing"
│  ├─ Progress bar: [██████████] 90%

After sync (Complete):
├─ Progress display hides
├─ [Server Sync] button re-enabled
├─ Message shows: "Sync complete"
└─ Authors table refreshes with new data

On error:
├─ Progress display shows:
│  ├─ "❌ Failed"
│  ├─ Progress bar stays at last position
│  └─ Message shows error details
└─ [Server Sync] button re-enabled
```

## Key Features

✅ **Works with existing sync infrastructure** - No changes needed to `SyncUnitOfWork` or other services

✅ **Thread-safe** - Already handles both MAUI and Blazor threading models via `MainThread.BeginInvokeOnMainThread()`

✅ **Bootstrap styling** - Uses standard Bootstrap alert, progress bar, and badge classes

✅ **Automatic UI state** - Button disables during sync, progress display shows/hides based on `isSyncing` flag

✅ **Backward compatible** - Original message format still displayed below for diagnostics

## Customization

### Change the styling
Modify the CSS in the progress container:
```html
<div class="sync-progress-container alert alert-info">
	<!-- Change "alert alert-info" to "alert alert-primary", etc. -->
</div>
```

### Hide item-level details
Remove or modify `BuildSyncMessage()` to just show stage name:
```csharp
return e.Message ?? "Processing...";
```

### Change stage emoji
Edit `GetStageDisplay()` method:
```csharp
BookShelves.Shared.Services.SyncStage.Pushing => "⬆️ Uploading",
```

### Add additional stages
If you add more stages to the sync process (e.g., validation), update:
1. `SyncStage` enum in `SyncProgressService.cs`
2. `GetStageDisplay()` method to handle new stage
3. Update `StageToStepMap` in `SyncProgressService.cs` if changing step count

## Files Modified

- `src/BookShelves.Shared/Components/Pages/Authors/Index.razor` - Added progress display UI and tracking properties

## Files That Support This

These files were created earlier to support the sync progress system:
- `src/BookShelves.Shared/Services/SyncProgressService.cs` - Enhanced with `SyncStage` enum and step calculations
- `src/BookShelves.Maui.Data/Infrastructure/SyncUnitOfWork.cs` - Updated to report stages using new enum

The Authors page now automatically receives and displays all this information in real-time.
