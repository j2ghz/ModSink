#tool "nuget:?package=GitVersion.CommandLine"
var solution = "ModSink.sln";
var csprojwpf = "src/ModSink.WPF/ModSink.WPF.csproj";
var csprojcommon = "src/ModSink.Common/ModSink.Common.csproj";

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
    DotNetCoreBuild(csprojcommon);
    DotNetCorePublish(csprojcommon);
});

Task("NuGet Restore")
    .Does(() => NuGetRestore(solution));

Task("DotNet Restore")
    .Does(() => NuGetRestore(solution));

Task("Build")
    .IsDependentOn("Build.WPF")
    .IsDependentOn("Build.Common");

RunTarget("Build");