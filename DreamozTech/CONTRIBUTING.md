**Contributing to OvenBites**

Thank you for contributing! This document explains how to contribute code, tests and documentation to the OvenBites project, particularly for the DreamozTech API and Square payment integration.

**Getting started**
- Fork the repository and create a feature branch named `feature/your-short-description`.
- Keep changes focused and atomic. One feature or fix per pull request (PR).

**Development environment**
- The project is a .NET web application found at the repository root. Use Visual Studio or `dotnet` CLI.
- Typical commands:

```powershell
dotnet restore
dotnet build
dotnet run --project ./OvenBites/OvenBites.csproj
```

- The application reads secrets from environment variables. See `API.md` for recommended variables and examples.

**Coding guidelines**
- Follow existing code style in the repository; keep changes minimal and consistent.
- Use descriptive names and small functions.
- Avoid adding TODOs without a corresponding issue.

**Testing**
- Add unit tests where appropriate. If you add or modify server logic, include tests that mock external dependencies (DreamozTech and Square).
- Run tests with the test runner used by the project (if present) or `dotnet test`.

**Working with DreamozTech API**
- The app consumes DreamozTech API for pages, products and content. Do not commit DreamozTech API keys to the repo.
- Use local or CI environment variables for keys. During development you can use sandbox keys if DreamozTech supplies them.

**Square integration**
- Use Square sandbox credentials for local development and testing.
- Never store Square production secrets in public repositories.

**Creating a pull request**
- Open a PR from your branch to `master` with a descriptive title and summary.
- Include screenshots if the change affects UI.
- Reference related issues (e.g. `Fixes #123`).

**Code review**
- PRs should include at least one approving review before merging.
- Address review comments and keep PRs green (pass tests/build).

**Security & secrets**
- Use environment variables for all secrets. Example variables are listed in `API.md`.
- If you accidentally commit a secret, rotate it immediately and notify maintainers.

**Need help?**
- Open an issue with details about what you want to change or a problem you found.

Thanks for improving OvenBites — your contributions keep this integration healthy and secure.
