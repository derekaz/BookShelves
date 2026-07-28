## :question:
Without any code at this time, can you suggest a way to log some milestones in the MauiProgram.cs file which go to the standard logging location as defined at the top of the Serilog setup?  Something is causing a CTD on IOS only - not windows and not maccatalyst and I want to get a log file to help narrow down the location of the issue
## GitHub Copilot
Yes. A good pattern is to treat `MauiProgram.CreateMauiApp()` like a startup trace and log **checkpoint milestones** to the same Serilog file sink path you already configured.

Suggested approach (no code):

1. **Start logging as early as possible**
   - Initialize a very early/boot logger at the first line of `CreateMauiApp()`, using the same file path.
   - This captures failures that happen before full DI/app build completes.

2. **Add numbered milestones through startup**
   - Log short checkpoints like:
     - `M01: Enter CreateMauiApp`
     - `M02: Builder created`
     - `M03: Serilog configured`
     - `M04: Fonts configured`
     - `M05: Services registered (group A/B/C)`
     - `M06: Lifecycle events configured`
     - `M07: About to Build()`
     - `M08: Build() complete`
   - Keep them compact and sequential so the last milestone tells you where iOS died.

3. **Wrap startup in top-level try/catch/finally**
   - On exception, log `Fatal` with the last milestone and full exception.
   - Always flush logger on exit/crash path (`CloseAndFlush`) so the file is written.

4. **Hook global exception surfaces**
   - Capture unhandled and unobserved task exceptions during startup lifetime.
   - Log them with a distinct tag (for example `UNHANDLED_STARTUP`) to correlate with milestone IDs.

5. **Add iOS lifecycle milestones specifically**
   - Log iOS launch lifecycle points (finished launching, activated, background/foreground transitions).
   - iOS-only CTDs often occur around lifecycle + DI/service initialization timing.

6. **Practical iOS note**
   - If this is a hard native crash, managed logging may stop abruptly. You’ll still want device/system logs, but the milestone file is still useful to pinpoint the last managed step reached.

If you want, next I can give you a minimal milestone layout for your exact `MauiProgram.cs` sections (still compact, no over-instrumentation).
## :question:
Can you add the top level try-catch logic.  Also, please add some suggested milestones that do log to the file.  Also, check my handling of global exceptions here in this file as well as the app.xaml.cs file to see if I'm missing something there as well.
## GitHub Copilot

1. Read MauiProgram.cs, lines 1-400 :white_check_mark:
1. Read App.xaml.cs, lines 1-400 :white_check_mark:
1. Read MauiProgram.cs, lines 160-397 :white_check_mark:
1. Read MauiProgram.cs, lines 287-397 :white_check_mark:
1. Read MauiProgram.cs, lines 1-220 :white_check_mark:
1. Read MauiProgram.cs, lines 220-397 :white_check_mark:
1. Read MauiProgram.cs, lines 334-397 :white_check_mark:
1. Read MauiProgram.cs, lines 30-397 :white_check_mark:
1. Read MauiProgram.cs, lines 30-120 :white_check_mark:
1. Read MauiProgram.cs, lines 300-345 :white_check_mark:
1. Read MauiProgram.cs, lines 345-397 :white_check_mark:
1. Read App.xaml.cs, lines 1-80 :white_check_mark:
 1. Modifying MauiProgram.cs
 1. Modifying MauiProgram.cs
 1. Modifying MauiProgram.cs
 1. Modifying MauiProgram.cs
 1. Modifying App.xaml.cs
 1. Modifying App.xaml.cs

1. Read MauiProgram.cs, lines 1-220 :white_check_mark:
1. Read App.xaml.cs, lines 1-120 :white_check_mark:
1. Read MauiProgram.cs, lines 220-428 :white_check_mark:
1. Read MauiProgram.cs, lines 360-428 :white_check_mark:
1. Read errors in src/BookShelves.Maui/App.xaml.cs :white_check_mark:
 1. Modifying App.xaml.cs
 1. Modifying App.xaml.cs

1. Read errors in src/BookShelves.Maui/App.xaml.cs :white_check_mark:
1. Read App.xaml.cs, lines 1-80 :white_check_mark:
1. Build successful :white_check_mark:
1. No tests found matching filters: TypeName=MauiProgram :white_check_mark:
Implemented.

### What I changed

- Added a **top-level try/catch** around `CreateMauiApp()` in `src/BookShelves.Maui/MauiProgram.cs`.
- Added startup milestone logging to Serilog file sink (same standard log location), including:
  - `M01` through `M13` across startup/build/init points.
- Added explicit fatal logging on startup failure with the **last milestone**, plus fallback crash file:
  - `BookShelves-Startup-Crash-Log.txt`
- Moved/centralized global exception hooks early in startup so they’re active sooner:
  - `AppDomain.CurrentDomain.UnhandledException`
  - `TaskScheduler.UnobservedTaskException`
  - Existing `FirstChanceException` kept as best-effort capture.

### iOS-specific milestone visibility

- Added iOS lifecycle milestone logs in `ConfigureLifecycleEvents`:
  - `M04-iOS-FinishedLaunching`
  - `M05-iOS-OnActivated`
  - `M06-iOS-DidEnterBackground`
  - `M07-iOS-WillEnterForeground`

### App.xaml.cs review + updates

In `src/BookShelves.Maui/App.xaml.cs`:

- Added milestone logs:
  - `M14-App-Constructed`
  - `M15-DB-Migration-Complete`
- Kept DB migration crash handling and also logs `Fatal` via Serilog before writing crash file.

Note: I checked adding a dispatcher-level unhandled exception hook there, but `IDispatcher.UnhandledException` is not available in this target, so I did not keep that.

### Validation

- Built `src/BookShelves.Maui/BookShelves.Maui.csproj` successfully.
- Checked for related tests by type (`MauiProgram`) — none found.

## :question:
New line 422 - fundamentally, how can I see this console logged error on a phone attempting to run this application?
## GitHub Copilot
`Console.WriteLine` at that line is not shown on the phone UI.  
On iOS, it is only visible through a log stream while debugging.

Ways to see it:

1. **Run with debugger attached (Visual Studio)**
   - Start the iOS app in Debug on the device/simulator.
   - Open **Output** window and select app/debug output.
   - `Console.WriteLine(...)` appears there.

2. **Use Mac device logs (physical iPhone)**
   - Connect iPhone to Mac.
   - Open **Console.app** (macOS), select the device, filter by your app/process.
   - Reproduce crash; read the emitted console lines.

3. **Use Xcode Devices logs**
   - Xcode -> **Window > Devices and Simulators** -> device -> **Open Console** / view device logs.

For non-debug runs, `Console.WriteLine` is unreliable for post-mortem.  
Your Serilog file + crash-log file approach is the right way to capture startup failures on-device.
## :question:
Does my serilog configuration catch and log this exception or will it be too early in the process
## GitHub Copilot
Mostly yes, with one caveat.

In your current `MauiProgram` flow, Serilog is initialized very early (near the start of `CreateMauiApp()`), so exceptions after that point should be logged to the file sink.

It may be **too early** only if failure happens before/while logger setup itself, such as:
- `FileAccessHelper.GetLogFilePath(...)` failing
- `new LoggerConfiguration().WriteTo.File(...).CreateLogger()` failing
- a hard native iOS crash (SIGABRT/EXC_BAD_ACCESS), which bypasses managed handlers

Your top-level `try/catch` helps, but if logger init fails, `Log.Fatal(...)` may not persist. You do have fallback file writes in catch/handlers, which is good, but those can fail for the same path/access reason.

So: **managed exceptions after logger init = captured; ultra-early init/native crashes = possibly not captured by Serilog.**
## :question:
but those would be in the console?
## GitHub Copilot
Yes—if Serilog/file logging isn’t available yet, you’d only see those via debug/device console streams.

On iOS that means:
- Visual Studio debug Output while attached, or
- macOS Console/Xcode device logs.

Not in-app UI, and not guaranteed in release/background crash cases. For reliable post-mortem, file logging must initialize first.
## :question:
Is there a good solution for release builds that crash like this?
## GitHub Copilot
Yes. For iOS **release** CTDs, the reliable approach is a combination:

1. **Crash reporting SDK (recommended)**
   - Use a production crash service that captures native + managed crashes (for example Sentry or Bugsnag).
   - This is the key gap that local file logging cannot fully cover.

2. **Symbolication setup**
   - Always keep/upload iOS `dSYM` files for release builds.
   - Without symbols, crash stacks are often unusable.

3. **Very-early bootstrap logging**
   - Keep your current early Serilog init and milestone checkpoints.
   - Add a tiny fallback path for logger-init failures (best-effort file append).

