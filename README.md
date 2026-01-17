# NuGet Downloader - Offline Dependency Resolver

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS-lightgrey)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)

**NuGet Downloader** is a powerful, open-source **CLI tool** designed for developers who need to download NuGet packages and their complete dependency trees for **offline use**. 

Whether you are building an air-gapped development environment or simply need to archive specific package versions, this tool simplifies the process by resolving dependencies recursively and saving them into a clean, flat directory structure.

## 🚀 Key Features

- **Recursive Dependency Resolution**: Automatically traverses the dependency graph to find every required package for your target framework (e.g., `net10.0`, `net8.0`, `netstandard2.0`).
- **Smart Deduplication**: Uses advanced caching to ensure unique package versions are downloaded only once, saving bandwidth and disk space.
- **Flat Directory Structure**: Downloads all `.nupkg` files into a single `./OfflinePackages` folder, creating an instant **local NuGet feed** source.
- **Cross-Platform Support**: Fully compatible with **Windows**, **macOS**, and **Linux** thanks to .NET 10.
- **Modern CLI Experience**: Built with **Spectre.Console** to provide a beautiful terminal interface with progress bars, spinners, and color-coded feedback.

## Prerequisites

To run this project from source, you need:

- **.NET 10 SDK** (or newer)

## How to Run

1. Clone this repository.
2. Open a terminal in the project folder.
3. Run the application:

```bash
dotnet run
```

4. Follow the on-screen prompts:
   - Enter the **Package ID** (e.g., `Microsoft.SemanticKernel`).
   - Enter the **Version** (press Enter for the latest stable).
   - Enter the **Target Framework** (e.g., `net10.0`).

## How to Build for Release (Single File)

You can compile this into a single executable file that requires **no .NET installation** on the target machine.

### For Windows
```bash
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true
```

### For macOS
```bash
dotnet publish -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained true
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

[MIT](https://choosealicense.com/licenses/mit/)
