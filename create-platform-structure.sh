#!/bin/bash

# Create solution
dotnet new sln -n Nalam360EnterprisePlatform

# Create source directory
mkdir -p src

# Create all platform projects
dotnet new classlib -n Nalam360.Platform.Core -o src/Nalam360.Platform.Core -f net8.0
dotnet new classlib -n Nalam360.Platform.Domain -o src/Nalam360.Platform.Domain -f net8.0
dotnet new classlib -n Nalam360.Platform.Application -o src/Nalam360.Platform.Application -f net8.0
dotnet new classlib -n Nalam360.Platform.Data -o src/Nalam360.Platform.Data -f net8.0
dotnet new classlib -n Nalam360.Platform.Messaging -o src/Nalam360.Platform.Messaging -f net8.0
dotnet new classlib -n Nalam360.Platform.Caching -o src/Nalam360.Platform.Caching -f net8.0
dotnet new classlib -n Nalam360.Platform.Serialization -o src/Nalam360.Platform.Serialization -f net8.0
dotnet new classlib -n Nalam360.Platform.Security -o src/Nalam360.Platform.Security -f net8.0
dotnet new classlib -n Nalam360.Platform.Observability -o src/Nalam360.Platform.Observability -f net8.0
dotnet new classlib -n Nalam360.Platform.Resilience -o src/Nalam360.Platform.Resilience -f net8.0
dotnet new classlib -n Nalam360.Platform.Integration -o src/Nalam360.Platform.Integration -f net8.0
dotnet new classlib -n Nalam360.Platform.FeatureFlags -o src/Nalam360.Platform.FeatureFlags -f net8.0
dotnet new classlib -n Nalam360.Platform.Tenancy -o src/Nalam360.Platform.Tenancy -f net8.0
dotnet new classlib -n Nalam360.Platform.Validation -o src/Nalam360.Platform.Validation -f net8.0
dotnet new classlib -n Nalam360.Platform.Documentation -o src/Nalam360.Platform.Documentation -f net8.0

# Create test projects
mkdir -p tests
dotnet new xunit -n Nalam360.Platform.Tests -o tests/Nalam360.Platform.Tests -f net8.0

# Create example application
mkdir -p examples
dotnet new webapi -n Nalam360.Platform.Example.Api -o examples/Nalam360.Platform.Example.Api -f net8.0

# Add all projects to solution
dotnet sln add src/Nalam360.Platform.Core/Nalam360.Platform.Core.csproj
dotnet sln add src/Nalam360.Platform.Domain/Nalam360.Platform.Domain.csproj
dotnet sln add src/Nalam360.Platform.Application/Nalam360.Platform.Application.csproj
dotnet sln add src/Nalam360.Platform.Data/Nalam360.Platform.Data.csproj
dotnet sln add src/Nalam360.Platform.Messaging/Nalam360.Platform.Messaging.csproj
dotnet sln add src/Nalam360.Platform.Caching/Nalam360.Platform.Caching.csproj
dotnet sln add src/Nalam360.Platform.Serialization/Nalam360.Platform.Serialization.csproj
dotnet sln add src/Nalam360.Platform.Security/Nalam360.Platform.Security.csproj
dotnet sln add src/Nalam360.Platform.Observability/Nalam360.Platform.Observability.csproj
dotnet sln add src/Nalam360.Platform.Resilience/Nalam360.Platform.Resilience.csproj
dotnet sln add src/Nalam360.Platform.Integration/Nalam360.Platform.Integration.csproj
dotnet sln add src/Nalam360.Platform.FeatureFlags/Nalam360.Platform.FeatureFlags.csproj
dotnet sln add src/Nalam360.Platform.Tenancy/Nalam360.Platform.Tenancy.csproj
dotnet sln add src/Nalam360.Platform.Validation/Nalam360.Platform.Validation.csproj
dotnet sln add src/Nalam360.Platform.Documentation/Nalam360.Platform.Documentation.csproj
dotnet sln add tests/Nalam360.Platform.Tests/Nalam360.Platform.Tests.csproj
dotnet sln add examples/Nalam360.Platform.Example.Api/Nalam360.Platform.Example.Api.csproj

echo "Platform structure created successfully!"
