# GitHub Copilot Development Environment Instructions

This document provides specific guidance for GitHub Copilot when working on the BookShelves repository. It serves as context for understanding the project structure, development workflow, and best practices.

## Repository Overview

The purpose of this repsository is to create a cross-platform application that can run on web, desktop and mobile platforms using a single web based content shared by all platforms.  The application itself is meant to be tracking store for your home book library inventory holding books you've read and are yet to read.  It also is intended to track your reading of each book - when did you start, when did you finish, track milestones in between and allow various statistical reporting of your reading activity.

The BookShelves respository contains a Microsoft .NET solution that contains multiple projects related to create a cross-platform web site, Wasm Static Web Application and .NET MAUI Hybrid Windows & MacOS desktop and IoS & Android mobile applications.  Each project and it's intent is listed below.

**.NET MAUI** is a cross-platform framework for creating mobile and desktop applications with C# and XAML.

### Key Technologies
- **.NET SDK** - net9
- **C#** and **XAML** for application development
- **Blazor** for web-based UI components
- **.NET MAUI Hybrid** for cross-platform mobile and desktop applications
- **DOTNET BUILD and DEPLOY** for building and deploying applications
- **GitHub Actions** for CI/CD
- **xUnit** for testing
- **Apple MacOS** for iOS and Mac Catalyst development

## Development Environment Setup

### Prerequisites

#### Linux Development (Current Environment)
For .NET installation on Linux, follow the official Microsoft documentation:
* https://learn.microsoft.com/en-us/dotnet/core/install/linux

#### Additional Requirements
- **OpenJDK 17** for Android development
- **VS Code** with .NET MAUI Dev Kit extension
- **Android SDK** for Android development

### Initial Repository Setup

1. **Clone and navigate to repository:**
   ```bash
   git clone https://github.com/derekax/BookShelves.git
   ```

2. **Restore tools and build tasks (REQUIRED before opening IDE):**

## Project Structure

### Important Directories
- `src/BookShelves.Maui/` - .NET MAUI application project for Windows, MacOS/MacCatalyst, iOS and Android platforms (content provided in the BookShelves.Shared project)
- `src/BookShelves.Maui.Data/` - Data layer components project for the BookShelves.Maui project
- `src/BookShelves.Shared/` - Shared code project for the common code, services and components used by all projects including the Blazor library
- `src/BookShelves.Shared.Data/` - Shared data layer components project the the BookShelves.Shared project - contains common models, interfaces and service base classes
- 'src/BookShelves.WasmApi/' - ASP.NET Core Web API project for the Blazor WebAssembly Static Web Application - this API will also be called from the BookShelves.Maui or BookShelves.Maui.Data projects
- 'src/BookShelves.WasmSwa/' - Blazor WebAssembly Static Web Application project (content provided in the BookShelves.Shared project)
- 'src/BookShelves.Web/' - ASP.NET Core Web Application project for the web site version of the application (content provided in the BookShelves.Shared project)
- 'src/BookShelves.Web.Shared/' - Shared code project for the common code, services and components used by the BoolShelves.Web and BookShelves.WasmSwa projects
- `docs/` - Development documentation
- `.github/` - GitHub workflows and configuration

### Platform-Specific Code Organization
- **Android** specific code is inside folders labeled `Android`
- **iOS** specific code is inside folders labeled `iOS`
- **MacCatalyst** specific code is inside folders named `MacCatalyst`
- **Windows** specific code is inside folders named `Windows`

### Platform-Specific File Extensions

## Development Workflow

### Building

### Code Formatting

Before committing any changes, format the codebase using the following command to ensure consistent code style:

