# NuGet Downloader

A modern .NET 10 Console Application to recursively calculate and download dependencies for any NuGet package into a flat directory structure. Built with **Spectre.Console** for a beautiful CLI experience.

## Features

- **Recursive Dependency Resolution**: Finds all dependencies for a package targeting a specific framework (e.g., `net10.0`, `net8.0`).
- **Smart Deduplication**: Ensures every unique package version is downloaded only once.
- **Flat Output**: All `.nupkg` files are saved to a single `./OfflinePackages` folder, making it easy to use as a local NuGet feed.
- **Resilient**: Checks for existing files to save bandwidth and handles errors gracefully.
- **Modern UI**: Features spinners, progress bars, and colored output.

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
