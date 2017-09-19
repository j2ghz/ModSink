#tool "nuget:?package=GitVersion.CommandLine"
#tool "Squirrel.Windows"
#tool "nuget:?package=gitreleasemanager" 
#addin "Cake.Squirrel"

var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");
var github_token = EnvironmentVariable("GITHUB_TOKEN") ?? "";

var root = Directory("./");
var solution = root + File("ModSink.WPF.sln");

var src = root + Directory("src");

var modSinkWpf_dir = src + Directory("ModSink.WPF");
var modSinkWpf_csproj = modSinkWpf_dir + File("ModSink.WPF.csproj");
var modSinkWpf_nuspec = modSinkWpf_dir + File("ModSink.WPF.nuspec");

var output = root + Directory("out");
var out_squirrel_nupkg = output + Directory("squirrel");
var out_squirrel = root + Directory("Releases");

var SquirrelVersion = "";
var Version = "";

Setup(context =>
{
    Information("Getting versions");
    var v = GitVersion();
    SquirrelVersion = v.LegacySemVerPadded;
    Information("Version for Squirrel: " + SquirrelVersion);
    Version = v.FullSemVer;
    Information("Full version info: "+ v.InformationalVersion);

    Information("Updating version in AssemblyInfo files");
    GitVersion(new GitVersionSettings { 
        UpdateAssemblyInfo = true, 
        OutputType = GitVersionOutput.BuildServer 
    });
});

Task("Build")
    .Does(() =>
    {
        NuGetRestore(solution);
        MSBuild(modSinkWpf_csproj, configurator => configurator.SetConfiguration(configuration).UseToolVersion(MSBuildToolVersion.VS2017));        
    });

Task("Pack")
    .IsDependentOn("Build")
    .Does(()=>{
        CreateDirectory(out_squirrel_nupkg);
        NuGetPack(modSinkWpf_nuspec, new NuGetPackSettings{ BasePath = modSinkWpf_dir + Directory("bin") + Directory(configuration), OutputDirectory = out_squirrel_nupkg, Version = SquirrelVersion });
        Squirrel(out_squirrel_nupkg + File("ModSink.WPF." + SquirrelVersion + ".nupkg"));    
    });

Task("Release")
    .IsDependentOn("Pack")
    .WithCriteria(BuildSystem.AppVeyor.IsRunningOnAppVeyor && BuildSystem.AppVeyor.Environment.Repository.Branch == "master")
    .Does(()=>{
        Information("Releasing version {0} on Github", SquirrelVersion);
        
        var files = new List<string>();
        foreach(var f in GetFiles(out_squirrel.ToString() + "/**/*")){
            Information("    {0}", f);
            files.Add(f.FullPath);
        }
        var filesSrt = String.Join(",",files);
        GitReleaseManagerCreate("j2ghz", github_token, "j2ghz", "modsink", new GitReleaseManagerCreateSettings {
            Name              = Version,
            InputFilePath     = "ReleaseNotesTemplate.md",
            Prerelease        = true,
            Assets            = filesSrt
            //TargetCommitish   = "master",
            //TargetDirectory   = "c:/repo",
            //LogFilePath       = "c:/temp/grm.log"
        });
        GitReleaseManagerPublish("j2ghz", github_token, "j2ghz", "modsink", Version);
    });

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Release");

RunTarget(target);
