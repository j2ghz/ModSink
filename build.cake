#tool "nuget:?package=GitVersion.CommandLine"
var solution = "ModSink.sln";
var csprojwpf = "src/ModSink.WPF/ModSink.WPF.csproj";
var csprojcommon = "src/ModSink.Common/ModSink.Common.csproj";
var csprojcore = "src/ModSink.Core/ModSink.Core.csproj";
var csprojcli = "src/ModSink.CLI/ModSink.CLI.csproj";
string version = "";

Task("UpdateAssemblyInfo")
    .Does(() =>
{
    version = GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true,
        OutputType = GitVersionOutput.BuildServer
    }).LegacySemVer;
});

Task("Build.WPF")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("NuGet Restore")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
    {
        MSBuild("src/ModSink.WPF/ModSink.WPF.csproj", configurator => configurator.SetConfiguration("Release"));
        NuGetPack("src/ModSink.WPF/ModSink.WPF.nuspec", new NuGetPackSettings{ BasePath = "src/ModSink.WPF", OutputDirectory = "src/ModSink.WPF/bin", Version = version });
    });

Task("Build.Common")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCoreBuild(csprojcommon, new DotNetCoreBuildSettings{ Configuration = "Release" });
    DotNetCorePack(csprojcommon, new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = "./artifacts/Common"
     });
});

Task("Build.Core")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCoreBuild(csprojcore, new DotNetCoreBuildSettings{ Configuration = "Release" });
    DotNetCorePack(csprojcore, new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = "./artifacts/Core"
     });
});

Task("Build.CLI")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCoreBuild(csprojcli, new DotNetCoreBuildSettings{ Configuration = "Release" });
    DotNetCorePublish(csprojcli, new DotNetCorePublishSettings{ Configuration = "Release", OutputDirectory = "./artifacts/CLI" });
});

Task("NuGet Restore")
    .Does(() => NuGetRestore(solution));

Task("DotNet Restore")
    .Does(() => DotNetCoreRestore(solution));

Task("Build")
    .IsDependentOn("Build.WPF")
    .IsDependentOn("Build.Common")
    .IsDependentOn("Build.Core")
    .IsDependentOn("Build.CLI");

RunTarget("Build");