# Package Icon Generation Guide

## Requirements
All NuGet packages require a package icon for professional appearance on NuGet.org.

### Specifications
- **Format**: PNG
- **Dimensions**: 128x128 pixels minimum (recommended: 256x256 or 512x512)
- **File Size**: < 1 MB
- **Background**: Transparent preferred
- **Design**: Should represent the package purpose

## Icons Needed (15 packages)

### Platform Modules (14)
1. `Nalam360.Platform.Core` - Foundation/core icon
2. `Nalam360.Platform.Domain` - DDD patterns icon
3. `Nalam360.Platform.Application` - CQRS/Mediator icon
4. `Nalam360.Platform.Data` - Database/repository icon
5. `Nalam360.Platform.Messaging` - Message bus icon
6. `Nalam360.Platform.Caching` - Cache icon
7. `Nalam360.Platform.Serialization` - JSON/XML icon
8. `Nalam360.Platform.Security` - Lock/shield icon
9. `Nalam360.Platform.Observability` - Metrics/monitoring icon
10. `Nalam360.Platform.Integration` - Integration/gRPC icon
11. `Nalam360.Platform.FeatureFlags` - Toggle/flag icon
12. `Nalam360.Platform.Tenancy` - Multi-tenant icon
13. `Nalam360.Platform.Validation` - Checkmark/validation icon
14. `Nalam360.Platform.Documentation` - Document icon

### UI Library (1)
15. `Nalam360Enterprise.UI` - Component library icon

## Icon Design Suggestions

### Color Scheme
- **Primary**: #0078D4 (Microsoft Blue)
- **Secondary**: #50E6FF (Accent Blue)
- **Success**: #107C10 (Green)
- **Warning**: #FFB900 (Yellow)
- **Error**: #E81123 (Red)

### Design Approach

#### Option 1: Single Unified Icon
Create one base icon for all packages with variations:
- Use "N360" monogram
- Add small module-specific accent/badge

#### Option 2: Module-Specific Icons
Each package gets unique icon representing its purpose:
- Core: Gear/foundation symbol
- Domain: Cube/building blocks
- Application: Arrow flow/pipeline
- Data: Database cylinder
- etc.

#### Option 3: Icon Family
Consistent style across all icons:
- Same shape/border
- Same color palette
- Different internal symbols

## Recommended: Use SVG to PNG Conversion

### Using Online Tools
1. **Canva** (https://www.canva.com)
   - Professional templates
   - Easy export to PNG
   - Free tier available

2. **Figma** (https://www.figma.com)
   - Vector-based design
   - High-quality exports
   - Free for personal use

3. **Adobe Express** (https://www.adobe.com/express)
   - Quick icon creation
   - Professional results

### Using Icon Libraries
1. **Font Awesome** (https://fontawesome.com)
   - Extensive icon library
   - Free and pro versions

2. **Material Icons** (https://fonts.google.com/icons)
   - Google's icon set
   - Free for all uses

3. **Lucide** (https://lucide.dev)
   - Open-source icons
   - Modern design

## Quick Generation Script (PowerShell)

```powershell
# This is a placeholder - actual icon creation requires design software
# For now, create text-based placeholders

$packages = @(
    "Core", "Domain", "Application", "Data", "Messaging",
    "Caching", "Serialization", "Security", "Observability",
    "Integration", "FeatureFlags", "Tenancy", "Validation",
    "Documentation", "UI"
)

foreach ($package in $packages) {
    Write-Host "Icon needed for: Nalam360.Platform.$package" -ForegroundColor Yellow
}
```

## Integration with .csproj

Once icons are created, add to each project file:

```xml
<PropertyGroup>
  <PackageIcon>icon.png</PackageIcon>
</PropertyGroup>

<ItemGroup>
  <None Include="..\..\assets\icons\$(MSBuildProjectName).png" Pack="true" PackagePath="\" />
  <!-- Or use a shared icon: -->
  <None Include="..\..\assets\icons\nalam360-icon.png" Pack="true" PackagePath="\" Link="icon.png" />
</ItemGroup>
```

## Temporary Placeholder

Until professional icons are created, you can use:
1. **Initials/Monogram**: "N360" text on colored background
2. **Emoji**: Use Unicode emoji (⚙️, 📦, 🔒, etc.) - but not recommended for production
3. **Generic**: Use a single Nalam360 logo for all packages initially

## Next Steps

1. **Immediate**: Create simple placeholder icon (N360 monogram)
2. **Short-term**: Design professional icon set
3. **Best Practice**: Hire designer for consistent branding

## File Locations

Store icons in:
```
assets/
  icons/
    nalam360-core.png
    nalam360-domain.png
    nalam360-application.png
    ... (one per package)
    
    # Or single shared icon:
    nalam360-platform.png
```

## Example: Simple Placeholder Creation

For immediate use, create a simple 128x128 PNG with:
- Background: #0078D4 (blue)
- Text: "N360" in white, centered
- Font: Bold, modern sans-serif

This can be refined later with professional design.

---

**Status**: 📋 Icons needed for 15 packages  
**Priority**: Medium (required before NuGet publication)  
**Estimated Time**: 2-4 hours for professional design, 30 minutes for placeholder
