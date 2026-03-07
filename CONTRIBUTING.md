# How to Contribute

We're always looking for people to help make Playarr even better. There are several ways to contribute.

## Documentation

Improvements to the [README](README.md), [docs/](docs/), and [wiki/](wiki/) are always welcome.

## Bug Reports

1. Search [existing issues](https://github.com/Psychotonikum/playarr/issues) to avoid duplicates
2. Open a new issue with:
   - Clear description of the bug
   - Steps to reproduce
   - Expected vs actual behavior
   - Playarr version, OS, and runtime version
   - Relevant log excerpts (set log level to Debug)

## Feature Requests

Open an issue labeled "feature request" describing the problem it solves and how you envision it working.

## Development

### Tools Required

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/) 
- [Yarn](https://yarnpkg.com/)
- [Git](https://git-scm.com/downloads)
- IDE of choice (VS Code, Rider, Visual Studio)

### Getting Started

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/playarr.git
   cd playarr
   ```
3. Install frontend dependencies: `yarn install`
4. Start the frontend dev server: `yarn start`
5. Build the backend: `dotnet build src/Playarr.sln`
6. Run: `dotnet run --project src/Playarr/Playarr.csproj`
7. Open `http://localhost:9797`

### Running Tests

```bash
# Core tests
dotnet test src/Playarr.Core.Test/Playarr.Core.Test.csproj

# All tests
dotnet test src/Playarr.sln
```

### Contributing Code

- Check existing issues before starting work to avoid duplication
- Rebase from `main`, don't merge
- Make meaningful, focused commits
- Feel free to open a draft PR early for feedback
- Add tests for new functionality
- One feature/bug fix per pull request

### Code Style

- 4-space indentation (no tabs)
- Follow existing StyleCop rules (see `.editorconfig`)
- Use `var` for variable declarations in C#
- Unix line endings
- Keep changes focused — don't refactor unrelated code

### Pull Requests

- Target the `main` branch
- Include a clear description of what changed and why
- Ensure backend and frontend both build: `dotnet build src/Playarr.sln && yarn build`
- Ensure tests pass
- Each PR should come from its own feature branch with a meaningful name:
  - `add-platform-filter` (good)
  - `fix-rom-import` (good)
  - `patch` (bad)
  - `dev` (bad)

## License

By contributing, you agree that your contributions will be licensed under the [GNU GPL v3](http://www.gnu.org/licenses/gpl.html).
