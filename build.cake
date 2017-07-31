#tool "nuget:?package=GitVersion.CommandLine"
#tool "Squirrel.Windows" 
#addin Cake.Squirrel

var solution = "ModSink.sln";
var csprojwpf = "src/ModSink.WPF/ModSink.WPF.csproj";
var csprojcommon = "src/ModSink.Common/ModSink.Common.csproj";
var csprojcore = "src/ModSink.Core/ModSink.Core.csproj";
var csprojcli = "src/ModSink.CLI/ModSink.CLI.csproj";
var SquirrelVersion = "";
var NuGetVersion = "";

Setup(context =>
{
    Information("Getting versions");
    var v = GitVersion();
    SquirrelVersion = v.LegacySemVerPadded;
    Information("Version for Squirrel: " + SquirrelVersion);
    NuGetVersion = v.NuGetVersionV2;
    Information("Version for NuGet libraries: " + NuGetVersion);
    Information("Full version info: "+ v.InformationalVersion);

    Information("Updating version in AssemblyInfo files");
    GitVersion(new GitVersionSettings { 
        UpdateAssemblyInfo = true, 
        OutputType = GitVersionOutput.BuildServer 
    });
});

Task("Build.WPF")
    .IsDependentOn("NuGet Restore")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
    {
        MSBuild("src/ModSink.WPF/ModSink.WPF.csproj", configurator => configurator.SetConfiguration("Release"));
        NuGetPack("src/ModSink.WPF/ModSink.WPF.nuspec", new NuGetPackSettings{ BasePath = "src/ModSink.WPF", OutputDirectory = "src/ModSink.WPF/bin", Version = SquirrelVersion });
        Squirrel(File("src/ModSink.WPF/bin/ModSink.WPF." + SquirrelVersion + ".nupkg"));
    });

Task("Build.Common")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCorePack(csprojcommon, new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = "./artifacts",
         ArgumentCustomization = args=>args.Append("/p:PackageVersion="+NuGetVersion)
     });
});

Task("Build.Core")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCorePack(csprojcore, new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = "./artifacts",
         ArgumentCustomization = args=>args.Append("/p:PackageVersion="+NuGetVersion)
     });
});



Task("NuGet Restore")
    .Does(() => NuGetRestore(solution));

Task("DotNet Restore")
    .Does(() => DotNetCoreRestore(solution));

Task("Build")
    .IsDependentOn("Build.Core")
    .IsDependentOn("Build.Common")
    .IsDependentOn("Build.WPF");

RunTarget("Build");