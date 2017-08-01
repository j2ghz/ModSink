#tool "nuget:?package=GitVersion.CommandLine"
#tool "Squirrel.Windows"
#tool "nuget:?package=gitreleasemanager"
#tool "nuget:?package=GitReleaseNotes"
#addin "Cake.Squirrel"
#addin nuget:?package=Cake.Git

var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");
var key_myget = EnvironmentVariable("MYGET_KEY") ?? "";
var key_nuget = EnvironmentVariable("NUGET_KEY") ?? "";
var url_myget_push = EnvironmentVariable("MYGET_URL") ?? "https://www.myget.org/F/modsink/api/v2/package";
var github_token = EnvironmentVariable("GITHUB_TOKEN") ?? "";

var root = Directory("./");
var solution = root + File("ModSink.sln");

var src = root + Directory("src");

var modSinkWpf_dir = src + Directory("ModSink.WPF");
var modSinkWpf_csproj = modSinkWpf_dir + File("ModSink.WPF.csproj");
var modSinkWpf_nuspec = modSinkWpf_dir + File("ModSink.WPF.nuspec");

var modSinkCore_dir = src + Directory("ModSink.Core");
var modSinkCore_csproj = modSinkCore_dir + File("ModSink.Core.csproj");

var modSinkCommon_dir = src + Directory("ModSink.Common");
var modSinkCommon_csproj = modSinkCommon_dir + File("ModSink.Common.csproj");

var modSinkCli_dir = src + Directory("ModSink.CLI");
var modSinkCli_csproj = modSinkCli_dir + File("ModSink.CLI.csproj");

var output = root + Directory("out");
var out_nuget = output + Directory("nuget");
var out_squirrel_nupkg = output + Directory("squirrel");
var out_squirrel = root + Directory("Releases");

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
        MSBuild(modSinkWpf_csproj, configurator => configurator.SetConfiguration(configuration));
        Information("Pack");
        CreateDirectory(out_squirrel_nupkg);
        NuGetPack(modSinkWpf_nuspec, new NuGetPackSettings{ BasePath = modSinkWpf_dir, OutputDirectory = out_squirrel_nupkg, Version = SquirrelVersion });
        Information("Releasify");
        Squirrel(out_squirrel_nupkg + File("ModSink.WPF." + SquirrelVersion + ".nupkg"));
    });

Task("Build.Common")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCorePack(modSinkCommon_csproj, new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = out_nuget,
         ArgumentCustomization = args=>args.Append("/p:PackageVersion="+NuGetVersion)
     });
});

Task("Build.Core")
    .IsDependentOn("DotNet Restore")
    .Does(() =>
{
    DotNetCorePack(modSinkCore_csproj, new DotNetCorePackSettings
     {
         Configuration = "Release",
         OutputDirectory = out_nuget,
         ArgumentCustomization = args=>args.Append("/p:PackageVersion="+NuGetVersion)
     });
});

Task("Publish.MyGet")
    .IsDependentOn("Build.Core")
    .IsDependentOn("Build.Common")
    .Does(()=>{
        foreach(var nupkg in GetFiles(out_nuget.ToString()+"/**/*.nupkg")){
            Information("Publishing: {0}", nupkg);
            NuGetPush(nupkg, new NuGetPushSettings {
                Source = url_myget_push,
                ApiKey = key_myget
            });
        }
    });

Task("Publish")
    .IsDependentOn("Publish.MyGet");

Task("Release")
//    .IsDependentOn("Release.NuGet")
    .IsDependentOn("Release.GitHub");

Task("Release.GitHub")
    .IsDependentOn("Build.WPF")
    .WithCriteria(GitBranchCurrent(root).CanonicalName == "refs/heads/master")
    .Does(()=>{
        Information("Releasing version {0} on Github", SquirrelVersion);
        
        var files = new List<string>();
        foreach(var f in GetFiles(out_squirrel.ToString() + "/**/*")){
            Information("    {0}", f);
            files.Add(f.FullPath);            
        }
        var filesSrt = String.Join(",",files);
        GitReleaseManagerCreate("j2ghz", github_token, "j2ghz", "modsink", new GitReleaseManagerCreateSettings {
            Name              = SquirrelVersion,
            InputFilePath     = "ReleaseNotesTemplate.md",
            Prerelease        = true,
            Assets            = filesSrt
            //TargetCommitish   = "master",
            //TargetDirectory   = "c:/repo",
            //LogFilePath       = "c:/temp/grm.log"
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

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Publish")
    .IsDependentOn("Release");

RunTarget(target);