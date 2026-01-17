# Code Quality and Pre-commit Checks

This project uses automated code quality tools and pre-commit checks to maintain consistent code style and catch issues early, similar to Python's Ruff.

## Tools Configured

### 1. dotnet format
- Built-in .NET code formatter
- Enforces rules defined in [.editorconfig](.editorconfig)
- Automatically fixes formatting issues

### 2. Roslyn Analyzers
The project includes three powerful analyzer packages:

- **StyleCop.Analyzers**: Enforces C# style and consistency rules
- **Roslynator.Analyzers**: Provides additional code analysis and refactoring suggestions
- **SonarAnalyzer.CSharp**: Detects code quality issues and security vulnerabilities

### 3. Husky.Net
- Git hooks manager for .NET
- Automatically runs checks before commits
- Prevents committing code that doesn't meet quality standards

## Pre-commit Checks

When you attempt to commit code, the following checks run automatically:

1. **Code Formatting** (`dotnet format --verify-no-changes`)
   - Ensures all code follows the .editorconfig rules
   - Fails if code is not properly formatted

2. **Build Verification** (`dotnet build --no-restore`)
   - Ensures the project builds successfully
   - Runs all configured analyzers
   - Fails if there are build errors

3. **Test Execution** (`dotnet test --no-build --no-restore`)
   - Runs all unit tests
   - Fails if any tests fail

## Usage

### Formatting Your Code

Before committing, format your code:

```bash
# Option 1: Use dotnet format directly
dotnet format

# Option 2: Use the provided script
./format.sh
```

### Checking Code Without Committing

Verify your code meets all standards:

```bash
# Check formatting only
dotnet format --verify-no-changes

# Build and run analyzers
dotnet build

# Run tests
dotnet test
```

### Bypassing Pre-commit Checks (Not Recommended)

In rare cases where you need to commit without running checks:

```bash
git commit --no-verify -m "Your message"
```

**Note**: Only use this when absolutely necessary, as it bypasses important quality checks.

## Configuration Files

- [.editorconfig](.editorconfig): Code style rules (indentation, spacing, naming conventions)
- [.globalconfig](.globalconfig): Analyzer severity overrides
- [stylecop.json](stylecop.json): StyleCop-specific configuration
- [.husky/pre-commit](.husky/pre-commit): Pre-commit hook script

## Analyzer Rules

### Disabled Rules

Some overly strict rules have been disabled or set to suggestion level:

- `SA1633`: File headers (disabled)
- `SA1600`/`SA1601`: XML documentation requirements (suggestion)
- `SA1309`: Underscore prefix for private fields (disabled)
- `SA1101`: 'this.' prefix requirement (disabled)

### Customizing Rules

To adjust rule severity, edit [.globalconfig](.globalconfig):

```ini
# Set a rule to error (fails build)
dotnet_diagnostic.SA1000.severity = error

# Set a rule to warning
dotnet_diagnostic.SA1000.severity = warning

# Set a rule to suggestion (IDE only)
dotnet_diagnostic.SA1000.severity = suggestion

# Disable a rule
dotnet_diagnostic.SA1000.severity = none
```

## IDE Integration

### Visual Studio / Rider
Analyzers work automatically in the IDE, showing warnings and suggestions as you type.

### VS Code
Install the C# extension. Analyzers will provide real-time feedback.

## CI/CD Integration

Add these commands to your CI/CD pipeline:

```yaml
# Example GitHub Actions
- name: Check code formatting
  run: dotnet format --verify-no-changes

- name: Build
  run: dotnet build

- name: Test
  run: dotnet test
```

## Common Issues

### "Code formatting check failed"
**Solution**: Run `dotnet format` or `./format.sh` before committing.

### "Build failed"
**Solution**: Fix the build errors shown in the output. The analyzers will point you to the issues.

### "Tests failed"
**Solution**: Fix the failing tests before committing.

### Pre-commit hook not running
**Solution**: Ensure Husky is installed:
```bash
dotnet tool restore
dotnet husky install
```

## Benefits

- **Consistency**: All code follows the same style guidelines
- **Early Detection**: Catch issues before they reach code review
- **Quality**: Automated enforcement of best practices
- **Security**: SonarAnalyzer detects potential security vulnerabilities
- **Productivity**: Less time spent on style discussions in code reviews

## Resources

- [EditorConfig Documentation](https://editorconfig.org/)
- [StyleCop Rules](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/DOCUMENTATION.md)
- [Roslynator Rules](https://github.com/dotnet/roslynator)
- [SonarC# Rules](https://rules.sonarsource.com/csharp/)
- [dotnet format Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format)
