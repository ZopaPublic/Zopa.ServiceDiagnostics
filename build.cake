#tool "nuget:?package=NUnit.ConsoleRunner"

var environmentKey = Environment.GetEnvironmentVariable("NugetKey");

var target = Argument("target", "Package");
var configuration = Argument("configuration", "Debug");
var nugetKey = Argument("nugetKey", environmentKey);

if(nugetKey == null)
{
	Warning("Nuget key is not set!");
}
else
{
var publicNugetKey =  nugetKey.Substring(0, Math.Min(nugetKey.Length, 5));
Information("nugetKey starts: " + publicNugetKey);
}


Task("Clean")
    .Does(() => 
{
    DeleteFiles("./*.nupkg");
});

Task("Restore_Nuget_Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./Zopa.ServiceDiagnostics.sln");
});

Task("Build")
    .IsDependentOn("Restore_Nuget_Packages")
    .Does(() =>
{
    MSBuild("./Zopa.ServiceDiagnostics.sln", new MSBuildSettings {
      ToolVersion = MSBuildToolVersion.VS2015,
      Configuration = configuration,
      PlatformTarget = PlatformTarget.MSIL
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var path = string.Format("./**/bin/{0}/Zopa.*.Tests.dll", configuration);
    NUnit3(path, new NUnit3Settings{
      Results = "./tests.xml"
    });

    if(BuildSystem.IsRunningOnAppVeyor)
    {
        BuildSystem.AppVeyor.UploadTestResults("./tests.xml", AppVeyorTestResultsType.NUnit3);
    }
});

Task("Package")
    .IsDependentOn("Test")
    .Does(() =>
{
    var binDir = string.Format("./Zopa.ServiceDiagnostics/bin/{0}", configuration);
	var settings = new NuGetPackSettings
    {
        BasePath = binDir,
        Symbols=true,
        Properties = new Dictionary<string, string>
        {
            { "Configuration", configuration }
        }
    };

    NuGetPack("./Zopa.ServiceDiagnostics/Zopa.ServiceDiagnostics.csproj", settings);
});

Task("Push")
    .IsDependentOn("Package")
    .Does(() =>
{
    if(string.IsNullOrEmpty(nugetKey))
    {
        throw new InvalidOperationException("Could not find nuget key.  It should be set in the 'NugetKey environment variable, or passed in as the 'nugetKey' argument");
    }

    var file = new DirectoryInfo(".")
                    .GetFiles("*.nupkg")
                    .OrderByDescending(x => x.CreationTimeUtc)
                    .FirstOrDefault();
    if(file==null)
    {
        throw new InvalidOperationException("Could not find any nupkg files");
    }

    var settings = new NuGetPushSettings{
        Source = "https://www.nuget.org/api/v2/package",
        ApiKey = nugetKey
    };

    NuGetPush(file.FullName, settings);
});

Task("default")
    .IsDependentOn("Package");

RunTarget(target);