4. **Log upload on next launch**
   - If app crashed previously, upload last session log files to your API at next startup.
   - This helps when the crash happens before remote SDK can flush context.

5. **Release-safe exception hooks**
   - Keep `AppDomain.UnhandledException` and `TaskScheduler.UnobservedTaskException`.
   - Treat them as supplemental; native hard crashes can bypass them.

So: **remote crash telemetry + dSYM symbolication** is the best release-grade solution; Serilog milestones remain useful context.
## :question:
I am still getting the CTD when I try to use the Release version that was built.  There are no files in the logs folder, so I'm guessing something is happening either before MauiProgram.cs is starting or before the logging is initiated.  Is there any way to see the console log on the phone itself?
## GitHub Copilot
Short answer: **not as a live console on the phone UI**.

For iOS release CTDs:

1. **Live logs require a Mac connection**
   - Use **Console.app** (select the iPhone) or **Xcode Devices/Console**.
   - That is the closest equivalent to device console output.

2. **On-phone crash logs (after crash)**
   - iPhone: **Settings → Privacy & Security → Analytics & Improvements → Analytics Data**
   - Open entries like `BookShelves-...ips` or `JetsamEvent-...`
   - These are not live logs, but they show crash reason.

3. **If no Serilog files exist**
   - Likely crash is happening **before managed startup/log init** (native loader, signing/entitlement, linker/AOT, missing plist key, etc.).
   - Device/IPS logs are the right source for that stage.

