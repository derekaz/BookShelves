# Sync Progress Display Implementation Guide

## Overview

The enhanced sync progress system allows you to display detailed synchronization progress on your UI with step tracking ("Step 1 of 3", etc.) and percentage completion.

## Architecture

### Components

1. **SyncProgressService** - Updated service that tracks sync progress with step information
2. **SyncProgressEventArgs** - Extended event arguments including:
   - `CurrentStep` - Current step number (0-3)
   - `TotalSteps` - Total steps in sync process (3)
   - `ProgressPercentage` - Overall percentage (0-100)
   - `SyncStage` - Enum-based stage (Started, Pushing, Pulling, Processing, Completed, Failed)

3. **SyncViewModel** - ViewModel for MAUI that binds to progress events
4. **SyncProgressDisplay** - MAUI component displaying progress visually

### Sync Stages

The synchronization process has 3 main stages:

| Stage | Step | Description | Icon |
|-------|------|-------------|------|
| Pushing | 1 | Uploading local changes to server | 📤 |
| Pulling | 2 | Downloading updates from server | 📥 |
| Processing | 3 | Merging/processing remote data | ⚙️ |

## Usage Example

### In Your XAML (Page)

```xml
<VerticalStackLayout>
	<!-- Your other content -->

	<!-- Sync Progress Display -->
	<controls:SyncProgressDisplay 
		BindingContext="{StaticResource SyncViewModel}"
		CurrentStep="{Binding CurrentStep}"
		TotalSteps="{Binding TotalSteps}"
		Message="{Binding Message}"
		ProgressPercentage="{Binding ProgressPercentage}"
		SyncStage="{Binding SyncStage}"
		IsVisible="{Binding IsSyncing}"/>
</VerticalStackLayout>
```

### Register in MauiProgram.cs

```csharp
builder
	.UseMauiApp<App>()
	.ConfigureFonts(fonts =>
	{
		fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
	})
	.Services
	.AddSingleton<ISyncProgressService, SyncProgressService>()
	.AddSingleton<SyncViewModel>();
```

### In Your Code-Behind or ViewModel

```csharp
// When initiating sync
private async Task StartSyncAsync()
{
	try
	{
		await _syncUnitOfWork.SynchronizeAsync();
	}
	catch (Exception ex)
	{
		Debug.WriteLine($"Sync failed: {ex.Message}");
	}
}
```

## Progress Tracking Flow

```
Step 1 of 3 - Pushing (📤)
├─ Sync Started
├─ Extracting local changes...
├─ Uploading item 1 of 100
├─ Uploading item 2 of 100
├─ ...
└─ Push complete → Progress: 33%

Step 2 of 3 - Pulling (📥)
├─ Downloading updates...
├─ Processing item 1 of 50
├─ ...
└─ Pull complete → Progress: 66%

Step 3 of 3 - Processing (⚙️)
├─ Merging changes...
├─ Processing item 1 of 150
├─ ...
└─ Sync complete → Progress: 100% ✅
```

## Customization

### Adding More Stages

If you need to add stages to the sync process:

1. Add new stage to `SyncStage` enum in `SyncProgressService.cs`:
   ```csharp
   public enum SyncStage
   {
	   // ... existing stages ...
	   Validating = 5,    // New stage
   }
   ```

2. Add mapping in `StageToStepMap` in `SyncProgressService.cs`:
   ```csharp
   { SyncStage.Validating, 4 }
   ```

3. Update stage detection in `SyncUnitOfWork.OnSynchronizationProgress()`:
   ```csharp
   "validating" => SyncStage.Validating,
   ```

4. Update `SyncStageToDisplayConverter` with new display text

### Custom UI Styling

Modify `SyncProgressDisplay.xaml` to match your app's design system:

```xml
<!-- Change colors -->
<ProgressBar 
	ProgressColor="#007AFF"           <!-- Change this -->
	BackgroundColor="#E0E0E0"/>        <!-- And this -->

<!-- Change fonts/sizes -->
<Label 
	FontSize="18"
	FontAttributes="Bold"
	FontFamily="YourCustomFont"/>
```

### Percentage Calculation

The percentage is calculated as:
- **Base**: `(CurrentStep - 1) / TotalSteps * 100`
- **With item progress**: Adds `(Current / Total) * (100 / TotalSteps)` for fine-grained progress

Example:
- Step 1 (Pushing): 0-33%
- Step 2 (Pulling): 33-66%
- Step 3 (Processing): 66-100%

## Event Handling

The `ISyncProgressService.ProgressChanged` event fires whenever progress is updated:

```csharp
_syncProgressService.ProgressChanged += (sender, args) =>
{
	Debug.WriteLine($"Step {args.CurrentStep} of {args.TotalSteps}: {args.Message}");
	Debug.WriteLine($"Progress: {args.ProgressPercentage}%");
};
```

## Notes

- Progress updates are best-effort; some stages may not report item-level progress
- The `MainThread.BeginInvokeOnMainThread()` call in the ViewModel ensures UI updates happen on the main thread
- The `IsSyncing` property automatically tracks whether sync is in progress
- Failed syncs will show "❌ Synchronization failed" and `IsSyncing` will be false
