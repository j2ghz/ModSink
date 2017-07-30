Task("Build.WPF")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    MSBuild("src/ModSink.WPF/ModSink.WPF.csproj", configurator =>
    configurator.SetConfiguration("Release")
        .UseToolVersion(MSBuildToolVersion.VS2017)
        .WithTarget("WriteVersionInfoToBuildLog")
        .WithTarget("Build")
        );
});

Task("Build.Netstandard")
    .Does(() =>
{
    DotNetCoreRestore();
    DotNetCoreBuild("src/ModSink.Common/ModSink.Common.csproj");
    DotNetCorePublish("src/ModSink.Common/ModSink.Common.csproj");
});

Task("Build")
    .IsDependentOn("Build.WPF")
    .IsDependentOn("Build.Netstandard");

RunTarget("Build");