#tool "nuget:?package=NUnit.ConsoleRunner"

var environmentKey = Environment.GetEnvironmentVariable("NugetKey");

var target = Argument("target", "Package");
var configuration = Argument("configuration", "Debug");
var nugetKey = Argument("nugetKey", environmentKey);

Information("nugetKey: " + nugetKey);

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
      ToolVersion = MSBuildToolVersion.VS2017,
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


Task("Push")
    .IsDependentOn("Test")
    .Does(() =>
{
    if(string.IsNullOrEmpty(nugetKey))
    {
        throw new InvalidOperationException("Could not find nuget key.  It should be set in the 'NugetKey environment variable, or passed in as the 'nugetKey' argument");
    }

	var path = "./Zopa.ServiceDiagnostics/bin/" + configuration;
    var file = new DirectoryInfo(path)
                    .GetFiles("*.nupkg")
                    .OrderByDescending(x => x.CreationTimeUtc)
                    .FirstOrDefault();
    if(file==null)
    {
        throw new InvalidOperationException("Could not find any nupkg files in " + path);
    }

    var settings = new NuGetPushSettings{
        Source = "https://www.nuget.org/api/v2/package",
        ApiKey = nugetKey
    };

    NuGetPush(file.FullName, settings);
});

Task("default")
    .IsDependentOn("Test");

RunTarget(target);