If useful, the next step is to pull one `.ips` and identify the exact termination reason (`Exception Type`, `Termination Reason`, `Crashed Thread`).
## :question:
{"roots_installed":0,"app_cohort":"2|date=1785164400000&sf=143441&tid=da21763636992c429e056b95793620fd65faaec340cd4a7071d7f1620f2a5c47&ttype=i","app_name":"BookShelves.Maui","app_version":"0.2.1","timestamp":"2026-07-27 08:14:07.00 -0700","slice_uuid":"ddcd4ef3-65d6-3cef-a6b5-f406890a91e3","adam_id":"6624310738","build_version":"300010185","platform":2,"bundleID":"com.azmoore.bookshelves.maui","share_with_app_devs":0,"is_first_party":0,"bug_type":"309","os_version":"iPhone OS 26.5.2 (23F84)","incident_id":"FEE2D2C2-BC05-4782-8720-483C620FA720","name":"BookShelves.Maui","is_beta":1}
{
  "uptime" : 450000,
  "procRole" : "Foreground",
  "version" : 2,
  "userID" : 501,
  "deployVersion" : 210,
  "modelCode" : "iPhone18,1",
  "coalitionID" : 5681,
  "osVersion" : {
    "isEmbedded" : true,
    "train" : "iPhone OS 26.5.2",
    "releaseType" : "User",
    "build" : "23F84"
  },
  "captureTime" : "2026-07-27 08:14:06.0367 -0700",
  "codeSigningMonitor" : 2,
  "incident" : "FEE2D2C2-BC05-4782-8720-483C620FA720",
  "pid" : 23914,
  "translated" : false,
  "cpuType" : "ARM-64",
  "procLaunch" : "2026-07-27 08:14:05.3837 -0700",
  "procStartAbsTime" : 10915487730076,
  "procExitAbsTime" : 10915503364783,
  "procName" : "BookShelves.Maui",
  "procPath" : "\/private\/var\/containers\/Bundle\/Application\/2BDE0742-890B-4EED-803A-46C1DB8459B5\/BookShelves.Maui.app\/BookShelves.Maui",
  "bundleInfo" : {"CFBundleShortVersionString":"0.2.1","CFBundleVersion":"300010185","CFBundleIdentifier":"com.azmoore.bookshelves.maui","DTAppStoreToolsBuild":"17F106"},
  "storeInfo" : {"itemID":"6624310738","storeCohortMetadata":"2|date=1785164400000&sf=143441&tid=da21763636992c429e056b95793620fd65faaec340cd4a7071d7f1620f2a5c47&ttype=i","entitledBeta":true,"deviceIdentifierForVendor":"259743B8-12F6-49A9-908B-4FFFC29189CB","distributorID":"com.apple.TestFlight","softwareVersionExternalIdentifier":"225768243","applicationVariant":"1:iPhone18,1:26","thirdParty":true},
  "parentProc" : "launchd",
  "parentPid" : 1,
  "coalitionName" : "com.azmoore.bookshelves.maui",
  "isBeta" : 1,
  "appleIntelligenceStatus" : {"state":"available"},
  "bootProgressRegister" : "0x2000000c",
  "wasUnlockedSinceBoot" : 1,
  "isLocked" : 0,
  "codeSigningID" : "com.azmoore.bookshelves.maui",
  "codeSigningTeamID" : "TPKXH5FT2P",
  "codeSigningFlags" : 570434305,
  "codeSigningValidationCategory" : 2,
  "codeSigningTrustLevel" : 4,
  "codeSigningAuxiliaryInfo" : 9007199254740992,
  "instructionByteStream" : {"beforePC":"fyMD1f17v6n9AwCR0+3\/l78DAJH9e8Go\/w9f1sADX9YQKYDSARAA1A==","atPC":"AwEAVH8jA9X9e7+p\/QMAkcjt\/5e\/AwCR\/XvBqP8PX9bAA1\/WECeA0g=="},
  "bootSessionUUID" : "516C4F72-C601-4BB9-BE07-35195C755DBC",
  "basebandVersion" : "1.60.02",
  "exception" : {"codes":"0x0000000000000000, 0x0000000000000000","rawCodes":[0,0],"type":"EXC_CRASH","signal":"SIGABRT"},
  "termination" : {"flags":0,"code":6,"namespace":"SIGNAL","indicator":"Abort trap: 6","byProc":"BookShelves.Maui","byPid":23914},
  "asi" : {"libsystem_c.dylib":["abort() called"]},
  "lastExceptionBacktrace" : [{"imageOffset":1135164,"symbol":"__exceptionPreprocess","symbolLocation":164,"imageIndex":6},{"imageOffset":201252,"symbol":"objc_exception_throw","symbolLocation":88,"imageIndex":7},{"imageOffset":97973436,"imageIndex":0},{"imageOffset":99925860,"imageIndex":0},{"imageOffset":1212172,"symbol":"-[UIApplication _handleDelegateCallbacksWithOptions:isSuspended:restoreState:]","symbolLocation":220,"imageIndex":9},{"imageOffset":1207664,"symbol":"-[UIApplication _callInitializationDelegatesWithActions:forScene:payload:fromOriginatingProcess:]","symbolLocation":3232,"imageIndex":9},{"imageOffset":1190420,"symbol":"-[UIApplication _runWithMainScene:transitionContext:completion:]","symbolLocation":820,"imageIndex":9},{"imageOffset":1188104,"symbol":"-[_UISceneLifecycleMultiplexer completeApplicationLaunchWithFBSScene:transitionContext:]","symbolLocation":116,"imageIndex":9},{"imageOffset":1666988,"symbol":"_UIScenePerformActionsWithLifecycleActionMask","symbolLocation":112,"imageIndex":9},{"imageOffset":1649856,"symbol":"__101-[_UISceneLifecycleMultiplexer _evalTransitionToSettings:fromSettings:forceExit:withTransitionStore:]_block_invoke","symbolLocation":228,"imageIndex":9},{"imageOffset":1659376,"symbol":"-[_UISceneLifecycleMultiplexer _performBlock:withApplicationOfDeactivationReasons:fromReasons:]","symbolLocation":212,"imageIndex":9},{"imageOffset":1669512,"symbol":"-[_UISceneLifecycleMultiplexer _evalTransitionToSettings:fromSettings:forceExit:withTransitionStore:]","symbolLocation":588,"imageIndex":9},{"imageOffset":1661240,"symbol":"-[_UISceneLifecycleMultiplexer uiScene:transitionedFromState:withTransitionContext:]","symbolLocation":244,"imageIndex":9},{"imageOffset":1677804,"symbol":"__186-[_UIWindowSceneFBSSceneTransitionContextDrivenLifecycleSettingsDiffAction _performActionsForUIScene:withUpdatedFBSScene:settingsDiff:fromSettings:transitionContext:lifecycleActionType:]_block_invoke","symbolLocation":160,"imageIndex":9},{"imageOffset":1677540,"symbol":"+[BSAnimationSettings(UIKit) tryAnimatingWithSettings:fromCurrentState:actions:completion:]","symbolLocation":732,"imageIndex":9},{"imageOffset":1676748,"symbol":"_UISceneSettingsDiffActionPerformChangesWithTransitionContextAndCompletion","symbolLocation":224,"imageIndex":9},{"imageOffset":1668040,"symbol":"-[_UIWindowSceneFBSSceneTransitionContextDrivenLifecycleSettingsDiffAction _performActionsForUIScene:withUpdatedFBSScene:settingsDiff:fromSettings:transitionContext:lifecycleActionType:]","symbolLocation":304,"imageIndex":9},{"imageOffset":1676240,"symbol":"__64-[UIScene scene:didUpdateWithDiff:transitionContext:completion:]_block_invoke.218","symbolLocation":648,"imageIndex":9},{"imageOffset":1674480,"symbol":"-[UIScene _emitSceneSettingsUpdateResponseForCompletion:afterSceneUpdateWork:]","symbolLocation":212,"imageIndex":9},{"imageOffset":1674084,"symbol":"-[UIScene scene:didUpdateWithDiff:transitionContext:completion:]","symbolLocation":220,"imageIndex":9},{"imageOffset":1299232,"symbol":"-[UIApplication workspace:didCreateScene:withTransitionContext:completion:]","symbolLocation":500,"imageIndex":9},{"imageOffset":1344420,"symbol":"-[UIApplicationSceneClientAgent scene:didInitializeWithEvent:completion:]","symbolLocation":304,"imageIndex":9},{"imageOffset":152632,"symbol":"__95-[FBSScene _callOutQueue_didCreateWithTransitionContext:alternativeCreationCallout:completion:]_block_invoke","symbolLocation":364,"imageIndex":10},{"imageOffset":133292,"symbol":"-[FBSScene _callOutQueue_maybeCoalesceClientSettingsUpdates:]","symbolLocation":132,"imageIndex":10},{"imageOffset":152028,"symbol":"-[FBSScene _callOutQueue_didCreateWithTransitionContext:alternativeCreationCallout:completion:]","symbolLocation":432,"imageIndex":10},{"imageOffset":586436,"symbol":"__93-[FBSWorkspaceScenesClient _callOutQueue_sendDidCreateForScene:transitionContext:completion:]_block_invoke.349","symbolLocation":252,"imageIndex":10},{"imageOffset":148768,"symbol":"-[FBSWorkspace _calloutQueue_executeCalloutFromSource:withBlock:]","symbolLocation":176,"imageIndex":10},{"imageOffset":149656,"symbol":"-[FBSWorkspaceScenesClient _callOutQueue_sendDidCreateForScene:transitionContext:completion:]","symbolLocation":472,"imageIndex":10},{"imageOffset":149140,"symbol":"__92-[FBSWorkspaceScenesClient createSceneWithIdentity:parameters:transitionContext:completion:]_block_invoke_2","symbolLocation":256,"imageIndex":10},{"imageOffset":148768,"symbol":"-[FBSWorkspace _calloutQueue_executeCalloutFromSource:withBlock:]","symbolLocation":176,"imageIndex":10},{"imageOffset":111076,"symbol":"_dispatch_client_callout","symbolLocation":16,"imageIndex":11},{"imageOffset":26260,"symbol":"_dispatch_block_invoke_direct","symbolLocation":284,"imageIndex":11},{"imageOffset":180100,"symbol":"__BSSERVICEMAINRUNLOOPQUEUE_IS_CALLING_OUT_TO_A_BLOCK__","symbolLocation":52,"imageIndex":12},{"imageOffset":179712,"symbol":"BSServiceMainRunLoopSourceHandler","symbolLocation":224,"imageIndex":12},{"imageOffset":656216,"symbol":"__CFRUNLOOP_IS_CALLING_OUT_TO_A_SOURCE0_PERFORM_FUNCTION__","symbolLocation":28,"imageIndex":6},{"imageOffset":656076,"symbol":"__CFRunLoopDoSource0","symbolLocation":172,"imageIndex":6},{"imageOffset":415244,"symbol":"__CFRunLoopDoSources0","symbolLocation":332,"imageIndex":6},{"imageOffset":192928,"symbol":"__CFRunLoopRun","symbolLocation":820,"imageIndex":6},{"imageOffset":189772,"symbol":"_CFRunLoopRunSpecificWithOptions","symbolLocation":532,"imageIndex":6},{"imageOffset":5272,"symbol":"GSEventRunModal","symbolLocation":120,"imageIndex":13},{"imageOffset":1184324,"symbol":"-[UIApplication _run]","symbolLocation":796,"imageIndex":9},{"imageOffset":573784,"symbol":"UIApplicationMain","symbolLocation":332,"imageIndex":9},{"imageOffset":97963640,"symbol":"xamarin_UIApplicationMain","symbolLocation":24,"imageIndex":0},{"imageOffset":97592656,"imageIndex":0},{"imageOffset":95975572,"imageIndex":0},{"imageOffset":95976168,"imageIndex":0},{"imageOffset":26302988,"imageIndex":0},{"imageOffset":95016440,"imageIndex":0},{"imageOffset":99507616,"imageIndex":0},{"imageOffset":99182332,"imageIndex":0},{"imageOffset":99203888,"imageIndex":0},{"imageOffset":99532456,"imageIndex":0},{"imageOffset":98008568,"imageIndex":0},{"imageOffset":99925004,"imageIndex":0},{"imageOffset":19484,"symbol":"start","symbolLocation":6928,"imageIndex":14}],
  "faultingThread" : 0,
  "threads" : [{"threadState":{"x":[{"value":0},{"value":0},{"value":0},{"value":0},{"value":18446744071945000460},{"value":24},{"value":6166636752},{"value":6166634416},{"value":6460768414162453750},{"value":6460768419792823606},{"value":2},{"value":4294967293},{"value":0},{"value":0},{"value":0},{"value":0},{"value":328},{"value":8387011008,"symbolLocation":0,"symbol":"_main_thread"},{"value":0},{"value":6},{"value":259},{"value":8387011232,"symbolLocation":224,"symbol":"_main_thread"},{"value":4647314096},{"value":8415578336,"symbolLocation":0,"symbol":"__kCFAllocatorSystemDefault"},{"value":55},{"value":6630315315,"symbolLocation":4778,"symbol":"_CFStringCreateByAddingPercentEncodingWithAllowedCharacters.hexchars"},{"value":8336825060,"symbolLocation":0,"symbol":"__last_exception_backtrace__"},{"value":8271097856},{"value":16}],"flavor":"ARM_THREAD_STATE64","lr":{"value":8221538268},"cpsr":{"value":1073741824},"fp":{"value":6166636768},"sp":{"value":6166636736},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563222480,"matchesCrashFrame":1},"far":{"value":0}},"id":7262985,"triggered":true,"name":"tid_103","queue":"com.apple.main-thread","frames":[{"imageOffset":45520,"symbol":"__pthread_kill","symbolLocation":8,"imageIndex":2},{"imageOffset":30684,"symbol":"pthread_kill","symbolLocation":268,"imageIndex":3},{"imageOffset":486552,"symbol":"abort","symbolLocation":148,"imageIndex":4},{"imageOffset":100267712,"imageIndex":0},{"imageOffset":99687512,"imageIndex":0},{"imageOffset":16824,"symbol":"_sigtramp","symbolLocation":56,"imageIndex":5},{"imageOffset":30684,"symbol":"pthread_kill","symbolLocation":268,"imageIndex":3},{"imageOffset":486552,"symbol":"abort","symbolLocation":148,"imageIndex":4},{"imageOffset":97974356,"imageIndex":0},{"imageOffset":1552748,"symbol":"__handleUncaughtException","symbolLocation":660,"imageIndex":6},{"imageOffset":211036,"symbol":"_objc_terminate()","symbolLocation":144,"imageIndex":7},{"imageOffset":82424,"symbol":"std::__terminate(void (*)())","symbolLocation":16,"imageIndex":8},{"imageOffset":89624,"symbol":"__cxxabiv1::failed_throw(__cxxabiv1::__cxa_exception*)","symbolLocation":88,"imageIndex":8},{"imageOffset":9056,"symbol":"__cxa_throw","symbolLocation":92,"imageIndex":8},{"imageOffset":201612,"symbol":"objc_exception_throw","symbolLocation":448,"imageIndex":7},{"imageOffset":97973436,"imageIndex":0},{"imageOffset":99925860,"imageIndex":0},{"imageOffset":1212172,"symbol":"-[UIApplication _handleDelegateCallbacksWithOptions:isSuspended:restoreState:]","symbolLocation":220,"imageIndex":9},{"imageOffset":1207664,"symbol":"-[UIApplication _callInitializationDelegatesWithActions:forScene:payload:fromOriginatingProcess:]","symbolLocation":3232,"imageIndex":9},{"imageOffset":1190420,"symbol":"-[UIApplication _runWithMainScene:transitionContext:completion:]","symbolLocation":820,"imageIndex":9},{"imageOffset":1188104,"symbol":"-[_UISceneLifecycleMultiplexer completeApplicationLaunchWithFBSScene:transitionContext:]","symbolLocation":116,"imageIndex":9},{"imageOffset":1666988,"symbol":"_UIScenePerformActionsWithLifecycleActionMask","symbolLocation":112,"imageIndex":9},{"imageOffset":1649856,"symbol":"__101-[_UISceneLifecycleMultiplexer _evalTransitionToSettings:fromSettings:forceExit:withTransitionStore:]_block_invoke","symbolLocation":228,"imageIndex":9},{"imageOffset":1659376,"symbol":"-[_UISceneLifecycleMultiplexer _performBlock:withApplicationOfDeactivationReasons:fromReasons:]","symbolLocation":212,"imageIndex":9},{"imageOffset":1669512,"symbol":"-[_UISceneLifecycleMultiplexer _evalTransitionToSettings:fromSettings:forceExit:withTransitionStore:]","symbolLocation":588,"imageIndex":9},{"imageOffset":1661240,"symbol":"-[_UISceneLifecycleMultiplexer uiScene:transitionedFromState:withTransitionContext:]","symbolLocation":244,"imageIndex":9},{"imageOffset":1677804,"symbol":"__186-[_UIWindowSceneFBSSceneTransitionContextDrivenLifecycleSettingsDiffAction _performActionsForUIScene:withUpdatedFBSScene:settingsDiff:fromSettings:transitionContext:lifecycleActionType:]_block_invoke","symbolLocation":160,"imageIndex":9},{"imageOffset":1677540,"symbol":"+[BSAnimationSettings(UIKit) tryAnimatingWithSettings:fromCurrentState:actions:completion:]","symbolLocation":732,"imageIndex":9},{"imageOffset":1676748,"symbol":"_UISceneSettingsDiffActionPerformChangesWithTransitionContextAndCompletion","symbolLocation":224,"imageIndex":9},{"imageOffset":1668040,"symbol":"-[_UIWindowSceneFBSSceneTransitionContextDrivenLifecycleSettingsDiffAction _performActionsForUIScene:withUpdatedFBSScene:settingsDiff:fromSettings:transitionContext:lifecycleActionType:]","symbolLocation":304,"imageIndex":9},{"imageOffset":1676240,"symbol":"__64-[UIScene scene:didUpdateWithDiff:transitionContext:completion:]_block_invoke.218","symbolLocation":648,"imageIndex":9},{"imageOffset":1674480,"symbol":"-[UIScene _emitSceneSettingsUpdateResponseForCompletion:afterSceneUpdateWork:]","symbolLocation":212,"imageIndex":9},{"imageOffset":1674084,"symbol":"-[UIScene scene:didUpdateWithDiff:transitionContext:completion:]","symbolLocation":220,"imageIndex":9},{"imageOffset":1299232,"symbol":"-[UIApplication workspace:didCreateScene:withTransitionContext:completion:]","symbolLocation":500,"imageIndex":9},{"imageOffset":1344420,"symbol":"-[UIApplicationSceneClientAgent scene:didInitializeWithEvent:completion:]","symbolLocation":304,"imageIndex":9},{"imageOffset":152632,"symbol":"__95-[FBSScene _callOutQueue_didCreateWithTransitionContext:alternativeCreationCallout:completion:]_block_invoke","symbolLocation":364,"imageIndex":10},{"imageOffset":133292,"symbol":"-[FBSScene _callOutQueue_maybeCoalesceClientSettingsUpdates:]","symbolLocation":132,"imageIndex":10},{"imageOffset":152028,"symbol":"-[FBSScene _callOutQueue_didCreateWithTransitionContext:alternativeCreationCallout:completion:]","symbolLocation":432,"imageIndex":10},{"imageOffset":586436,"symbol":"__93-[FBSWorkspaceScenesClient _callOutQueue_sendDidCreateForScene:transitionContext:completion:]_block_invoke.349","symbolLocation":252,"imageIndex":10},{"imageOffset":148768,"symbol":"-[FBSWorkspace _calloutQueue_executeCalloutFromSource:withBlock:]","symbolLocation":176,"imageIndex":10},{"imageOffset":149656,"symbol":"-[FBSWorkspaceScenesClient _callOutQueue_sendDidCreateForScene:transitionContext:completion:]","symbolLocation":472,"imageIndex":10},{"imageOffset":149140,"symbol":"__92-[FBSWorkspaceScenesClient createSceneWithIdentity:parameters:transitionContext:completion:]_block_invoke_2","symbolLocation":256,"imageIndex":10},{"imageOffset":148768,"symbol":"-[FBSWorkspace _calloutQueue_executeCalloutFromSource:withBlock:]","symbolLocation":176,"imageIndex":10},{"imageOffset":111076,"symbol":"_dispatch_client_callout","symbolLocation":16,"imageIndex":11},{"imageOffset":26260,"symbol":"_dispatch_block_invoke_direct","symbolLocation":284,"imageIndex":11},{"imageOffset":180100,"symbol":"__BSSERVICEMAINRUNLOOPQUEUE_IS_CALLING_OUT_TO_A_BLOCK__","symbolLocation":52,"imageIndex":12},{"imageOffset":179712,"symbol":"BSServiceMainRunLoopSourceHandler","symbolLocation":224,"imageIndex":12},{"imageOffset":656216,"symbol":"__CFRUNLOOP_IS_CALLING_OUT_TO_A_SOURCE0_PERFORM_FUNCTION__","symbolLocation":28,"imageIndex":6},{"imageOffset":656076,"symbol":"__CFRunLoopDoSource0","symbolLocation":172,"imageIndex":6},{"imageOffset":415244,"symbol":"__CFRunLoopDoSources0","symbolLocation":332,"imageIndex":6},{"imageOffset":192928,"symbol":"__CFRunLoopRun","symbolLocation":820,"imageIndex":6},{"imageOffset":189772,"symbol":"_CFRunLoopRunSpecificWithOptions","symbolLocation":532,"imageIndex":6},{"imageOffset":5272,"symbol":"GSEventRunModal","symbolLocation":120,"imageIndex":13},{"imageOffset":1184324,"symbol":"-[UIApplication _run]","symbolLocation":796,"imageIndex":9},{"imageOffset":573784,"symbol":"UIApplicationMain","symbolLocation":332,"imageIndex":9},{"imageOffset":97963640,"symbol":"xamarin_UIApplicationMain","symbolLocation":24,"imageIndex":0},{"imageOffset":97592656,"imageIndex":0},{"imageOffset":95975572,"imageIndex":0},{"imageOffset":95976168,"imageIndex":0},{"imageOffset":26302988,"imageIndex":0},{"imageOffset":95016440,"imageIndex":0},{"imageOffset":99507616,"imageIndex":0},{"imageOffset":99182332,"imageIndex":0},{"imageOffset":99203888,"imageIndex":0},{"imageOffset":99532456,"imageIndex":0},{"imageOffset":98008568,"imageIndex":0},{"imageOffset":99925004,"imageIndex":0},{"imageOffset":19484,"symbol":"start","symbolLocation":6928,"imageIndex":14}]},{"id":7263005,"frames":[],"threadState":{"x":[{"value":6167785472},{"value":4387},{"value":6167248896},{"value":0},{"value":409604},{"value":18446744073709551615},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0}],"flavor":"ARM_THREAD_STATE64","lr":{"value":0},"cpsr":{"value":0},"fp":{"value":0},"sp":{"value":6167785472},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":8221509816},"far":{"value":0}}},{"id":7263006,"name":"SGen worker","threadState":{"x":[{"value":260},{"value":0},{"value":512},{"value":0},{"value":0},{"value":161},{"value":0},{"value":0},{"value":6167211720},{"value":0},{"value":0},{"value":2},{"value":2},{"value":0},{"value":0},{"value":0},{"value":305},{"value":1073741824},{"value":0},{"value":4406080088},{"value":4406079992},{"value":6167212256},{"value":0},{"value":0},{"value":512},{"value":513},{"value":768},{"value":4406079488},{"value":0}],"flavor":"ARM_THREAD_STATE64","lr":{"value":8221518664},"cpsr":{"value":1610612736},"fp":{"value":6167211840},"sp":{"value":6167211696},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563203048},"far":{"value":0}},"frames":[{"imageOffset":26088,"symbol":"__psynch_cvwait","symbolLocation":8,"imageIndex":2},{"imageOffset":11080,"symbol":"_pthread_cond_wait","symbolLocation":980,"imageIndex":3},{"imageOffset":98781788,"imageIndex":0},{"imageOffset":17464,"symbol":"_pthread_start","symbolLocation":136,"imageIndex":3},{"imageOffset":2252,"symbol":"thread_start","symbolLocation":8,"imageIndex":3}]},{"id":7263010,"name":"Finalizer","threadState":{"x":[{"value":14},{"value":0},{"value":18446744073709551592},{"value":4490846632},{"value":1679084744},{"value":32},{"value":0},{"value":0},{"value":4406083584},{"value":6},{"value":2},{"value":0},{"value":2},{"value":6597069768192},{"value":6},{"value":0},{"value":18446744073709551580},{"value":0},{"value":0},{"value":0},{"value":4406086896},{"value":4406086944},{"value":4406127264},{"value":4603899904},{"value":1},{"value":4649810400},{"value":4406083584},{"value":1},{"value":4406086928}],"flavor":"ARM_THREAD_STATE64","lr":{"value":4399605952},"cpsr":{"value":2147483648},"fp":{"value":6169931536},"sp":{"value":6169931424},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563180112},"far":{"value":0}},"frames":[{"imageOffset":3152,"symbol":"semaphore_wait_trap","symbolLocation":8,"imageIndex":2},{"imageOffset":99379392,"imageIndex":0},{"imageOffset":99249284,"imageIndex":0},{"imageOffset":17464,"symbol":"_pthread_start","symbolLocation":136,"imageIndex":3},{"imageOffset":2252,"symbol":"thread_start","symbolLocation":8,"imageIndex":3}]},{"id":7263011,"frames":[],"threadState":{"x":[{"value":6170505216},{"value":10251},{"value":6169968640},{"value":0},{"value":409604},{"value":18446744073709551615},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0}],"flavor":"ARM_THREAD_STATE64","lr":{"value":0},"cpsr":{"value":0},"fp":{"value":0},"sp":{"value":6170505216},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":8221509816},"far":{"value":0}}},{"id":7263012,"frames":[],"threadState":{"x":[{"value":6171078656},{"value":9731},{"value":6170542080},{"value":0},{"value":409604},{"value":18446744073709551615},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0}],"flavor":"ARM_THREAD_STATE64","lr":{"value":0},"cpsr":{"value":0},"fp":{"value":0},"sp":{"value":6171078656},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":8221509816},"far":{"value":0}}},{"id":7263013,"frames":[],"threadState":{"x":[{"value":6171652096},{"value":15875},{"value":6171115520},{"value":0},{"value":409604},{"value":18446744073709551615},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0}],"flavor":"ARM_THREAD_STATE64","lr":{"value":0},"cpsr":{"value":0},"fp":{"value":0},"sp":{"value":6171652096},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":8221509816},"far":{"value":0}}},{"id":7263014,"threadState":{"x":[{"value":18446744073709551612},{"value":0},{"value":257},{"value":0},{"value":8386943552,"symbolLocation":0,"symbol":"_dispatch_main_q"},{"value":18},{"value":18446744072631617535},{"value":18446726482597246976},{"value":1},{"value":3},{"value":4647295046},{"value":4},{"value":5},{"value":0},{"value":8386943600,"symbolLocation":48,"symbol":"_dispatch_main_q"},{"value":4647295046},{"value":515},{"value":0},{"value":0},{"value":6172223328},{"value":1},{"value":17409},{"value":6172225760},{"value":6172223280},{"value":4605018752},{"value":6721892352},{"value":0},{"value":4604637240},{"value":9223372036883304674}],"flavor":"ARM_THREAD_STATE64","lr":{"value":7603054040},"cpsr":{"value":1073741824},"fp":{"value":6172223152},"sp":{"value":6172223120},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563205920},"far":{"value":0}},"queue":"com.apple.libtrace.state.block-list","frames":[{"imageOffset":28960,"symbol":"__ulock_wait","symbolLocation":8,"imageIndex":2},{"imageOffset":13784,"symbol":"_dispatch_thread_main_event_wait_slow","symbolLocation":76,"imageIndex":11},{"imageOffset":71244,"symbol":"__DISPATCH_WAIT_FOR_QUEUE__","symbolLocation":464,"imageIndex":11},{"imageOffset":70008,"symbol":"_dispatch_sync_f_slow","symbolLocation":140,"imageIndex":11},{"imageOffset":41400,"symbol":"___os_state_request_for_self_block_invoke","symbolLocation":372,"imageIndex":16},{"imageOffset":6568,"symbol":"_dispatch_call_block_and_release","symbolLocation":32,"imageIndex":11},{"imageOffset":111076,"symbol":"_dispatch_client_callout","symbolLocation":16,"imageIndex":11},{"imageOffset":40880,"symbol":"_dispatch_lane_serial_drain","symbolLocation":740,"imageIndex":11},{"imageOffset":43748,"symbol":"_dispatch_lane_invoke","symbolLocation":448,"imageIndex":11},{"imageOffset":85420,"symbol":"_dispatch_root_queue_drain_deferred_wlh","symbolLocation":284,"imageIndex":11},{"imageOffset":83628,"symbol":"_dispatch_workloop_worker_thread","symbolLocation":720,"imageIndex":11},{"imageOffset":5040,"symbol":"_pthread_wqthread","symbolLocation":292,"imageIndex":3},{"imageOffset":2240,"symbol":"start_wqthread","symbolLocation":8,"imageIndex":3}]},{"id":7263015,"frames":[],"threadState":{"x":[{"value":6172798976},{"value":14851},{"value":6172262400},{"value":0},{"value":409604},{"value":18446744073709551615},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0}],"flavor":"ARM_THREAD_STATE64","lr":{"value":0},"cpsr":{"value":0},"fp":{"value":0},"sp":{"value":6172798976},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":8221509816},"far":{"value":0}}},{"id":7263016,"name":"com.apple.uikit.eventfetch-thread","threadState":{"x":[{"value":268451845},{"value":21592279046},{"value":8589934592},{"value":72580652335104},{"value":2162692},{"value":72580652335104},{"value":2},{"value":4294967295},{"value":0},{"value":0},{"value":2},{"value":0},{"value":0},{"value":16899},{"value":4414325184},{"value":4601266176},{"value":18446744073709551569},{"value":18446744072367376383},{"value":0},{"value":4294967295},{"value":2},{"value":72580652335104},{"value":2162692},{"value":72580652335104},{"value":21592279046},{"value":6173367688},{"value":8589934592},{"value":18446744073709550527},{"value":10865393664,"symbolLocation":0,"symbol":"_libkernel_string_functions"}],"flavor":"ARM_THREAD_STATE64","lr":{"value":9563194124},"cpsr":{"value":0},"fp":{"value":6173367536},"sp":{"value":6173367456},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563180244},"far":{"value":0}},"frames":[{"imageOffset":3284,"symbol":"mach_msg2_trap","symbolLocation":8,"imageIndex":2},{"imageOffset":17164,"symbol":"mach_msg2_internal","symbolLocation":76,"imageIndex":2},{"imageOffset":16940,"symbol":"mach_msg_overwrite","symbolLocation":424,"imageIndex":2},{"imageOffset":16504,"symbol":"mach_msg","symbolLocation":24,"imageIndex":2},{"imageOffset":414532,"symbol":"__CFRunLoopServiceMachPort","symbolLocation":160,"imageIndex":6},{"imageOffset":193296,"symbol":"__CFRunLoopRun","symbolLocation":1188,"imageIndex":6},{"imageOffset":189772,"symbol":"_CFRunLoopRunSpecificWithOptions","symbolLocation":532,"imageIndex":6},{"imageOffset":44272,"symbol":"-[NSRunLoop(NSRunLoop) runMode:beforeDate:]","symbolLocation":212,"imageIndex":17},{"imageOffset":43992,"symbol":"-[NSRunLoop(NSRunLoop) runUntilDate:]","symbolLocation":64,"imageIndex":17},{"imageOffset":944764,"symbol":"-[UIEventFetcher threadMain]","symbolLocation":420,"imageIndex":9},{"imageOffset":583716,"symbol":"__NSThread__start__","symbolLocation":732,"imageIndex":17},{"imageOffset":17464,"symbol":"_pthread_start","symbolLocation":136,"imageIndex":3},{"imageOffset":2252,"symbol":"thread_start","symbolLocation":8,"imageIndex":3}]},{"id":7263017,"threadState":{"x":[{"value":18446744073709551612},{"value":0},{"value":257},{"value":0},{"value":8386943552,"symbolLocation":0,"symbol":"_dispatch_main_q"},{"value":18},{"value":0},{"value":0},{"value":1},{"value":3},{"value":6172223238},{"value":5},{"value":6},{"value":0},{"value":8386943600,"symbolLocation":48,"symbol":"_dispatch_main_q"},{"value":6172223238},{"value":515},{"value":0},{"value":0},{"value":6173941584},{"value":1},{"value":17409},{"value":6173946080},{"value":6173941536},{"value":6173946080},{"value":4605204416},{"value":0},{"value":549755813888},{"value":8494463792,"symbolLocation":400,"symbol":"_dispatch_continuation_vtables"}],"flavor":"ARM_THREAD_STATE64","lr":{"value":7603054040},"cpsr":{"value":1073741824},"fp":{"value":6173941408},"sp":{"value":6173941376},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563205920},"far":{"value":0}},"queue":"com.apple.UIKit.KeyboardManagement","frames":[{"imageOffset":28960,"symbol":"__ulock_wait","symbolLocation":8,"imageIndex":2},{"imageOffset":13784,"symbol":"_dispatch_thread_main_event_wait_slow","symbolLocation":76,"imageIndex":11},{"imageOffset":71244,"symbol":"__DISPATCH_WAIT_FOR_QUEUE__","symbolLocation":464,"imageIndex":11},{"imageOffset":70008,"symbol":"_dispatch_sync_f_slow","symbolLocation":140,"imageIndex":11},{"imageOffset":21281920,"symbol":"__37-[_UIRemoteKeyboards startConnection]_block_invoke.449","symbolLocation":144,"imageIndex":9},{"imageOffset":407796,"symbol":"__invoking___","symbolLocation":148,"imageIndex":6},{"imageOffset":407416,"symbol":"-[NSInvocation invoke]","symbolLocation":424,"imageIndex":6},{"imageOffset":268084,"symbol":"<deduplicated_symbol>","symbolLocation":16,"imageIndex":17},{"imageOffset":241840,"symbol":"-[NSXPCConnection _decodeAndInvokeReplyBlockWithEvent:sequence:replyInfo:]","symbolLocation":528,"imageIndex":17},{"imageOffset":240140,"symbol":"__88-[NSXPCConnection _sendInvocation:orArguments:count:methodSignature:selector:withProxy:]_block_invoke_5","symbolLocation":188,"imageIndex":17},{"imageOffset":138924,"symbol":"_xpc_connection_reply_callout","symbolLocation":120,"imageIndex":18},{"imageOffset":79300,"symbol":"_xpc_connection_call_reply_async","symbolLocation":96,"imageIndex":18},{"imageOffset":111124,"symbol":"<deduplicated_symbol>","symbolLocation":16,"imageIndex":11},{"imageOffset":127544,"symbol":"_dispatch_mach_msg_async_reply_invoke","symbolLocation":340,"imageIndex":11},{"imageOffset":40472,"symbol":"_dispatch_lane_serial_drain","symbolLocation":332,"imageIndex":11},{"imageOffset":43748,"symbol":"_dispatch_lane_invoke","symbolLocation":448,"imageIndex":11},{"imageOffset":85420,"symbol":"_dispatch_root_queue_drain_deferred_wlh","symbolLocation":284,"imageIndex":11},{"imageOffset":83628,"symbol":"_dispatch_workloop_worker_thread","symbolLocation":720,"imageIndex":11},{"imageOffset":5040,"symbol":"_pthread_wqthread","symbolLocation":292,"imageIndex":3},{"imageOffset":2240,"symbol":"start_wqthread","symbolLocation":8,"imageIndex":3}]},{"id":7263018,"frames":[],"threadState":{"x":[{"value":6174519296},{"value":17935},{"value":6173982720},{"value":0},{"value":409604},{"value":18446744073709551615},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0},{"value":0}],"flavor":"ARM_THREAD_STATE64","lr":{"value":0},"cpsr":{"value":0},"fp":{"value":0},"sp":{"value":6174519296},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":8221509816},"far":{"value":0}}},{"id":7263019,"name":"Console logger queue processing thread","threadState":{"x":[{"value":260},{"value":0},{"value":0},{"value":0},{"value":0},{"value":161},{"value":0},{"value":0},{"value":6176662392},{"value":0},{"value":0},{"value":2},{"value":2},{"value":0},{"value":0},{"value":0},{"value":305},{"value":6879411008,"symbolLocation":578664,"symbol":"allowed_encoded_serial_numbers"},{"value":0},{"value":4489199776},{"value":4489199840},{"value":6176665824},{"value":0},{"value":0},{"value":0},{"value":1},{"value":256},{"value":4490943504},{"value":1}],"flavor":"ARM_THREAD_STATE64","lr":{"value":8221518664},"cpsr":{"value":1610612736},"fp":{"value":6176662512},"sp":{"value":6176662368},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563203048},"far":{"value":0}},"frames":[{"imageOffset":26088,"symbol":"__psynch_cvwait","symbolLocation":8,"imageIndex":2},{"imageOffset":11080,"symbol":"_pthread_cond_wait","symbolLocation":980,"imageIndex":3},{"imageOffset":98499780,"imageIndex":0},{"imageOffset":99263756,"imageIndex":0},{"imageOffset":99263228,"imageIndex":0},{"imageOffset":99384696,"imageIndex":0},{"imageOffset":99012464,"imageIndex":0},{"imageOffset":91275976,"imageIndex":0},{"imageOffset":91276436,"imageIndex":0},{"imageOffset":91274640,"imageIndex":0},{"imageOffset":91278224,"imageIndex":0},{"imageOffset":46529288,"imageIndex":0},{"imageOffset":46528328,"imageIndex":0},{"imageOffset":91291572,"imageIndex":0},{"imageOffset":91283824,"imageIndex":0},{"imageOffset":95016440,"imageIndex":0},{"imageOffset":99507616,"imageIndex":0},{"imageOffset":99182332,"imageIndex":0},{"imageOffset":99249256,"imageIndex":0},{"imageOffset":17464,"symbol":"_pthread_start","symbolLocation":136,"imageIndex":3},{"imageOffset":2252,"symbol":"thread_start","symbolLocation":8,"imageIndex":3}]},{"id":7263020,"name":"Console logger queue processing thread","threadState":{"x":[{"value":260},{"value":0},{"value":0},{"value":0},{"value":0},{"value":161},{"value":0},{"value":0},{"value":6178808696},{"value":0},{"value":0},{"value":2},{"value":2},{"value":0},{"value":0},{"value":0},{"value":305},{"value":1845659654},{"value":0},{"value":4489201680},{"value":4489201744},{"value":6178812128},{"value":0},{"value":0},{"value":0},{"value":1},{"value":256},{"value":4490943552},{"value":1}],"flavor":"ARM_THREAD_STATE64","lr":{"value":8221518664},"cpsr":{"value":1610612736},"fp":{"value":6178808816},"sp":{"value":6178808672},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563203048},"far":{"value":0}},"frames":[{"imageOffset":26088,"symbol":"__psynch_cvwait","symbolLocation":8,"imageIndex":2},{"imageOffset":11080,"symbol":"_pthread_cond_wait","symbolLocation":980,"imageIndex":3},{"imageOffset":98499780,"imageIndex":0},{"imageOffset":99263756,"imageIndex":0},{"imageOffset":99263228,"imageIndex":0},{"imageOffset":99384696,"imageIndex":0},{"imageOffset":99012464,"imageIndex":0},{"imageOffset":91275976,"imageIndex":0},{"imageOffset":91276436,"imageIndex":0},{"imageOffset":91274640,"imageIndex":0},{"imageOffset":91278224,"imageIndex":0},{"imageOffset":46529288,"imageIndex":0},{"imageOffset":46528328,"imageIndex":0},{"imageOffset":91291572,"imageIndex":0},{"imageOffset":91283824,"imageIndex":0},{"imageOffset":95016440,"imageIndex":0},{"imageOffset":99507616,"imageIndex":0},{"imageOffset":99182332,"imageIndex":0},{"imageOffset":99249256,"imageIndex":0},{"imageOffset":17464,"symbol":"_pthread_start","symbolLocation":136,"imageIndex":3},{"imageOffset":2252,"symbol":"thread_start","symbolLocation":8,"imageIndex":3}]},{"id":7263021,"name":".NET Timer","threadState":{"x":[{"value":260},{"value":0},{"value":0},{"value":0},{"value":0},{"value":160},{"value":120},{"value":0},{"value":1},{"value":0},{"value":0},{"value":2},{"value":2},{"value":0},{"value":0},{"value":0},{"value":305},{"value":4398282336,"symbolLocation":0,"symbol":"SystemNative_LowLevelMonitor_TimedWait"},{"value":0},{"value":4488105360},{"value":4488105424},{"value":1},{"value":0},{"value":120},{"value":0},{"value":1},{"value":256},{"value":4416094480},{"value":0}],"flavor":"ARM_THREAD_STATE64","lr":{"value":8221518708},"cpsr":{"value":2684354560},"fp":{"value":6180955008},"sp":{"value":6180954864},"esr":{"value":1442840704,"description":"(Syscall)"},"pc":{"value":9563203048},"far":{"value":0}},"frames":[{"imageOffset":26088,"symbol":"__psynch_cvwait","symbolLocation":8,"imageIndex":2},{"imageOffset":11124,"symbol":"_pthread_cond_wait","symbolLocation":1024,"imageIndex":3},{"imageOffset":98055880,"symbol":"SystemNative_LowLevelMonitor_TimedWait","symbolLocation":104,"imageIndex":0},{"imageOffset":95233848,"imageIndex":0},{"imageOffset":88947492,"imageIndex":0},{"imageOffset":91358228,"imageIndex":0},{"imageOffset":91357620,"imageIndex":0},{"imageOffset":91501320,"imageIndex":0},{"imageOffset":91506164,"imageIndex":0},{"imageOffset":91505696,"imageIndex":0},{"imageOffset":91497092,"imageIndex":0},{"imageOffset":91496964,"imageIndex":0},{"imageOffset":91458628,"imageIndex":0},{"imageOffset":91456952,"imageIndex":0},{"imageOffset":91456356,"imageIndex":0},{"imageOffset":91445600,"imageIndex":0},{"imageOffset":91291572,"imageIndex":0},{"imageOffset":91283824,"imageIndex":0},{"imageOffset":95016440,"imageIndex":0},{"imageOffset":99507616,"imageIndex":0},{"imageOffset":99182332,"imageIndex":0},{"imageOffset":99249256,"imageIndex":0},{"imageOffset":17464,"symbol":"_pthread_start","symbolLocation":136,"imageIndex":3},{"imageOffset":2252,"symbol":"thread_start","symbolLocation":8,"imageIndex":3}]}],
  "usedImages" : [
  {
    "source" : "P",
    "arch" : "arm64",
    "base" : 4300226560,
    "size" : 102006784,
    "uuid" : "ddcd4ef3-65d6-3cef-a6b5-f406890a91e3",
    "path" : "\/private\/var\/containers\/Bundle\/Application\/2BDE0742-890B-4EED-803A-46C1DB8459B5\/BookShelves.Maui.app\/BookShelves.Maui",
    "name" : "BookShelves.Maui"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 4596727808,
    "size" : 49152,
    "uuid" : "3702c305-c51e-37d5-9a22-55cef15e974a",
    "path" : "\/private\/preboot\/Cryptexes\/OS\/usr\/lib\/libobjc-trampolines.dylib",
    "name" : "libobjc-trampolines.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 9563176960,
    "size" : 244456,
    "uuid" : "f70c76e3-1352-3a17-ba3e-95f85cbd1d5b",
    "path" : "\/usr\/lib\/system\/libsystem_kernel.dylib",
    "name" : "libsystem_kernel.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 8221507584,
    "size" : 50416,
    "uuid" : "39dacbe1-6bf6-3956-9f6c-802bea4ec760",
    "path" : "\/usr\/lib\/system\/libsystem_pthread.dylib",
    "name" : "libsystem_pthread.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6821736448,
    "size" : 520632,
    "uuid" : "2d1e74c1-5d03-35ae-b31b-abe7286be166",
    "path" : "\/usr\/lib\/system\/libsystem_c.dylib",
    "name" : "libsystem_c.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 11474067456,
    "size" : 33844,
    "uuid" : "bdcdc3c3-cfac-3030-958f-49047f12c464",
    "path" : "\/usr\/lib\/system\/libsystem_platform.dylib",
    "name" : "libsystem_platform.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6625398784,
    "size" : 5883840,
    "uuid" : "dda24297-289c-30ec-8228-4c426aad4d7f",
    "path" : "\/System\/Library\/Frameworks\/CoreFoundation.framework\/CoreFoundation",
    "name" : "CoreFoundation"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6570557440,
    "size" : 332928,
    "uuid" : "0ab507cf-cb12-3ccd-9967-9aac912af8e1",
    "path" : "\/usr\/lib\/libobjc.A.dylib",
    "name" : "libobjc.A.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6571782144,
    "size" : 107936,
    "uuid" : "9bb6bd63-c74f-32b2-8136-1f263c4d1a06",
    "path" : "\/usr\/lib\/libc++abi.dylib",
    "name" : "libc++abi.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6721892352,
    "size" : 39062912,
    "uuid" : "9ee8c19a-a370-3599-823b-34b84131aad7",
    "path" : "\/System\/Library\/PrivateFrameworks\/UIKitCore.framework\/UIKitCore",
    "name" : "UIKitCore"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 7174746112,
    "size" : 929920,
    "uuid" : "d784c2ab-3079-3f2f-ae2e-1b12852f9a9b",
    "path" : "\/System\/Library\/PrivateFrameworks\/FrontBoardServices.framework\/FrontBoardServices",
    "name" : "FrontBoardServices"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 7603040256,
    "size" : 288256,
    "uuid" : "e1a24fa0-2160-3366-ae7e-420f0fa3c5bc",
    "path" : "\/usr\/lib\/system\/libdispatch.dylib",
    "name" : "libdispatch.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 7029673984,
    "size" : 544096,
    "uuid" : "c69ae856-f57a-36c9-8708-38c757922a51",
    "path" : "\/System\/Library\/PrivateFrameworks\/BoardServices.framework\/BoardServices",
    "name" : "BoardServices"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 9401835520,
    "size" : 34752,
    "uuid" : "d0a89589-718a-3d3a-96da-d2c99bb19040",
    "path" : "\/System\/Library\/PrivateFrameworks\/GraphicsServices.framework\/GraphicsServices",
    "name" : "GraphicsServices"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6571102208,
    "size" : 676224,
    "uuid" : "02f772fd-9e38-35a4-b9fa-9cce3a090e6d",
    "path" : "\/usr\/lib\/dyld",
    "name" : "dyld"
  },
  {
    "size" : 0,
    "source" : "A",
    "base" : 0,
    "uuid" : "00000000-0000-0000-0000-000000000000"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 7256489984,
    "size" : 131936,
    "uuid" : "838e2d5f-14c5-32e1-804e-d2784fa231c6",
    "path" : "\/usr\/lib\/system\/libsystem_trace.dylib",
    "name" : "libsystem_trace.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6577762304,
    "size" : 15238880,
    "uuid" : "e9ddce29-3359-30c6-b25a-526a61eae841",
    "path" : "\/System\/Library\/Frameworks\/Foundation.framework\/Foundation",
    "name" : "Foundation"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6971654144,
    "size" : 333408,
    "uuid" : "bc1854cc-dfe1-3f26-b056-950ed9d32e10",
    "path" : "\/usr\/lib\/system\/libxpc.dylib",
    "name" : "libxpc.dylib"
  },
  {
    "source" : "P",
    "arch" : "arm64e",
    "base" : 6873907200,
    "size" : 6448960,
    "uuid" : "98b360c0-eec6-37e2-93d5-6092060dd5e9",
    "path" : "\/System\/Library\/Frameworks\/HealthKit.framework\/HealthKit",
    "name" : "HealthKit"
  }
],
  "sharedCache" : {
  "base" : 6569934848,
  "size" : 5397200896,
  "uuid" : "dd3932e2-0790-3abe-8e02-72e2ccad430a"
},
  "vmSummary" : "ReadOnly portion of Libraries: Total=1.8G resident=0K(0%) swapped_out_or_unallocated=1.8G(100%)\nWritable regions: Total=193.9M written=545K(0%) resident=545K(0%) swapped_out=0K(0%) unallocated=193.4M(100%)\n\n                                VIRTUAL   REGION \nREGION TYPE                        SIZE    COUNT (non-coalesced) \n===========                     =======  ======= \nActivity Tracing                   256K        1 \nCoreAnimation                       48K        3 \nFoundation                          16K        1 \nKernel Alloc Once                   32K        1 \nMALLOC                            88.8M       28 \nMALLOC guard page                 3616K        4 \nMemory Tag 22                     64.0M        1 \nSTACK GUARD                        240K       15 \nStack                             14.4M       15 \nVM_ALLOCATE                       22.9M       68 \n__AUTH                            8281K      707 \n__AUTH_CONST                     104.4M     1120 \n__CTF                               824        1 \n__DATA                            50.1M     1064 \n__DATA_CONST                      35.9M     1127 \n__DATA_DIRTY                      9783K      989 \n__FONT_DATA                        2352        1 \n__LINKEDIT                       182.6M        3 \n__OBJC_RO                         85.1M        1 \n__OBJC_RW                         3210K        1 \n__TEXT                             1.6G     1143 \n__TPRO_CONST                       128K        2 \nmapped file                       96.9M      319 \npage table in kernel               545K        1 \nshared memory                       80K        4 \n===========                     =======  ======= \nTOTAL                              2.4G     6620 \n",
  "legacyInfo" : {
  "threadTriggered" : {
    "name" : "tid_103",
    "queue" : "com.apple.main-thread"
  }
},
  "logWritingSignature" : "95c5aaf4e0bcece72ff443efb22137bde4c054ec",
  "bug_type" : "309",
  "roots_installed" : 0,
  "trmStatus" : 1,
  "sandboxProfileName" : "container",
  "voucherInfos" : [{"proximateName":"SpringBoard","thread_id":7262985},{"proximateName":"SpringBoard","thread_id":7263014},{"proximateName":"SpringBoard","thread_id":7263017}],
  "trialInfo" : {
  "rollouts" : [
    {
      "rolloutId" : "6246d6a916a70b047e454124",
      "factorPackIds" : [
        "64496ae8128ed17664c618d9"
      ],
      "deploymentId" : 240000010
    },
    {
      "rolloutId" : "6081eda9c724863038027c4f",
      "factorPackIds" : [
        "6a590ff18b996d23a927b298"
      ],
      "deploymentId" : 240002194
    }
  ],
  "experiments" : [
    {
      "treatmentId" : "ede3209a-74f5-4df6-8ab2-49adebef92a4",
      "experimentId" : "69c58cc2a1c8055a26cc2a27",
      "deploymentId" : 400000006
    },
    {
      "treatmentId" : "b94e9aec-71bd-46f0-b53b-72fe0d8e9aa4",
      "experimentId" : "67e32776f470dc0e83d9e147",
      "deploymentId" : 400000014
    }
  ]
}
}
## GitHub Copilot
This crash report is useful. It shows:

