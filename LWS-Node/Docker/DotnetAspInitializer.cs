using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace LWS_Node.Docker;

[ExcludeFromCodeCoverage]
public class AspInitResult<T>
{
    public bool IsSucceed { get; set; }
    public string Message { get; set; }
    public T? TargetObject { get; set; }
}

public class DotnetAspInitializer
{
    private readonly string _tempPath = Path.GetTempPath();
    private readonly string _repositoryUrl;
    private readonly string _jsonSettings;
    private string _absoluteProjectPath;
    private string _programName;
    
    public DotnetAspInitializer(string repositoryUrl, string jsonSettings)
    {
        _repositoryUrl = repositoryUrl;
        _jsonSettings = jsonSettings;
    }

    public AspInitResult<string> InitAsp()
    {
        // Clone Repository
        var repositoryPath = CloneRepository();

        if (!repositoryPath.IsSucceed) return repositoryPath;
        
        // Setup Necessary Paths
        SetupPaths(repositoryPath.TargetObject!);
        
        // Setup New Settings
        ReplaceAppSettings(repositoryPath.TargetObject!);
        
        InitiateDockerFile(repositoryPath.TargetObject);

        return new AspInitResult<string>
        {
            IsSucceed = true,
            Message = "",
            TargetObject = repositoryPath.TargetObject
        };
    }

    private void SetupPaths(string repositoryPath)
    {
        // Find csproj
        _absoluteProjectPath = Directory.EnumerateFiles(repositoryPath, "*.csproj", SearchOption.AllDirectories)
            .Select(a => a.Split($"{repositoryPath}/")[1])
            .First(a => !a.Contains("Test"));
        
        // From csproj path, Get Program Name
        var fileName = Path.GetFileName(_absoluteProjectPath);
        var programName = fileName!.Split(".csproj");
        _programName = $"{programName[0]}.dll";
    }

    private AspInitResult<string> CloneRepository()
    {
        var tmpRepositoryPath = Path.Join(_tempPath, GenerateRandomToken());
        try
        {
            Repository.Clone(_repositoryUrl, tmpRepositoryPath);
        }
        catch (Exception e)
        {
            return new AspInitResult<string>
            {
                IsSucceed = false,
                Message = $"Cannot clone repository from url {_repositoryUrl}, message: {e.Message}"
            };
        }
        

        return new AspInitResult<string>
        {
            IsSucceed = true,
            TargetObject = tmpRepositoryPath
        };
    }

    private void ReplaceAppSettings(string repositoryPath)
    {
        // Find App Settings
        foreach (string eachPath in Directory.EnumerateFiles(repositoryPath, "appsettings.json" ,SearchOption.AllDirectories))
        {
            if (File.Exists(eachPath))
            {
                File.Delete(eachPath);
            }
            
            // Write Json Settings to appsettings.json
            using var stream = File.OpenWrite(eachPath);
            using var fileWriter = new StreamWriter(stream);
            fileWriter.Write(_jsonSettings);
        }
    }

    private void InitiateDockerFile(string repositoryPath)
    {
        // Remove Any Dockerfile
        foreach (string eachPath in Directory.EnumerateFiles(repositoryPath, "Dockerfile", SearchOption.AllDirectories))
        {
            File.Delete(eachPath);
        }
        
        // On Top of repository path, create dockerfile
        using var file = File.OpenWrite(Path.Join(repositoryPath, "Dockerfile"));
        using var fileWriter = new StreamWriter(file);
        
        // Write it
        fileWriter.WriteLine("FROM mcr.microsoft.com/dotnet/sdk:6.0");
        fileWriter.WriteLine("COPY . .");
        fileWriter.WriteLine($"RUN dotnet build \"{_absoluteProjectPath}\" -c Release -o /app/build");
        fileWriter.WriteLine("WORKDIR /app/build");
        fileWriter.WriteLine($"ENTRYPOINT [\"dotnet\", \"{_programName}\"]");
    }

    private string GenerateRandomToken(int length = 8)
    {
        var random = new Random();
        var charDictionary = "1234567890abcdefghijklmnopqrstuvwxyz";

        return new string(Enumerable.Repeat(charDictionary, length)
            .Select(a => a[random.Next(charDictionary.Length)]).ToArray());
    }
}