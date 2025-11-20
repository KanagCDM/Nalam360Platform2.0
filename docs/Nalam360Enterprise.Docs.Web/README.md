# Nalam360Enterprise.Docs.Web

Interactive documentation website for the Nalam360 Enterprise UI component library.

## Features

- **Component Browser** - Search and filter all 144+ components
- **Interactive Playground** - Live component editor with real-time preview
- **API Reference** - Comprehensive parameter, event, and method documentation
- **Code Examples** - Copy-paste ready code snippets
- **Accessibility Guide** - WCAG compliance and keyboard navigation details
- **Getting Started** - Installation and configuration guide

## Running Locally

### Prerequisites
- .NET 9.0 SDK
- Syncfusion license key (for components)

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/KanagCDM/Nalam360EnterprisePlatform.git
   cd Nalam360EnterprisePlatform/docs/Nalam360Enterprise.Docs.Web
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

4. **Open browser**
   Navigate to `https://localhost:5001` or `http://localhost:5000`

## Project Structure

```
Nalam360Enterprise.Docs.Web/
├── Components/
│   └── Pages/
│       ├── Home.razor              # Landing page
│       ├── Components.razor        # Component browser
│       ├── ComponentDocs.razor     # Individual component docs
│       └── GettingStarted.razor    # Getting started guide
├── wwwroot/
│   ├── css/                        # Custom styles
│   └── js/                         # JavaScript interop
├── Program.cs                      # Application entry point
└── Nalam360Enterprise.Docs.Web.csproj
```

## Deployment

### GitHub Pages

1. Build for production:
   ```bash
   dotnet publish -c Release
   ```

2. Configure `gh-pages` branch:
   ```bash
   git checkout --orphan gh-pages
   cp -r bin/Release/net9.0/publish/wwwroot/* .
   git add .
   git commit -m "Deploy documentation"
   git push origin gh-pages
   ```

3. Enable GitHub Pages in repository settings

### Azure Static Web Apps

1. Create Azure Static Web App resource
2. Connect to GitHub repository
3. Configure build:
   ```yaml
   app_location: "/docs/Nalam360Enterprise.Docs.Web"
   output_location: "wwwroot"
   ```

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nalam360Enterprise.Docs.Web.dll"]
```

## Contributing

Contributions to the documentation are welcome!

### Adding Component Documentation

1. Navigate to `Components/Pages/ComponentDocs.razor`
2. Add component metadata to the component list
3. Create example code snippets
4. Document parameters, events, and methods

### Adding Examples

1. Create new `.razor` file in `Components/Pages/Examples/`
2. Add code examples with descriptions
3. Include live preview and code snippets

## Technology Stack

- **.NET 9.0** - Framework
- **Blazor Web App** - UI framework
- **Syncfusion Blazor** - UI components
- **Markdig** - Markdown rendering
- **Prism.js** - Syntax highlighting

## Links

- **Live Documentation**: https://kanagcdm.github.io/Nalam360EnterprisePlatform
- **GitHub Repository**: https://github.com/KanagCDM/Nalam360EnterprisePlatform
- **NuGet Package**: https://nuget.org/packages/Nalam360Enterprise.UI
- **Issue Tracker**: https://github.com/KanagCDM/Nalam360EnterprisePlatform/issues

## License

MIT License - see [LICENSE](../../LICENSE) for details

---

Built with ❤️ by the Nalam360 team
