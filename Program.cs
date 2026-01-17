using NuGet.Frameworks;
using NuGet.Versioning;
using Spectre.Console;
using NuGetDownloader;

public class Program
{
    public static async Task Main(string[] args)
    {
        // 1. ASCII Header
        AnsiConsole.Write(
            new FigletText("NuGet Downloader")
                .Color(Color.Cyan1));
        AnsiConsole.WriteLine("------------------------------------------");

        try
        {
            // 2. Inputs
            var packageId = AnsiConsole.Ask<string>("[green]Package ID[/] (e.g. Microsoft.SemanticKernel):").Trim();
            var versionInput = AnsiConsole.Prompt(
                new TextPrompt<string>("[green]Version[/] (optional, press Enter for latest):")
                    .AllowEmpty())?.Trim();
            
            // Default to current runtime or net8.0 if user types nothing? 
            // The prompt says "e.g. net10.0". Let's ask for it.
            var frameworkString = AnsiConsole.Ask<string>("[green]Target Framework[/] (e.g. net10.0):", "net10.0").Trim();
            var targetFramework = NuGetFramework.Parse(frameworkString);

            var nugetService = new NugetService();
            var cts = new CancellationTokenSource();

            // 3. Resolve & Deduplicate with Spinner
            var packagesToDownload = new List<NuGet.Packaging.Core.PackageIdentity>();

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("Resolving dependencies...", async ctx =>
                {
                    // Find root package
                    ctx.Status("Locating root package...");
                    var rootPackage = await nugetService.GetPackageIdentityAsync(packageId, string.IsNullOrEmpty(versionInput) ? null : versionInput, cts.Token);

                    if (rootPackage == null)
                    {
                        throw new Exception($"Package '{packageId}' not found on NuGet.org.");
                    }

                    AnsiConsole.MarkupLine($"[bold]Found:[/] {rootPackage.Id} {rootPackage.Version}");

                    // Resolve dependencies
                    ctx.Status("Walking dependency graph...");
                    packagesToDownload = await nugetService.GetDependenciesAsync(rootPackage, targetFramework, cts.Token);
                });

            AnsiConsole.MarkupLine($"[bold]Total unique packages to download:[/] {packagesToDownload.Count}");

            // 4. Download with Progress Bar
            var outputDir = Path.Combine(Directory.GetCurrentDirectory(), "OfflinePackages");
            Directory.CreateDirectory(outputDir);

            await AnsiConsole.Progress()
                .StartAsync(async ctx => 
                {
                    var task = ctx.AddTask($"[green]Downloading {packagesToDownload.Count} packages...[/]");
                    task.MaxValue = packagesToDownload.Count;

                    foreach (var package in packagesToDownload)
                    {
                        task.Description = $"Downloading [bold]{package.Id}[/]...";
                        
                        try
                        {
                            await nugetService.DownloadPackageAsync(package, outputDir, cts.Token);
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue
                            AnsiConsole.MarkupLine($"[red]Error downloading {package.Id}: {ex.Message}[/]");
                        }
                        
                        task.Increment(1);
                    }
                });

            // 5. Summary
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[green]Success![/] Total {packagesToDownload.Count} packages downloaded to [blue]{outputDir}[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
        }
    }
}