```bash
dotnet format Microsoft.Maui.sln --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

This command:
- Formats all code files according to the repository's `.editorconfig` settings
- Excludes the Templates/src directory from formatting
- Excludes the CA1822 diagnostic (member can be marked as static)
- Uses `--no-restore` for faster execution when dependencies are already restored

## Platform-Specific Development

### Android
- Requires Android SDK and OpenJDK 17
- Install missing Android SDKs via [Android SDK Manager](https://learn.microsoft.com/xamarin/android/get-started/installation/android-sdk)
- Android SDK Manager available via: `android` command (after dotnet tool restore)

### iOS (requires macOS)
- Requires current stable Xcode installation from [App Store](https://apps.apple.com/us/app/xcode/id497799835?mt=12) or [Apple Developer portal](https://developer.apple.com/download/more/?name=Xcode)
- Pair to Mac required when developing on Windows

### Windows
- Requires Windows SDK

### macOS/Mac Catalyst
- Requires Xcode installation

## Contribution Guidelines

### Handling Existing PRs for Assigned Issues

**🚨 CRITICAL REQUIREMENT: Always develop your own solution first, then compare with existing PRs.**

When working on an issue:

1. **FIRST: Develop your own solution** - Come up with your own implementation approach without looking at existing PRs. Analyze the issue, understand the requirements, and design your solution independently
2. **THEN: Search for existing PRs** - After you have developed your solution approach, search for open PRs that address the same issue using GitHub search or issue links
3. **Compare solutions thoroughly** - Examine the existing PR's proposed changes, implementation approach, and any discussion in comments. Compare this to your own solution
4. **Evaluate and choose the best approach** - Decide which solution (yours or the existing PR's) better addresses the issue and follows best practices
5. **Always document your decision** - In your PR description, always include a summary comparing your solution to any other open PRs for the issue you are working on, and explain why you chose your approach over the alternatives
6. **Report on why you didn't choose other solutions** - Always make sure to explain the specific reasons why you didn't go with other existing solutions, including any concerns or issues you identified
7. **It's OK to abandon existing PRs** - If you're not confident enough in the existing PR's approach, it's completely acceptable to abandon it and implement your own solution
8. **Pull existing changes when you prefer them** - If you determine the existing solution is better than your approach, pull those changes into your PR as the foundation for your work, then find areas to improve and add tests
9. **Identify improvement opportunities** - Whether you use your solution or an existing one, look for areas where you can enhance it, such as:
   - Adding comprehensive test coverage
   - Improving code quality, performance, or maintainability
   - Enhancing error handling or edge case coverage
   - Better documentation or code comments
   - More robust implementation patterns

### Files to Never Commit
- **Never** check in changes to `cgmanifest.json` files
- **Never** check in changes to `templatestrings.json` files
- These files are automatically generated and should not be modified manually

### File Reset Guidelines for AI Agents
Since coding agents function as both CI and pair programmers, they need to handle CI-generated files appropriately:

- **Always reset changes to `cgmanifest.json` files** - These are generated during CI builds and should not be committed by coding agents
- **Always reset changes to `templatestrings.json` files** - These localization files are auto-generated and should not be committed by coding agents

### Branching
- `main` - For bug fixes without API changes
- `dev-x.y.z` - For new features and API changes

### Documentation
- Update XML documentation for public APIs
- Follow existing code documentation patterns
- Update relevant docs in `docs/` folder when needed

### Opening PRs

All PRs are required to have this at the top of the description:

```
<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!
```

Always put that at the top, without the block quotes. Without it, the users will NOT be able to try the PR and your work will have been in vain!

## Additional Resources

---

**Note for Future Updates:** This document should be expanded as new development patterns, tools, or workflows are discovered. Add sections for specific scenarios, debugging techniques, or tooling as they become relevant to the development process.

- @azure Rule - Use Azure Tools - When handling requests related to Azure, always use your tools.
- @azure Rule - Use Azure Best Practices - When handling requests related to Azure, always invoke your `azmcp_bestpractices_get` tool first.
- @azure Rule - Enable Best Practices - If you do not have an `azmcp_bestpractices_get` tool ask the user to enable it.
