var solution = "ModSink.sln";
var csprojwpf = "src/ModSink.WPF/ModSink.WPF.csproj";
var csprojcommon = "src/ModSink.Common/ModSink.Common.csproj";

Task("Build.WPF")
    .IsDependentOn("NuGet Restore")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
    MSBuild("src/ModSink.WPF/ModSink.WPF.csproj", configurator =>
        configurator.SetConfiguration("Release")
            .UseToolVersion(MSBuildToolVersion.VS2017)
            .WithTarget("WriteVersionInfoToBuildLog")
            .WithTarget("Build")
    ));

Task("Build.Common")
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