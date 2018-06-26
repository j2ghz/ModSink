#tool "nuget:?package=GitVersion.CommandLine"
#tool "Squirrel.Windows"
#addin "Cake.Squirrel"

var target = Argument("target", "Default");
var configuration = Argument("Configuration", "Release");

var root = Directory("./");
var solution = root + File("ModSink.sln");

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
});

Task("Pack")
    .Does(()=>{
        CreateDirectory(out_squirrel_nupkg);
        NuGetPack(modSinkWpf_nuspec, new NuGetPackSettings{ BasePath = modSinkWpf_dir + Directory("bin") + Directory(configuration) + Directory("net461"), OutputDirectory = out_squirrel_nupkg, Version = SquirrelVersion, Verbosity = NuGetVerbosity.Detailed });
        Squirrel(out_squirrel_nupkg + File("ModSink.WPF." + SquirrelVersion + ".nupkg"));    
    });

Task("Default")
    .IsDependentOn("Pack");

RunTarget(target);
