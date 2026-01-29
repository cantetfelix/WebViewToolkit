# Contributing to WebView Toolkit

Thank you for your interest in contributing to WebView Toolkit! We welcome contributions from the community.

## Code of Conduct

This project adheres to a Code of Conduct. By participating, you are expected to uphold this code. Please read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing.

## How to Contribute

### Reporting Bugs

If you find a bug, please open an issue on GitHub with:
- A clear, descriptive title
- Steps to reproduce the issue
- Expected behavior vs actual behavior
- Unity version and OS version
- WebView2 Runtime version (if applicable)
- Screenshots or code samples if helpful

### Suggesting Features

Feature requests are welcome! Please open an issue with:
- A clear description of the feature
- Use cases and examples
- Why this would be valuable to other users

### Pull Requests

We actively welcome pull requests! Here's how to contribute code:

1. **Fork the repository**
   - Fork the WebView Toolkit repository to your GitHub account
   - Clone your fork locally

2. **Create a feature branch**
   ```bash
   git checkout -b feature/amazing-feature
   ```

3. **Make your changes**
   - Follow the code style guidelines below
   - Add tests for new features
   - Update documentation for API changes

4. **Update CHANGELOG.md**
   - Add an entry describing your changes under the "Unreleased" section
   - Follow the existing format (Added/Changed/Fixed/Removed)

5. **Ensure builds succeed locally**
   - Run native build: `.\WebViewToolkitPlugin\Build.ps1`
   - Test in Unity with both sample scenes
   - Verify no new warnings or errors

6. **Commit your changes**
   - Use clear, descriptive commit messages
   - Reference issue numbers if applicable (e.g., "Fix #123: Handle null texture")

7. **Push to your branch**
   ```bash
   git push origin feature/amazing-feature
   ```

8. **Open a Pull Request**
   - Provide a clear description of the changes
   - Link to related issues
   - Explain testing performed

## Development Setup

### Prerequisites

**For C++ Plugin Development:**
- Windows 10/11 x64
- Visual Studio 2022 with "Desktop development with C++" workload
- CMake 3.21 or later
- PowerShell (for build scripts)

**For Unity Package Development:**
- Unity 2021.3 LTS or later (Unity 6000.3+ recommended)
- WebView2 Runtime (usually pre-installed on Windows 10/11)

### Building from Source

1. **Clone the repository**
   ```bash
   git clone https://github.com/cantetfelix/WebViewToolkit.git
   cd WebViewToolkit
   ```

2. **Setup dependencies** (C++ plugin only)
   ```powershell
   cd WebViewToolkitPlugin
   .\Setup.ps1
   ```
   This will:
   - Configure CMake with the release preset
   - Download dependencies via vcpkg (WebView2 SDK, WIL)

3. **Build the native plugin**
   ```powershell
   .\Build.ps1
   ```
   This will:
   - Compile the C++ plugin (Release configuration)
   - Copy the DLL to `WebViewToolkit/Runtime/Plugins/x86_64/`

4. **Open in Unity**
   - Add the `WebViewToolkit` folder as a local package
   - Open one of the sample scenes to test

### Running Tests

**Unity Tests:**
1. Open Unity Test Runner (Window → General → Test Runner)
2. Run "EditMode" tests for component tests
3. Run "PlayMode" tests for integration tests

**Native Tests:**
- Currently no native unit tests (contributions welcome!)

## Code Style Guidelines

### C# (.NET/Unity)

- **Naming**:
  - PascalCase for public members, types, namespaces
  - camelCase for private fields (with `_` prefix for instance fields)
  - PascalCase for properties and methods

- **Formatting**:
  - 4 spaces for indentation (no tabs)
  - Opening braces on new line (Allman style)
  - Use XML documentation comments for all public APIs

- **Conventions**:
  - Prefer `var` when type is obvious
  - Use nullable reference types where appropriate
  - Implement `IDisposable` for types managing unmanaged resources

**Example:**
```csharp
/// <summary>
/// Creates a new WebView instance.
/// </summary>
/// <param name="width">Width in pixels</param>
/// <param name="height">Height in pixels</param>
/// <returns>WebView instance or null if creation failed</returns>
public WebViewInstance CreateWebView(int width, int height)
{
    // Implementation
}
```

### C++ (Native Plugin)

- **Standard**: C++20

- **Naming**:
  - PascalCase for classes and types
  - camelCase for functions and methods
  - m_ prefix for member variables
  - s_ prefix for static variables

- **Formatting**:
  - 4 spaces for indentation
  - Opening braces on same line
  - Use Doxygen-style comments for APIs

- **Conventions**:
  - Prefer `std::unique_ptr` and `std::shared_ptr` over raw pointers
  - Use `const` liberally
  - Prefer range-based for loops
  - Use C++20 features where appropriate (concepts, ranges, coroutines)

**Example:**
```cpp
/**
 * @brief Creates a new WebView instance
 * @param width Width in pixels
 * @param height Height in pixels
 * @return WebView handle or 0 if creation failed
 */
uint32_t CreateWebView(int width, int height);
```

## Testing Requirements

All contributions should include appropriate tests:

- **Bug fixes**: Add a regression test that fails without the fix
- **New features**: Add tests covering typical use cases and edge cases
- **API changes**: Update existing tests and add new ones as needed

### Test Guidelines

- Tests should be fast and deterministic
- Use descriptive test names (`Test_FeatureName_Scenario_ExpectedBehavior`)
- Clean up resources properly (dispose WebView instances)
- Use `[UnityTest]` for tests requiring frame updates
- Handle async initialization with proper timeouts

## Versioning

We follow [Semantic Versioning](https://semver.org/):
- **MAJOR**: Breaking changes to public APIs
- **MINOR**: New features, backward compatible
- **PATCH**: Bug fixes, backward compatible

When making changes:
- Update `WebViewToolkit/package.json` version field
- Add entry to `CHANGELOG.md` under appropriate section
- CI will validate version increases on PR

## Changelog Format

We follow [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
## [Unreleased]

### Added
- New feature descriptions

### Changed
- Changes to existing functionality

### Fixed
- Bug fix descriptions

### Removed
- Removed features
```

## Branch Naming

Use descriptive branch names:
- `feature/feature-name` - New features
- `fix/issue-description` - Bug fixes
- `docs/topic` - Documentation updates
- `refactor/component-name` - Code refactoring

## Commit Messages

Write clear, descriptive commit messages:
- Use present tense ("Add feature" not "Added feature")
- Be concise but descriptive
- Reference issues/PRs when relevant
- Use conventional commits format if possible

**Examples:**
```
Add mouse wheel event support (#42)
Fix texture leak on WebView disposal
Update API documentation for WebViewPanel
Refactor navigation state management
```

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Questions?

If you have questions about contributing, please:
- Open a GitHub Discussion for general questions
- Open an issue for bug reports or feature requests
- Check existing documentation in the [README](README.md)

Thank you for contributing to WebView Toolkit! 🎉
