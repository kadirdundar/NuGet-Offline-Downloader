using System.Collections.Concurrent;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuGetDownloader;

public class NugetService
{
    private readonly SourceCacheContext _cacheContext;
    private readonly SourceRepository _repository;
    private readonly ILogger _logger;

    public NugetService()
    {
        _cacheContext = new SourceCacheContext();
        _logger = NullLogger.Instance; // We handle logging via Spectre in Program.cs, keeping this clean.
        
        // Define the source directly as requested
        var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
        _repository = Repository.Factory.GetCoreV3(packageSource);
    }

    public async Task<PackageIdentity?> GetPackageIdentityAsync(string packageId, string? version, CancellationToken cancellationToken)
    {
        var findPackageResource = await _repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

        if (string.IsNullOrWhiteSpace(version))
        {
            // Get all versions and pick the latest stable one
            var versions = await findPackageResource.GetAllVersionsAsync(packageId, _cacheContext, _logger, cancellationToken);
            var latestVersion = versions.Where(v => !v.IsPrerelease).Max();
            
            // If no stable version, try prerelease
            if (latestVersion == null)
            {
                latestVersion = versions.Max();
            }

            if (latestVersion == null) return null;
            
            return new PackageIdentity(packageId, latestVersion);
        }
        else
        {
            if (NuGetVersion.TryParse(version, out var nuGetVersion))
            {
                // Verify it exists
                var exists = await findPackageResource.DoesPackageExistAsync(packageId, nuGetVersion, _cacheContext, _logger, cancellationToken);
                return exists ? new PackageIdentity(packageId, nuGetVersion) : null;
            }
            return null;
        }
    }

    public async Task<List<PackageIdentity>> GetDependenciesAsync(PackageIdentity rootPackage, NuGetFramework targetFramework, CancellationToken cancellationToken)
    {
        var uniquePackages = new HashSet<PackageIdentity>(PackageIdentityComparer.Default);
        var toProcess = new Queue<PackageIdentity>();
        
        toProcess.Enqueue(rootPackage);

        var dependencyInfoResource = await _repository.GetResourceAsync<DependencyInfoResource>(cancellationToken);

        while (toProcess.Count > 0)
        {
            var currentPackage = toProcess.Dequeue();

            if (!uniquePackages.Add(currentPackage))
            {
                continue;
            }

            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                currentPackage, targetFramework, _cacheContext, _logger, cancellationToken);

            if (dependencyInfo == null)
            {
                // Could not resolve dependency, might be platform specific or missing. 
                // In a robust app we might log this, but for now we continue.
                continue;
            }

            foreach (var dependency in dependencyInfo.Dependencies)
            {
                // Resolve the best match for the dependency version range
                // Note: This is a simplified resolution strategy. 
                // Real resolution (like NuGet restore) is much more complex involving the graph.
                // We will pick the lowest version that satisfies the range (min version strategy)
                // which is standard for "installing" packages.
                
                var depIdentity = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);
                if (!uniquePackages.Contains(depIdentity))
                {
                    toProcess.Enqueue(depIdentity);
                }
            }
        }

        return uniquePackages.ToList();
    }

    public async Task DownloadPackageAsync(PackageIdentity package, string outputDirectory, CancellationToken cancellationToken)
    {
        var findPackageResource = await _repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);
        
        // Ensure flat structure name: PackageId.Version.nupkg
        var fileName = $"{package.Id}.{package.Version}.nupkg";
        var filePath = Path.Combine(outputDirectory, fileName);

        if (File.Exists(filePath))
        {
            return;
        }

        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await findPackageResource.CopyNupkgToStreamAsync(
            package.Id, 
            package.Version, 
            fileStream, 
            _cacheContext, 
            _logger, 
            cancellationToken);
    }
}