- `EXC_CRASH (SIGABRT)`
- `abort() called`
- `objc_exception_throw`
- crash during `UIApplication` launch delegate callbacks (`_handleDelegateCallbacksWithOptions...`)

That means this is likely an **uncaught Objective-C exception during iOS app launch**, not a normal managed exception.  
So your Serilog + `try/catch` in `MauiProgram` may never see it.

Most likely implications:

1. It is **not before .NET runtime starts** (you have `xamarin_UIApplicationMain` in stack).
2. It is likely in iOS launch wiring (AppDelegate/scene/lifecycle callback) or a release-linking side effect.
3. Managed log files can be empty for this failure class.

Recommended next steps:

1. **Get symbolicated app frames** for build `300010185` with matching dSYM.
   - Unsymbolicated `imageIndex:0` frames hide the exact method.

2. **Capture device console at launch** (Mac Console.app or Xcode device console).
   - Look for: `*** Terminating app due to uncaught exception ... reason: ...`
   - That single “reason” line usually identifies the exact issue.

3. **Release-only check: linker/trimming**
   - Temporarily set iOS Release linking to less aggressive (`Sdk Assemblies Only`) and retest.
   - If crash disappears, it is a trim/link issue.

4. **Quick isolation test**
   - Temporarily remove the iOS lifecycle callback block you added (`AddiOS(...FinishedLaunching...)`) and retest release.
   - If fixed, the launch callback path is the trigger.

