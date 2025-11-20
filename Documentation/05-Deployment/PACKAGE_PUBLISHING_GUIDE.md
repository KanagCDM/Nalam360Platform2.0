# 📦 Package Publishing Guide

## Overview

This guide provides step-by-step instructions for publishing the Nalam360 Enterprise Platform packages to NuGet.

---

## Prerequisites

### 1. NuGet Account Setup

1. **Create NuGet.org account**
   - Visit https://www.nuget.org/users/account/LogOn
   - Sign up or log in

2. **Generate API Key**
   - Go to https://www.nuget.org/account/apikeys
   - Click "Create"
   - Name: `Nalam360-CI-CD`
   - Glob Pattern: `Nalam360.*`
   - Select packages: Choose specific packages or use glob pattern
   - Click "Create"
   - **Copy the API key immediately** (you won't see it again)

3. **Add API Key to GitHub Secrets**
   - Go to your repository on GitHub
   - Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Paste your NuGet API key
   - Click "Add secret"

---

## Publishing Methods

### Method 1: Automated via GitHub Actions (Recommended)

#### For Platform Modules (.NET 8)

```bash
# Tag the release for platform modules
git tag -a platform-v1.0.0 -m "Platform Modules v1.0.0"
git push origin platform-v1.0.0

# GitHub Actions will:
# 1. Run security scan
# 2. Build all 14 platform modules
# 3. Run tests
# 4. Pack all modules
# 5. Publish to NuGet.org
```

#### For UI Library (.NET 9)

```bash
# Tag the release for UI library
git tag -a ui-v1.0.0 -m "UI Library v1.0.0"
git push origin ui-v1.0.0

# GitHub Actions will:
# 1. Run security scan
# 2. Build UI library
# 3. Run tests
# 4. Pack UI library
# 5. Publish to NuGet.org
```

#### For Both (All Packages)

```bash
# Tag for combined release
git tag -a v1.0.0 -m "Nalam360 Enterprise Platform v1.0.0"
git push origin v1.0.0

# This will publish both platform modules and UI library
```

### Method 2: Manual Publishing

#### Step 1: Update Package Versions

Update `<Version>` in each `.csproj` file:

```xml
<PropertyGroup>
  <Version>1.0.0</Version>
</PropertyGroup>
```

#### Step 2: Build and Pack Platform Modules

```bash
# Navigate to solution root
cd "D:\Mocero\Healthcare Platform\Nalam360EnterprisePlatform"

# Build in Release mode
dotnet build Nalam360EnterprisePlatform.sln --configuration Release

# Pack all platform modules
for /d %i in (src\Nalam360.Platform.*) do (
    dotnet pack "%i\%~nxi.csproj" --configuration Release --output ./nupkg
)
```

#### Step 3: Build and Pack UI Library

```bash
# Pack UI library
dotnet pack src\Nalam360Enterprise.UI\Nalam360Enterprise.UI\Nalam360Enterprise.UI.csproj `
    --configuration Release `
    --output ./nupkg
```

#### Step 4: Publish to NuGet

```bash
# Set API key (do once per machine)
$env:NUGET_API_KEY = "your-api-key-here"

# Publish all packages
dotnet nuget push .\nupkg\*.nupkg `
    --api-key $env:NUGET_API_KEY `
    --source https://api.nuget.org/v3/index.json `
    --skip-duplicate
```

---

## Package Metadata Checklist

Before publishing, ensure each `.csproj` has complete metadata:

```xml
<PropertyGroup>
  <!-- Required -->
  <PackageId>Nalam360.Platform.Core</PackageId>
  <Version>1.0.0</Version>
  <Authors>Nalam360 Team</Authors>
  <Description>Core functionality for Nalam360 Enterprise Platform</Description>
  
  <!-- Recommended -->
  <Company>Nalam360</Company>
  <Product>Nalam360 Enterprise Platform</Product>
  <PackageTags>enterprise;clean-architecture;ddd;cqrs</PackageTags>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  
  <!-- Optional but helpful -->
  <RepositoryUrl>https://github.com/KanagCDM/Nalam360EnterprisePlatform</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageProjectUrl>https://github.com/KanagCDM/Nalam360EnterprisePlatform</PackageProjectUrl>
  <PackageIcon>icon.png</PackageIcon>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <PackageReleaseNotes>See CHANGELOG.md</PackageReleaseNotes>
</PropertyGroup>

<ItemGroup>
  <None Include="..\..\README.md" Pack="true" PackagePath="\" />
  <None Include="..\..\icon.png" Pack="true" PackagePath="\" />
</ItemGroup>
```

---

## Version Numbering Strategy

We use **Semantic Versioning** (SemVer):

```
MAJOR.MINOR.PATCH

MAJOR: Breaking changes
MINOR: New features (backward compatible)
PATCH: Bug fixes (backward compatible)
```

### Examples

- **1.0.0** - Initial release
- **1.0.1** - Bug fix release
- **1.1.0** - New features added
- **2.0.0** - Breaking changes

### Pre-release Versions

```
1.0.0-alpha.1  - Alpha release
1.0.0-beta.1   - Beta release
1.0.0-rc.1     - Release candidate
```

---

## Publishing Checklist

### Before Publishing

- [ ] All tests passing
- [ ] Security vulnerabilities addressed
- [ ] Package metadata complete
- [ ] Version numbers updated
- [ ] CHANGELOG.md updated
- [ ] README.md updated
- [ ] Documentation complete
- [ ] Icon file included (icon.png)
- [ ] License file included (LICENSE)

### After Publishing

- [ ] Verify packages on NuGet.org
- [ ] Test package installation in clean project
- [ ] Update documentation with new version
- [ ] Create GitHub release with release notes
- [ ] Announce release (blog, Twitter, etc.)

---

## Testing Packages Locally

Before publishing to NuGet, test packages locally:

```bash
# Pack packages
dotnet pack --configuration Release --output ./local-feed

# Add local feed
dotnet nuget add source "D:\Mocero\Healthcare Platform\Nalam360EnterprisePlatform\local-feed" --name LocalNalam360

# Test in another project
dotnet new webapi -n TestApp
cd TestApp
dotnet add package Nalam360.Platform.Core --source LocalNalam360

# Remove local feed after testing
dotnet nuget remove source LocalNalam360
```

---

## Package Dependencies

### Platform Modules Dependencies

```
Nalam360.Platform.Core (No dependencies)
├── Nalam360.Platform.Domain (depends on Core)
│   └── Nalam360.Platform.Application (depends on Domain, Core)
│       └── Nalam360.Platform.Data (depends on Application, Domain, Core)
├── Nalam360.Platform.Messaging (depends on Core)
├── Nalam360.Platform.Caching (depends on Core)
├── Nalam360.Platform.Security (depends on Core)
└── ... (other modules depend on Core)
```

### UI Library Dependencies

```
Nalam360Enterprise.UI
├── Syncfusion.Blazor (27.1.48+)
├── Microsoft.AspNetCore.Components.Web
└── (Optional) Nalam360.Platform.* modules for backend integration
```

---

## Troubleshooting

### Issue: "Package already exists"

**Solution**: NuGet doesn't allow overwriting existing versions. Increment version number.

### Issue: "API key is invalid"

**Solution**: 
1. Verify API key hasn't expired
2. Check API key has correct permissions
3. Regenerate API key if necessary

### Issue: "Package validation failed"

**Solution**:
1. Check all required metadata is present
2. Ensure package dependencies are correct
3. Verify files are properly included

### Issue: "Build failed on CI/CD"

**Solution**:
1. Check GitHub Actions logs
2. Verify all dependencies are restored
3. Ensure .NET versions are correct (8.0 for platform, 9.0 for UI)

---

## Alternative Publishing Targets

### Azure Artifacts

```bash
# Add Azure Artifacts source
dotnet nuget add source https://pkgs.dev.azure.com/YOUR_ORG/_packaging/YOUR_FEED/nuget/v3/index.json `
    --name AzureArtifacts `
    --username YOUR_USERNAME `
    --password YOUR_PAT `
    --store-password-in-clear-text

# Publish
dotnet nuget push .\nupkg\*.nupkg `
    --source AzureArtifacts `
    --api-key az
```

### GitHub Packages

```bash
# Add GitHub Packages source
dotnet nuget add source https://nuget.pkg.github.com/KanagCDM/index.json `
    --name github `
    --username KanagCDM `
    --password YOUR_GITHUB_PAT `
    --store-password-in-clear-text

# Publish
dotnet nuget push .\nupkg\*.nupkg `
    --source github `
    --api-key YOUR_GITHUB_PAT
```

---

## CI/CD Pipeline Status

Check the build status on GitHub Actions:
- https://github.com/KanagCDM/Nalam360EnterprisePlatform/actions

### Pipeline Stages

1. **Security Scan** - Checks for vulnerable packages
2. **Build Platform** - Builds all 14 platform modules (.NET 8)
3. **Build UI** - Builds UI library (.NET 9)
4. **Publish Platform** - Publishes platform modules to NuGet
5. **Publish UI** - Publishes UI library to NuGet
6. **Publish Docs** - Publishes documentation

---

## Support

For publishing issues:
- Check GitHub Actions logs
- Review NuGet.org package validation messages
- Contact: support@nalam360.com (if applicable)
- GitHub Issues: https://github.com/KanagCDM/Nalam360EnterprisePlatform/issues

---

**Last Updated**: November 17, 2025  
**Version**: 1.0.0
