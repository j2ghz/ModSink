#tool "nuget:?package=GitVersion.CommandLine"
var solution = "ModSink.sln";
var csprojwpf = "src/ModSink.WPF/ModSink.WPF.csproj";
var csprojcommon = "src/ModSink.Common/ModSink.Common.csproj";
var csprojcli = "src/ModSink.CLI/ModSink.CLI.csproj";

Task("UpdateAssemblyInfo")
    .Does(() =>
{
    GitVersion(new GitVersionSettings {
        UpdateAssemblyInfo = true,
        OutputType = GitVersionOutput.BuildServer
    });
});

Task("Build.WPF")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("NuGet Restore")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
    MSBuild("src/ModSink.WPF/ModSink.WPF.csproj", configurator =>
        configurator.SetConfiguration("Release")
    ));

Task("Build.Common")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCoreRestore();
    DotNetCoreBuild(csprojcommon, new DotNetCoreBuildSettings{ Configuration = "Release" });
    DotNetCorePublish(csprojcommon, new DotNetCorePublishSettings{ Configuration = "Release" });
});

Task("Build.CLI")
    .IsDependentOn("UpdateAssemblyInfo")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCoreRestore();
    DotNetCoreBuild(csprojcli, new DotNetCoreBuildSettings{ Configuration = "Release" });
    DotNetCorePublish(csprojcli, new DotNetCorePublishSettings{ Configuration = "Release" });
});

Task("NuGet Restore")
    .Does(() => NuGetRestore(solution));

Task("DotNet Restore")
    .Does(() => DotNetCoreRestore(solution));

Task("Build")
    .IsDependentOn("Build.WPF")
    .IsDependentOn("Build.Common");

RunTarget("Build");