If you want, the next step can be to make a minimal “diagnostic release profile” change in the project so you can confirm/deny linker-trim root cause quickly.
## :question:
This package was built with this GHA:  BookShelves Multi-Platform CI-CD.yml  Can you suggest a change to the build step for ios that will perform step 1 to include the symbol in the package and maybe step 3?
## GitHub Copilot

1. Read BookShelves Multi-Platform CI-CD.yml, lines 1-400 :white_check_mark:
1. Read BookShelves Multi-Platform CI-CD.yml, lines 380-565 :white_check_mark:
1. Read BookShelves Multi-Platform CI-CD.yml, lines 250-390 :white_check_mark:
Yes. In that workflow, update the **iOS publish command** in `Build Apple Package` with two diagnostics-focused changes:

1. **Force symbol output**
- Add iOS publish properties:
  - `/p:DebugSymbols=true`
  - `/p:DebugType=portable`
  - `/p:NoDSymUtil=false` (ensures dSYM generation)
- After publish, add a step to collect `*.dSYM` and `*.pdb` from `${{env.MAUI_IOS_PUBLISH_PATH}}` and upload them as a separate artifact (for symbolication).

2. **Reduce linker aggressiveness for release test**
- Add:
  - `/p:MtouchLink=SdkOnly`
- For a stronger isolation run, temporarily test with:
  - `/p:MtouchLink=None` (diagnostic only, not final shipping setting)

