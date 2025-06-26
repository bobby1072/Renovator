# Renovator

A .NET console application for automating package dependency upgrades across multiple ecosystems. Currently only able to do NPM package renovation for Node.js projects, Renovator is designed as a generic renovation platform that will expand to support additional package managers and project types in the future.

Renovator helps you keep your projects up-to-date by analyzing dependency files and suggesting or automatically applying dependency upgrades.

## Features

### Current NPM Support

- **View Potential Upgrades**: Analyze package.json files to see available package upgrades with version information and release dates
- **Local Repository Renovation**: Automatically upgrade dependencies in local Node.js projects
- **Git Repository Renovation**: Clone and renovate remote Git repositories temporarily
- **Interactive Console Interface**: User-friendly menu-driven interface with arrow key navigation
- **Safe Rollback**: Automatic rollback on failed upgrades
- **NPM Registry Integration**: Fetches latest package information from the NPM registry

### Future Expansion

Renovator is architected as a generic renovation platform with plans to support:

- **NuGet** packages for .NET projects
- **pip** packages for Python projects
- **Maven/Gradle** dependencies for Java projects
- **Composer** packages for PHP projects
- **Go modules** for Go projects
- **Cargo** crates for Rust projects

## Prerequisites

### Current Requirements (NPM Support)

- [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- [Node.js](https://nodejs.org/) and NPM installed on your system
- [Git](https://git-scm.com/) (required for Git repository renovation features)

### Future Requirements

As support for additional package managers is added, you'll need the respective tools installed:

- **.NET SDK** for NuGet package renovation
- **Python & pip** for Python package renovation
- **Java & Maven/Gradle** for Java dependency renovation
- And so on...

## Installation

### Option 1: Clone and Build from Source

```powershell
# Clone the repository
git clone <repository-url>
cd Renovator

# Navigate to the main solution
cd src/Renovator

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run the application
dotnet run --project Renovator.ConsoleApp
```

### Option 2: Build and Run Console App Directly

```powershell
# Navigate to the console app directory
cd src/Renovator/Renovator.ConsoleApp

# Run the application
dotnet run
```

## Configuration

The application uses `appsettings.json` for configuration. The default configuration includes:

```json
{
  "NpmJsRegistryHttpClientSettings": {
    "BaseUrl": "https://registry.npmjs.com/",
    "TimeoutInSeconds": 45,
    "TotalAttempts": 3,
    "DelayBetweenAttemptsInSeconds": 1,
    "UseJitter": true
  }
}
```

### Configuration Options

- **BaseUrl**: NPM registry URL (default: https://registry.npmjs.com/)
- **TimeoutInSeconds**: HTTP request timeout
- **TotalAttempts**: Number of retry attempts for failed requests
- **DelayBetweenAttemptsInSeconds**: Delay between retry attempts
- **UseJitter**: Enable jitter for retry delays to avoid thundering herd problems

## Usage

Run the application and you'll be presented with a main menu:

```
Welcome to Renovator.ConsoleApp...

This app can be used to renovate your projects.

Currently supporting Node.JS projects with plans to expand to other ecosystems.

1. View potential package upgrades for project within your local file system.
2. Attempt to renovate project within your local file system.
3. Attempt to renovate public remote git repo.
4. Exit.
```

### Option 1: View Potential Upgrades

- Select option 1 from the main menu
- Enter the path to your package.json file (relative or absolute)
- The application will display current package versions and available upgrades with release dates

### Option 2: Renovate Local Project

- Select option 2 from the main menu
- Enter the path to your package.json file
- Review the list of potential upgrades
- Use arrow keys to select which packages to upgrade
- The application will:
  - Update the package.json file
  - Run `npm install`
  - Automatically rollback if the installation fails

### Option 3: Renovate Git Repository

- Select option 3 from the main menu
- Enter the Git repository URL
- Select which package.json file to renovate (if multiple exist)
- Choose upgrades to apply
- The application will:
  - Clone the repository to a temporary location
  - Apply upgrades
  - Run `npm install`
  - Clean up temporary files

## Project Structure

```
src/
├── Renovator/
│   ├── Renovator.sln                    # Main solution file
│   ├── Renovator.ConsoleApp/            # Console application entry point
│   ├── Renovator.Common/                # Shared utilities and constants
│   ├── Renovator.Domain.Models/         # Domain models and data structures
│   ├── Renovator.Domain.Services/       # Business logic and services
│   ├── Renovator.NpmHttpClient/         # NPM registry HTTP client
│   └── Renovator.Tests/                 # Unit tests
└── Submodules/
    └── BT/                              # External dependencies
```

### Key Components

#### Current NPM Implementation

- **ConsoleApplicationService**: Main application logic and user interface
- **NpmRenovatorProcessingManager**: Handles local repository renovations
- **GitNpmRenovatorProcessingManager**: Handles Git repository renovations
- **NpmJsRegistryHttpClient**: Communicates with NPM registry
- **RepoExplorerService**: Analyzes package.json files
- **NpmCommandService**: Executes NPM commands
- **GitCommandService**: Executes Git commands

#### Generic Architecture

The application is designed with extensibility in mind:

- **Processing Managers**: Abstract base classes for different package managers
- **Registry Clients**: Pluggable HTTP clients for various package registries
- **Command Services**: Abstracted command execution for different toolchains
- **Repository Explorers**: Generic file analysis for different project types

## Development

### Running Tests

```powershell
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Dependencies

### Main Application

- Microsoft.Extensions.Hosting
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging

### HTTP Client

- Custom BT.Common.Http libraries for resilient HTTP communication
- Polly for retry policies

### Testing

- xUnit testing framework
- AutoFixture for test data generation
