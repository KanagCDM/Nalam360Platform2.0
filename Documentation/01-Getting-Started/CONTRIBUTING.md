# Contributing to Nalam360 Enterprise UI

Thank you for your interest in contributing to the Nalam360 Enterprise UI Component Library! We welcome contributions from the community.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Guidelines](#coding-guidelines)
- [Testing Requirements](#testing-requirements)
- [Submitting Changes](#submitting-changes)
- [Component Development Guidelines](#component-development-guidelines)

## Code of Conduct

This project adheres to a Code of Conduct that all contributors are expected to follow. Please be respectful and constructive in all interactions.

### Our Standards

- **Be Respectful**: Treat everyone with respect and kindness
- **Be Collaborative**: Work together toward common goals
- **Be Professional**: Maintain professionalism in all communications
- **Be Inclusive**: Welcome diverse perspectives and backgrounds

## Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022, Visual Studio Code, or JetBrains Rider
- Git

### Setting Up Your Development Environment

1. **Fork the Repository**
   ```bash
   # Fork on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/nalam360enterprise-ui.git
   cd nalam360enterprise-ui
   ```

2. **Add Upstream Remote**
   ```bash
   git remote add upstream https://github.com/nalam360/nalam360enterprise-ui.git
   ```

3. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Solution**
   ```bash
   dotnet build
   ```

5. **Run Tests**
   ```bash
   dotnet test
   ```

## Development Workflow

### Branching Strategy

We follow a simplified Git Flow:

- `main`: Production-ready code
- `develop`: Integration branch for features
- `feature/*`: Feature branches
- `bugfix/*`: Bug fix branches
- `hotfix/*`: Emergency fixes for production

### Creating a Feature Branch

```bash
git checkout develop
git pull upstream develop
git checkout -b feature/your-feature-name
```

### Making Changes

1. Make your changes in your feature branch
2. Write or update tests for your changes
3. Ensure all tests pass: `dotnet test`
4. Commit your changes with clear, descriptive messages

### Commit Message Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(inputs): add N360NumericTextBox component

Implemented NumericTextBox with validation, RBAC, and audit support.
Includes unit tests and documentation.

Closes #123
```

```
fix(grid): resolve paging issue with server-side filtering

Fixed bug where server-side filtering would reset to page 1.

Fixes #456
```

## Coding Guidelines

### C# Code Style

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments to all public APIs
- Maximum line length: 120 characters
- Use file-scoped namespaces (C# 10+)

### Razor Component Guidelines

- Component files should use PascalCase: `N360TextBox.razor`
- Use `@code` blocks for component logic
- Separate complex logic into separate C# files: `N360TextBox.razor.cs`
- Always include XML documentation for parameters
- Use proper parameter attributes: `[Parameter]`, `[CascadingParameter]`

### Example Component Structure

```razor
@typeparam TValue
@inject IPermissionService PermissionService

<div class="n360-component @CssClass">
    <!-- Component markup -->
</div>

@code {
    /// <summary>
    /// Gets or sets the CSS class for the component
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }

    /// <summary>
    /// Gets or sets the value
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Event callback when value changes
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
    }
}
```

### CSS Guidelines

- Use BEM naming convention: `.n360-component__element--modifier`
- Leverage design tokens from `design-tokens.css`
- Ensure RTL compatibility
- Support all theme variants

### Example CSS

```css
.n360-text-input {
    display: flex;
    flex-direction: column;
    gap: var(--n360-space-2);
}

.n360-text-input__label {
    font-size: var(--n360-font-size-sm);
    font-weight: var(--n360-font-weight-medium);
    color: var(--n360-color-neutral-700);
}

.n360-text-input__input {
    height: var(--n360-input-height-base);
    padding: var(--n360-space-2) var(--n360-space-3);
    border: var(--n360-input-border-width) solid var(--n360-color-neutral-300);
    border-radius: var(--n360-radius-base);
}

.n360-text-input__input:focus {
    outline: none;
    border-color: var(--n360-color-primary-500);
    box-shadow: 0 0 0 var(--n360-input-focus-ring-width) 
                rgba(33, 150, 243, var(--n360-input-focus-ring-opacity));
}
```

## Testing Requirements

### Unit Testing

All components must have unit tests using bUnit:

```csharp
[Fact]
public void ComponentName_ShouldBehavior_WhenCondition()
{
    // Arrange
    using var ctx = new TestContext();
    
    // Act
    var cut = ctx.RenderComponent<ComponentName>(parameters => parameters
        .Add(p => p.Property, value));
    
    // Assert
    cut.Find("selector").TextContent.Should().Be("expected");
}
```

### Test Coverage Requirements

- Minimum 80% code coverage for all components
- All public APIs must be tested
- All validation rules must be tested
- All RBAC scenarios must be tested

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Run specific test
dotnet test --filter "FullyQualifiedName~N360TextBoxTests"
```

## Component Development Guidelines

### Required Features for All Components

Every component must include:

1. **RBAC Support**
   - `RequiredPermission` parameter
   - `HideIfNoPermission` parameter
   - Integration with `IPermissionService`

2. **Audit Logging**
   - `EnableAudit` parameter
   - `AuditResource` parameter
   - Integration with `IAuditService`

3. **Validation** (for input components)
   - `ValidationRules` parameter
   - Support for custom validation rules
   - Display of validation errors

4. **Theming**
   - Use design tokens for colors, spacing, etc.
   - Support all theme variants
   - Custom CSS class parameter

5. **Accessibility**
   - Proper ARIA attributes
   - Keyboard navigation
   - Screen reader support

6. **Internationalization**
   - RTL support via `IsRtl` parameter
   - Localization-ready labels

7. **Documentation**
   - XML documentation for all public APIs
   - Usage examples in docs site
   - Parameter descriptions

### Component Checklist

Before submitting a new component:

- [ ] Component renders correctly
- [ ] All parameters are documented
- [ ] RBAC integration works
- [ ] Audit logging works
- [ ] Validation works (if applicable)
- [ ] Theming works across all themes
- [ ] Accessibility requirements met
- [ ] RTL support works
- [ ] Unit tests written (80%+ coverage)
- [ ] Documentation page created
- [ ] Usage examples provided
- [ ] CHANGELOG.md updated

## Submitting Changes

### Pull Request Process

1. **Update Your Branch**
   ```bash
   git checkout develop
   git pull upstream develop
   git checkout your-feature-branch
   git rebase develop
   ```

2. **Push to Your Fork**
   ```bash
   git push origin your-feature-branch
   ```

3. **Create Pull Request**
   - Go to GitHub and create a PR from your fork to `upstream/develop`
   - Fill out the PR template completely
   - Link any related issues

4. **Code Review**
   - Address review comments promptly
   - Make requested changes in new commits
   - Don't force-push after review has started

5. **Merge**
   - Once approved, a maintainer will merge your PR
   - Your branch will be deleted automatically

### Pull Request Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Checklist
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] All tests passing
- [ ] Code follows style guidelines

## Related Issues
Closes #123
```

## Questions?

If you have questions:

- Check existing [Issues](https://github.com/nalam360/nalam360enterprise-ui/issues)
- Start a [Discussion](https://github.com/nalam360/nalam360enterprise-ui/discussions)
- Contact the maintainers

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to Nalam360 Enterprise UI! 🎉