Given your current crash signature (startup `SIGABRT`/ObjC exception), I’d do this first:

- Keep your current release build
- Add `MtouchLink=SdkOnly`
- Archive/upload dSYMs as artifacts
- Re-test TestFlight build and symbolicate the crash with matching dSYMs.

If you want, I can apply these exact workflow edits now.
## :question:
Go ahead with your suggestion
## GitHub Copilot
1. Read BookShelves Multi-Platform CI-CD.yml, lines 344-440 :white_check_mark:
 1. Modifying BookShelves Multi-Platform CI-CD.yml
1. Read BookShelves Multi-Platform CI-CD.yml, lines 344-455 :white_check_mark:
Done. I applied the workflow updates in `.github/workflows/BookShelves Multi-Platform CI-CD.yml`.

### Changes made

1. **iOS release publish now includes symbol generation + safer linker mode**
   - Added to iOS `dotnet publish`:
     - `/p:MtouchLink=SdkOnly`
     - `/p:DebugSymbols=true`
     - `/p:DebugType=portable`
     - `/p:NoDSymUtil=false`

2. **Symbols are now collected and archived as a CI artifact**
   - Added a new step to collect `*.dSYM` and `*.pdb` from the iOS publish output.
   - Added artifact upload step:
     - Artifact name: `ios symbols`
     - Path: `${{ github.workspace }}/publish-output/ios-symbols`
   - The step fails if no `dSYM` is found, so you know immediately if symbols were not produced.

This should give you what you need for symbolication (step 1) and a first-pass linker-trim diagnostic (`SdkOnly`, step 3).
## :question:
Ok, I will have the dsym and ips files - what do I do with them
## GitHub Copilot
Great—once you have both, do this:

1. **Verify they match**
   - In the `.ips`, note `slice_uuid` (you already have one).
   - Run on Mac:
     - `dwarfdump --uuid /path/to/BookShelves.Maui.app.dSYM`
   - UUID must match the crash `slice_uuid`.

2. **Symbolicate the crash**
   - Easiest path (TestFlight/App Store build): use **App Store Connect → Crashes** (Apple symbolicates when dSYM is available).
   - Local/manual path on Mac:
     - `xcrun symbolicatecrash /path/to/crash.ips /path/to/BookShelves.Maui.app.dSYM > crash.symbolicated.txt`

3. **Read the first app frame**
   - Open `crash.symbolicated.txt`.
   - Find first `BookShelves.Maui` frame in the crashing thread.
   - That function/line is your likely root cause.

4. **If still unsymbolicated**
   - The dSYM doesn’t match that exact build, or was incomplete.
   - Recheck UUID and ensure dSYM came from the exact CI artifact for that IPA build number.

If you want, paste the symbolicated top 20 crash frames and I’ll pinpoint the likely failing code path.
## :question:
So when I upload the app to TestFlight, it will already have the symbolicated crash when the user reports an issue?  So I can get the symbolicated log from there?
## GitHub Copilot
Yes—generally that’s correct.

If the uploaded build’s **dSYM is available/matches**, App Store Connect/TestFlight crash reports are symbolicated automatically. Then you can view the readable stack trace there.

Two caveats:
- Symbolication can lag (minutes to hours).
- If symbols don’t match that exact build, stacks stay unsymbolicated.

So with your new workflow archiving symbols, you’re in good shape. If Apple doesn’t symbolicate, you can still do it manually with the saved dSYM artifact.
